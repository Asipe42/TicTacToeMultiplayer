using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button rematchButton;
    [SerializeField] private TextMeshProUGUI resultTextMesh;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;

    private void Awake()
    {
        rematchButton.onClick.AddListener(() => GameManager.Instance.RematchRpc());
    }

    void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
        Hide();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinArgs e)
    {
        Debug.Log($"{nameof(GameManager_OnGameWin)}");
        
        if (GameManager.Instance.GetLocalPlayerType() == e.winPlayerType)
        {
            resultTextMesh.color = winColor;
            resultTextMesh.text = "YOU WIN!";
        }
        else
        {
            resultTextMesh.color = loseColor;
            resultTextMesh.text = "YOU LOSE!";
        }

        Show();
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
