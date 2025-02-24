using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Lance
{
    static class CurrentGDPR
    {
        private static bool mIsGDPROn;
        private static string mPurposeConsent, mVendorConsent, mVendorLi, mPurposeLi, mPartnerConsent;

        static CurrentGDPR()
        {
            SetData();
        }

        public static void SetData()
        {
            int gdprNum;

#if UNITY_EDITOR // 에디터에서는 자바 호출이 에러나서 에외처리
            gdprNum = 1;
            mPurposeConsent = "0000000000";
            mVendorConsent = "0000000000";
            mVendorLi = "";
            mPurposeLi = "";
            mPartnerConsent = "";
#elif UNITY_ANDROID
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass preferenceManagerClass = new AndroidJavaClass("android.preference.PreferenceManager");
            AndroidJavaObject sharedPreferences = 
                        preferenceManagerClass.CallStatic<AndroidJavaObject>("getDefaultSharedPreferences", currentActivity);

            gdprNum = sharedPreferences.Call<int>("getInt", "IABTCF_gdprApplies", 0);
            mPurposeConsent = sharedPreferences.Call<string>("getString", "IABTCFmPurposeConsents", "");
            mVendorConsent = sharedPreferences.Call<string>("getString", "IABTCFmVendorConsents", "");
            mVendorLi = sharedPreferences.Call<string>("getString", "IABTCF_VendorLegitimateInterests", "");
            mPurposeLi = sharedPreferences.Call<string>("getString", "IABTCF_PurposeLegitimateInterests", "");
            mPartnerConsent = sharedPreferences.Call<string>("getString", "IABTCF_AddtlConsent", "");
#elif UNITY_IOS
            gdprNum = PlayerPrefs.GetInt("IABTCF_gdprApplies", 0);
            mPurposeConsent = PlayerPrefs.GetString("IABTCFmPurposeConsents", "");
            mVendorConsent = PlayerPrefs.GetString("IABTCFmVendorConsents", "");
            mVendorLi = PlayerPrefs.GetString("IABTCF_VendorLegitimateInterests", "");
            mPurposeLi = PlayerPrefs.GetString("IABTCF_PurposeLegitimateInterests", "");
            mPartnerConsent = PlayerPrefs.GetString("IABTCF_AddtlConsent", "");
#endif
            // 0 이면 아예 GDPR 대상이 아님. 1이어야 GDPR
            if (gdprNum == 1)
                mIsGDPROn = true;
            else
                mIsGDPROn = false;

            Debug.Log("GDPR을 띄우는가? " + mIsGDPROn);
            Debug.Log("광고에 필요한 권한 동의: " + mPurposeConsent);
            Debug.Log("광고에 필요한 적법관심(?) 동의: " + mVendorConsent);
            Debug.Log("구글에 동의처리가 되어있는가?: " + mVendorLi);
            Debug.Log("구글에 적법관심(?) 처리 여부: " + mPurposeConsent);
            Debug.Log("파트너 네트워크 여부: " + mPartnerConsent);
        }

        // GDPR을 띄워야 할 유저인지(= 유럽 + 영국) 리턴
        public static bool IsGDPR()
        {
            return mIsGDPROn;
        }

        // 광고가 보여지는지 여부 리턴
        public static bool CanAdShow()
        {
            int googleId = 755;
            bool hasGoogleVendorConsent = HasAttribute(mVendorConsent, googleId);
            bool hasGoogleVendorLi = HasAttribute(mVendorLi, googleId);

            // 광고 가능 - 비개인화 광고
            // return HasConsentFor(new List<int> { 1 }, mPurposeConsent, hasGoogleVendorConsent)
            //        && HasConsentOrLegitimateInterestFor(new List<int> { 2, 7, 9, 10 }, 
            //            mPurposeConsent, mPurposeLi, hasGoogleVendorConsent, hasGoogleVendorLi);

            // 광고 가능 - 제한적인 광고 - 1에 대한 권한이 없어도 됨 ㅇㅇ
            return HasConsentOrLegitimateInterestFor(new List<int> { 2, 7, 9, 10 },
                       mPurposeConsent, mPurposeLi, hasGoogleVendorConsent, hasGoogleVendorLi);
        }

        // 개인화 광고가 보여지는지 여부 리턴
        public static bool CanShowPersonalizedAds()
        {
            int googleId = 755;
            bool hasGoogleVendorConsent = HasAttribute(mVendorConsent, googleId);
            bool hasGoogleVendorLi = HasAttribute(mVendorLi, googleId);

            return HasConsentFor(new List<int> { 1, 3, 4 }, mPurposeConsent, hasGoogleVendorConsent)
                   && HasConsentOrLegitimateInterestFor(new List<int> { 2, 7, 9, 10 },
                       mPurposeConsent, mPurposeLi, hasGoogleVendorConsent, hasGoogleVendorLi);
        }

        public static bool IsPartnerConsent(string partnerID) // 파트너 권한 있는지 확인
        {
            return mPartnerConsent.Contains(partnerID);
        }

        // 이진 문자열의 "index" 위치에 "1"이 있는지 확인합니다(1 기반).
        private static bool HasAttribute(string input, int index)
        {
            return input.Length >= index && input[index - 1] == '1';
        }

        // 목적 목록에 대한 동의가 주어졌는지 확인합니다.
        private static bool HasConsentFor(List<int> purposes, string purposeConsent, bool hasVendorConsent)
        {
            return purposes.All(p => HasAttribute(purposeConsent, p)) && hasVendorConsent;
        }
        // 목적 목록에 대한 공급자의 동의 또는 정당한 이익이 있는지 확인합니다.
        private static bool HasConsentOrLegitimateInterestFor(List<int> purposes, string purposeConsent, string purposeLI, bool hasVendorConsent, bool hasVendorLI)
        {
            return purposes.All(p =>
                (HasAttribute(purposeLI, p) && hasVendorLI) ||
                (HasAttribute(purposeConsent, p) && hasVendorConsent));
        }
    }
}

