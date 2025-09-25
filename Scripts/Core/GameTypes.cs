using UnityEngine;
using System;

namespace SymbiosisDefense.Core
{
    // ������������
    public enum TowerType
    {
        Root,
        Shield,
        Catalyst,
        Regenerator
    }

    public enum EnemyType
    {
        Basic,
        Fast,
        Heavy,
        Flying
    }

    public enum SymbiosisType
    {
        Nutritional,    // +����
        Protective,     // +������
        Amplifying,     // +�������� �����
        Healing         // +�����������
    }

    // ������� ���� ������
    [System.Serializable]
    public class GridCell
    {
        public Vector3 worldPosition;
        public Vector2Int gridPosition;
        public bool isOccupied;
        public GameObject occupant;
        public bool isBuildable = true;

        public GridCell(Vector3 worldPos, Vector2Int gridPos)
        {
            worldPosition = worldPos;
            gridPosition = gridPos;
            isOccupied = false;
            occupant = null;
        }
    }
}