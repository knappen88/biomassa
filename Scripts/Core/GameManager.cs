using UnityEngine;
using SymbiosisDefense.Gameplay;
using SymbiosisDefense.UI;

namespace SymbiosisDefense.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game State")]
        [SerializeField] private bool isGameActive = false;
        [SerializeField] private int currentWave = 0;

        [Header("References")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private TowerManager towerManager;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private ResourceManager resourceManager;

        void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Регистрируем сервисы в правильном порядке
            ServiceLocator.Register<IResourceManager>(resourceManager);
            ServiceLocator.Register<IGridManager>(gridManager);
            ServiceLocator.Register<ITowerManager>(towerManager);
            ServiceLocator.Register<IEnemySpawner>(enemySpawner);

            // Инициализируем системы
            gridManager.Initialize();
            towerManager.Initialize();
            enemySpawner.Initialize();
            resourceManager.Initialize();

            Debug.Log("Game systems initialized successfully!");
            isGameActive = true;
        }

        public void StartWave()
        {
            if (!isGameActive) return;

            currentWave++;
            enemySpawner.StartWave(currentWave);
            Debug.Log($"Wave {currentWave} started!");
        }

        public void EndWave()
        {
            Debug.Log($"Wave {currentWave} completed!");
            // Логика окончания волны
        }
    }
}