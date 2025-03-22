using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public partial class GameManager
{
    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DialogA,
        DialogB
    }
    
    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }
}

public partial class GameManager
{
    public EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public EventHandler OnGameStarted;
    public EventHandler OnCurrentPlayablePlayerTypeChanged;
    public EventHandler<OnGameWinArgs> OnGameWin;

    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }
    
    public class OnGameWinArgs : EventArgs
    {
        public Line line;
        public PlayerType winPlayerType;
    }
}

public partial class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new();
    
    private PlayerType[,] playerTypeArray;
    private List<Line> lineList;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"More than one instance of GameManager!");
        }
        Instance = this;

        playerTypeArray = new PlayerType[3, 3];
        
        lineList = new List<Line>()
        {
            // Horizontal
            new Line()
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
                centerGridPosition = new Vector2Int(1, 0),
                orientation = Orientation.Horizontal
            },
            
            new Line()
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Horizontal
            },
            
            new Line()
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2) },
                centerGridPosition = new Vector2Int(1, 2),
                orientation = Orientation.Horizontal
            },
            
            // Vertical
            new Line()
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) },
                centerGridPosition = new Vector2Int(0, 1),
                orientation = Orientation.Vertical
            },

            new Line()
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Vertical
            },
            
            new Line()
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2) },
                centerGridPosition = new Vector2Int(2, 1),
                orientation = Orientation.Vertical
            },
            
            // Dialogs
            new Line()
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2) },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DialogA
            },

            new Line()
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0) },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DialogB
            },
        };
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"{nameof(OnNetworkSpawn)}: {NetworkManager.Singleton.LocalClientId}");
        
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }
        
        currentPlayablePlayerType.OnValueChanged += (oldValue, newValue) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if (playerType != currentPlayablePlayerType.Value)
        {
            return;
        }

        if (playerTypeArray[x, y] != PlayerType.None)
        {
            return;
        }
        
        playerTypeArray[x, y] = playerType;
        
        OnClickedOnGridPosition.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x, 
            y = y,
            playerType = playerType
        });

        switch (currentPlayablePlayerType.Value)
        {
            case PlayerType.Cross:
                currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }

        TestWinner();
    }

    private void TestWinner()
    {
        for (int i = 0; i < lineList.Count; i++)
        {
            if (TestWinnerLine(lineList[i]))
            {
                currentPlayablePlayerType.Value = PlayerType.None;
                TriggerOnGameWinRpc(i, playerTypeArray[lineList[i].centerGridPosition.x, lineList[i].centerGridPosition.y]);
                break;
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType)
    {
        Debug.Log($"{nameof(TriggerOnGameWinRpc)}");
        
        Line line = lineList[lineIndex];
        OnGameWin?.Invoke(this, new OnGameWinArgs()
        {
            line = line,
            winPlayerType = winPlayerType
        });
    }
    
    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine
        (
            a: playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            b: playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            c: playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
        );
    }

    private bool TestWinnerLine(PlayerType a, PlayerType b, PlayerType c)
    {
        return a != PlayerType.None && a == b && b == c;
    }

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }
}
