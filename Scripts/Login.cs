using System.Collections;
using System.Reflection;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BackEnd;
using BackEnd.Group;
using UnityEngine.UI;
using LitJson;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using DG.Tweening;
using UnityEngine.Networking;
using System.Text;
using System.IO;
using System.IO.Compression;
using Assets.SimpleZip;
using ExcelToObject;
using BackEnd.GlobalSupport;
using CodeStage.AntiCheat.Detectors;

namespace Lance
{
    class Login : MonoBehaviour
    {
        // 1.5.1 https://drive.google.com/file/d/15UHT03oXXuzaOWRZCzU5VWTNZvzWchLW/view?usp=sharing
        const string DriveDataSheetId = "15UHT03oXXuzaOWRZCzU5VWTNZvzWchLW";

        const string GlobalGroupName = "global";
        const string GlobalGroupUuid = "0193ab4c-ed64-7784-9423-ca4c26004240";

        const string KoreaGroupName = "korea";
        const string KoreaGroupUuid = "0193ab4c-d10a-7438-a6b9-e56142eade11";

        bool mLoginStart;
        bool mAllowPush;
        bool mUpdateAvailable;
        bool mInitGameData;
        delegate void LoadingAction();
        Queue<LoadingAction> mLoadingActions;

        TextMeshProUGUI mTextGoogleLogin;
        TextMeshProUGUI mTextGuestLogin;

        GameObject mButtonsObj;
        GameObject mStartLoadingObj;
        Image mImageModal;
        CanvasGroup mLoadingGroup;
        void Awake()
        {
            GameObject contextLoginObj = GameObject.Find("Context_Login");

            var contextLogin = FindAnyObjectByType<Context_Login>();

            if (contextLogin == null)
            {
                contextLoginObj = Util.Instantiate("Prefabs/Context_Login");

                DontDestroyOnLoad(contextLoginObj);
            }
            else
            {
                contextLoginObj = contextLogin.gameObject;
            }

            if (IsRooted())
            {
                StartCoroutine(StartForcedQuit());
            }
            else
            {
                if (contextLoginObj != null)
                    Lance.InitLogin(contextLoginObj, StartLogin);
            }

            mUpdateAvailable = true;
        }

        IEnumerator StartForcedQuit()
        {
            UIUtil.ShowSystemMessage("System Rooting Game Quit", 1f);

            SoundPlayer.PlayErrorSound();

            yield return new WaitForSeconds(1f);

            Application.Quit();
        }

        public void SetUpdateAvailable(bool available)
        {
            mUpdateAvailable = available;

            // 업데이트가 없으니까 바로 로그인 시도
            if (!mUpdateAvailable)
            {
                StartCoroutine(DLZip(GPGSAuchenticate));
            }
        }

        void StartLogin(bool successBackendInit)
        {
            if (successBackendInit)
            {
                try
                {
                    if (Application.systemLanguage != SystemLanguage.Korean)
                    {
                        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");
                        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo("en-US");

                        if (Lance.LocalSave.LangCode == LangCode.KR)
                            Lance.LocalSave.LangCode = LangCode.US;
                    }
                }
                catch (Exception e)
                {

                }

                mLoginStart = false;

                mLoadingActions = new Queue<LoadingAction>();

                mStartLoadingObj = gameObject.FindGameObject("StartLoading");
                mStartLoadingObj.SetActive(false);

                mImageModal = mStartLoadingObj.FindComponent<Image>("Image_Black");
                mLoadingGroup = mStartLoadingObj.FindComponent<CanvasGroup>("Loading");

                var textVersion = gameObject.FindComponent<TextMeshProUGUI>("Text_Version");
                textVersion.text = $"Ver {Application.version}";

                mButtonsObj = gameObject.FindGameObject("Buttons");
                var buttonLoginGoogle = mButtonsObj.FindComponent<Button>("Button_LoginGoogle");
                buttonLoginGoogle.SetButtonAction(OnButtonLoginGoogle);

                mTextGoogleLogin = buttonLoginGoogle.FindComponent<TextMeshProUGUI>("Text_LoginGoogle");

                var buttonLoginGuest = mButtonsObj.FindComponent<Button>("Button_LoginGuest");
                buttonLoginGuest.SetButtonAction(OnButtonLoginGuest);

                mTextGuestLogin = buttonLoginGuest.FindComponent<TextMeshProUGUI>("Text_LoginGuest");

                RrefreshStr();

#if !UNITY_EDITOR
                InitFirebase();
#endif
                mUpdateAvailable = false;

                if (SaveBitFlags.ServiceTermsofUseAgree.IsOff())
                {
                    var popup = Lance.PopupManager.CreatePopup<Popup_ServiceTermsofUseUI>();
                    popup.Init((allowPush) =>
                    {
                        mAllowPush = allowPush;

                        StartLogin();
                    });
                }
                else
                {
                    StartLogin();
                }
            }
            else
            {
                SceneManager.LoadScene("Login");
            }

            void StartLogin()
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                //StartCoroutine(GetGameData(GPGSAuchenticate));
                //StartCoroutine(DLZip(GPGSAuchenticate));
                Lance.GameData = GameDataReader.ReadGameData("DataSheet");

                LoginGuest();
#endif

#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            var inappUpdateManager = new InAppUpdateManager();
            inappUpdateManager.Init(this);

            StartCoroutine(inappUpdateManager.CheckForUpdate());
#endif
            }
        }

