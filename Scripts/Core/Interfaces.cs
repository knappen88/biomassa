using UnityEngine;

namespace SymbiosisDefense.Core
{
    public interface IResourceManager
    {
        int Biomass { get; }
        int Energy { get; }
        bool CanAfford(int cost);
        void SpendBiomass(int amount);
        void AddBiomass(int amount);
        void SpendEnergy(int amount);
        void AddEnergy(int amount);
    }

    public interface IGridManager
    {
        void Initialize();
        bool IsValidPosition(Vector3 worldPosition);
        Vector3 SnapToGrid(Vector3 worldPosition);
        GridCell GetCell(Vector3 worldPosition);
        bool IsOccupied(Vector3 worldPosition);
        void OccupyCell(Vector3 worldPosition, GameObject occupant);
        void FreeCell(Vector3 worldPosition);
    }

    public interface ITowerManager
    {
        void Initialize();
        bool CanBuildTower(Vector3 position, TowerType type);
        GameObject BuildTower(Vector3 position, TowerType type);
        void DestroyTower(GameObject tower);
        GameObject[] GetAllTowers();
    }

    public interface IEnemySpawner
    {
        void Initialize();
        void StartWave(int waveNumber);
        void SpawnEnemy(EnemyType type, Vector3 position);
    }
}