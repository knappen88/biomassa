using UnityEngine;
using SymbiosisDefense.Core;
using System.Collections;
using System.Collections.Generic;

namespace SymbiosisDefense.Gameplay
{
    public class EnemySpawner : MonoBehaviour, IEnemySpawner
    {
        [Header("Spawn Configuration")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private EnemyWaveData[] waveData;
        [SerializeField] private EnemyPrefabData[] enemyPrefabs;

        [Header("Runtime")]
        [SerializeField] private int currentWave = 0;
        [SerializeField] private bool isSpawning = false;
        [SerializeField] private List<BaseEnemy> activeEnemies = new();

        private Coroutine currentWaveCoroutine;

        public void Initialize()
        {
            if (spawnPoint == null)
            {
                GameObject spawnObj = new GameObject("EnemySpawnPoint");
                spawnObj.transform.position = new Vector3(-10, 0, 0);
                spawnPoint = spawnObj.transform;
            }

            Debug.Log("EnemySpawner initialized");
        }

        public void StartWave(int waveNumber)
        {
            if (isSpawning)
            {
                Debug.LogWarning("Wave already in progress!");
                return;
            }

            currentWave = waveNumber;

            if (currentWaveCoroutine != null)
                StopCoroutine(currentWaveCoroutine);

            currentWaveCoroutine = StartCoroutine(SpawnWaveCoroutine(waveNumber));

            GameEvents.WaveStarted?.Invoke(waveNumber);
        }

        private IEnumerator SpawnWaveCoroutine(int waveNumber)
        {
            isSpawning = true;

            EnemyWaveData wave = GetWaveData(waveNumber);
            if (wave == null)
            {
                Debug.LogError($"Wave data for wave {waveNumber} not found!");
                yield break;
            }

            Debug.Log($"Starting wave {waveNumber}: {wave.totalEnemies} enemies");

            for (int i = 0; i < wave.totalEnemies; i++)
            {
                EnemyType enemyType = wave.GetEnemyTypeForIndex(i);
                SpawnEnemy(enemyType, spawnPoint.position);

                yield return new WaitForSeconds(wave.spawnInterval);
            }

            isSpawning = false;

            // Ждем пока все враги не будут уничтожены
            yield return new WaitUntil(() => activeEnemies.Count == 0);

            GameEvents.WaveCompleted?.Invoke(waveNumber);
        }

        public void SpawnEnemy(EnemyType type, Vector3 position)
        {
            GameObject prefab = GetEnemyPrefab(type);
            if (prefab == null)
            {
                Debug.LogError($"Prefab for {type} not found!");
                return;
            }

            GameObject enemyObj = Instantiate(prefab, position, Quaternion.identity);
            BaseEnemy enemy = enemyObj.GetComponent<BaseEnemy>();

            if (enemy != null)
            {
                activeEnemies.Add(enemy);
                enemy.OnEnemyKilled += OnEnemyKilled;
                enemy.OnEnemyReachedEnd += OnEnemyReachedEnd;
            }

            Debug.Log($"Spawned {type} enemy at {position}");
        }

        private void OnEnemyKilled(BaseEnemy enemy)
        {
            activeEnemies.Remove(enemy);
        }

        private void OnEnemyReachedEnd(BaseEnemy enemy)
        {
            activeEnemies.Remove(enemy);
            // TODO: Damage player base
            Debug.LogWarning("Enemy reached the end!");
        }

        private EnemyWaveData GetWaveData(int waveNumber)
        {
            if (waveNumber <= 0 || waveNumber > waveData.Length)
            {
                // Генерируем процедурную волну для высоких номеров
                return GenerateProceduralWave(waveNumber);
            }

            return waveData[waveNumber - 1];
        }

        private EnemyWaveData GenerateProceduralWave(int waveNumber)
        {
            // Простая процедурная генерация
            EnemyWaveData wave = ScriptableObject.CreateInstance<EnemyWaveData>();
            wave.totalEnemies = Mathf.Min(5 + waveNumber * 2, 20);
            wave.spawnInterval = Mathf.Max(0.5f, 2f - waveNumber * 0.1f);
            wave.enemyTypes = new EnemyType[] { EnemyType.Basic };

            return wave;
        }

        private GameObject GetEnemyPrefab(EnemyType type)
        {
            foreach (var data in enemyPrefabs)
            {
                if (data.enemyType == type)
                    return data.prefab;
            }
            return null;
        }
    }

    // Вспомогательные классы для врагов
    [System.Serializable]
    public class EnemyPrefabData
    {
        public EnemyType enemyType;
        public GameObject prefab;
    }

    [CreateAssetMenu(fileName = "EnemyWaveData", menuName = "Symbiosis Defense/Enemy Wave Data")]
    public class EnemyWaveData : ScriptableObject
    {
        public int totalEnemies = 10;
        public float spawnInterval = 1f;
        public EnemyType[] enemyTypes = { EnemyType.Basic };

        public EnemyType GetEnemyTypeForIndex(int index)
        {
            return enemyTypes[index % enemyTypes.Length];
        }
    }

    [CreateAssetMenu(fileName = "EnemyStats", menuName = "Symbiosis Defense/Enemy Stats")]
    public class EnemyStats : ScriptableObject
    {
        [Header("Identity")]
        public EnemyType enemyType;
        public string displayName;

        [Header("Combat")]
        public float maxHealth = 50f;
        public float moveSpeed = 2f;
        public float armor = 0f;

        [Header("Rewards")]
        public int biomassReward = 10;
        public int energyReward = 5;
    }
}