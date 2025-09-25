using UnityEngine;
using SymbiosisDefense.Core;

namespace SymbiosisDefense.Gameplay
{
    public abstract class BaseEnemy : MonoBehaviour, IDamageable
    {
        [Header("Enemy Configuration")]
        [SerializeField] protected EnemyStats stats;
        [SerializeField] protected Transform[] waypoints;

        [Header("Runtime Status")]
        [SerializeField] protected float currentHealth;
        [SerializeField] protected int currentWaypointIndex = 0;
        [SerializeField] protected bool isAlive = true;

        // ������������ ����������
        protected Transform cachedTransform;
        protected Rigidbody cachedRigidbody;

        // �������
        public System.Action<BaseEnemy> OnEnemyKilled;
        public System.Action<BaseEnemy> OnEnemyReachedEnd;

        // �������� ���������� IDamageable
        public float CurrentHealth => currentHealth;
        public bool IsAlive => isAlive && currentHealth > 0;

        // �������������� ��������
        public EnemyType EnemyType => stats.enemyType;
        public Vector3 Position => cachedTransform.position;
        public float MoveSpeed => stats.moveSpeed;

        protected virtual void Awake()
        {
            cachedTransform = transform;
            cachedRigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            if (stats == null)
            {
                Debug.LogError($"EnemyStats not assigned on {gameObject.name}!");
                return;
            }

            currentHealth = stats.maxHealth;
            SetupWaypoints();

            Debug.Log($"Enemy {stats.enemyType} spawned with {currentHealth} HP");
        }

        protected virtual void SetupWaypoints()
        {
            // ������� ���� �� ������ ����� ��� Milestone 1
            if (waypoints == null || waypoints.Length == 0)
            {
                CreateDefaultWaypoints();
            }
        }

        private void CreateDefaultWaypoints()
        {
            // ������� ������� ������ ����
            var gridManager = ServiceLocator.Get<IGridManager>();
            if (gridManager != null)
            {
                waypoints = new Transform[3];

                // ��������� ������� (�����)
                GameObject startWaypoint = new GameObject("StartWaypoint");
                startWaypoint.transform.position = new Vector3(-5, 0, 0);
                waypoints[0] = startWaypoint.transform;

                // ������� �������
                GameObject middleWaypoint = new GameObject("MiddleWaypoint");
                middleWaypoint.transform.position = new Vector3(0, 0, 0);
                waypoints[1] = middleWaypoint.transform;

                // �������� ������� (������)
                GameObject endWaypoint = new GameObject("EndWaypoint");
                endWaypoint.transform.position = new Vector3(15, 0, 0);
                waypoints[2] = endWaypoint.transform;
            }
        }

        public virtual void TakeDamage(float damage)
        {
            if (!IsAlive) return;

            currentHealth = Mathf.Max(0, currentHealth - damage);

            OnDamageTaken(damage);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void OnDamageTaken(float damage)
        {
            // ������ ��������� �����
            Debug.Log($"{stats.enemyType} took {damage} damage, HP: {currentHealth}/{stats.maxHealth}");
        }

        protected virtual void Die()
        {
            isAlive = false;
            OnEnemyKilled?.Invoke(this);

            // ���� ������� ������
            var resourceManager = ServiceLocator.Get<IResourceManager>();
            resourceManager?.AddBiomass(stats.biomassReward);

            PlayDeathEffect();
            Destroy(gameObject, 0.5f);
        }

        protected virtual void PlayDeathEffect()
        {
            // TODO: Death VFX
            Debug.Log($"{stats.enemyType} died!");
        }

        protected abstract void UpdateMovement();

        protected virtual void Update()
        {
            if (IsAlive)
            {
                UpdateMovement();
                CheckReachedEnd();
            }
        }

        protected virtual void CheckReachedEnd()
        {
            if (waypoints != null && currentWaypointIndex >= waypoints.Length)
            {
                OnEnemyReachedEnd?.Invoke(this);
                Destroy(gameObject);
            }
        }
    }

    // ���������� ��� �����
    public class BasicEnemy : BaseEnemy
    {
        protected override void UpdateMovement()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            if (currentWaypointIndex < waypoints.Length)
            {
                Vector3 targetPosition = waypoints[currentWaypointIndex].position;
                Vector3 direction = (targetPosition - cachedTransform.position).normalized;

                // ������� �������� � waypoint'�
                cachedTransform.position += direction * stats.moveSpeed * Time.deltaTime;

                // ������������ � ����
                if (direction != Vector3.zero)
                    cachedTransform.rotation = Quaternion.LookRotation(direction);

                // ��������� ���������� waypoint'�
                if (Vector3.Distance(cachedTransform.position, targetPosition) < 0.5f)
                {
                    currentWaypointIndex++;
                }
            }
        }
    }
}