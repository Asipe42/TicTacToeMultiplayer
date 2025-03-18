using System;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject circleYouText;
    [SerializeField] private GameObject crossYouText;
    [SerializeField] private GameObject circleArrow;
    [SerializeField] private GameObject crossArrow;

    private void Awake()
    {
        circleYouText.SetActive(false);
        crossYouText.SetActive(false);
        circleArrow.SetActive(false);
        crossArrow.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted = GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged = GameManager_OnCurrentPlayablePlayerTypeChanged;
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Circle)
        {
            circleArrow.SetActive(true);
            crossArrow.SetActive(false);
        }
        else
        {
            circleArrow.SetActive(false);
            crossArrow.SetActive(true);  
        }
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Circle)
        {
            circleYouText.SetActive(true);
            crossYouText.SetActive(false);
        }
        else
        {
            circleYouText.SetActive(false);
            crossYouText.SetActive(true);
        }
        
        UpdateCurrentArrow();
    }

    private void GameManager_OnCurrentPlayablePlayerTypeChanged(object sender, EventArgs e)
    {
        UpdateCurrentArrow();
    }
}
