using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Popup_ServiceTermsofUseUI : PopupBase
    {
        Image mImagePrivacyPolicyCheck;
        Image mImageTermsofUseCheck;
        Image mImagePushCheck;

        TextMeshProUGUI mTextPrivacyPoilcy;
        TextMeshProUGUI mTextPrivacyPoilcyUrl;

        TextMeshProUGUI mTextTermsofUse;
        TextMeshProUGUI mTextTermsofUseUrl;

        TextMeshProUGUI mTextPush;
        TextMeshProUGUI mTextAllSelect;
        TextMeshProUGUI mTextStart;

        bool mPrivacyPolicyAgree;
        bool mTermsofUseAgree;
        bool mPushAgree;

        Button mButtonStart;

        public void Init(Action<bool> onStart)
        {
            HideCloseButton();

            bool isKorean = Application.systemLanguage == SystemLanguage.Korean;

            string title = isKorean ? "약관 동의" : "Accept Terms";

            SetTitleText(title);
            mTextPrivacyPoilcy = gameObject.FindComponent<TextMeshProUGUI>("Text_PrivacyPolicy");
            mTextPrivacyPoilcy.text = isKorean ? "( 필수 ) 개인정보 수집 및 이용 동의" : "(Required) Consent to Use Personal Information";
            mTextPrivacyPoilcyUrl = gameObject.FindComponent<TextMeshProUGUI>("Text_PrivacyPolicyShowUrl");
            mTextPrivacyPoilcyUrl.text = isKorean ? "개인정보처리방침 보기" : "View Privacy Policy";

            mTextTermsofUse = gameObject.FindComponent<TextMeshProUGUI>("Text_TermsofUse");
            mTextTermsofUse.text = isKorean ? "( 필수 ) 서비스 이용 약관 동의" : "(Required) Consent to Terms of Service";
            mTextTermsofUseUrl = gameObject.FindComponent<TextMeshProUGUI>("Text_ShowUrl");
            mTextTermsofUseUrl.text = isKorean ? "서비스 이용약관 보기" : "View Terms of Service";

            mTextPush = gameObject.FindComponent<TextMeshProUGUI>("Text_Push");
            mTextPush.text = isKorean ? "( 선택 ) 주간 푸시 동의" : "(Optional) Consent to Weekly Push Notifications";
            mTextAllSelect = gameObject.FindComponent<TextMeshProUGUI>("Text_AllSelect");
            mTextAllSelect.text = isKorean ? "전체 선택" : "Consent to All";
            mTextStart = gameObject.FindComponent<TextMeshProUGUI>("Text_Start");
            mTextStart.text = isKorean ? "시작하기" : "Start";

            var privacyPolicyObj = gameObject.FindGameObject("PrivacyPolicy");
            var buttonPrivacyPolicy = privacyPolicyObj.FindComponent<Button>("Button_Policy");
            buttonPrivacyPolicy.SetButtonAction(() => 
            {
                mPrivacyPolicyAgree = !mPrivacyPolicyAgree;

                Refresh();
            });

            mImagePrivacyPolicyCheck = privacyPolicyObj.FindComponent<Image>("Image_Check");

            var buttonShowUrl = privacyPolicyObj.FindComponent<Button>("Button_ShowUrl");
            buttonShowUrl.SetButtonAction(() =>
            {
                string url = "https://storage.thebackend.io/af5876822aeb30999917884780f8f560b8ca631e0dcde0878da93b7ecde3ac6e/privacy.html";
                string url2 = "https://storage.thebackend.io/af5876822aeb30999917884780f8f560b8ca631e0dcde0878da93b7ecde3ac6e/privacy2.html";

                if (isKorean)
                {
                    Application.OpenURL(url);
                }
                else
                {
                    Application.OpenURL(url2);
                }
            });

            var termsofUseObj = gameObject.FindGameObject("TermsofUse");
            var buttonTermsofUse = termsofUseObj.FindComponent<Button>("Button_TermsofUse");
            buttonTermsofUse.SetButtonAction(() =>
            {
                mTermsofUseAgree = !mTermsofUseAgree;

                Refresh();
            });

            var buttonShowUrl2 = termsofUseObj.FindComponent<Button>("Button_ShowUrl");
            buttonShowUrl2.SetButtonAction(() =>
            {
                string url = "https://storage.thebackend.io/af5876822aeb30999917884780f8f560b8ca631e0dcde0878da93b7ecde3ac6e/terms.html";

                string url2 = "https://storage.thebackend.io/af5876822aeb30999917884780f8f560b8ca631e0dcde0878da93b7ecde3ac6e/terms2.html";

                if (isKorean)
                {
                    Application.OpenURL(url);
                }
                else
                {
                    Application.OpenURL(url2);
                }
            });

            mImageTermsofUseCheck = termsofUseObj.FindComponent<Image>("Image_Check");

            var pushObj = gameObject.FindGameObject("Push");
            var buttonPush = pushObj.FindComponent<Button>("Button_Push");
            buttonPush.SetButtonAction(() =>
            {
                mPushAgree = !mPushAgree;

                Refresh();
            });

            mImagePushCheck = pushObj.FindComponent<Image>("Image_Check");

            var buttonAllSelct = gameObject.FindComponent<Button>("Button_AllSelect");
            buttonAllSelct.SetButtonAction(() =>
            {
                mPrivacyPolicyAgree = true;
                mTermsofUseAgree = true;
                mPushAgree = true;

                Refresh();
            });

            mButtonStart = gameObject.FindComponent<Button>("Button_Start");
            mButtonStart.SetButtonAction(() =>
            {
                if (mPrivacyPolicyAgree == false)
                {
                    string message = isKorean ? "개인 정보 수집 및 이용 동의가 필요합니다." : "Consent to the collection and use of personal information is required.";

                    UIUtil.ShowSystemMessage(message);

                    return;
                }

                if (mTermsofUseAgree == false)
                {
                    string message = isKorean ? "서비스 이용 약관 동의가 필요합니다." : "Consent to the Terms of Service is required.";

                    UIUtil.ShowSystemMessage(message);

                    return;
                }

                Close();

                onStart?.Invoke(mPushAgree);

                SaveBitFlags.ServiceTermsofUseAgree.Set(true);
            });

            Refresh();
        }

        void Refresh()
        {
            mImagePrivacyPolicyCheck.gameObject.SetActive(mPrivacyPolicyAgree);
            mImageTermsofUseCheck.gameObject.SetActive(mTermsofUseAgree);
            mImagePushCheck.gameObject.SetActive(mPushAgree);

            mButtonStart.SetActiveFrame(mPrivacyPolicyAgree && mTermsofUseAgree);
        }
    }
}