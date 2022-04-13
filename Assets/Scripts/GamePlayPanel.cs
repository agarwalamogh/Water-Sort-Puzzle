

using System;
using Game;
using dotmob;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayPanel : ShowHidable
{
    [SerializeField] private Text _lvlTxt;
    [SerializeField] private Text txtTutorial;

    private void Start()
    {
        _lvlTxt.text = $" LEVEL\n {LevelManager.Instance.Level.no}";

        if(LevelManager.Instance.Level.no == 1)
        {
            txtTutorial.gameObject.SetActive(true);
        }
        else
        {
            txtTutorial.gameObject.SetActive(false);
        }
        //Debug.Log("Level :" + LevelManager.Instance.Level.no);
    }

    public void OnClickUndo()
    {
        LevelManager.Instance.OnClickUndo();
    }

    public void OnClickRestart()
    {
        GameManager.LoadGame(new LoadGameData
        {
            Level = LevelManager.Instance.Level,
            GameMode = LevelManager.Instance.GameMode,
        },false);
    }

    public void OnClickSkip()
    {
        if (!AdsManager.IsVideoAvailable())
        {
            SharedUIManager.PopUpPanel.ShowAsInfo("Notice", "Sorry no video ads available.Check your internet connection!");
            return;
        }

        SharedUIManager.PopUpPanel.ShowAsConfirmation("Skip Level","Do you want watch Video ads to skip this level", success =>
        {
            if(!success)
                return;

            AdsManager.ShowVideoAds(true, s =>
            {
                if(!s)
                    return;
                ResourceManager.CompleteLevel(LevelManager.Instance.GameMode, LevelManager.Instance.Level.no);
                UIManager.Instance.LoadNextLevel();
            });
          
        });
    }

    public void OnClickMenu()
    {
        //SharedUIManager.PopUpPanel.ShowAsConfirmation("PAUSE", "Are you sure want to exit the game?", success =>
        // {
        //     if (!success)
        //         return;

        //     GameManager.LoadScene("MainMenu");
        // });

        SharedUIManager.PausePanel.Show();

    }

    private void Update()
    {
    }
}