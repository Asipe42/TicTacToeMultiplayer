using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public EventHandler OnGameStarted;
    public EventHandler OnCurrentPlayablePlayerTypeChanged;

    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    private PlayerType localPlayerType;
    private PlayerType currentPlayablePlayerType;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"More than one instance of GameManager!");
        }
        Instance = this;
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
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            currentPlayablePlayerType = PlayerType.Cross;
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
        if (playerType != currentPlayablePlayerType)
        {
            return;
        }
        
        OnClickedOnGridPosition.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x, 
            y = y,
            playerType = playerType
        });

        switch (currentPlayablePlayerType)
        {
            case PlayerType.Cross:
                currentPlayablePlayerType = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType = PlayerType.Cross;
                break;
        }

        TriggerOnCurrentPlayablePlayerTypeChangedRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnCurrentPlayablePlayerTypeChangedRpc()
    {
        OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType;
    }
}
