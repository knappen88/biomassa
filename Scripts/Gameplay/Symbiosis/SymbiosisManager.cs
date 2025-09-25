using SymbiosisDefense.Core;
using SymbiosisDefense.Gameplay;
using System.Collections.Generic;
using UnityEngine;

public class SymbiosisManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameObject connectionPrefab;

    [Header("Runtime")]
    [SerializeField] private List<SymbiosisConnection> activeConnections = new();

    public void CreateConnection(BaseTower source, BaseTower target, SymbiosisType type)
    {
        if (connectionPrefab == null)
        {
            Debug.LogError("Connection prefab not set!");
            return;
        }

        GameObject connectionObj = Instantiate(connectionPrefab);
        SymbiosisConnection connection = connectionObj.GetComponent<SymbiosisConnection>();

        if (connection != null)
        {
            connection.Initialize(source, target, type);
            activeConnections.Add(connection);
        }
    }

    public void RemoveConnection(SymbiosisConnection connection)
    {
        activeConnections.Remove(connection);
    }

    public SymbiosisConnection[] GetConnectionsForTower(BaseTower tower)
    {
        return activeConnections.FindAll(c =>
            c.SourceTower == tower || c.TargetTower == tower
        ).ToArray();
    }
}