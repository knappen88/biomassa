using UnityEngine;
using SymbiosisDefense.Core;

namespace SymbiosisDefense.Gameplay
{
    public abstract class BaseTower : MonoBehaviour, IDamageable
    {
        [Header("Base Tower Configuration")]
        [SerializeField] private TowerStats stats;
        [SerializeField] protected Transform firePoint;

        [Header("Runtime Status")]
        [SerializeField] protected bool isActive = true;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected int currentLevel = 1;

        // ������������ ����������
        protected Transform cachedTransform;
        protected Collider cachedCollider;

        // �������
        public System.Action<BaseTower> OnTowerDestroyed;
        public System.Action<BaseTower, float> OnTowerDamaged;
        public System.Action<BaseTower, int> OnTowerUpgraded;

        // �������� ���������� IDamageable
        public float CurrentHealth => currentHealth;
        public bool IsAlive => isActive && currentHealth > 0;

        // �������� �����
        public TowerType TowerType => Stats != null ? Stats.towerType : TowerType.Root;
        public bool IsActive => isActive && currentHealth > 0;
        public Vector3 Position => cachedTransform.position;
        public float MaxHealth => Stats != null ? Stats.baseHealth : 100f;
        public TowerStats Stats => stats;  // ��������� ������ � stats

        protected virtual void Awake()
        {
            CacheComponents();
        }

        protected virtual void Start()
        {
            Initialize();
        }

        private void CacheComponents()
        {
            cachedTransform = transform;
            cachedCollider = GetComponent<Collider>();
        }

        protected virtual void Initialize()
        {
            if (Stats == null)
            {
                Debug.LogError($"TowerStats not assigned on {gameObject.name}!");
                return;
            }

            currentHealth = Stats.baseHealth;

            if (firePoint == null)
                firePoint = cachedTransform;

            Debug.Log($"Tower {Stats.towerType} initialized at {Position}");
        }

        public virtual void TakeDamage(float damage)
        {
            if (!IsActive) return;

            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnTowerDamaged?.Invoke(this, damage);

            if (currentHealth <= 0)
            {
                DestroyTower();
            }
        }

        public virtual bool CanUpgrade()
        {
            var resourceManager = ServiceLocator.Get<IResourceManager>();
            return resourceManager != null &&
                   resourceManager.CanAfford(Stats.upgradeCost) &&
                   currentLevel < Stats.maxLevel;
        }

        public virtual void Upgrade()
        {
            if (!CanUpgrade()) return;

            var resourceManager = ServiceLocator.Get<IResourceManager>();
            resourceManager.SpendBiomass(Stats.upgradeCost);

            currentLevel++;
            ApplyUpgradeBonus();
            OnTowerUpgraded?.Invoke(this, currentLevel);

            Debug.Log($"Tower upgraded to level {currentLevel}");
        }

        protected virtual void ApplyUpgradeBonus()
        {
            // ������� ����� +25% � �������� �� �������
            float healthBonus = Stats.baseHealth * 0.25f * (currentLevel - 1);
            currentHealth += healthBonus;
        }

        protected virtual void DestroyTower()
        {
            isActive = false;
            OnTowerDestroyed?.Invoke(this);

            // ����������� ������ �����
            var gridManager = ServiceLocator.Get<IGridManager>();
            gridManager?.FreeCell(Position);

            // �������� ����������
            PlayDestroyEffect();

            Destroy(gameObject, 1f);
        }

        protected virtual void PlayDestroyEffect()
        {
            // TODO: �������� VFX ����������
            Debug.Log($"Tower {TowerType} destroyed at {Position}");
        }

        // ����������� ������ ��� �����������
        protected abstract void UpdateTower();

        protected virtual void Update()
        {
            if (IsActive)
                UpdateTower();
        }

        // Gizmos ��� �������
        protected virtual void OnDrawGizmosSelected()
        {
            if (Stats != null)
            {
                // ���������� ������ �����
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, Stats.attackRange);
            }
        }
    }

    // ���������� ���� ����� ��� Milestone 1
    public class RootTower : BaseTower
    {
        [Header("Root Tower Specific")]
        [SerializeField] private float nutritionRadius = 3f;
        [SerializeField] private float damageBonus = 0.15f;

        protected override void UpdateTower()
        {
            // ���� ������� ������ - ����� ������� ������� ��������
            ProvideNutrition();
        }

        private void ProvideNutrition()
        {
            // TODO: Implement symbiosis system
            // ����� ����� � ������� � ���� �� �����
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // ���������� ������ �������
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, nutritionRadius);
        }
    }

    public class CombatTower : BaseTower
    {
        [Header("Combat Tower Specific")]
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private GameObject projectilePrefab;

        private float lastAttackTime;
        private Transform currentTarget;

        protected override void UpdateTower()
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                FindAndAttackTarget();
            }
        }

        private void FindAndAttackTarget()
        {
            // ���������� ����� ����
            Collider[] enemies = Physics.OverlapSphere(
                Position,
                Stats.attackRange,
                LayerMask.GetMask("Enemy")
            );

            if (enemies.Length > 0)
            {
                currentTarget = enemies[0].transform;
                Attack();
            }
        }

        private void Attack()
        {
            if (currentTarget == null) return;

            lastAttackTime = Time.time;

            // ������� ����� - ���������� ����
            if (currentTarget.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(Stats.baseDamage);
                PlayAttackEffect();
            }

            Debug.Log($"Combat Tower attacked {currentTarget.name}");
        }

        private void PlayAttackEffect()
        {
            // TODO: Spawn projectile/laser effect
            if (projectilePrefab != null)
            {
                GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                // TODO: Make projectile move to target
            }
        }
    }
}