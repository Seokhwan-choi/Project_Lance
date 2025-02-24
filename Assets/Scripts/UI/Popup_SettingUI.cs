using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using BackEnd;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GoogleMobileAds.Ump.Api;

namespace Lance
{
    enum SettingOnNOff
    {
        BGM,
        SFX,
        Camera,
        Skill,
        Statistics,
        SleepMode,
        Push,

        Count,
    }

    class Popup_SettingUI : PopupBase
    {
        public void Init()
        {
            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get("Title_Setting"));

            for(int i = 0; i < (int)SettingOnNOff.Count; ++i)
            {
                SettingOnNOff setting = (SettingOnNOff)i;

                var settingObj = gameObject.FindGameObject($"{setting}");

                var settingOnNOffObj = settingObj.FindGameObject("On&Off");

                var settingOnNOff = settingOnNOffObj.GetOrAddComponent<OnNOffButtons>();

                settingOnNOff.Init(setting.ChangeToSaveBitFlags());
            }

            // 구글 계정 연동
            InitGoogleLinkButtons();

            // 커뮤니티
            var buttonCommunity = gameObject.FindComponent<Button>("Button_Community");
            buttonCommunity.SetButtonAction(() =>
            {
                Application.OpenURL(Lance.GameData.CommonData.communityUrl);
            });

            // 쿠폰
            var buttonCoupon = gameObject.FindComponent<Button>("Button_Coupon");
            buttonCoupon.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_CouponUI>();

                popup.Init();
            });

            // 언어변경
            var buttonLanguage = gameObject.FindComponent<Button>("Button_Language");
            buttonLanguage.SetButtonAction(() =>
            {
                if (Lance.LocalSave.LangCode == LangCode.KR)
                    Lance.LocalSave.LangCode = LangCode.US;
                else
                    Lance.LocalSave.LangCode = LangCode.KR;

                Lance.LocalSave.Save();

                Lance.Lobby.Localize();
                Lance.GameManager.Localize();

                var readerUIs = gameObject.GetComponentsInChildren<StringTableUIReader>(true);
                foreach (var readerUI in readerUIs)
                {
                    readerUI.Localize();
                }

                SetTitleText(StringTableUtil.Get("Title_Setting"));
            });

            if (CurrentGDPR.IsGDPR())
            {
                var button = gameObject.FindComponent<Button>("Button_GDPR");
                button.gameObject.SetActive(true);
                button.SetButtonAction(() =>
                {
                    string title = StringTableUtil.Get("Title_Confirm");
                    string desc = StringTableUtil.GetDesc("GDPR");

                    UIUtil.ShowConfirmPopup(title, desc, () =>
                    {
                        ConsentInformation.Reset(); // 기존 GDPR 정보 초기화
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
                                    Lance.AdManager.LoadAndShowConsentForm();
                                }
                                else if (consentStatus == ConsentStatus.Obtained)
                                {
                                    if (ConsentInformation.CanRequestAds())
                                    {
                                        Lance.AdManager.InitAds();
                                    }
                                }
                                else if (consentStatus == ConsentStatus.NotRequired)
                                {
                                    Lance.AdManager.InitAds();
                                }
                            }
                            else
                            {
                                Debug.LogError("Error updating consent information: " + error.Message);
                            }
                        });
                    }, null);
                });
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            var buttonFastMode = gameObject.FindComponent<Button>("Button_FastMode");
            buttonFastMode.SetButtonAction(() =>
            {
                Lance.GameManager.FastMode(10f);
            });

            var buttonFaseMode2 = gameObject.FindComponent<Button>("Button_FastMode2");
            buttonFaseMode2.SetButtonAction(() =>
            {
                Lance.GameManager.FastMode(5f);
            });
