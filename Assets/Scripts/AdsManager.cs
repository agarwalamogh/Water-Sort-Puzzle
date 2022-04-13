using System;
using Game;
#if ADMOB
using GoogleMobileAds.Api;
#endif
using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements;

#endif

// ReSharper disable once HollowTypeName
public partial class AdsManager : Singleton<AdsManager>
{
#if ADMOB
    private static string AdmobInterstitialID => Application.platform == RuntimePlatform.Android
        ? GameSettings.Default.AndroidAdmobSetting.interstitialId
        : GameSettings.Default.IosAdmobSetting.interstitialId;

    private static string AdmobRewardedID => Application.platform == RuntimePlatform.Android
        ? GameSettings.Default.AndroidAdmobSetting.admobRewardedId
        : GameSettings.Default.IosAdmobSetting.admobRewardedId;


    private static string AdmobBannerID => Application.platform == RuntimePlatform.Android
        ? GameSettings.Default.AndroidAdmobSetting.bannerId
        : GameSettings.Default.IosAdmobSetting.bannerId;
#endif


#if UNITY_ADS
    private static string UnityRewardedPlacementID => "rewardedVideo";
#endif

    public bool Initialized { get; private set; }

    // ReSharper disable once NotAccessedField.Local
    private Action<bool> _pendingCallback;

    public static int InterstitialCount { get; private set; } = 0;

    public static bool HaveSetupConsent => PrefManager.HasKey(nameof(ConsentActive));

    public static bool ConsentActive
    {
        get => PrefManager.GetBool(nameof(ConsentActive));
        set => PrefManager.SetBool(nameof(ConsentActive), value);
    }

    private static bool IsAdmobRewardedAvailable
    {
        get
        {
#if ADMOB
            return _rewardBaseVideo != null && _rewardBaseVideo.IsLoaded();
#endif
            // ReSharper disable once HeuristicUnreachableCode
#pragma warning disable 162
            return false;
#pragma warning restore 162
        }
    }

    private static bool IsAdmobInterstitialAvailable
    {
        get
        {
#if ADMOB
            return _interstitialAd.IsLoaded();
#endif
#pragma warning disable 162
            return false;
#pragma warning restore 162
        }
    }


#if ADMOB
    private static RewardBasedVideoAd _rewardBaseVideo;
    private static InterstitialAd _interstitialAd;
    private static BannerView _bannerAd;

#endif

    void Start()
    {
        if (HaveSetupConsent)
            Init();
    }


    public void Init()
    {
        if (Initialized)
            return;


#if ADMOB
        _rewardBaseVideo = RewardBasedVideoAd.Instance;
        _rewardBaseVideo.OnAdRewarded += RewardBaseVideoOnOnAdRewarded;
        _rewardBaseVideo.OnAdFailedToLoad += (sender, args) =>
        {
            //            PlatformUtils.ShowToast($"Video Ads Loaded Failed:{args.Message}");
            Invoke(nameof(RequestAdmobRewardVideo), 6f);
        };
        _rewardBaseVideo.OnAdClosed += (sender, args) =>
        {
            //            PlatformUtils.ShowToast($"Video Ads Loaded Failed:{args.Message}");
            Invoke(nameof(RequestAdmobRewardVideo), 5f);
        };
        //        _rewardBaseVideo.OnAdLoaded += (sender, args) => { PlatformUtils.ShowToast($"Video Ads Loaded"); };
        RequestAdmobRewardVideo();
        RequestAdmobInterstitial();
        if (ResourceManager.EnableAds)
        {
            RequestAdmobBanner();
        }
#endif


        Initialized = true;
    }


#if ADMOB
    // ReSharper disable once TooManyDeclarations
    private void RequestAdmobRewardVideo()
    {
        var request = new AdRequest.Builder()
            .AddTestDevice(AdRequest.TestDeviceSimulator)
            .AddTestDevice("0123456789ABCDEF0123456789ABCDEF")
            .Build();
        _rewardBaseVideo.LoadAd(request, AdmobRewardedID);
    }


    private void RequestAdmobInterstitial()
    {
        _interstitialAd = new InterstitialAd(AdmobInterstitialID);
        _interstitialAd.OnAdClosed += InterstitialAdOnOnAdClosed;
        _interstitialAd.OnAdFailedToLoad += (sender, args) =>
        {
            Instance.Invoke(nameof(RequestAdmobInterstitial), 10);
        };

        var request = new AdRequest.Builder()
            .AddTestDevice(AdRequest.TestDeviceSimulator)
            .AddTestDevice("0123456789ABCDEF0123456789ABCDEF")
            .Build();
        _interstitialAd.LoadAd(request);
    }


    private void RequestAdmobBanner()
    {
        _bannerAd = new BannerView(AdmobBannerID, AdSize.Banner, AdPosition.Bottom);
        var request = new AdRequest.Builder()
            .AddTestDevice(AdRequest.TestDeviceSimulator)
            .AddTestDevice("0123456789ABCDEF0123456789ABCDEF")
            .Build();
        _bannerAd.LoadAd(request);

    }

    private static void InterstitialAdOnOnAdClosed(object sender, EventArgs eventArgs)
    {
        Instance.RequestAdmobInterstitial();
    }