        public IEnumerator DLZip(Action onFinishGameDataRead)
        {
            Lance.TouchBlock.SetActive(true);

            string googldDrivedownloadLink = $"https://drive.google.com/uc?export=download&id={DriveDataSheetId}";

            //string gabiaLink = $"http://theduckgames.gabia.io/{DataSheetName}.zip";

            UnityWebRequest www = UnityWebRequest.Get(googldDrivedownloadLink);

            www.downloadHandler = new DownloadHandlerBuffer();

            www.timeout = 10;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Lance.TouchBlock.SetActive(false);

                Lance.GameData = GameDataReader.ReadGameData("DataSheet");

                mInitGameData = true;

                onFinishGameDataRead?.Invoke();

                //UIUtil.ShowSystemMessage("로컬에서 데이터 읽어옴!");
                //yield return ReadLocalData(onFinishGameDataRead);
            }
            else
            {
                Lance.TouchBlock.SetActive(false);

                if (www.downloadHandler.data == null)
                {
                    UIUtil.ShowSystemMessage("게임 데이터를 불러오는데 실패하였습니다. \n" +
                    "재접속을 시도합니다.");

                    yield return new WaitForSeconds(3f);

                    SceneManager.LoadScene("Login");
                }
                else
                {
                    //File.WriteAllBytes($"{Application.persistentDataPath}/DataSheet.zip", www.downloadHandler.data);

                    Zip.DecompressArchive(www.downloadHandler.data, $"{Application.persistentDataPath}/");

                    var result = File.ReadAllBytes($"{Application.persistentDataPath}/DataSheet.txt");

                    GameData gameData = new GameData();

                    gameData = JsonUtil.Base64FromJsonNewton<GameData>(Encoding.Default.GetString(result));

                    Lance.GameData = gameData;

                    //Zip.DecompressArchive()

                    //var result = Zip.Decompress(www.downloadHandler.data);//, $"{Application.persistentDataPath}/");

                    //var result = File.ReadAllBytes($"{Application.persistentDataPath}/DataSheet.txt");

                    //GameData gameData = new GameData();

                    //gameData = JsonUtil.Base64FromJsonNewton<GameData>(Encoding.Default.GetString(result));

                    //Lance.GameData = gameData;

                    mInitGameData = true;

                    onFinishGameDataRead?.Invoke();

                    //UIUtil.ShowSystemMessage("서버에서 데이터 읽어옴!");
                }
            }

            www.Dispose();

