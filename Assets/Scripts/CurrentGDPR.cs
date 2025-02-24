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

#if UNITY_EDITOR // �����Ϳ����� �ڹ� ȣ���� �������� ����ó��
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
            // 0 �̸� �ƿ� GDPR ����� �ƴ�. 1�̾�� GDPR
            if (gdprNum == 1)
                mIsGDPROn = true;
            else
                mIsGDPROn = false;

            Debug.Log("GDPR�� ���°�? " + mIsGDPROn);
            Debug.Log("���� �ʿ��� ���� ����: " + mPurposeConsent);
            Debug.Log("���� �ʿ��� ��������(?) ����: " + mVendorConsent);
            Debug.Log("���ۿ� ����ó���� �Ǿ��ִ°�?: " + mVendorLi);
            Debug.Log("���ۿ� ��������(?) ó�� ����: " + mPurposeConsent);
            Debug.Log("��Ʈ�� ��Ʈ��ũ ����: " + mPartnerConsent);
        }

        // GDPR�� ����� �� ��������(= ���� + ����) ����
        public static bool IsGDPR()
        {
            return mIsGDPROn;
        }

        // ���� ���������� ���� ����
        public static bool CanAdShow()
        {
            int googleId = 755;
            bool hasGoogleVendorConsent = HasAttribute(mVendorConsent, googleId);
            bool hasGoogleVendorLi = HasAttribute(mVendorLi, googleId);

            // ���� ���� - ����ȭ ����
            // return HasConsentFor(new List<int> { 1 }, mPurposeConsent, hasGoogleVendorConsent)
            //        && HasConsentOrLegitimateInterestFor(new List<int> { 2, 7, 9, 10 }, 
            //            mPurposeConsent, mPurposeLi, hasGoogleVendorConsent, hasGoogleVendorLi);

            // ���� ���� - �������� ���� - 1�� ���� ������ ��� �� ����
            return HasConsentOrLegitimateInterestFor(new List<int> { 2, 7, 9, 10 },
                       mPurposeConsent, mPurposeLi, hasGoogleVendorConsent, hasGoogleVendorLi);
        }

        // ����ȭ ���� ���������� ���� ����
        public static bool CanShowPersonalizedAds()
        {
            int googleId = 755;
            bool hasGoogleVendorConsent = HasAttribute(mVendorConsent, googleId);
            bool hasGoogleVendorLi = HasAttribute(mVendorLi, googleId);

            return HasConsentFor(new List<int> { 1, 3, 4 }, mPurposeConsent, hasGoogleVendorConsent)
                   && HasConsentOrLegitimateInterestFor(new List<int> { 2, 7, 9, 10 },
                       mPurposeConsent, mPurposeLi, hasGoogleVendorConsent, hasGoogleVendorLi);
        }

        public static bool IsPartnerConsent(string partnerID) // ��Ʈ�� ���� �ִ��� Ȯ��
        {
            return mPartnerConsent.Contains(partnerID);
        }

        // ���� ���ڿ��� "index" ��ġ�� "1"�� �ִ��� Ȯ���մϴ�(1 ���).
        private static bool HasAttribute(string input, int index)
        {
            return input.Length >= index && input[index - 1] == '1';
        }

        // ���� ��Ͽ� ���� ���ǰ� �־������� Ȯ���մϴ�.
        private static bool HasConsentFor(List<int> purposes, string purposeConsent, bool hasVendorConsent)
        {
            return purposes.All(p => HasAttribute(purposeConsent, p)) && hasVendorConsent;
        }
        // ���� ��Ͽ� ���� �������� ���� �Ǵ� ������ ������ �ִ��� Ȯ���մϴ�.
        private static bool HasConsentOrLegitimateInterestFor(List<int> purposes, string purposeConsent, string purposeLI, bool hasVendorConsent, bool hasVendorLI)
        {
            return purposes.All(p =>
                (HasAttribute(purposeLI, p) && hasVendorLI) ||
                (HasAttribute(purposeConsent, p) && hasVendorConsent));
        }
    }
}