    private void RewardBaseVideoOnOnAdRewarded(object sender, Reward reward)
    {
        _pendingCallback?.Invoke(true);
        _pendingCallback = null;
    }
#endif


#if UNITY_ADS
    // ReSharper disable once FlagArgument
    private static void ShowUnityVideoAds(bool rewarded, Action<bool> completed = null)
    {
        if (rewarded && !IsUnityRewardedAdsAvailable || !rewarded && !IsUnityDefaultAdsAvailable)
        {
            completed?.Invoke(false);
            return;
        }


        var showOptions = new ShowOptions
        {
            resultCallback = result =>
            {
                switch (result)
                {
                    case ShowResult.Failed:
                        completed?.Invoke(false);
                        break;
                    case ShowResult.Skipped:
                        completed?.Invoke(true);
                        break;
                    case ShowResult.Finished:
                        completed?.Invoke(true);
                        break;
                }
            }
        };
        if (rewarded)
            Advertisement.Show(UnityRewardedPlacementID, showOptions);
        else
        {
            Advertisement.Show(showOptions);
        }
    }

#endif




    public enum VideoType
    {
        Skip,
        Full
    }
}

//#if UNITY_ADS
//public partial class AdsManager : IUnityAdsListener
//{
//    public void OnUnityAdsReady(string placementId)
//    {
//        
//    }
//
//    public void OnUnityAdsDidError(string message)
//    {
//    }
//
//    public void OnUnityAdsDidStart(string placementId)
//    {
//    }
//
//    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
//    {
//
//        _pendingCallback?.Invoke(showResult != ShowResult.Failed);
//        _pendingCallback = null;
//    }
//}
//#endif

// ReSharper disable once HollowTypeName
public partial class AdsManager
{
    public static void ShowInterstitial()
    {
        //        PlatformUtils.ShowToast(nameof(ShowInterstitial)+IsAdmobInterstitialAvailable);
        if (IsAdmobInterstitialAvailable)
        {
#if ADMOB
            _interstitialAd.Show();
#endif
        }
    }


    //    // ReSharper disable once FlagArgument
    //    // ReSharper disable once MethodTooLong
    public static void ShowVideoAds(bool rewarded, Action<bool> completed = null)
    {
        if (!rewarded)
        {
            if (IsUnityDefaultAdsAvailable)
            {
#if UNITY_ADS
                ShowUnityVideoAds(false, completed);
#endif
            }
            else
            {
                completed?.Invoke(false);
            }
        }
        else
        {
            if (IsAdmobRewardedAvailable && Application.platform != RuntimePlatform.WindowsEditor)
            {
                Instance._pendingCallback = completed;
#if ADMOB
                _rewardBaseVideo.Show();

#endif
            }
            else if (IsUnityRewardedAdsAvailable)
            {
                #if UNITY_ADS
                ShowUnityVideoAds(true, completed);
#endif
            }
            else
            {
                completed?.Invoke(false);
            }
        }
    }


    public static bool IsVideoAvailable(bool rewarded = true)
    {
        return IsAdmobRewardedAvailable || IsUnityRewardedAdsAvailable;
    }

    public static bool IsUnityDefaultAdsAvailable
    {
        get
        {
#if UNITY_ADS
            return Advertisement.IsReady();
#endif
            // ReSharper disable once HeuristicUnreachableCode
#pragma warning disable 162
            return false;
#pragma warning restore 162
        }
    }


    public static bool IsUnityRewardedAdsAvailable
    {
        get
        {
#if UNITY_ADS
            return Advertisement.IsReady(UnityRewardedPlacementID);
#endif
            // ReSharper disable once HeuristicUnreachableCode
#pragma warning disable 162
            return false;
#pragma warning restore 162
        }
    }


    public static bool IsInterstitialAvailable()
    {
        var available = //Application.internetReachability != NetworkReachability.NotReachable ||
            IsAdmobInterstitialAvailable; //|| IsUnityAdsAvailable(VideoType.Skip);

        return available;
    }
}


public partial class AdsManager
{
    private static int AdsPassLeftCount
    {
        get
        {
            if (!PlayerPrefs.HasKey(nameof(AdsPassLeftCount)))
            {
                SetForNextAds();
            }

            return PlayerPrefs.GetInt(nameof(AdsPassLeftCount));
        }
        set => PlayerPrefs.SetInt(nameof(AdsPassLeftCount), value);
    }


    public static void ShowOrPassAdsIfCan()
    {
        if (!ResourceManager.EnableAds)
            return;

        if (AdsPassLeftCount <= 0)
        {
            ShowAdsIfPassedIfCan();
        }
        else
        {
            PassAdsIfCan();
        }
    }

    public static void ShowAdsIfPassedIfCan()
    {
        if (!ResourceManager.EnableAds)
            return;
        if (AdsPassLeftCount <= 0)
        {

            ShowInterstitial();
            SetForNextAds();

        }
    }

    private static void SetForNextAds()
    {
        AdsPassLeftCount =
            UnityEngine.Random.Range(GameSettings.Default.AdsSettings.minAndMaxGameOversBetweenInterstitialAds.x,
                GameSettings.Default.AdsSettings.minAndMaxGameOversBetweenInterstitialAds.y + 1);
    }

    public static void PassAdsIfCan()
    {
        if (!ResourceManager.EnableAds)
            return;

        AdsPassLeftCount = Mathf.Max(AdsPassLeftCount - 1, 0);
    }
}