            yield return null;
        }

        IEnumerator ReadLocalData(Action onFinishGameDataRead)
        {
            string fileName = "DataSheet.zip";
            string filePath = string.Empty;
#if UNITY_EDITOR || UNITY_STANDALONE
            filePath = Path.Combine(Application.streamingAssetsPath, fileName);
#elif UNITY_ANDROID
			filePath = Path.Combine("jar:file://" + Application.dataPath + "!/assets/", fileName);
#elif UNITY_IOS
            //filePath = Path.Combine(Application.streamingAssetsPath, fileName);
#else
            filePath = null;
#endif
            UnityWebRequest www2 = UnityWebRequest.Get(filePath);

            yield return www2.SendWebRequest();

            if (www2.result != UnityWebRequest.Result.Success)
            {
                UIUtil.ShowSystemMessage("게임 데이터를 불러오는데 실패하였습니다. \n" +
                    "재접속을 시도합니다.");

                yield return new WaitForSeconds(3f);

                SceneManager.LoadScene("Login");
            }
            else
            {
                Zip.DecompressArchive(www2.downloadHandler.data, $"{Application.persistentDataPath}/");

                var result2 = File.ReadAllBytes($"{Application.persistentDataPath}/DataSheet.txt");

                GameData gameData2 = new GameData();

                gameData2 = JsonUtil.Base64FromJsonNewton<GameData>(Encoding.Default.GetString(result2));

                Lance.GameData = gameData2;

                mInitGameData = true;

                onFinishGameDataRead?.Invoke();

                Lance.TouchBlock.SetActive(false);
            }

            www2.Dispose();
        }

        //IEnumerator GetGameData(Action onFinishGameDataRead)
        //{
        //    Lance.TouchBlock.SetActive(true);

        //    UnityWebRequest www = UnityWebRequest.Get($"http://theduckgames.gabia.io/{DataSheetName}.txt");

        //    www.timeout = 10;

        //    yield return www.SendWebRequest();

        //    if (www.result != UnityWebRequest.Result.Success)
        //    {
        //        // 로컬 데이터 그냥 불러와주자
        //        yield return ReadLocalData(onFinishGameDataRead);
        //    }
        //    else
        //    {
        //        if (www.downloadHandler == null)
        //        {
        //            UIUtil.ShowSystemMessage("게임 데이터를 불러오는데 실패하였습니다. \n" +
        //            "재접속을 시도합니다.");

        //            Debug.LogError($"다운로드 핸들러 : {www.downloadHandler}");

        //            yield return new WaitForSeconds(3f);

        //            SceneManager.LoadScene("Login");
        //        }
        //        else
        //        {
        //            Lance.TouchBlock.SetActive(false);

        //            GameData gameData = new GameData();

        //            var tableList = new TableList(www.downloadHandler.data);

        //            tableList.MapInto(gameData);

        //            Lance.GameData = gameData;

        //            //string text = Encoding.Default.GetString(www.downloadHandler.data);

        //            //GameData gameData = new GameData();

        //            //gameData = JsonUtil.Base64FromJsonNewton<GameData>(text);

        //            //Lance.GameData = gameData;

        //            mInitGameData = true;

        //            onFinishGameDataRead?.Invoke();
        //        }
        //    }
        //}

        private void Update()
        {
            // 뒤로가기 버튼을 눌렀을 때 동작
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Lance.PopupManager.Count > 0)
                {
                    Lance.PopupManager.OnBackButton();
                }
                else
                {
                    UIUtil.ShowConfirmPopup("확인", "게임을 종료하겠습니까?", OnConfirm, null);

                    void OnConfirm()
                    {
#if UNITY_EDITOR
                        EditorApplication.ExitPlaymode();
#else
                        Application.Quit();
#endif
                    }
                }
            }
        }

        void InitFirebase()
        {
            Lance.Firebase = new FirebaseSetup();
            Lance.Firebase.Init();
        }

        void OnButtonLoginGoogle()
        {
            if (mUpdateAvailable)
            {
                bool isKorean = Lance.Account?.IsKorean ?? false;
                string title = isKorean ? "오류" : "Error";
                string desc = isKorean ? "업데이트를 실패하였습니다.\n" +
                    "업데이트를 위해 재로그인을 시도합니다." : "The update failed.\n" +
                    "Attempt to re-login for updates.";

                var popup = UIUtil.ShowConfirmPopup(title, desc, null, null, ignoreBackButton: true);
                popup.SetHideCancelButton();
                popup.SetOnCloseAction(OnConfirm);

                void OnConfirm()
                {
                    SceneManager.LoadScene("Login");
                }

                return;
            }

            if (mInitGameData == false)
            {
                bool isKorean = Lance.Account?.IsKorean ?? false;
                string title = isKorean ? "오류" : "Error";
                string desc = isKorean ? "게임 데이터를 불러오는데 실패하였습니다.\n" +
                    "재접속을 시도합니다." : "Failed to load game data.\n" +
                    "Attempt to reconnect.";

                var popup = UIUtil.ShowConfirmPopup(title, desc, null, null, ignoreBackButton: true);
                popup.SetHideCancelButton();
                popup.SetOnCloseAction(OnConfirm);

                void OnConfirm()
                {
                    SceneManager.LoadScene("Login");
                }

                return;
            }

            if (mLoginStart)
                return;

            mLoginStart = true;

            mButtonsObj.SetActive(false);

            PlayGamesPlatform.Instance.ManuallyAuthenticate((status) =>
            {
                if (status == SignInStatus.Success)
                {
                    LoginGPGS();
                }
                else
                {
                    mButtonsObj.SetActive(true);

                    mLoginStart = false;
                }
            });
        }

        void OnButtonLoginGuest()
        {
            if (mUpdateAvailable)
            {
                bool isKorean = Lance.Account?.IsKorean ?? false;
                string title = isKorean ? "오류" : "Error";
                string desc = isKorean ? "업데이트를 실패하였습니다.\n" +
                    "업데이트를 위해 재로그인을 시도합니다." : "The update failed.\n" +
                    "Attempt to re-login for updates.";

                var popup = UIUtil.ShowConfirmPopup(title, desc, null, null, ignoreBackButton: true);
                popup.SetHideCancelButton();
                popup.SetOnCloseAction(OnConfirm);

                void OnConfirm()
                {
                    SceneManager.LoadScene("Login");
                }

                return;
            }

            if (mInitGameData == false)
            {
                bool isKorean = Lance.Account?.IsKorean ?? false;
                string title = isKorean ? "오류" : "Error";
                string desc = isKorean ? "게임 데이터를 불러오는데 실패하였습니다.\n" +
                    "재접속을 시도합니다." : "Failed to load game data.\n" +
                    "Attempt to reconnect.";

                var popup = UIUtil.ShowConfirmPopup(title, desc, null, null, ignoreBackButton: true);
                popup.SetHideCancelButton();
                popup.SetOnCloseAction(OnConfirm);

                void OnConfirm()
                {
                    SceneManager.LoadScene("Login");
                }

                return;
            }

            mLoginStart = true;

            mButtonsObj.SetActive(false);

            // 경고 팝업
            if (Backend.BMember.GetGuestID().IsValid() == false)
            {
                string title = StringTableUtil.Get("Title_Caution");
                string desc = StringTableUtil.Get("Desc_GuestLoginCaution");

                var popup = UIUtil.ShowConfirmPopup(title, desc, null, null);
                popup.SetHideCancelButton();
                popup.SetOnCloseAction(LoginGuest);
            }
            else
            {
                LoginGuest();
            }
        }

        void GPGSAuchenticate()
        {
            mButtonsObj.SetActive(false);

            PlayGamesPlatform.Instance.Authenticate((status) =>
            {
                if (status == SignInStatus.Success)
                {
                    LoginGPGS();
                }
                else
                {
                    if (Backend.BMember.GetGuestID().IsValid())
                    {
                        LoginGuest();
                    }
                    else
                    {
                        mButtonsObj.SetActive(true);

                        mLoginStart = false;
                    }
                }
            });
        }

        void LoginGPGS()
        {
            try
            {
                PlayGamesPlatform.Instance.RequestServerSideAccess(
                /* forceRefreshToken= */ false,
                code =>
                {
                    Debug.Log("구글 인증 코드 : " + code);

                    Backend.BMember.GetGPGS2AccessToken(code, googleCallback =>
                    {
                        Debug.Log("GetGPGS2AccessToken 함수 호출 결과 " + googleCallback);

                        string accessToken = "";

                        if (googleCallback.IsSuccess())
                        {
                            accessToken = googleCallback.GetReturnValuetoJSON()["access_token"].ToString();

                            Backend.BMember.AuthorizeFederation(accessToken, FederationType.GPGS2, callback =>
                            {
                                ResultBackendLogin(callback);
                            });
                        }
                        else
                        {
                            string title = StringTableUtil.Get("Title_Error");
                            string desc = StringTableUtil.Get("Desc_GoogleLoginFailed");

                            var popup = UIUtil.ShowConfirmPopup(title, desc, null, null);
                            popup.SetHideCancelButton();

                            mButtonsObj.SetActive(true);
                        }

                        mLoginStart = false;
                    });
                });
            }
            catch
            {
                string title = StringTableUtil.Get("Title_Error");
                string desc = StringTableUtil.Get("Desc_NetworkError");

                var popup = UIUtil.ShowConfirmPopup(title, desc, null, null);
                popup.SetHideCancelButton();

                mButtonsObj.SetActive(true);

                mLoginStart = false;
            }
        }

        void LoginGuest()
        {
            SendQueue.Enqueue(Backend.BMember.GuestLogin, callback =>
            {
                ResultBackendLogin(callback);
            });
        }

        void ResultBackendLogin(BackendReturnObject callback)
        {
            if (callback.IsSuccess())
            {
                // 닉네임이 없을 경우
                if (string.IsNullOrEmpty(Backend.UserNickName))
                {
                    // 닉네임 입력 팝업
                    var popup = Lance.PopupManager.CreatePopup<Popup_NicknameUI>("Popup_NicknameUI");

                    popup.Init(SuccessLogin);
                }
                else
                {
                    SuccessLogin();
                }
            }
            else
            {
                Lance.MainThreadDispatcher.QueueOnMainThread(() =>
                {
                    var status = callback.GetStatusCode();
                    if (status == "400")
                    {
                        // 잘 못된 디바이스 정보 , 디바이스 정보가 null
                        string title = StringTableUtil.Get("Title_Error");
                        string desc = StringTableUtil.Get("Desc_UndefineDevice");

                        var popup = UIUtil.ShowConfirmPopup(title, desc, null, null);
                        popup.SetHideCancelButton();

                    }
                    else if (status == "401")
                    {
                        // 존재하지 않는 아이디
                        // 게스트 계정을 페더레이션 계정으로 변경한 후 게스트 로그인을 시도
                        string title = StringTableUtil.Get("Title_Error");
                        string desc = StringTableUtil.Get("Desc_BadId");

                        var popup = UIUtil.ShowConfirmPopup(title, desc, null, null);
                        popup.SetHideCancelButton();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Backend.BMember.DeleteGuestInfo();
#endif
                    }
                    else if (status == "403")
                    {
                        // 차단당한 계정
                        string title = StringTableUtil.Get("Title_Error");
                        string desc = StringTableUtil.Get("Desc_BadId");

                        var popup = UIUtil.ShowConfirmPopup(title, desc, null, null);
                        popup.SetHideCancelButton();
                    }
                    else if (status == "408")
                    {
                        // 타임 아웃
                        string title = StringTableUtil.Get("Title_Error");
                        string desc = StringTableUtil.Get("Desc_NetworkError");

                        var popup = UIUtil.ShowConfirmPopup(title, desc, null, null);
                        popup.SetHideCancelButton();
                    }

                    mButtonsObj.SetActive(true);
                });
            }

            mLoginStart = false;

            Lance.TouchBlock.SetActive(false);
        }

        void SuccessLogin()
        {
            Lance.Account = new Account();
            Lance.Account.Notice.GetNoticeList(() =>
            {
                var noticeList = Lance.Account.Notice.GetNoticeList();
                if (noticeList != null)
                {
                    var popup = Lance.PopupManager.CreatePopup<Popup_NoticeUI>();
                    popup.Init(noticeList[0]);

                    popup.SetOnCloseAction(StartLoading);
                }
                else
                {
                    StartLoading();
                }
            });
        }

        void StartLoading()
        {
            Lance.LocalSave.LastLoginDateNum = TimeUtil.NowDateNum();

            ReadUserInfos();

            SyncServerTime();

            mStartLoadingObj.SetActive(true);

            mImageModal.DOFade(1f, 0.25f)
                .OnComplete(() =>
                {
                    DOTween.To(() => mLoadingGroup.alpha, (f) => mLoadingGroup.alpha = f, 1f, 0.25f);
                });
        }

        void ReadUserInfos()
        {
            ReadTransaction();
            ReadMyCountryCode();
            ReadMyGroup();
            ReadRankInfos();
            ReadPost();

            if (mAllowPush)
            {
                mLoadingActions.Enqueue(() => SetPush());

                void SetPush()
                {
                    Lance.Firebase.SetPush(true, (success) =>
                    {
                        OnNextStep(success, string.Empty, string.Empty, string.Empty);

                    }, ignoreTouchBlock: true);
                }
            }
        }

        void SyncServerTime()
        {
            mLoadingActions.Enqueue(() =>
            {
                string url = Lance.GameData.CommonData.timeCheatingDetectUrl;

                StartCoroutine(TimeCheatingDetector.GetOnlineTimeCoroutine(url, (result) =>
                {
                    if (result.Success)
                        TimeUtil.SyncTo(result.OnlineDateTimeUtc);

                    OnNextStep(true, string.Empty, string.Empty, string.Empty);
                }));
            });
        }

        void OnNextStep(bool isSuccess, string className, string funcName, string errorInfo)
        {
            if (isSuccess)
            {
                if (mLoadingActions.Count > 0)
                {
                    mLoadingActions.Dequeue().Invoke();
                }
                else
                {
                    // 아직 처리중이라면 무시한다.
                    if (SendQueue.UnprocessedFuncCount > 0)
                        return;

                    Lance.BackEnd.InitChatting();

                    // 모든 로딩이 완료되었다 게임을 시작하자
                    SceneManager.LoadSceneAsync("Main");
                }
            }
            else
            {
                if (errorInfo.IsValid())
                {
                    Debug.LogError(errorInfo);

                    Lance.BackEnd.SendBugReport(className, funcName, errorInfo);
                }

                SceneManager.LoadScene("Login");
            }
        }

        // 트랜잭션 읽기 호출 함수
        void ReadTransaction()
        {
            bool isSuccess = false;
            string className = GetType().Name;
            string functionName = MethodBase.GetCurrentMethod()?.Name;
            string errorInfo = string.Empty;

            var accountInfos = Lance.Account.AccountInfos;

            int sendCount = 0;
            int finishCount = 0;

            if (accountInfos.Count > 10)
            {
                List<AccountBase> temp = new List<AccountBase>();

                var values = Lance.Account.AccountInfos.Values;

                foreach (var info in values)
                {
                    temp.Add(info);

                    if (temp.Count == 10)
                    {
                        SendTransaction(temp.ToArray());

                        temp.Clear();
                    }
                }

                if (temp.Count > 0)
                {
                    SendTransaction(temp.ToArray());
                }
            }
            else
            {
                SendTransaction(accountInfos.Values.ToArray());
            }

            void SendTransaction(AccountBase[] accountInfos)
            {
                sendCount++;
                //트랜잭션 리스트 생성
                List<TransactionValue> transactionList = new List<TransactionValue>();

                // 게임 테이블 데이터만큼 트랜잭션 불러오기
                foreach (var info in accountInfos)
                {
                    transactionList.Add(info.GetTransactionGetValue());
                }

                // [뒤끝] 트랜잭션 읽기 함수
                SendQueue.Enqueue(Backend.GameData.TransactionReadV2, transactionList, callback =>
                {
                    try
                    {
                        Debug.Log($"Backend.GameData.TransactionReadV2 : {callback}");

                        // 데이터를 모두 불러왔을 경우
                        if (callback.IsSuccess())
                        {
                            JsonData jsonData = callback.GetFlattenJSON()["Responses"];

                            int index = 0;

                            foreach (var info in accountInfos)
                            {
                                mLoadingActions.Enqueue(() =>
                                {
                                    info.BackendGameDataLoadByTransaction(jsonData[index++], OnNextStep);
                                });
                            }

                            isSuccess = true;
                        }
                        else
                        {
                            // 트랜잭션으로 데이터를 찾지 못하여 에러가 발생한다면 개별로 GetMyData로 호출
                            foreach (var info in accountInfos)
                            {
                                mLoadingActions.Enqueue(() =>
                                {
                                    info.BackendGameDataLoad(OnNextStep);
                                });
                            }

                            isSuccess = true;
                        }
                    }
                    catch (Exception e)
                    {
                        errorInfo = e.ToString();
                    }
                    finally
                    {
                        finishCount++;
                        if (sendCount == finishCount)
                        {
                            OnNextStep(isSuccess, className, functionName, errorInfo);
                        }
                    }
                });
            }
        }

        void ReadRankInfos()
        {
            mLoadingActions.Enqueue(() => Lance.Account.Leaderboard.BackendLoad(OnNextStep));
        }

        void ReadPost()
        {
            mLoadingActions.Enqueue(() => Lance.Account.Post.BackendLoad(OnNextStep));
            mLoadingActions.Enqueue(() => Lance.Account.Post.BackendLoadForRank(OnNextStep));
        }

        void ReadMyCountryCode()
        {
            mLoadingActions.Enqueue(() =>
            {
                SendQueue.Enqueue(Backend.BMember.GetMyCountryCode, (callback) =>
                {
                    string errorInfo = string.Empty;
                    string countryCode = string.Empty;

                    try
                    {
                        if (callback.IsSuccess())
                        {
                            countryCode = callback.GetReturnValuetoJSON()["country"]["S"].ToString();
                        }
                    }
                    catch (Exception e)
                    {
                        errorInfo = e.ToString();
                    }
                    finally
                    {
                        if (countryCode.IsValid())
                        {
                            // 등록되어 있는 국가코드가 있다면 캐싱
                            Lance.Account.CountryCode = countryCode;

                            OnNextStep(true, "GetMyCountryCode", "ReadMyCountryCode", errorInfo);
                        }
                        else
                        {
                            // 현재 위치를 기준으로 국가 코드를 획득하고 서버에 등록
                            SendQueue.Enqueue(Backend.LocationProperties.UpdateLocationProperties, callback =>
                            {
                                string errorInfo = string.Empty;

                                try
                                {
                                    if (callback.IsSuccess())
                                    {
                                        if (Backend.LocationProperties.Country.IsValid())
                                        {
                                            var countryCode = CountryNameChangeToCode(Backend.LocationProperties.Country);
                                            if (countryCode.IsValid())
                                            {
                                                Lance.Account.CountryCode = countryCode;

                                                Lance.LocalSave.LangCode = Lance.Account.IsKorean ? LangCode.KR : LangCode.US;
                                                Lance.LocalSave.Save();

                                                RrefreshStr();
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    errorInfo = e.ToString();
                                }
                                finally
                                {
                                    if (Lance.Account.CountryCode.IsValid())
                                    {
                                        // 국가코드로 변환
                                        CountryCode countryCode = CountryCodeDic.GetCountryName(Lance.Account.CountryCode);

                                        // 국가 코드 등록
                                        SendQueue.Enqueue(Backend.BMember.UpdateCountryCode, countryCode, callback => { });
                                    }

                                    OnNextStep(true, "UpdateLocationProperties", "ReadLocalCountry", errorInfo);
                                }
                            });
                        }
                    }
                });
            });
        }

        // 서버 그룹 목록을 불러오자
        void ReadMyGroup()
        {
            mLoadingActions.Enqueue(() =>
            {
                Backend.Group.Get(bro =>
                {
                    try
                    {
                        if (bro.IsSuccess())
                        {
                            GroupItem groupItem = bro.GetGroup();
                            if (groupItem != null)
                            {
                                Lance.Account.GroupUuid = groupItem.groupUuid;
                                Lance.Account.GroupName = groupItem.groupName;
                            }
                        }

                        // 등록된 그룹 정보가 없다면
                        if (Lance.Account.GroupUuid.IsValid() == false || Lance.Account.GroupName.IsValid() == false)
                        {
                            // 나의 현재 국가 정보를 가지고 그룹을 임의로 등록해주자
                            // *** 그룹 리스트를 불러올 수 있지만 결국 문자열을 비교해서 구분해줘야한다.
                            // 하드 코딩으로 그냥 처리해주자

                            // 한국 그룹
                            if (Lance.Account.IsKorean)
                            {
                                Lance.Account.GroupUuid = KoreaGroupUuid;
                                Lance.Account.GroupName = KoreaGroupName;
                            }
                            // 글로벌 그룹
                            else
                            {
                                Lance.Account.GroupUuid = GlobalGroupUuid;
                                Lance.Account.GroupName = GlobalGroupName;
                            }

                            // 그룹 등록
                            Backend.Group.Update(Lance.Account.GroupUuid, Lance.Account.GroupName, bro =>
                            {

                                string errorInfo = string.Empty;
                                try
                                {
                                    OnNextStep(bro.IsSuccess(), "", "ReadMyGroupAndUpdateMyGroup", errorInfo);
                                }
                                catch (Exception e)
                                {
                                    errorInfo = e.ToString();
                                }
                            });
                        }
                        else
                        {
                            OnNextStep(bro.IsSuccess(), "", "ReadMyGroup", string.Empty);
                        }
                    }
                    catch (Exception e)
                    {
                        OnNextStep(false, "", "ReadMyGroup", e.ToString());
                    }
                });
            });
        }

        string CountryNameChangeToCode(string name)
        {
            if (name == "Korea, Republic of" || name == "South Korea")
            {
                return "KR"; // 한국
            }
            else if (name == "United States of America" || name == "United States")
            {
                return "US"; // 미국
            }
            else if (name == "Canada")
            {
                return "CA"; // 캐나다
            }
            else if (name == "China")
            {
                return "CN"; // 중국
            }
            else if (name == "Hong Kong")
            {
                return "HK"; // 홍콩
            }
            else if (name == "Taiwan, Province of China" ||
                name == "Taiwan")
            {
                return "TW"; // 대만
            }
            else if (name == "Thailand")
            {
                return "TH"; // 태국
            }
            else if (name == "Indonesia")
            {
                return "ID"; // 인도네시아
            }
            else if (name == "Germany" ||
                name == "Deutschland" ||
                name == "Germany, Federal Republic of")
            {
                return "DE"; // 독일
            }
            else if (name == "France") //|| name == "Includes Clipperton Island")
            {
                return "FR"; // 프랑스
            }
            //else if (name == "United Kingdom of Great Britain and Northern Ireland" ||
            //    name == "United Kingdom of Great Britain and Northern Ireland" ||
            //    name == "United Kingdom")
            //{
            //    return "GB"; // 영국
            //}
            else
            {
                return "GB"; // 영국
            }
        }

        void RrefreshStr()
        {
            bool isKorean = Lance.LocalSave.LangCode == LangCode.KR;

            mTextGoogleLogin.text = isKorean ? "구글 계정 로그인" : "Google Login";
            mTextGuestLogin.text = isKorean ? "게스트 로그인" : "Guest Login";

            var imageTitle = gameObject.FindComponent<Image>("Image_TitleLogo");
            var imageTitleEng = gameObject.FindComponent<Image>("Image_TitleLogo_Eng");

            imageTitle.gameObject.SetActive(isKorean);
            imageTitleEng.gameObject.SetActive(!isKorean);

            var textWarning = gameObject.FindComponent<TextMeshProUGUI>("Text_Warning");
            textWarning.text = isKorean ? "계정을 저장하지 않고 게임을 <color=#FD4F29>강제 종료</color> 하는 경우 <color=#FD4F29>계정 정보가 손실</color> 될 수 있습니다.\n" +
                "반드시 <color=#FD4F29>계정 저장 후 게임을 종료</color>해주세요." : "If you <color=#FD4F29>force quit</color> the game without saving your account, \n<color=#FD4F29>account information may be lost</color>.\n" +
                "Please make sure to<color=#FD4F29>save your account before exiting the game</color>.";
        }


        public static bool IsRooted()
        {
            bool isRoot = false;

            if (Application.platform == RuntimePlatform.Android)
            {
                if (IsRootedPrivate("/system/bin/su"))
                    isRoot = true;
                if (IsRootedPrivate("/system/xbin/su"))
                    isRoot = true;
                if (IsRootedPrivate("/system/app/SuperUser.apk"))
                    isRoot = true;
                if (IsRootedPrivate("/data/data/com.noshufou.android.su"))
                    isRoot = true;
                if (IsRootedPrivate("/sbin/su"))
                    isRoot = true;
            }
            return isRoot;
        }

        public static bool IsRootedPrivate(string path)
        {
            bool boolTemp = false;

            if (File.Exists(path))
            {
                boolTemp = true;
            }

            return boolTemp;
        }
    }
}
