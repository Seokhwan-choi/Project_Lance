using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BackEnd;
using UnityEngine.SceneManagement;
//using System.Globalization;
//using System.Threading;
using System.Linq;
using BackEndChat;
using BackEnd.GlobalSupport;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Lance
{
    class BackendManager2 : MonoBehaviour
    {
        bool mIsCheater;
        bool mIsErrorOccured; // 치명적인 에러 발생 여부 
        ChattingManager mChatting;
        public ChattingManager ChattingManager => mChatting;
        // 뒤끝 매니저 초기화 함수
        public void Init(Action<bool> callBack)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                try
                {
                    //mCultureInfo = Thread.CurrentThread.CurrentCulture;

                    Backend.InitializeAsync((bro) =>
                    {
                        if (bro.IsSuccess())
                        {
                            Debug.Log("뒤끝 초기화가 완료되었습니다.");

                            CreateSendQueueMgr();

                            SetErrorHandler();
                            //string hash = Backend.Utils.GetGoogleHash();

                            //Debug.Log("구글 해시 키 : " + hash);

                            callBack?.Invoke(true);
                        }
                        else
                        {
                            string title = "NetworkError";
                            string desc = "인터넷 연결 상태가 원활하지 않습니다.\n 잠시 후 재접속 해주세요.";

                            var popup = UIUtil.ShowConfirmPopup(title, desc, () => { callBack?.Invoke(false); }, null);
                            popup.SetHideCancelButton();
                        }
                    });
                }
                catch
                {
                    string title = "NetworkError";
                    string desc = "인터넷 연결 상태가 원활하지 않습니다.\n 잠시 후 재접속 해주세요.";

                    var popup = UIUtil.ShowConfirmPopup(title, desc, () => { callBack?.Invoke(false); }, null);
                    popup.SetHideCancelButton();
                }
            }
            else
            {
                string title = "NetworkError";
                string desc = "인터넷 연결 상태가 원활하지 않습니다.\n 잠시 후 재접속 해주세요.";

                var popup = UIUtil.ShowConfirmPopup(title, desc, () => { callBack?.Invoke(false); }, null);
                popup.SetHideCancelButton();
            }
        }

        public void InitChatting()
        {
            mChatting = new ChattingManager();
            mChatting.Init();
        }

        //비동기 함수를 메인쓰레드로 보내어 UI에 용이하게 접근하도록 도와주는 Poll 함수
        void Update()
        {
            mChatting?.OnUpate();
        }

        private void OnDestroy()
        {
            mChatting?.OnObjectDestory();
        }

        private void OnApplicationQuit()
        {
            mChatting?.OnAppQuit();
        }

        // 모든 뒤끝 함수에서 에러 발생 시, 각 에러에 따라 호출해주는 핸들러
        void SetErrorHandler()
        {
            // 서버 점검 에러 발생 시
            Backend.ErrorHandler.OnMaintenanceError = () => {
                UIUtil.ShowConfirmPopup("서버 점검 중", "현재 서버 점검중입니다.\n 자세한 내용은 네이버 라운지를 통해 확인해주세요.", () => { Application.OpenURL("https://game.naver.com/lounge/lance/home"); }, null);
            };
            // 403 에러 발생시
            Backend.ErrorHandler.OnTooManyRequestError = () => {
                UIUtil.ShowConfirmPopup("비정상적인 행동 감지", "비정상적인 행동이 감지되었습니다.\n타이틀로 돌아갑니다.", () => { SceneManager.LoadScene("Login"); }, null);
            };
            // 액세스토큰 만료 후 리프레시 토큰 실패 시
            Backend.ErrorHandler.OnOtherDeviceLoginDetectedError = () => {
                UIUtil.ShowConfirmPopup("비정상적인 행동 감지", "비정상적인 행동이 감지되었습니다.\n타이틀로 돌아갑니다.", () => { SceneManager.LoadScene("Login"); }, null);
            };
        }

        //SendQueue를 관리해주는 SendQueue 매니저 생성
        void CreateSendQueueMgr()
        {
            var obj = new GameObject();
            obj.name = "SendQueueManager";
            obj.transform.SetParent(this.transform);
            obj.AddComponent<SendQueueManager>();
        }

        // 일정주기마다 데이터를 저장/불러오는 코루틴 시작(인게임 시작 시)
        public void StartAutoUpdate()
        {
            mIsErrorOccured = true;

            StartCoroutine(UpdateGameDataTransaction());
            StartCoroutine(UpdateRanks());
            StartCoroutine(GetAdminPostList());
            StartCoroutine(DetectCheater());
        }

        // 호출 시, 코루틴 내 함수들의 동작을 멈추게 하는 함수
        public void StopUpdate()
        {
            Debug.Log("자동 저장을 중지합니다.");

            mIsErrorOccured = false;
        }

        IEnumerator DetectCheater()
        {
            var seconds = new WaitForSecondsRealtime(300);

            yield return seconds;

            while (mIsErrorOccured)
            {
                if (mIsCheater)
                {
                    UIUtil.ShowSystemErrorMessage("DetectCheater");

                    yield return new WaitForSecondsRealtime(5f);

                    Application.Quit();
                }

                yield return seconds;
            }
        }

        // 약 20분에 한번씩 보내보자
        // 슬립 모드일때는 60분 정도로 바꾸자
        IEnumerator UpdateGameDataTransaction()
        {
            var seconds = new WaitForSecondsRealtime(1200);
            var seconds_SleepMode = new WaitForSecondsRealtime(3600);

            yield return seconds;

            while (mIsErrorOccured)
            {
                UpdateAllAccountInfos();

                if (Lance.GameManager.IsSleepMode() == false)
                {
                    yield return seconds;
                }
                else
                {
                    yield return seconds_SleepMode;
                }
            }
        }

        // 업데이트가 발생한 이후에 호출에 대한 응답을 반환해주는 대리자 함수
        public delegate void AfterUpdateFunc(BackendReturnObject callback);

        // 값이 바뀐 데이터가 있는지 체크후 바뀐 데이터들은 바로 저장 혹은 트랜잭션에 묶어 저장을 진행하는 함수
        public void UpdateAllAccountInfos(AfterUpdateFunc onFinishUpdate = null)
        {
            string updateTableName = string.Empty;

            // 바뀐 데이터가 몇개 있는지 체크
            List<AccountBase> accountInfos = new List<AccountBase>();

            foreach (var info in Lance.Account.AccountInfos)
            {
                if (info.Value.IsChangedData)
                {
                    accountInfos.Add(info.Value);
                }
            }

            if (accountInfos.Count <= 0)
            {
                onFinishUpdate?.Invoke(null);

                return;
            }

            // 딱 하나만 바뀌었다.
            if (accountInfos.Count == 1)
            {
                AccountBase info = accountInfos[0];

                updateTableName += info.GetTableName() + "\n";

                info.Update(callback => {

                    // 성공할경우 데이터 변경 여부를 false로 변경
                    if (callback.IsSuccess())
                    {
                        info.SetIsChangedData(false);
                    }
                    else
                    {
                        SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), callback.ToString() + "\n" + updateTableName);
                    }
                    Debug.Log($"UpdateV2 : {callback}\n업데이트 테이블 : \n{updateTableName}");

                    onFinishUpdate.Invoke(callback);
                });
            }
            // 여러개 바뀌었다.
            else
            {
                int sendCount = 0;
                int finishCount = 0;
                // 최대 10개까지만 한번의 트랜잭션으로 보낼 수 있기 때문에
                // 10개 이상의 경우 10개 단위로 끊어서 작업한다.
                if (accountInfos.Count < 10)
                {
                    SendTransaction(accountInfos);
                }
                else
                {
                    List<AccountBase> temp = new List<AccountBase>();

                    foreach (var info in accountInfos)
                    {
                        temp.Add(info);

                        if (temp.Count == 10)
                        {
                            SendTransaction(temp);

                            temp.Clear();
                        }
                    }

                    if (temp.Count > 0)
                    {
                        SendTransaction(temp);
                    }
                }

                void SendTransaction(List<AccountBase> accountInfos)
                {
                    sendCount++;

                    string updateTableName = string.Empty;
                    // 2개 이상이라면 트랜잭션에 묶어서 업데이트
                    List<TransactionValue> transactionList = new List<TransactionValue>();

                    // 변경된 데이터만큼 트랜잭션 추가
                    foreach (var info in accountInfos)
                    {
                        updateTableName += info.GetTableName() + "\n";

                        transactionList.Add(info.GetTransactionUpdateValue());
                    }

                    SendQueue.Enqueue(Backend.GameData.TransactionWriteV2, transactionList, callback => 
                     {
                        //Debug.Log($"Backend.BMember.TransactionWriteV2 : {callback}");

                        if (callback.IsSuccess())
                        {
                            foreach (var info in accountInfos)
                            {
                                info.SetIsChangedData(false);
                            }
                        }
                        else
                        {
                            SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), callback.ToString() + "\n" + updateTableName);
                        }

                        //Debug.Log($"TransactionWriteV2 : {callback}\n업데이트 테이블 : \n{updateTableName}");

                        finishCount++;

                        if (sendCount <= finishCount)
                        {
                            onFinishUpdate?.Invoke(callback);
                        }
                    });
                }
            }
        }

        // 일정 주기마다 랭킹 데이터 업데이트 호출
        private IEnumerator UpdateRanks()
        {
            var seconds = new WaitForSecondsRealtime(3600);

            yield return seconds;

            // 에러 발생시 true가 될때까지
            while (mIsErrorOccured)
            {
                UpdateUserRankScores();

                yield return seconds;
            }

        }

        public static bool IsCountedRank()
        {
            // 한국 시간으로 새벽4시 ~ 새벽5시 사이에는 랭킹이 정산중이기 때문에 갱신이 불가능
            // 한국 시간은 UTC + 9
            DateTime dateTime = TimeUtil.UtcNow.AddHours(9);

            return dateTime.Hour >= 4 && dateTime.Hour < 5;
        }

        public void UpdateUserRankScores()
        {
#if UNITY_EDITOR
            //return;
#endif
            foreach (var info in Lance.Account.Leaderboard.GetLeaderboardInfos())
            {
                UpdateUserRankScores(info.Uuid, null);
            }
        }

        //bool mIsCounted;         // 랭킹 정산 중
        //bool mIsCounterJousting; // 마상 시합 정산 중
        public void UpdateUserRankScores(string uuid, AfterUpdateFunc afterUpdateFunc)
        {
#if UNITY_EDITOR
            //afterUpdateFunc?.Invoke(null);

            //return;
#endif
            if (IsCountedRank())
            {
                // 로컬에 저장된 값도 0으로 바꿔주자
                Lance.Account.Dungeon.SetRaidBossDamage(0);
                Lance.Account.NewBeginnerRaidScore.SetRaidBossDamage(0);

                var nowDayOfWeek = TimeUtil.NowDayOfWeek();
                if (nowDayOfWeek == DayOfWeek.Monday)
                {
                    Lance.Account.Currency.SetJoustingTicket(0);

                    // 로컬에 저장된 값도 0으로 바꿔주고
                    if (ContentsLockUtil.IsLockContents(ContentsLockType.Jousting) == false)
                    {
                        Lance.Account.JoustRankInfo.SetRankScore(0);

                        string matchUrl = Lance.GameData.JoustingCommonData.matchingUuid_kor;

                        var countryCode = Lance.Account.CountryCode;
                        if (countryCode != "KR")
                            matchUrl = Lance.GameData.JoustingCommonData.matchingUuid_glb;

                        bool ignoreUpdate = false;

                        if (Lance.GameData.RankUpdateIgnoreData != null)
                        {
                            foreach (var ignore in Lance.GameData.RankUpdateIgnoreData.ignores)
                            {
                                if (ignore == Backend.UserNickName)
                                {
                                    ignoreUpdate = true;
                                    break;
                                }
                            }
                        }

                        // 랜덤 조회도 최신화해주자
                        if (ignoreUpdate == false)
                            SendQueue.Enqueue(Backend.RandomInfo.SetRandomData, RandomType.User, matchUrl, Lance.Account.JoustRankInfo.GetRankScore(), (bro) => { });
                    }
                }

                return;
            }

            List<AccountBase> updates = new List<AccountBase>();

            foreach (var accountBase in Lance.Account.AccountInfos.Values)
            {
                if (accountBase.CanUpdateRankScore())
                    updates.Add(accountBase);
            }

            if (Lance.GameData.RankUpdateIgnoreData.ignores != null)
            {
                for (int i = 0; i < Lance.GameData.RankUpdateIgnoreData.ignores.Length; ++i)
                {
                    string ignoreNickName = Lance.GameData.RankUpdateIgnoreData.ignores[i];

                    if (ignoreNickName == Backend.UserNickName)
                    {
                        updates.Clear();
                        break;
                    }
                }
            }

            foreach (LeaderboardInfo info in Lance.Account.Leaderboard.GetLeaderboardInfos())
            {
                //업데이트하고자 하는 uuid 존재하는지 확인
                if (info.Uuid.Equals(uuid))
                {
                    // 랭크 리스트에 있는 테이블 이름과 현재 테이블 이름이 있는지 확인하고 존재한다면 해당 게임테이블을 전체 업데이트한다
                    int index = updates.FindIndex(item => item.GetTableName().Equals(info.Table));
                    if (index < 0)
                    {
                        afterUpdateFunc?.Invoke(null);
                    }
                    else
                    {
                        try
                        {
                            Backend.Leaderboard.User.UpdateMyDataAndRefreshLeaderboard(info.Uuid, info.Table, updates[index].InDate.ToString(), updates[index].GetParam(), callback =>
                            {
                                if (callback.IsSuccess())
                                {
                                    afterUpdateFunc?.Invoke(callback);
                                }
                                else
                                {
                                    SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), callback.ToString());
                                }
                            });
                        }
                        catch (Exception e)
                        {
                            SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), e.ToString());
                        }
                    }
                }
            }
        }

        // 일정 주기마다 우편을 불러오는 코루틴 함수
        private IEnumerator GetAdminPostList()
        {
            var seconds = new WaitForSecondsRealtime(3600);
            yield return seconds;

            while (mIsErrorOccured)
            {
                // 현재 post 함수 체크
                int postCount = Lance.Account.Post.GetPostItemCount(PostType.Admin);

                // 랭크보상&쿠폰은 우편함열면서 바로 갱신
                // 관리자우편은 자동으로 일정주기마다 호출하도록 구성
                Lance.Account.Post.GetPostList(PostType.Admin, (success, info) =>
                {
                    if (success)
                    {
                        int newPostCount = Lance.Account.Post.GetPostItemCount(PostType.Admin);

                        //호출하기 전 우편의 갯수와 동일하지 않다면 우편 아이콘 오른쪽에 표시
                        if (postCount != newPostCount)
                        {
                            if (newPostCount > 0)
                            {
                                // 레드닷 보여주자
                                Lance.Lobby.RefreshPostRedDot();
                            }
                        }
                    }
                    else
                    {
                        //에러가 발생할 경우 버그 리포트 발송
                        SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), info);
                    }
                });
                yield return seconds;
            }
        }

        // 에러 발생시 게임로그를 삽입하는 함수
        public void SendBugReport(string className, string functionName, string errorInfo, int repeatCount = 3)
        {
            Param param = new Param();
            param.Add("className", className);
            param.Add("functionName", functionName);
            param.Add("errorPath", errorInfo);

            InsertLog("error", param, 7);
        }

        public void SendCheatDetect(string cheatName)
        {
            if (mIsCheater == false)
            {
                Param param = new Param();
                param.Add("cheatName", cheatName);

                InsertLog("CheatDetect", param, 7);

                mIsCheater = true;
            }
        }

        public void InsertLog(string logType, Param param, int expireDay, int repeatCount = 3)
        {
            // 에러가 실패할 경우 재귀함수를 통해 최대 3번까지 호출을 시도한다.
            if (repeatCount <= 0)
            {
                param = null;

                return;
            }

            // 아직 로그인되지 않을 경우 뒤끝 함수 호출이 불가능하여 UI에 띄운다.
            if (string.IsNullOrEmpty(Backend.UserInDate))
            {
                //StaticManager.UI.AlertUI.SetYetLoginErrorText();
                return;
            }

            // [뒤끝] 로그 삽입 함수
            Backend.GameLog.InsertLogV2(logType, param, expireDay, callback => {
                // 에러가 발생할 경우 재귀
                if (callback.IsSuccess() == false)
                {
                    InsertLog(logType, param, expireDay, repeatCount - 1);
                }
                else
                {
                    param = null;
                }
            });
        }

        public void SendChatMessage(string message)
        {
            mChatting?.SendChatMessage(message);
        }

        public List<MessageInfo> GetCurrentChannelMessageInfos()
        {
            return mChatting?.GetCurrentChannelMessageInfos();
        }

        public void OnSendReport(ulong index, string tag)
        {
            mChatting?.SendReportChat(index, tag, "report", "report");
        }
    }
}


