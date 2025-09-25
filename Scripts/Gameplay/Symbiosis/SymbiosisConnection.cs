using UnityEngine;
using SymbiosisDefense.Core;

namespace SymbiosisDefense.Gameplay
{
    public class SymbiosisConnection : MonoBehaviour
    {
        [Header("Connection")]
        [SerializeField] private BaseTower sourceTower;
        [SerializeField] private BaseTower targetTower;
        [SerializeField] private SymbiosisType connectionType;

        [Header("Visual")]
        [SerializeField] private LineRenderer connectionLine;
        [SerializeField] private Color connectionColor = Color.green;
        [SerializeField] private float energyFlowSpeed = 5f;

        [Header("Stats")]
        [SerializeField] private float energyCost = 10f;
        [SerializeField] private bool isActive = false;

        // Свойства
        public BaseTower SourceTower => sourceTower;
        public BaseTower TargetTower => targetTower;
        public SymbiosisType ConnectionType => connectionType;
        public bool IsActive => isActive && CanMaintainConnection();

        void Awake()
        {
            SetupVisual();
        }

        public void Initialize(BaseTower source, BaseTower target, SymbiosisType type)
        {
            sourceTower = source;
            targetTower = target;
            connectionType = type;

            if (CanCreateConnection())
            {
                CreateConnection();
            }
            else
            {
                Debug.LogWarning($"Cannot create {type} connection between towers");
                Destroy(gameObject);
            }
        }

        private bool CanCreateConnection()
        {
            if (sourceTower == null || targetTower == null) return false;

            // Проверяем расстояние
            float distance = Vector3.Distance(sourceTower.Position, targetTower.Position);
            if (distance > GetMaxConnectionDistance()) return false;

            // Проверяем энергию
            var resourceManager = ServiceLocator.Get<IResourceManager>();
            if (resourceManager == null || resourceManager.Energy < energyCost) return false;

            return true;
        }

        private void CreateConnection()
        {
            isActive = true;
            UpdateVisual();
            ApplySymbiosisEffect();

            Debug.Log($"Created {connectionType} connection: {sourceTower.TowerType} -> {targetTower.TowerType}");
        }

        private bool CanMaintainConnection()
        {
            if (sourceTower == null || targetTower == null) return false;
            if (!sourceTower.IsActive || !targetTower.IsActive) return false;

            var resourceManager = ServiceLocator.Get<IResourceManager>();
            return resourceManager != null && resourceManager.Energy >= energyCost;
        }

        private void ApplySymbiosisEffect()
        {
            // Простые эффекты для Milestone 1
            switch (connectionType)
            {
                case SymbiosisType.Nutritional:
                    ApplyNutritionalBonus();
                    break;
                case SymbiosisType.Protective:
                    ApplyProtectiveBonus();
                    break;
                case SymbiosisType.Amplifying:
                    ApplyAmplifyingBonus();
                    break;
                case SymbiosisType.Healing:
                    ApplyHealingBonus();
                    break;
            }
        }

        private void ApplyNutritionalBonus()
        {
            // TODO: Implement damage bonus
            Debug.Log($"Applied nutritional bonus to {targetTower.TowerType}");
        }

        private void ApplyProtectiveBonus()
        {
            // TODO: Implement shield bonus
            Debug.Log($"Applied protective bonus to {targetTower.TowerType}");
        }

        private void ApplyAmplifyingBonus()
        {
            // TODO: Implement attack speed bonus
            Debug.Log($"Applied amplifying bonus to {targetTower.TowerType}");
        }

        private void ApplyHealingBonus()
        {
            // TODO: Implement healing bonus
            Debug.Log($"Applied healing bonus to {targetTower.TowerType}");
        }

        private void SetupVisual()
        {
            if (connectionLine == null)
            {
                connectionLine = gameObject.AddComponent<LineRenderer>();
            }

            // Создаем материал для линии
            Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
            lineMaterial.color = connectionColor;

            connectionLine.material = lineMaterial;
            connectionLine.startWidth = 0.1f;
            connectionLine.endWidth = 0.1f;
            connectionLine.positionCount = 2;
            connectionLine.useWorldSpace = true;
        }

        private void UpdateVisual()
        {
            if (connectionLine != null && sourceTower != null && targetTower != null)
            {
                connectionLine.SetPosition(0, sourceTower.Position + Vector3.up * 0.5f);
                connectionLine.SetPosition(1, targetTower.Position + Vector3.up * 0.5f);
                connectionLine.enabled = isActive;
            }
        }

        private float GetMaxConnectionDistance()
        {
            // Базовое расстояние соединения
            return 5f;
        }

        void Update()
        {
            if (IsActive)
            {
                UpdateVisual();
                ConsumeEnergy();
            }
            else
            {
                DestroyConnection();
            }
        }

        private void ConsumeEnergy()
        {
            var resourceManager = ServiceLocator.Get<IResourceManager>();
            if (resourceManager != null)
            {
                resourceManager.SpendEnergy(Mathf.RoundToInt(energyCost * Time.deltaTime));
            }
        }

        private void DestroyConnection()
        {
            isActive = false;
            if (connectionLine != null)
                connectionLine.enabled = false;

            Debug.Log($"Symbiosis connection destroyed");
            Destroy(gameObject);
        }
    }
}