#endif
            // 개인 정보 처리 방침
            var buttonPrivacypolicy = gameObject.FindComponent<Button>("Button_Privacypolicy");
            buttonPrivacypolicy.SetButtonAction(() =>
            {
                if (Lance.LocalSave.LangCode == LangCode.KR)
                {
                    Application.OpenURL(Lance.GameData.CommonData.privacypolicyUrl);
                }
                else
                {
                    Application.OpenURL(Lance.GameData.CommonData.privacypolicyUrl_eng);
                }
            });

            // 문의하기
            var buttonInquiry = gameObject.FindComponent<Button>("Button_Inquiry");
            buttonInquiry.SetButtonAction(() =>
            {
                string mailto = Lance.GameData.CommonData.inquiryMail;
                string subject = MyEscapeURL($"{StringTableUtil.Get("UIString_InquiryTitle")}"); 
                string body = MyEscapeURL(
                    $"{StringTableUtil.Get("UIString_InputContents")}\n\n\n\n" + "________" +"\n\n"+
                    "NickName : " + Backend.UserNickName + "\n\n" +
                    "Device Model : " + SystemInfo.deviceModel + "\n\n" + 
                    "Device OS : " + SystemInfo.operatingSystem + "\n\n" + 
                    "________"); 

                Application.OpenURL("mailto:" + mailto + "?subject=" + subject + "&body=" + body);

                string MyEscapeURL(string url)
                {
                    return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
                }
            });

            var buttonThanksTo = gameObject.FindComponent<Button>("Button_ThanksTo");
            buttonThanksTo.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_ThanksToUI>();

                popup.Init();
            });
        }

        void InitGoogleLinkButtons()
        {
            var buttonLinkGoogle = gameObject.FindComponent<Button>("Button_LinkGoogle");
            var buttonAlreadyLinkGoogle = gameObject.FindComponent<Button>("Button_AlreadyLinkGoogle");

            buttonAlreadyLinkGoogle.SetButtonAction(() =>
            {
                UIUtil.ShowSystemErrorMessage("AlreadyGoogleLink");
            });

            buttonLinkGoogle.SetButtonAction(() =>
            {
                string title = StringTableUtil.Get("Title_Caution");
                string desc = StringTableUtil.Get("Desc_GoogleLink");

                var popup = UIUtil.ShowConfirmPopup(title, desc, OnConfirm, null);

                void OnConfirm()
                {
                    Lance.TouchBlock.SetActive(true);

                    PlayGamesPlatform.Instance.ManuallyAuthenticate((status) =>
                    {
                        if (status == SignInStatus.Success)
                        {
                            PlayGamesPlatform.Instance.RequestServerSideAccess(false, code =>
                            {
                                Backend.BMember.GetGPGS2AccessToken(code, googleCallback =>
                                {
                                    string accessToken = "";

                                    if (googleCallback.IsSuccess())
                                    {
                                        accessToken = googleCallback.GetReturnValuetoJSON()["access_token"].ToString();

                                        SendQueue.Enqueue(Backend.BMember.ChangeCustomToFederation, accessToken, FederationType.GPGS2, callback => {
                                            if (callback.IsSuccess())
                                            {
                                                UIUtil.ShowSystemMessage(StringTableUtil.GetSystemMessage("GoogleLinkSuccess"));

                                                RefreshGoogleLinkButtons();

                                                Lance.TouchBlock.SetActive(false);
                                            }
                                            else
                                            {
                                                if (callback.GetStatusCode() == "409")
                                                {
                                                    // 이미 가입한 계정이 있어서 연동 실패할 수 있다.
                                                    UIUtil.ShowSystemErrorMessage("AlreadyGoogleSignIn");

                                                    Lance.TouchBlock.SetActive(false);
                                                }
                                                else
                                                {
                                                    FailedGoogleLink();
                                                }
                                            }
                                        });
                                    }
                                    else
                                    {
                                        FailedGoogleLink();
                                    }
                                });
                            });
                        }
                        else
                        {
                            FailedGoogleLink();
                        }
                    });

                    void FailedGoogleLink()
                    {
                        UIUtil.ShowSystemErrorMessage("GoogleLinkFailed");

                        Lance.TouchBlock.SetActive(false);
                    }
                }
            });

            RefreshGoogleLinkButtons();

            void RefreshGoogleLinkButtons()
            {
                bool googleLink = PlayGamesPlatform.Instance.IsAuthenticated();

                buttonAlreadyLinkGoogle.gameObject.SetActive(googleLink);
                buttonLinkGoogle.gameObject.SetActive(!googleLink);
            }
        }
    }

    class OnNOffButtons : MonoBehaviour
    {
        SaveBitFlags mBitFlag;

        Color mActiveTextColor;
        Color mInActiveTextColor;

        TextMeshProUGUI mTextOn;
        Image mImageOnActive;

        TextMeshProUGUI mTextOff;
        Image mImageOffActive;
        public void Init(SaveBitFlags bitFlag)
        {
            mBitFlag = bitFlag;

            ColorUtility.TryParseHtmlString("#2A2220", out mActiveTextColor);
            ColorUtility.TryParseHtmlString("#8C8681", out mInActiveTextColor);

            var buttonOn = gameObject.FindComponent<Button>("Button_On");
            buttonOn.SetButtonAction(() => OnButtonAction(on: true));

            mTextOn = buttonOn.gameObject.FindComponent<TextMeshProUGUI>("Text_On");
            mImageOnActive = buttonOn.gameObject.FindComponent<Image>("Image_Active");

            var buttonOff = gameObject.FindComponent<Button>("Button_Off");
            buttonOff.SetButtonAction(() => OnButtonAction(on: false));

            mTextOff = buttonOff.gameObject.FindComponent<TextMeshProUGUI>("Text_Off");
            mImageOffActive = buttonOff.gameObject.FindComponent<Image>("Image_Active");

            Refresh();
        }

        void OnButtonAction(bool on)
        {
            if (mBitFlag != SaveBitFlags.Push)
            {
                mBitFlag.Set(on);

                Lance.GameManager.OnChangeBitFlags();

                Refresh();
            }
            else
            {
                // 푸시 설정
                Lance.Firebase.SetPush(on, OnFinishPush);
            }
        }
        
        void OnFinishPush(bool success)
        {
            Refresh();
        }

        void Refresh()
        {
            if (mBitFlag != SaveBitFlags.Push)
            {
                RefreshUI(mBitFlag.IsOn());
            }
            else
            {
                // 푸시
                RefreshUI(Lance.Account.UserInfo.GetPushAllow());
            }

            void RefreshUI(bool on)
            {
                if (on)
                {
                    mTextOn.color = mActiveTextColor;
                    mImageOnActive.gameObject.SetActive(true);

                    mTextOff.color = mInActiveTextColor;
                    mImageOffActive.gameObject.SetActive(false);
                }
                else
                {
                    mTextOn.color = mInActiveTextColor;
                    mImageOnActive.gameObject.SetActive(false);

                    mTextOff.color = mActiveTextColor;
                    mImageOffActive.gameObject.SetActive(true);
                }
            }
        }
    }

    static class SettingOnNOffExt
    {
        public static SaveBitFlags ChangeToSaveBitFlags(this SettingOnNOff setting)
        {
            switch (setting)
            {
                case SettingOnNOff.BGM:
                    return SaveBitFlags.BGMSound;
                case SettingOnNOff.SFX:
                    return SaveBitFlags.SFXSound;
                case SettingOnNOff.Camera:
                    return SaveBitFlags.CameraEffect;
                case SettingOnNOff.SleepMode:
                    return SaveBitFlags.AutoSleepMode;
                case SettingOnNOff.Skill:
                    return SaveBitFlags.SkillEffect;
                case SettingOnNOff.Statistics:
                    return SaveBitFlags.AutoResetStatictics;
                case SettingOnNOff.Push:
                default:
                    return SaveBitFlags.Push;
            }
        }
    }
}



