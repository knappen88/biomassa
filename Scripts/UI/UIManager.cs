using SymbiosisDefense.Core;
using SymbiosisDefense.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SymbiosisDefense.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Resource Display")]
        [SerializeField] private TextMeshProUGUI biomassText;
        [SerializeField] private TextMeshProUGUI energyText;

        [Header("Tower Building")]
        [SerializeField] private TowerBuildButton[] towerButtons;
        [SerializeField] private Button startWaveButton;

        [Header("Game State")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private GameObject gameOverPanel;

        private ITowerManager towerManager;
        private IResourceManager resourceManager;
        private GameManager gameManager;
        private TowerType selectedTowerType;
        private bool isBuildModeActive = false;

        void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeUI()
        {
            // �������� ������ �� ���������
            resourceManager = ServiceLocator.Get<IResourceManager>();
            towerManager = ServiceLocator.Get<ITowerManager>();
            gameManager = FindObjectOfType<GameManager>();

            // ����������� ������ �����
            SetupTowerButtons();

            // ����������� ������ �����
            if (startWaveButton != null)
                startWaveButton.onClick.AddListener(StartWave);

            // �������� ������ ���������
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            Debug.Log("UI initialized");
        }

        private void SetupTowerButtons()
        {
            if (towerButtons == null) return;

            for (int i = 0; i < towerButtons.Length; i++)
            {
                var button = towerButtons[i];
                if (button != null)
                {
                    var towerType = button.towerType;
                    button.button.onClick.AddListener(() => SelectTowerType(towerType));
                }
            }
        }

        private void SubscribeToEvents()
        {
            GameEvents.BiomassChanged += UpdateBiomassDisplay;
            GameEvents.EnergyChanged += UpdateEnergyDisplay;
            GameEvents.WaveStarted += UpdateWaveDisplay;
            GameEvents.TowerBuilt += OnTowerBuilt;  // ������ ���������� ���������
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.BiomassChanged -= UpdateBiomassDisplay;
            GameEvents.EnergyChanged -= UpdateEnergyDisplay;
            GameEvents.WaveStarted -= UpdateWaveDisplay;
            GameEvents.TowerBuilt -= OnTowerBuilt;  // ������ ���������� ���������
        }

        // ������������ ����� - ��������� GameObject ������ Tower
        private void OnTowerBuilt(GameObject towerObj)
        {
            // ����� �������� �������� ��� �������
            var baseTower = towerObj.GetComponent<BaseTower>();
            if (baseTower != null)
            {
                Debug.Log($"UI: Tower {baseTower.TowerType} built");
            }
        }

        void Update()
        {
            HandleInput();
            UpdateTowerButtons();
        }

        private void HandleInput()
        {
            if (isBuildModeActive)
            {
                HandleTowerPlacement();
            }

            // ESC ��� ������ �������������
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelBuildMode();
            }
        }

        private void HandleTowerPlacement()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
                {
                    TryBuildTower(hit.point);
                }
            }

            // ������ ���� ��� ������
            if (Input.GetMouseButtonDown(1))
            {
                CancelBuildMode();
            }
        }

        private void TryBuildTower(Vector3 position)
        {
            if (towerManager != null && towerManager.CanBuildTower(position, selectedTowerType))
            {
                var tower = towerManager.BuildTower(position, selectedTowerType);
                if (tower != null)
                {
                    Debug.Log($"Built {selectedTowerType} tower at {position}");
                    // �������� � ������ ������������� ��� ��������
                }
            }
            else
            {
                Debug.Log("Cannot build tower at this position");
            }
        }

        private void SelectTowerType(TowerType towerType)
        {
            selectedTowerType = towerType;
            isBuildModeActive = true;

            Debug.Log($"Selected tower type: {towerType}");
            UpdateTowerButtons();
        }

        private void CancelBuildMode()
        {
            isBuildModeActive = false;
            selectedTowerType = TowerType.Root; // default
            UpdateTowerButtons();
        }

        private void StartWave()
        {
            if (gameManager != null)
            {
                gameManager.StartWave();
            }
        }

        // UI Update ������
        private void UpdateBiomassDisplay(int biomass)
        {
            if (biomassText != null)
                biomassText.text = $"Biomass: {biomass}";
        }

        private void UpdateEnergyDisplay(int energy)
        {
            if (energyText != null)
                energyText.text = $"Energy: {energy}";
        }

        private void UpdateWaveDisplay(int wave)
        {
            if (waveText != null)
                waveText.text = $"Wave: {wave}";
        }

        private void UpdateTowerButtons()
        {
            if (towerButtons == null || resourceManager == null) return;

            foreach (var button in towerButtons)
            {
                if (button?.button == null) continue;

                // ��������� ����������� ������
                bool canAfford = resourceManager.CanAfford(button.cost);
                bool isSelected = isBuildModeActive && button.towerType == selectedTowerType;

                button.button.interactable = canAfford;

                // ��������� �������� ��������� ������
                var colors = button.button.colors;
                colors.normalColor = isSelected ? Color.yellow : Color.white;
                button.button.colors = colors;

                // ��������� ����� � �����
                if (button.costText != null)
                {
                    button.costText.text = $"{button.cost}";
                    button.costText.color = canAfford ? Color.white : Color.red;
                }
            }
        }

        private void OnTowerBuilt(Tower tower)
        {
            // ����� �������� �������� ��� �������
            Debug.Log($"UI: Tower {tower.Type} built");
        }

        // ��������� ������ ��� �������� ������
        public void ShowGameOverPanel()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }

    // ��������������� ����� ��� ������ �����
    [System.Serializable]
    public class TowerBuildButton
    {
        public TowerType towerType;
        public Button button;
        public TextMeshProUGUI costText;
        public int cost = 50;
        public string description = "Basic tower";
    }
}
