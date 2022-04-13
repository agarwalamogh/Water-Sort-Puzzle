using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class NoAdsButton : MonoBehaviour, IPointerClickHandler
{

    private void Awake()
    {
#if IN_APP
        gameObject.SetActive(ResourceManager.EnableAds);
#else
        gameObject.SetActive(false);
#endif
    }
#if IN_APP
    private void OnEnable()
    {
        ResourceManager.ProductPurchased+=ResourceManagerOnProductPurchased;
    }



    private void OnDisable()
    {
        ResourceManager.ProductPurchased-=ResourceManagerOnProductPurchased;
    }

    private void ResourceManagerOnProductPurchased(string productId)
    {
        gameObject.SetActive(ResourceManager.EnableAds);
    }
#endif
    public void OnPointerClick(PointerEventData eventData)
    {
#if IN_APP
        ResourceManager.PurchaseNoAds(success => { });
#endif
    }

}