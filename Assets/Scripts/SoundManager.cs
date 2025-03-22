using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSFXPrefab;
    [SerializeField] private Transform winSFXPrefab;
    [SerializeField] private Transform loseSFXPrefab;

    private void Start()
    {
        GameManager.Instance.OnPlacedObject += GameManager_OnPlacedObject;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void GameManager_OnPlacedObject(object sender, EventArgs e)
    {
        Transform sfxTransform = Instantiate(placeSFXPrefab);
        Destroy(sfxTransform.gameObject, 5f);
    }
    
    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == e.winPlayerType)
        {
            Transform sfxTransform = Instantiate(winSFXPrefab);
            Destroy(sfxTransform.gameObject, 5f);
        }
        else
        {
            Transform sfxTransform = Instantiate(loseSFXPrefab);
            Destroy(sfxTransform.gameObject, 5f);
        }
    }
}