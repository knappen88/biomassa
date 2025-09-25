using SymbiosisDefense.Core;
using TMPro;
using UnityEngine;

namespace SymbiosisDefense.UI
{
    public class GameplayHUD : MonoBehaviour
    {
        [Header("Debug Display")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private TextMeshProUGUI debugText;

        [Header("Performance Monitor")]
        [SerializeField] private bool showFPS = true;
        [SerializeField] private TextMeshProUGUI fpsText;

        private float frameCount;
        private float dt;
        private float fps;
        private float updateRate = 4.0f;

        void Start()
        {
            if (debugText == null && showDebugInfo)
            {
                CreateDebugText();
            }

            if (fpsText == null && showFPS)
            {
                CreateFPSText();
            }
        }

        void Update()
        {
            if (showFPS)
                UpdateFPS();

            if (showDebugInfo)
                UpdateDebugInfo();
        }

        private void UpdateFPS()
        {
            frameCount++;
            dt += Time.deltaTime;

            if (dt > 1.0 / updateRate)
            {
                fps = frameCount / dt;
                frameCount = 0;
                dt -= 1.0f / updateRate;
            }

            if (fpsText != null)
            {
                fpsText.text = $"FPS: {Mathf.Round(fps)}";

                // Меняем цвет в зависимости от производительности
                if (fps >= 60) fpsText.color = Color.green;
                else if (fps >= 30) fpsText.color = Color.yellow;
                else fpsText.color = Color.red;
            }
        }

        private void UpdateDebugInfo()
        {
            if (debugText == null) return;

            var towerManager = ServiceLocator.Get<ITowerManager>();
            var resourceManager = ServiceLocator.Get<IResourceManager>();

            string info = $"=== DEBUG INFO ===\n";

            if (towerManager != null)
            {
                var towers = towerManager.GetAllTowers();
                info += $"Active Towers: {towers?.Length ?? 0}\n";
            }

            if (resourceManager != null)
            {
                info += $"Biomass: {resourceManager.Biomass}\n";
                info += $"Energy: {resourceManager.Energy}\n";
            }

            info += $"Time Scale: {Time.timeScale}\n";
            info += $"Memory: {System.GC.GetTotalMemory(false) / 1024 / 1024} MB";

            debugText.text = info;
        }

        private void CreateDebugText()
        {
            GameObject debugObj = new GameObject("DebugText");
            debugObj.transform.SetParent(transform);

            debugText = debugObj.AddComponent<TextMeshProUGUI>();
            debugText.text = "Debug Info";
            debugText.fontSize = 12;
            debugText.color = Color.white;

            RectTransform rect = debugText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(10, -10);
        }

        private void CreateFPSText()
        {
            GameObject fpsObj = new GameObject("FPSText");
            fpsObj.transform.SetParent(transform);

            fpsText = fpsObj.AddComponent<TextMeshProUGUI>();
            fpsText.text = "FPS: --";
            fpsText.fontSize = 16;
            fpsText.color = Color.green;

            RectTransform rect = fpsText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-10, -10);
        }

        // Публичные методы для отладки
        public void ToggleDebugInfo()
        {
            showDebugInfo = !showDebugInfo;
            if (debugText != null)
                debugText.gameObject.SetActive(showDebugInfo);
        }

        public void ToggleFPS()
        {
            showFPS = !showFPS;
            if (fpsText != null)
                fpsText.gameObject.SetActive(showFPS);
        }
    }
}