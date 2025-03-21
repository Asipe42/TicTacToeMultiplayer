using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;
    
    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform lineCompletePrefab;

    private List<GameObject> visualGameObjectList = new List<GameObject>();
    
    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
    }

    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinArgs e)
    {
        if (NetworkManager.Singleton.IsServer == false)
        {
            return;
        }
        
        float eulerZ;
        switch (e.line.orientation)
        {
            case GameManager.Orientation.Horizontal:
                eulerZ = 0f;
                break;
            case GameManager.Orientation.Vertical:
                eulerZ = 90f;
                break;
            case GameManager.Orientation.DialogA:
                eulerZ = 45f;
                break;
            case GameManager.Orientation.DialogB:
                eulerZ = -45f;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        Transform lineCompleteTransform = Instantiate
        (
            original: lineCompletePrefab, 
            position:GetGridWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y),
            rotation: Quaternion.Euler(0, 0, eulerZ)
        );
        lineCompleteTransform.GetComponent<NetworkObject>().Spawn(true);
        visualGameObjectList.Add(lineCompleteTransform.gameObject);
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        if (NetworkManager.Singleton.IsServer == false)
        {
            return;
        }
        
        foreach (var each in visualGameObjectList)
        {
            Destroy(each);
        }
        
        visualGameObjectList.Clear();
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Transform prefab;
        switch (playerType)
        {
            case GameManager.PlayerType.Cross:
            {
                prefab = crossPrefab;
                break;
            }
            
            case GameManager.PlayerType.Circle:
            default:
            {
                prefab = circlePrefab;
                break;
            }
        }
        
        Transform spawnedCrossTransform =  Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);
        visualGameObjectList.Add(spawnedCrossTransform.gameObject);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2
        (
            x: (x * GRID_SIZE) - GRID_SIZE,
            y: (y * GRID_SIZE) - GRID_SIZE
        );
    }
}
