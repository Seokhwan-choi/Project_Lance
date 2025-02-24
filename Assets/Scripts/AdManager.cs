using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BackEnd;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;

namespace Lance
{
    class AdManager
    {
#if UNITY_ANDROID
        string mAppKey = "1de8ba0cd";
#else
        string mAppKey = "unexpected_platform";
#endif
        bool mIsOpenedAd;
        Action mOnRewardedEvent;

        public void Init()
        {
            Debug.Log($"deviceUniqueIdentifier : {SystemInfo.deviceUniqueIdentifier}");
#if DEVBUILD
            var debugSettings = new ConsentDebugSettings
            {
                // Geography appears as in EEA for debug devices.
                DebugGeography = DebugGeography.EEA,
                TestDeviceHashedIds = new List<string>
                            {
                                "04E49E9AA3DAC238286A65213888A39C",
                                "D51D3C6B2A63F9FE3E955D08866F4086"
                            }
            };

            ConsentRequestParameters request = new ConsentRequestParameters
            {
                ConsentDebugSettings = debugSettings,
            };

            ConsentInformation.Reset(); // 테스트를 위해 기존 GDPR 정보 초기화
#else
                        // Create a ConsentRequestParameters object.
            ConsentRequestParameters request = new ConsentRequestParameters();
#endif

            // Check the current consent information status.
            ConsentInformation.Update(request, (FormError error) =>
            {
                if (error == null)
                {
                    Debug.Log("Consent information updated.");

                    var consentStatus = ConsentInformation.ConsentStatus;

                    Debug.Log("Current Consent Status: " + consentStatus);

                    if (consentStatus == ConsentStatus.Required)
                    {
                        // EEA 지역에 있으며 동의가 필요한 경우만 폼 로드
                        LoadAndShowConsentForm();
                    }
                    else if (consentStatus == ConsentStatus.Obtained)
                    {
                        if (ConsentInformation.CanRequestAds())
                        {
                            InitAds();
                        }
                    }
                    else if (consentStatus == ConsentStatus.NotRequired)
                    {
                        InitAds();
                    }
                }
                else
                {
                    Debug.LogError("Error updating consent information: " + error.Message);
                }
            });
        }

        public void LoadAndShowConsentForm()
        {
            if (ConsentInformation.IsConsentFormAvailable())
            {
                ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
                {
                    if (formError != null)
                    {
                        // Consent gathering failed.
                        Debug.LogError("Error updating consent information: " + formError.Message);
                        return;
                    }

                    // Consent has been gathered.
                    if (ConsentInformation.CanRequestAds())
                    {
                        InitAds();
                    }
                });
            }
            else
            {
                InitAds();
            }
        }

        public void InitAds()
        {
            if (Lance.Account.PackageShop.IsPurchasedRemoveAD() == false && Lance.IAPManager.IsPurchasedRemoveAd() == false)
            {
                bool anyReady = false;
                // Initialize the Mobile Ads SDK.
                MobileAds.Initialize((initStatus) =>
                {
                    Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
                    foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
                    {
                        string className = keyValuePair.Key;
                        AdapterStatus status = keyValuePair.Value;
                        switch (status.InitializationState)
                        {
                            case AdapterState.NotReady:
                                // The adapter initialization did not complete.
                                //MonoBehaviour.print("Adapter: " + className + " not ready.");
                                break;
                            case AdapterState.Ready:

                                anyReady = true;

                                break;
                        }
                    }

                    if (anyReady)
                        LoadRewardedAd();
                });
            }
        }

        // These ad units are configured to always serve test ads.
        // android "ca-app-pub-3940256099942544/5224354917";
        // ios "ca-app-pub-3940256099942544/1712485313"
#if UNITY_ANDROID
        private string _adUnitId = "ca-app-pub-3056719834738780/7164298205";
#elif UNITY_IPHONE
  private string _adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
  private string _adUnitId = "unused";
#endif

        RewardedAd mRewardedAd;

        /// <summary>
        /// Loads the rewarded ad.
        /// </summary>
        public void LoadRewardedAd()
        {
            // Clean up the old ad before loading a new one.
            if (mRewardedAd != null)
            {
                mRewardedAd.Destroy();
                mRewardedAd = null;
            }

            Debug.Log("Loading the rewarded ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            RewardedAd.Load(_adUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
                    {
                        Debug.LogError("Rewarded ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Rewarded ad loaded with response : "
                              + ad.GetResponseInfo());

                    mRewardedAd = ad;

                    RegisterEventHandlers(mRewardedAd);
                });
        }

        public void ShowRewardedAd(AdType showAd, Action onRewardedEvent)
        {
            if (IsRewardedVideoAvailable())
            {
                mRewardedAd.Show((Reward reward) =>
                {
                    onRewardedEvent?.Invoke();

                    // 광고 보상 지급 로그 기록
                    Param param = new Param();
                    param.Add("adType", $"{showAd}");

                    Lance.BackEnd.InsertLog("AdRewarded", param, 7);
                });
            }
            else
            {
                LoadRewardedAd();
            }
        }

        // 광고가 준비되었는지 확인
        public bool IsRewardedVideoAvailable()
        {
            return mRewardedAd != null && mRewardedAd.CanShowAd();
        }

        void RegisterEventHandlers(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad full screen content closed.");

                // Reload the ad so that we can show another as soon as possible.
                LoadRewardedAd();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Lance.BackEnd.SendBugReport("AdShowFailed", "", "Rewarded ad failed to open full screen content " +
                               "with error : " + error);

                UIUtil.ShowSystemErrorMessage("IsRewardedVieidoUnavailable");

                // Reload the ad so that we can show another as soon as possible.
                LoadRewardedAd();
            };
        }
    }
}