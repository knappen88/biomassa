using UnityEngine;
using SymbiosisDefense.Core;
using System.Collections.Generic;

namespace SymbiosisDefense.Gameplay
{
    public class GridManager : MonoBehaviour, IGridManager
    {
        [Header("Grid Configuration")]
        [SerializeField] private int gridWidth = 20;
        [SerializeField] private int gridHeight = 15;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;

        [Header("Visual")]
        [SerializeField] private bool showGridGizmos = true;
        [SerializeField] private Color gridColor = Color.white;
        [SerializeField] private Color occupiedColor = Color.red;
        [SerializeField] private Color unbuildableColor = Color.gray;

        private GridCell[,] grid;
        private Dictionary<Vector3, GridCell> worldToGridMap = new();

        public void Initialize()
        {
            CreateGrid();
            Debug.Log($"Grid initialized: {gridWidth}x{gridHeight} cells, size: {cellSize}");
        }

        private void CreateGrid()
        {
            grid = new GridCell[gridWidth, gridHeight];
            worldToGridMap.Clear();

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 worldPosition = GridToWorldPosition(x, y);
                    Vector2Int gridPosition = new Vector2Int(x, y);

                    GridCell cell = new GridCell(worldPosition, gridPosition);

                    // Определяем buildable области (исключаем путь врагов)
                    cell.isBuildable = DetermineIfBuildable(x, y);

                    grid[x, y] = cell;
                    worldToGridMap[worldPosition] = cell;
                }
            }
        }

        private bool DetermineIfBuildable(int x, int y)
        {
            // Простая логика: средняя линия - путь врагов (не строим)
            // В полной версии будет более сложная система путей
            if (y >= gridHeight / 2 - 1 && y <= gridHeight / 2 + 1)
                return false;

            return true;
        }

        public bool IsValidPosition(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldToGridPosition(worldPosition);
            return IsValidGridPosition(gridPos.x, gridPos.y);
        }

        public Vector3 SnapToGrid(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldToGridPosition(worldPosition);

            // Зажимаем в границы сетки
            gridPos.x = Mathf.Clamp(gridPos.x, 0, gridWidth - 1);
            gridPos.y = Mathf.Clamp(gridPos.y, 0, gridHeight - 1);

            return GridToWorldPosition(gridPos.x, gridPos.y);
        }

        public GridCell GetCell(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldToGridPosition(worldPosition);

            if (IsValidGridPosition(gridPos.x, gridPos.y))
                return grid[gridPos.x, gridPos.y];

            return null;
        }

        public bool IsOccupied(Vector3 worldPosition)
        {
            GridCell cell = GetCell(worldPosition);
            return cell?.isOccupied ?? true;
        }

        public void OccupyCell(Vector3 worldPosition, GameObject occupant)
        {
            GridCell cell = GetCell(worldPosition);
            if (cell != null && !cell.isOccupied)
            {
                cell.isOccupied = true;
                cell.occupant = occupant;
            }
        }

        public void FreeCell(Vector3 worldPosition)
        {
            GridCell cell = GetCell(worldPosition);
            if (cell != null)
            {
                cell.isOccupied = false;
                cell.occupant = null;
            }
        }

        // Utility методы
        private Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            Vector3 localPosition = worldPosition - gridOrigin;

            int x = Mathf.RoundToInt(localPosition.x / cellSize);
            int y = Mathf.RoundToInt(localPosition.z / cellSize);

            return new Vector2Int(x, y);
        }

        private Vector3 GridToWorldPosition(int x, int y)
        {
            return gridOrigin + new Vector3(x * cellSize, 0, y * cellSize);
        }

        private bool IsValidGridPosition(int x, int y)
        {
            return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
        }

        // Debug визуализация
        void OnDrawGizmos()
        {
            if (!showGridGizmos) return;

            if (grid != null)
            {
                // Рисуем заполненную сетку
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        GridCell cell = grid[x, y];
                        Vector3 center = cell.worldPosition;

                        if (cell.isOccupied)
                        {
                            Gizmos.color = occupiedColor;
                            Gizmos.DrawCube(center, Vector3.one * cellSize * 0.9f);
                        }
                        else if (!cell.isBuildable)
                        {
                            Gizmos.color = unbuildableColor;
                            Gizmos.DrawWireCube(center, Vector3.one * cellSize);
                        }
                        else
                        {
                            Gizmos.color = gridColor;
                            Gizmos.DrawWireCube(center, Vector3.one * cellSize);
                        }
                    }
                }
            }
            else
            {
                // Рисуем пустую сетку для настройки в редакторе
                DrawEmptyGrid();
            }
        }

        private void DrawEmptyGrid()
        {
            Gizmos.color = gridColor;

            for (int x = 0; x <= gridWidth; x++)
            {
                Vector3 start = gridOrigin + new Vector3(x * cellSize, 0, 0);
                Vector3 end = start + new Vector3(0, 0, gridHeight * cellSize);
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= gridHeight; y++)
            {
                Vector3 start = gridOrigin + new Vector3(0, 0, y * cellSize);
                Vector3 end = start + new Vector3(gridWidth * cellSize, 0, 0);
                Gizmos.DrawLine(start, end);
            }
        }

        // Публичные геттеры для отладки
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float CellSize => cellSize;
        public Vector3 GridOrigin => gridOrigin;
    }
}