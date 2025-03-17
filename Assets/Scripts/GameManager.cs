using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;

    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
    }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"More than one instance of GameManager!");
        }
        Instance = this;
    }

    public void ClickedOnGridPosition(int x, int y)
    {
        OnClickedOnGridPosition.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x, y = y
        });
    }
}
