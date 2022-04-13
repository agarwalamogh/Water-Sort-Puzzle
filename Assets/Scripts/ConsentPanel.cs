using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsentPanel : ShowHidable
{

    [SerializeField] private Button _privatePolicyBtn;


    private void Awake()
    {
        _privatePolicyBtn.gameObject.SetActive(GameSettings.Default.PrivatePolicySetting.enable);
    }


    public void OnClickYes()
    {
        AdsManager.ConsentActive = true;
        AdsManager.Instance.Init();
        Hide();
    }

    public void OnClickPrivacy()
    {
        Application.OpenURL(GameSettings.Default.PrivatePolicySetting.url);
    }

    public void OnClickNo()
    {
        AdsManager.ConsentActive = false;
        AdsManager.Instance.Init();
        Hide();
    }
}
