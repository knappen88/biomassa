using UnityEngine;
using SymbiosisDefense.Core;

namespace SymbiosisDefense.Gameplay
{
    [CreateAssetMenu(fileName = "TowerStats", menuName = "Symbiosis Defense/Tower Stats")]
    public class TowerStats : ScriptableObject
    {
        [Header("Identity")]
        public TowerType towerType;
        public string displayName;
        [TextArea(2, 4)]
        public string description;

        [Header("Combat")]
        public float baseDamage = 10f;
        public float attackRange = 5f;
        public float fireRate = 1f;

        [Header("Survival")]
        public float baseHealth = 100f;
        public float armor = 0f;

        [Header("Economy")]
        public int buildCost = 50;
        public int upgradeCost = 25;
        public int maxLevel = 3;

        [Header("Symbiosis")]
        public SymbiosisType[] supportedSymbiosis;
        public float energyConsumption = 10f;

        void OnValidate()
        {
            baseDamage = Mathf.Max(0, baseDamage);
            attackRange = Mathf.Max(0.1f, attackRange);
            fireRate = Mathf.Max(0.1f, fireRate);
            baseHealth = Mathf.Max(1, baseHealth);
            buildCost = Mathf.Max(1, buildCost);
            upgradeCost = Mathf.Max(1, upgradeCost);
            maxLevel = Mathf.Max(1, maxLevel);
        }
    }
}