using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : MonoBehaviour
{
    private const float GRID_SIZE = 3.1f;
    
    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition = GameManager_OnClickedOnGridPosition;
    }

    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        Transform spawnedCrossTransform =  Instantiate(crossPrefab);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);
        spawnedCrossTransform.transform.position = GetGridWorldPosition(e.x, e.y);
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
