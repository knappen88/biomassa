using UnityEngine;
using SymbiosisDefense.Core;
using System.Collections.Generic;
using System.Linq;

namespace SymbiosisDefense.Gameplay
{
    public class TowerManager : MonoBehaviour, ITowerManager
    {
        [Header("Tower Configuration")]
        [SerializeField] private TowerPrefabData[] towerPrefabs;

        [Header("Runtime")]
        [SerializeField] private List<BaseTower> activeTowers = new();

        private IGridManager gridManager;
        private IResourceManager resourceManager;

        public void Initialize()
        {
            gridManager = ServiceLocator.Get<IGridManager>();
            resourceManager = ServiceLocator.Get<IResourceManager>();

            if (gridManager == null || resourceManager == null)
            {
                Debug.LogError("TowerManager: Required services not found!");
            }

            Debug.Log("TowerManager initialized");
        }

        public bool CanBuildTower(Vector3 position, TowerType type)
        {
            // Проверяем позицию
            if (!gridManager.IsValidPosition(position) || gridManager.IsOccupied(position))
                return false;

            // Проверяем ресурсы
            TowerStats stats = GetTowerStats(type);
            if (stats == null || !resourceManager.CanAfford(stats.buildCost))
                return false;

            return true;
        }

        public GameObject BuildTower(Vector3 position, TowerType type)
        {
            if (!CanBuildTower(position, type))
            {
                Debug.LogWarning($"Cannot build {type} tower at {position}");
                return null;
            }

            // Снапаем к сетке
            Vector3 snapPosition = gridManager.SnapToGrid(position);

            // Находим префаб
            GameObject prefab = GetTowerPrefab(type);
            if (prefab == null)
            {
                Debug.LogError($"Prefab for {type} not found!");
                return null;
            }

            // Создаем башню
            GameObject towerObj = Instantiate(prefab, snapPosition, Quaternion.identity);
            BaseTower baseTower = towerObj.GetComponent<BaseTower>();

            if (baseTower != null)
            {
                // Регистрируем
                activeTowers.Add(baseTower);
                gridManager.OccupyCell(snapPosition, towerObj);

                // Тратим ресурсы
                TowerStats stats = GetTowerStats(type);
                if (stats != null)
                {
                    resourceManager.SpendBiomass(stats.buildCost);
                }

                // Подписываемся на события
                baseTower.OnTowerDestroyed += OnTowerDestroyed;

                Debug.Log($"Built {type} tower at {snapPosition}");

                // Создаем wrapper для события
                Tower tower = new Tower(baseTower, type);
                GameEvents.TowerBuilt?.Invoke(towerObj);
            }

            return towerObj;
        }

        public void DestroyTower(GameObject towerObj)
        {
            if (towerObj == null) return;

            BaseTower baseTower = towerObj.GetComponent<BaseTower>();
            if (baseTower != null)
            {
                activeTowers.Remove(baseTower);

                // Освобождаем ячейку
                gridManager.FreeCell(baseTower.Position);

                Debug.Log($"Tower destroyed");
            }

            // Уничтожаем объект
            if (towerObj != null)
                Destroy(towerObj);
        }

        public GameObject[] GetAllTowers()
        {
            return activeTowers
                .Where(t => t != null && t.IsActive)
                .Select(t => t.gameObject)
                .ToArray();
        }

        // Дополнительные методы для внутреннего использования
        public Tower[] GetAllTowersAsWrappers()
        {
            return activeTowers
                .Where(t => t != null && t.IsActive)
                .Select(t => new Tower(t, t.TowerType))
                .ToArray();
        }

        private void OnTowerDestroyed(BaseTower tower)
        {
            activeTowers.Remove(tower);
            if (tower.gameObject != null)
                GameEvents.TowerDestroyed?.Invoke(tower.gameObject);
        }

        private GameObject GetTowerPrefab(TowerType type)
        {
            var data = towerPrefabs.FirstOrDefault(t => t.towerType == type);
            return data?.prefab;
        }

        private TowerStats GetTowerStats(TowerType type)
        {
            var data = towerPrefabs.FirstOrDefault(t => t.towerType == type);
            var baseTower = data?.prefab?.GetComponent<BaseTower>();
            return baseTower?.Stats;  // Используем публичное свойство
        }

        // Debug info
        void OnGUI()
        {
            if (!Debug.isDebugBuild) return;

            GUILayout.BeginArea(new Rect(10, 10, 200, 100));
            GUILayout.Label($"Active Towers: {activeTowers.Count}");
            GUILayout.EndArea();
        }
    }

    // Вспомогательные классы
    [System.Serializable]
    public class TowerPrefabData
    {
        public TowerType towerType;
        public GameObject prefab;
    }

    // Wrapper класс Tower для удобства
    public class Tower
    {
        public BaseTower BaseTower { get; }
        public TowerType Type { get; }
        public Vector3 Position => BaseTower.Position;
        public bool IsActive => BaseTower.IsActive;

        public Tower(BaseTower baseTower, TowerType type)
        {
            BaseTower = baseTower;
            Type = type;
        }
    }
}