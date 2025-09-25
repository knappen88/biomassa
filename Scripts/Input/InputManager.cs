using UnityEngine;
using SymbiosisDefense.Core;

namespace SymbiosisDefense.Gameplay
{
    public class InputManager : MonoBehaviour
    {
        [Header("Camera Controls")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float panSpeed = 5f;
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 15f;

        [Header("Selection")]
        [SerializeField] private LayerMask selectableLayer = -1;

        private Vector3 lastMousePosition;
        private BaseTower selectedTower;

        void Start()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        void Update()
        {
            HandleCameraControls();
            HandleSelection();
        }

        private void HandleCameraControls()
        {
            // Панорамирование камеры средней кнопкой мыши
            if (Input.GetMouseButtonDown(2))
            {
                lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(2))
            {
                Vector3 delta = lastMousePosition - Input.mousePosition;
                Vector3 move = new Vector3(delta.x, 0, delta.y) * panSpeed * Time.deltaTime;
                playerCamera.transform.Translate(move, Space.World);
                lastMousePosition = Input.mousePosition;
            }

            // Зум колесом мыши
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                float currentSize = playerCamera.orthographicSize;
                currentSize -= scroll * zoomSpeed;
                currentSize = Mathf.Clamp(currentSize, minZoom, maxZoom);
                playerCamera.orthographicSize = currentSize;
            }

            // Управление WASD
            Vector3 moveInput = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) moveInput += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) moveInput += Vector3.back;
            if (Input.GetKey(KeyCode.A)) moveInput += Vector3.left;
            if (Input.GetKey(KeyCode.D)) moveInput += Vector3.right;

            if (moveInput != Vector3.zero)
            {
                playerCamera.transform.Translate(moveInput * panSpeed * Time.deltaTime, Space.World);
            }
        }

        private void HandleSelection()
        {
            // Выбор башен левым кликом (если не в режиме строительства)
            if (Input.GetMouseButtonDown(0) && !IsUIElementClicked())
            {
                TrySelectTower();
            }
        }

        private bool IsUIElementClicked()
        {
            // Проверяем, кликнул ли игрок по UI элементу
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }

        private void TrySelectTower()
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayer))
            {
                BaseTower tower = hit.collider.GetComponent<BaseTower>();
                if (tower != null)
                {
                    SelectTower(tower);
                }
                else
                {
                    DeselectTower();
                }
            }
            else
            {
                DeselectTower();
            }
        }

        private void SelectTower(BaseTower tower)
        {
            if (selectedTower != null)
                DeselectTower();

            selectedTower = tower;
            Debug.Log($"Selected tower: {tower.TowerType} at {tower.Position}");

            // Визуальное выделение
            ShowSelectionEffect(tower);
        }

        private void DeselectTower()
        {
            if (selectedTower != null)
            {
                HideSelectionEffect(selectedTower);
                selectedTower = null;
            }
        }

        private void ShowSelectionEffect(BaseTower tower)
        {
            // Простой эффект выделения
            GameObject selectionRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            selectionRing.name = "SelectionRing";
            selectionRing.transform.SetParent(tower.transform);
            selectionRing.transform.localPosition = Vector3.zero;
            selectionRing.transform.localScale = new Vector3(2f, 0.1f, 2f);

            // Убираем коллайдер
            Destroy(selectionRing.GetComponent<Collider>());

            // Делаем желтым и прозрачным
            Renderer renderer = selectionRing.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 1f, 0f, 0.5f);
            mat.SetFloat("_Mode", 3); // Transparent mode
            renderer.material = mat;
        }

        private void HideSelectionEffect(BaseTower tower)
        {
            Transform selectionRing = tower.transform.Find("SelectionRing");
            if (selectionRing != null)
                Destroy(selectionRing.gameObject);
        }

        // Публичные методы
        public BaseTower GetSelectedTower()
        {
            return selectedTower;
        }

        public void ClearSelection()
        {
            DeselectTower();
        }
    }
}