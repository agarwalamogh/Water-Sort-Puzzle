using System.Collections;
using System.Collections.Generic;
using dotmob;
using UnityEngine;
using UnityEngine.UI;

public class PausePanel : ShowHidable
{
    [SerializeField] private Button _removeAdsBtn;
    [SerializeField] private Button _menuBtn;
    [SerializeField] private Button _restorePurchaseBtn;


    private void Awake()
    {
#if IN_APP
        _removeAdsBtn.gameObject.SetActive(ResourceManager.EnableAds);
#else
        _removeAdsBtn.gameObject.SetActive(false);
#endif




        _restorePurchaseBtn.gameObject.SetActive(Application.platform == RuntimePlatform.IPhonePlayer
#if IN_APP
                             && ResourceManager.AbleToRestore
#endif
        );
    }


#if IN_APP
    private void OnEnable()
    {
        ResourceManager.ProductRestored += ResourceManagerOnProductRestored;
        ResourceManager.ProductPurchased += ResourceManagerOnProductPurchased;
    }



    private void OnDisable()
    {
        ResourceManager.ProductRestored -= ResourceManagerOnProductRestored;
        ResourceManager.ProductPurchased -= ResourceManagerOnProductPurchased;
    }

    private void ResourceManagerOnProductPurchased(string productId)
    {
        gameObject.SetActive(ResourceManager.EnableAds);
    }


    private void ResourceManagerOnProductRestored(bool b)
    {
        gameObject.SetActive(ResourceManager.AbleToRestore);
    }

#endif


    public void OnClickCloseButton()
    {
        SharedUIManager.PausePanel.Hide();
    }

    public void OnClickMenuButton()
    {
        Debug.Log("Go to menu");
        GameManager.LoadScene("MainMenu");
        SharedUIManager.PausePanel.Hide();
    }

    public void OnClickRemoveAds()
    {
        Debug.Log("Remove Ads");
#if IN_APP
        ResourceManager.PurchaseNoAds(success => { });
#endif
    }

public void OnClickRestore()
    {
#if IN_APP
        ResourceManager.RestorePurchase();
#endif
    }




    


}
