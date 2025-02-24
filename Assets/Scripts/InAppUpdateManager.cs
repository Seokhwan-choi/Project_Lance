using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Play.Common;
using Google.Play.AppUpdate;
using UnityEngine.SceneManagement;
using System;

namespace Lance
{
    class InAppUpdateManager
    {
        Login mLogin;
        AppUpdateManager mManager;
        public void Init(Login login)
        {
            mLogin = login;
            mManager = new AppUpdateManager();
        }

        public IEnumerator CheckForUpdate()
        {
            PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation =
              mManager.GetAppUpdateInfo();

            yield return appUpdateInfoOperation;

            if (appUpdateInfoOperation.IsSuccessful)
            {
                AppUpdateInfo appUpdateInfoResult = appUpdateInfoOperation.GetResult();
                
                if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable)
                {
                    var popup = UIUtil.ShowConfirmPopup("확인", "업데이트가 가능합니다. \n" +
                        "스토어로 이동합니다.", null, null, ignoreBackButton: true);
                    popup.SetHideCancelButton();
                    popup.SetOnCloseAction(OnConfirm);

                    void OnConfirm()
                    {
                        Application.OpenURL("https://play.google.com/store/apps/details?id=com.theduckgames.lance");
                    }
                }
                else if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateNotAvailable)
                {
                    mLogin.SetUpdateAvailable(false);
                }
            }
            else
            {
                Debug.LogError($"{appUpdateInfoOperation.Error}");

                string title = StringTableUtil.Get("Title_Error");
                string desc = StringTableUtil.Get("Desc_FailedInAppUpdate");

                var popup = UIUtil.ShowConfirmPopup(title, desc, null, null, ignoreBackButton:true);
                popup.SetHideCancelButton();
                popup.SetOnCloseAction(OnConfirm);

                void OnConfirm()
                {
                    SceneManager.LoadScene("Login");
                }
            }
        }
    }
}