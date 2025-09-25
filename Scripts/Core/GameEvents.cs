using System;
using UnityEngine;

namespace SymbiosisDefense.Core
{
    // ������� ������� ����
    public static class GameEvents
    {
        // Tower �������
        public static Action<GameObject> TowerBuilt;
        public static Action<GameObject> TowerDestroyed;
        public static Action<GameObject, int> TowerUpgraded;

        // Resource �������  
        public static Action<int> BiomassChanged;
        public static Action<int> EnergyChanged;

        // Wave �������
        public static Action<int> WaveStarted;
        public static Action<int> WaveCompleted;

        // Enemy �������
        public static Action<GameObject> EnemyKilled;
        public static Action<GameObject> EnemyReachedEnd;

        // Utility ������ ��� ������ �������
        public static void InvokeTowerBuilt(GameObject tower) => TowerBuilt?.Invoke(tower);
        public static void InvokeTowerDestroyed(GameObject tower) => TowerDestroyed?.Invoke(tower);
        public static void InvokeBiomassChanged(int amount) => BiomassChanged?.Invoke(amount);
        public static void InvokeEnergyChanged(int amount) => EnergyChanged?.Invoke(amount);
        public static void InvokeWaveStarted(int wave) => WaveStarted?.Invoke(wave);
        public static void InvokeWaveCompleted(int wave) => WaveCompleted?.Invoke(wave);
    }

    // ��������� ��� ���� ��������, ������� ����� �������� ����
    public interface IDamageable
    {
        void TakeDamage(float damage);
        float CurrentHealth { get; }
        bool IsAlive { get; }
    }

    // ��������� ��� ��������, ������� ����� ����������� �������
    public interface IUpdatable
    {
        void UpdateBehavior();
        bool IsActive { get; }
    }

    // ��������� ��� �������-�����
    public interface ISymbiosisNode
    {
        Vector3 Position { get; }
        SymbiosisType[] SupportedSymbiosisTypes { get; }
        bool CanConnectTo(ISymbiosisNode other);
        void AddConnection(ISymbiosisConnection connection);
        void RemoveConnection(ISymbiosisConnection connection);
    }

    // ��������� ��� ������ ��������  
    public interface ISymbiosisConnection
    {
        ISymbiosisNode Source { get; }
        ISymbiosisNode Target { get; }
        SymbiosisType ConnectionType { get; }
        bool IsActive { get; }
        float EnergyCost { get; }
        void Activate();
        void Deactivate();
    }
}