using UnityEngine;
using SymbiosisDefense.Core;

namespace SymbiosisDefense.Gameplay
{
    public class ResourceManager : MonoBehaviour, IResourceManager
    {
        [Header("Starting Resources")]
        [SerializeField] private int startingBiomass = 500;
        [SerializeField] private int startingEnergy = 1000;

        [Header("Current Resources")]
        [SerializeField] private int currentBiomass;
        [SerializeField] private int currentEnergy;

        [Header("Resource Generation")]
        [SerializeField] private int energyGenerationRate = 10; // per second
        [SerializeField] private float generationInterval = 1f;

        // Свойства интерфейса
        public int Biomass => currentBiomass;
        public int Energy => currentEnergy;

        private float lastGenerationTime;

        public void Initialize()
        {
            currentBiomass = startingBiomass;
            currentEnergy = startingEnergy;
            lastGenerationTime = Time.time;

            Debug.Log($"Resources initialized: Biomass={currentBiomass}, Energy={currentEnergy}");

            // Уведомляем UI
            GameEvents.BiomassChanged?.Invoke(currentBiomass);
            GameEvents.EnergyChanged?.Invoke(currentEnergy);
        }

        void Update()
        {
            if (Time.time - lastGenerationTime >= generationInterval)
            {
                GenerateEnergy();
                lastGenerationTime = Time.time;
            }
        }

        private void GenerateEnergy()
        {
            AddEnergy(energyGenerationRate);
        }

        public bool CanAfford(int cost)
        {
            return currentBiomass >= cost;
        }

        public void SpendBiomass(int amount)
        {
            if (amount <= 0) return;

            currentBiomass = Mathf.Max(0, currentBiomass - amount);
            GameEvents.BiomassChanged?.Invoke(currentBiomass);

            Debug.Log($"Spent {amount} biomass. Remaining: {currentBiomass}");
        }

        public void AddBiomass(int amount)
        {
            if (amount <= 0) return;

            currentBiomass += amount;
            GameEvents.BiomassChanged?.Invoke(currentBiomass);

            Debug.Log($"Gained {amount} biomass. Total: {currentBiomass}");
        }

        public void SpendEnergy(int amount)
        {
            if (amount <= 0) return;

            currentEnergy = Mathf.Max(0, currentEnergy - amount);
            GameEvents.EnergyChanged?.Invoke(currentEnergy);

            if (currentEnergy == 0)
            {
                Debug.LogWarning("Energy depleted! Symbiosis connections may fail.");
            }
        }

        public void AddEnergy(int amount)
        {
            if (amount <= 0) return;

            currentEnergy += amount;
            GameEvents.EnergyChanged?.Invoke(currentEnergy);
        }

        // Debug методы
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void AddBiomassCheat(int amount)
        {
            AddBiomass(amount);
            Debug.Log($"[CHEAT] Added {amount} biomass");
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void AddEnergyCheat(int amount)
        {
            AddEnergy(amount);
            Debug.Log($"[CHEAT] Added {amount} energy");
        }
    }
}