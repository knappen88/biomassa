using UnityEngine;

namespace SymbiosisDefense.Gameplay
{
    [RequireComponent(typeof(GridManager))]
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Runtime Visualization")]
        [SerializeField] private bool showValidPlacement = true;
        [SerializeField] private GameObject placementIndicatorPrefab;
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;

        private GridManager gridManager;
        private GameObject currentIndicator;
        private Camera playerCamera;

        void Awake()
        {
            gridManager = GetComponent<GridManager>();
            playerCamera = Camera.main;
        }

        void Update()
        {
            if (showValidPlacement)
                UpdatePlacementIndicator();
        }

        private void UpdatePlacementIndicator()
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = playerCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                Vector3 snappedPosition = gridManager.SnapToGrid(hit.point);
                ShowPlacementIndicator(snappedPosition);
            }
            else
            {
                HidePlacementIndicator();
            }
        }

        private void ShowPlacementIndicator(Vector3 position)
        {
            if (currentIndicator == null && placementIndicatorPrefab != null)
            {
                currentIndicator = Instantiate(placementIndicatorPrefab);
            }

            if (currentIndicator != null)
            {
                currentIndicator.transform.position = position;
                currentIndicator.SetActive(true);

                // Меняем цвет в зависимости от возможности размещения
                bool canPlace = gridManager.IsValidPosition(position) &&
                               !gridManager.IsOccupied(position);

                Renderer renderer = currentIndicator.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = canPlace ? validMaterial : invalidMaterial;
                }
            }
        }

        private void HidePlacementIndicator()
        {
            if (currentIndicator != null)
                currentIndicator.SetActive(false);
        }
    }
}