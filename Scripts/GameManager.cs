using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Detectors;
using BackEnd;
using UnityEngine.SceneManagement;
using System.Linq;
using DG.Tweening;

namespace Lance
{
    class GameManager : MonoBehaviour
    {
        const float TimeUpdateInterval = 1f;
        const int TimeUpdateIntervalForMinute = 60;

        bool mInit;
        bool mOnReceiveOfflineReward;
        int mLastUpdateTime;
        int mLastUpdateDateNum;
        int mLastRankUpdateDateNum;
        float mFirstGuideQuestAutoTime;
        float mTimeUpdateInterval;
        float mRandomizeIntervalTime;
        Coroutine mStageCoroutine;
        StageManager mStageManager;
        SleepModeManager mSleepModeManager;
        
        Transform mCharacterParent;
        RectTransform mHpGaugeParent;
        RectTransform mDamageTextParent;

        RankProfileManager mRankProfileManager;

        PlayerPreview mCostumePreview;
        PlayerPreview mJoustingPlayerPreview;
        PlayerPreview mJoustingOpponentPreview;
        
        public Transform CharacterParent => mCharacterParent;
        public RectTransform HpGaugeParent => mHpGaugeParent;
        public RectTransform DamageTextParent => mDamageTextParent;
        public StageManager StageManager => mStageManager;
        public bool IsInit => mInit;

#if UNITY_EDITOR
        public int TestIndex { get; set; } 
#endif
        public void Init()
        {
            mCharacterParent = GameObject.Find("Characters").transform;
            mHpGaugeParent = GameObject.Find("HpGaugeParent").GetOrAddComponent<RectTransform>();
            mDamageTextParent = GameObject.Find("DamageTextParent").GetComponent<RectTransform>();

            mCostumePreview = GameObject.Find("CostumePreview").GetOrAddComponent<PlayerPreview>();
            mCostumePreview.Init();

            mJoustingPlayerPreview = GameObject.Find("JoustingPlayerPreview").GetOrAddComponent<PlayerPreview>();
            mJoustingPlayerPreview.Init();

            mJoustingOpponentPreview = GameObject.Find("JoustingOpponentPreview").GetOrAddComponent<PlayerPreview>();
            mJoustingOpponentPreview.Init();

            mStageManager = new StageManager();
            mStageManager.Init();

            mSleepModeManager = new SleepModeManager();
            mSleepModeManager.Init();

            mRankProfileManager = new RankProfileManager();
            mRankProfileManager.Init();

            mOnReceiveOfflineReward = false;

            mFirstGuideQuestAutoTime = 3.5f;

#if UNITY_EDITOR
            Application.runInBackground = true;
#endif

            // 필수 공지
            CheckConfirmMaintanceInfo();

            // 오프라인 보상 확인
            CheckOfflineReward();

            // 출석부 확인
            CheckAttendance();

            // 월정액 확인
            AnyCanReceiveMonthlyFeeDailyReward();

            // 로그인 횟수 저장
            if (Lance.Account.UserInfo.UpdateLoginCount())
            {
                CheckQuest(QuestType.Login, Lance.Account.UserInfo.GetLoginCount());
            }

            Lance.BackEnd.StartAutoUpdate();

            StartCoroutine(UpdateUnscaledTimeForSeconds());
            StartCoroutine(UpdateUnscaledTimeForMinute());

            NormalizeUserInfos();

            //Lance.BackEnd.ChattingManager.ReEntryChattinChannel();

            mInit = true;
        }

        void NormalizeUserInfos()
        {
            Lance.Account.UpdatePassRewardValues();

            Lance.Account.Achievement.CheckQuest(QuestType.ClearBountyQuest, Lance.Account.GetQuestTypeValue(QuestType.ClearBountyQuest));

            if (Lance.Account.SpeedMode.InSpeedMode())
            {
                OnChangeSpeedMode();
            }

            bool purchasedRemovedAD = Lance.Account.PackageShop.IsPurchasedRemoveAD() || Lance.IAPManager.IsPurchasedRemoveAd();
            if (purchasedRemovedAD)
            {
                if (Lance.Account.Buff.AnyCanActive())
                {
                    Lance.Account.Buff.OnPurchasedRemoveAd();

                    UpdatePlayerStat();
                }
            }

            if (Lance.Account.UserInfo.GetPowerLevel() <= 0 || Lance.Account.UserInfo.GetBestPowerLevel() <= 0)
            {
                UpdatePlayerStat();

                var leaderboardInfo = Lance.Account.Leaderboard.GetLeaderboardInfo(RankingTab.PowerLevel.ChangeToTableName());

                leaderboardInfo?.GetMyRank((success, rankItem) => { });
            }

            if (Lance.Account.Currency.GetRetroactiveElementalStone() == false)
            {
                int beforeElementalStone = Lance.Account.Currency.GetElementalStone();

                Lance.Account.Currency.RetroactiveElementalStone();

                int afterElementalStone = Lance.Account.Currency.GetElementalStone();

                Param param = new Param();
                param.Add("beforeElementalStone", beforeElementalStone);
                param.Add("afterElementalStone", afterElementalStone);
                param.Add("nowDateNum", TimeUtil.NowDateNum());

                Lance.BackEnd.InsertLog("RetroactiveElementalStone", param, 7);

                // 바로 저장
                Lance.BackEnd.UpdateAllAccountInfos();
            }

            // 이전에 구매했던 기록 소급
            if (Lance.Account.UserInfo.GetStackedPayments() <= 0)
            {
                int totalPayments = Lance.Account.CalcTotalPayments();

                if (totalPayments > 0)
                {
                    Lance.Account.UserInfo.StackPayments(totalPayments);

                    CheckQuest(QuestType.Payments, totalPayments);

                    Lance.Lobby.RefreshEventRedDot();
                    Lance.Lobby.RefreshQuestRedDot();
                }
            }

            GiveLimitBreakSkill();

            if (Lance.Account.GuideQuest.IsReceivedCurrentReward())
            {
                int guideQuestMaxStep = Lance.GameData.GuideData.Max(x => x.Value.step);
                int guideQuestStep = Lance.Account.GuideQuest.GetCurrentStep();

                if (guideQuestMaxStep > guideQuestStep)
                {
                    Lance.Account.GuideQuest.NextStep();

                    Lance.Lobby.RefreshGuideQuestUI();
                }
            }

            try
            {
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

                // 랜덤 조회 최신화
                if (ignoreUpdate == false)
                {
                    if (ContentsLockUtil.IsLockContents(ContentsLockType.Jousting) == false)
                        SendQueue.Enqueue(Backend.RandomInfo.SetRandomData, RandomType.User, matchUrl, Lance.Account.JoustRankInfo.GetRankScore(), (bro) => { });
                    else
                        SendQueue.Enqueue(Backend.RandomInfo.DeleteRandomData, RandomType.User, matchUrl, (bro) => { });
                }
            }
            catch (Exception e)
            {
                Lance.BackEnd.SendBugReport("GameManager", "NormalizeUserInfos", e.ToString());
            }

            Lance.Account.JoustBattleInfo.SetLevel(Lance.Account.ExpLevel.GetLevel());
            Lance.Account.JoustBattleInfo.SetPowerLevel(Lance.Account.UserInfo.GetBestPowerLevel());
            Lance.Account.JoustBattleInfo.SetCostumes(Lance.Account.Costume.GetEquippedCostumeIds());
            Lance.Account.JoustBattleInfo.SetNickName(Backend.UserNickName);
            Lance.Account.JoustBattleInfo.Update(bro => { });

            if (Lance.Account.Excalibur.GetStep() > 0 || Lance.Account.Excalibur.AnyForceUpgraded())
            {
                if (Lance.Account.AncientArtifact.IsAllArtifactMaxLevel() == false)
                {
                    int totalUsedAncientEssence = 0;

                    foreach (var force in Lance.Account.Excalibur.GetExcaliburForces())
                    {
                        totalUsedAncientEssence += force.GetTotalUsedAncientEssence();

                        force.SetLevel(0);
                        force.SetStep(0);
                    }

                    Debug.LogError($"TotalUsedAncientEssence : {totalUsedAncientEssence}");

                    Lance.Account.Excalibur.SetStep(0);
                    Lance.Account.Currency.AddAncientEssence(totalUsedAncientEssence);

                    Lance.Lobby.RefreshAllTab(LobbyTab.Stature);
                }
            }

            if (Lance.Account.Currency.GetRetroactiveGloryToken() == false)
            {
                int beforeGloryToken = Lance.Account.Currency.GetGloryToken();

                if (Lance.Account.Currency.RetroactiveGloryToken())
                {
                    bool purchasedDungeonMonthlyFee = Lance.Account.PackageShop.IsPurchasedMonthlyFee("Package_MonthlyFeeDungeonTicket");

                    int usedJoustingTicketCount = CalcMaxJoustingTicket() - Lance.Account.Currency.GetJoustingTicket();

                    int retroactiveGloryToken = usedJoustingTicketCount * Lance.GameData.JoustingCommonData.resultGloryTokenReward;

                    Lance.Account.Currency.AddGloryToken(retroactiveGloryToken);

                    int afterGloryToken = Lance.Account.Currency.GetGloryToken();

                    Param param = new Param();
                    param.Add("purchasedDungeonMonthlyFee", purchasedDungeonMonthlyFee ? "True" : "False");
                    param.Add("usedJoustingTicketCount", usedJoustingTicketCount);
                    param.Add("beforeGloryToken", beforeGloryToken);
                    param.Add("afterGloryToken", afterGloryToken);
                    param.Add("retroactiveGloryToken", retroactiveGloryToken);
                    param.Add("nowDateNum", TimeUtil.NowDateNum());

                    Lance.BackEnd.InsertLog("RetroactiveGloryToken", param, 7);

                    // 바로 저장
                    Lance.BackEnd.UpdateAllAccountInfos();

                    int CalcMaxJoustingTicket()
                    {
                        // 오전 6시를 기준으로 현재 무슨 요일인지 확인
                        // UTC + 9가 한국 -6하면 오전 6시가 자정으로 인식, 즉 UTC + 3
                        DayOfWeek nowDayOfWeek = TimeUtil.NowDayOfWeek(3);
                        int dayOfWeekInt = 0;

                        if (nowDayOfWeek == DayOfWeek.Sunday)
                        {
                            dayOfWeekInt = 7;
                        }
                        else
                        {
                            dayOfWeekInt = (int)nowDayOfWeek;
                        }

                        return purchasedDungeonMonthlyFee ?
                            dayOfWeekInt * Lance.GameData.JoustingCommonData.dailyFreeTicket + dayOfWeekInt :
                            dayOfWeekInt * Lance.GameData.JoustingCommonData.dailyFreeTicket;
                    }
                }
            }

            if (Lance.Account.NewBeginnerRaidScore.GetMoveToNew() == false)
            {
                Lance.Account.NewBeginnerRaidScore.SetMoveToNew(true);

                for(int i = 0; i < (int)ElementalType.Count; ++i)
                {
                    ElementalType type = (ElementalType)i;

                    double damage = Lance.Account.BeginnerRaidScore.GetRaidBossBestDamage(type);

                    if (damage > 0)
                    {
                        Lance.Account.NewBeginnerRaidScore.UpdateRaidBossDamage(type, damage);
                    }
                }

                double bestDamage = Lance.Account.BeginnerRaidScore.GetRaidBossDamage();
                if (bestDamage > 0)
                {
                    Lance.Account.NewBeginnerRaidScore.SetRaidBossDamage(bestDamage);
                }
            }

            if (ContentsLockUtil.IsLockContents(ContentsLockType.DemonicRealm) == false)
                Lance.Account.Currency.UpdateFreeDemonicRealmStone();

            if (Lance.Account.UserInfo.IsFinishedRaidBadProcess() == false)
            {
                SendQueue.Enqueue(Backend.BMember.GetUserInfo, (callback) =>
                {
                    string uuid = callback.GetReturnValuetoJSON()["row"]["gamerId"].ToString();

                    Lance.Account.UserInfo.SetIsFinishedRaidBadProcess(true);

                    if (Lance.GameData.RaidBadData.ContainsKey(uuid))
                    {
                        var data = Lance.GameData.RaidBadData.TryGet(uuid);
                        if (data != null)
                        {
                            List<PetEquipmentInst> removeList = new List<PetEquipmentInst>();

                            int totalElementalStone = 200 * data.count;
                            // 진화석을 최고 수준 200개를 기준으로 강제 차감
                            Lance.Account.Currency.AddForcedElementalStone(-totalElementalStone);

                            // 불 PetEquipment_16, PetEquipment_17, PetEquipment_18, PetEquipment_19, PetEquipment_20, PetEquipment_21
                            // 물 PetEquipment_37, PetEquipment_38, PetEquipment_39, PetEquipment_40, PetEquipment_41, PetEquipment_42
                            // 풀 PetEquipment_58, PetEquipment_59, PetEquipment_60, PetEquipment_61, PetEquipment_62, PetEquipment_63

                            // 모든 신수 장비 16단계 이상 전부 삭제 처리
                            // 불
                            var remove = Lance.Account.PetFireInventory.RemoveItem("PetEquipment_16");
                            if (remove != null)
                                removeList.Add(remove);

                            var remove2 = Lance.Account.PetFireInventory.RemoveItem("PetEquipment_17");
                            if (remove2 != null)
                                removeList.Add(remove2);

                            var remove3 = Lance.Account.PetFireInventory.RemoveItem("PetEquipment_18");
                            if (remove3 != null)
                                removeList.Add(remove3);

                            var remove4 = Lance.Account.PetFireInventory.RemoveItem("PetEquipment_19");
                            if (remove4 != null)
                                removeList.Add(remove4);

                            var remove5 = Lance.Account.PetFireInventory.RemoveItem("PetEquipment_20");
                            if (remove5 != null)
                                removeList.Add(remove5);

                            var remove6 = Lance.Account.PetFireInventory.RemoveItem("PetEquipment_21");
                            if (remove6 != null)
                                removeList.Add(remove6);

                            // 물
                            var remove7 = Lance.Account.PetWaterInventory.RemoveItem("PetEquipment_37");
                            if (remove7 != null)
                                removeList.Add(remove7);

                            var remove8 = Lance.Account.PetWaterInventory.RemoveItem("PetEquipment_38");
                            if (remove8 != null)
                                removeList.Add(remove8);

                            var remove9 = Lance.Account.PetWaterInventory.RemoveItem("PetEquipment_39");
                            if (remove9 != null)
                                removeList.Add(remove9);

                            var remove10 = Lance.Account.PetWaterInventory.RemoveItem("PetEquipment_40");
                            if (remove10 != null)
                                removeList.Add(remove10);

                            var remove11 = Lance.Account.PetWaterInventory.RemoveItem("PetEquipment_41");
                            if (remove11 != null)
                                removeList.Add(remove11);

                            var remove12 = Lance.Account.PetWaterInventory.RemoveItem("PetEquipment_42");
                            if (remove12 != null)
                                removeList.Add(remove12);

                            // 풀
                            var remove13 = Lance.Account.PetGrassInventory.RemoveItem("PetEquipment_58");
                            if (remove13 != null)
                                removeList.Add(remove13);

                            var remove14 = Lance.Account.PetGrassInventory.RemoveItem("PetEquipment_59");
                            if (remove14 != null)
                                removeList.Add(remove14);

                            var remove15 = Lance.Account.PetGrassInventory.RemoveItem("PetEquipment_60");
                            if (remove15 != null)
                                removeList.Add(remove15);

                            var remove16 = Lance.Account.PetGrassInventory.RemoveItem("PetEquipment_61");
                            if (remove16 != null)
                                removeList.Add(remove16);

                            var remove17 = Lance.Account.PetGrassInventory.RemoveItem("PetEquipment_62");
                            if (remove17 != null)
                                removeList.Add(remove17);

                            var remove18 = Lance.Account.PetGrassInventory.RemoveItem("PetEquipment_63");
                            if (remove18 != null)
                                removeList.Add(remove18);

                            foreach(var removeitem in removeList)
                            {
                                removeitem.ReadyToSave();
                            }

                            Param param = new Param();
                            param.Add("totalElementalStone", -totalElementalStone);
                            param.Add("nickName", Backend.UserNickName);
                            param.Add("removeList", removeList.ToArray());

                            Lance.BackEnd.InsertLog("ProcessBadRaidUser", param, 7);
                        }
                    }
                });

                
            }

            Lance.Account.DeleteDissatisfyingCollection(ItemType.Skill);
        }

        public void MoveToBountyQuestTarget(string id, Action onMove)
        {
            var bountyQuestData = Lance.GameData.BountyQuestData.TryGet(id);
            if (bountyQuestData != null)
            {
                // 내가 갈 수 있는 최고 난이도를 우선 확인해보자
                StageDifficulty myBestDiff = Lance.Account.StageRecords.GetCurDifficulty();

                bool moveSuccess = false;

                while(myBestDiff >= StageDifficulty.Beginner)
                {
                    // 내가 갈 수 있는 최고 난이도의 스테이지에 갈 수 있는지 확인한다.
                    bool canChangeStage = Lance.Account.StageRecords.CanChangeStage(myBestDiff, bountyQuestData.moveChapter, bountyQuestData.moveStage);
                    if (canChangeStage)
                    {
                        // 바로 이동
                        var stageData = DataUtil.GetStageData(myBestDiff, bountyQuestData.moveChapter, bountyQuestData.moveStage);
                        if (stageData != null)
                        {
                            Lance.Account.StageRecords.ChangeStage(stageData.diff, stageData.chapter, stageData.stage);

                            if (bountyQuestData.monsterType == MonsterType.monster)
                            {
                                if (SaveBitFlags.BossBreakThrough.IsOn())
                                    SaveBitFlags.BossBreakThrough.Set(false);
                            }
                            else
                            {
                                if (SaveBitFlags.BossBreakThrough.IsOff())
                                    SaveBitFlags.BossBreakThrough.Set(true);
                            }

                            ChangeStage(stageData);

                            onMove?.Invoke();

                            moveSuccess = true;

                            break;
                        }
                        else
                        {
                            UIUtil.ShowSystemDefaultErrorMessage();

                            break;
                        }
                    }

                    myBestDiff = (StageDifficulty)((int)myBestDiff - 1);
                }

                if (moveSuccess == false)
                    UIUtil.ShowSystemDefaultErrorMessage();
            }
        }

        public int GetJoustingComboAtkCount()
        {
            return mStageManager.GetJoustingComboAtkCount();
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private void OnGUI()
        {
            //GUIStyle style = new GUIStyle();

            //Rect rect = new Rect(30, 30, Screen.width, Screen.height);
            //style.alignment = TextAnchor.UpperLeft;
            //style.fontSize = 40;
            //style.normal.textColor = Color.red;

            //GUI.Label(rect, $"게임 누적 시간 : {Lance.Account.UserInfo.GetStackPlayTime()}분", style);
        }
#endif

        bool mIsCounted;
        IEnumerator UpdateUnscaledTimeForSeconds()
        {
            var waitForSeconds = new WaitForSecondsRealtime(1f);

            while(true)
            {
                yield return waitForSeconds;

                mLastUpdateTime = TimeUtil.Now;

                int nowDateNum = TimeUtil.NowDateNum();
                int nowRankDateNum = TimeUtil.RankNowDateNum();

                // 하루가 지났다.
                if (nowDateNum != mLastUpdateDateNum)
                {
                    // 퀘스트 갱신
                    Lance.Account.Event.UpdateEventQuest();
                    Lance.Account.DailyQuest.Update();
                    Lance.Account.WeeklyQuest.Update();

                    // 출석부 갱신
                    Lance.Account.Attendance.Check();

                    // 현상금 갱신
                    Lance.Account.Bounty.Update();

                    Lance.Lobby.RefreshMenuRedDots();

                    if (ContentsLockUtil.IsLockContents(ContentsLockType.DemonicRealm) == false)
                        Lance.Account.Currency.UpdateFreeDemonicRealmStone();
                }

                mLastUpdateDateNum = nowDateNum;

                // 랭킹 기준으로 하루가 지났다
                if (nowRankDateNum != mLastRankUpdateDateNum)
                {
                    AnyCanReceiveMonthlyFeeDailyReward();
                }

                if (BackendManager2.IsCountedRank() && mIsCounted == false)
                {
                    mIsCounted = true;
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
                }
                else
                {
                    mIsCounted = false;
                }

                mLastRankUpdateDateNum = nowRankDateNum;

                if (mOnReceiveOfflineReward)
                {
                    Lance.Account.UserInfo.UpdateOnlineTime();
                }

                Lance.Account.Lotto.Update();
                Lance.Lobby.RefreshLottoRedDot();

                Lance.Lobby.UpdateWeekendDoubleUI();

                if (mFirstGuideQuestAutoTime > 0)
                {
                    mFirstGuideQuestAutoTime -= 1;

                    if (mFirstGuideQuestAutoTime <= 0)
                    {
                        if (Lance.Account.GuideQuest.GetCurrentStep() == 1)
                            Lance.Lobby.StartGuideAction(isAuto: true);
                    }
                }

                if (!mStageManager.IsJousting)
                {
                    bool purchasedRemovedAD = Lance.Account.PackageShop.IsPurchasedRemoveAD() || Lance.IAPManager.IsPurchasedRemoveAd();

                    Lance.Account.Buff.OnUpdate(TimeUpdateInterval, purchasedRemovedAD);
                    Lance.Lobby.UpdateBuffUI(purchasedRemovedAD);

                    Lance.Account.SpeedMode.OnUpdate(TimeUpdateInterval, purchasedRemovedAD);
                    Lance.Lobby.UpdateSpeedModeUI(purchasedRemovedAD);

                    bool prevEssence = Lance.Account.Essence.IsActiveCentralEssence();
                    Lance.Account.Essence.OnUpdate(TimeUpdateInterval);
                    Lance.Lobby.UpdateCentralEssenceUI();
                    bool afterEssence = Lance.Account.Essence.IsActiveCentralEssence();

                    if (prevEssence != afterEssence)
                    {
                        UpdatePlayerStat();
                    }
                }
            }
        }

        IEnumerator UpdateUnscaledTimeForMinute()
        {
            var waitForMinute = new WaitForSecondsRealtime(60f);

            while (true)
            {
                yield return waitForMinute;

                Lance.Account.UserInfo.StackPlayTime(1);

                CheckQuest(QuestType.PlayTime, 1);

                Lance.Account.UpdatePassRewardValue(PassType.PlayTime);

                Lance.Lobby.RefreshPassRedDot();
            }
        }

        bool mTestLangCode;

        void Update()
        {
            float dt = Time.deltaTime;
            float unscaledDt = Time.unscaledDeltaTime;

            UpdateScaleTime(dt);
            UpdateUnscaledTime(unscaledDt);

#if UNITY_EDITOR
            if (Input.GetMouseButton(0))
            {
                mStageManager.OnAnyScreenTouch();
                mSleepModeManager.OnAnyScreenTouch();
            }
#else
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    mStageManager.OnAnyScreenTouch();
                    mSleepModeManager.OnAnyScreenTouch();
                }
            }
#endif
            // 뒤로가기 버튼을 눌렀을 때 동작
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackButton();
            }

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.L))
            {
                ReLogin();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                Lance.Account.AddDungeonTicket(StageType.Gold, 3);
                Lance.Account.AddDungeonTicket(StageType.Stone, 3);
                Lance.Account.AddDungeonTicket(StageType.Raid, 300);
                Lance.Account.Currency.AddSoulStone(100000);
                Lance.Account.AddGem(2000000);
                Lance.Account.AddGold(50000000000000);
                Lance.Account.AddUpgradeStone(50000000000000);
                Lance.Account.Currency.AddCostumeUpgrade(10000);
                Lance.Account.Currency.AddReforgeStone(50000000000000);
                Lance.Account.Currency.AddAncientEssence(int.MaxValue / 2);
                Lance.Account.Currency.AddGloryToken(500000);
                Lance.Account.Currency.AddManaEssence(50000000);

                Lance.Account.Achievement.CompleteAchievement("Achievement_SnakeNewYear2025");
                Lance.Account.Achievement.ReceiveAchievement("Achievement_SnakeNewYear2025");
                //Lance.Account.Event.AddEventCurrency("Event_Summer202406", 500000);

                Lance.Account.Currency.AddPetFood(5000000);
                Lance.Account.Currency.AddElementalStone(500000);
                Lance.Account.ExpLevel.AddAP(1000);

                for (int i = 0; i < (int)EssenceType.Count; ++i)
                {
                    EssenceType type = (EssenceType)i;

                    Lance.Account.Currency.AddEssence(type, 50000);
                }

                foreach (var data in Lance.GameData.EventData.Values)
                {
                    if (data.currencyDropProb > 0)
                    {
                        Lance.Account.Event.AddEventCurrency(data.id, 50000);
                    }
                }

                //Lance.Account.MileageShop.AddMileage(1000);


                mIsAutoStep = !mIsAutoStep;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                Lance.Lobby.ToggleMovieMode();

                //Lance.Account.GuideQuest.SkipGuideStep(1);

                //Lance.Lobby.RefreshGuideQuestUI();
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                Lance.Account.GuideQuest.SkipGuideStep(1734);

                Lance.Account.ExpLevel.SetLevel(2250);

                Lance.Account.StageRecords.SetBestStage(12301);

                Lance.Lobby.RefreshGuideQuestUI();
            }
            else if (Input.GetKeyDown(KeyCode.J))
            {
                // 퀘스트 갱신
                Lance.Account.Event.UpdateEventQuest();
                Lance.Account.DailyQuest.CreateNew();
                Lance.Account.WeeklyQuest.CreateNew();
                Lance.Account.Attendance.Check();
                Lance.Account.Bounty.CreateNewQuests();
                Lance.Lobby.RefreshMenuRedDots();
                Lance.Account.Currency.UpdateFreeDemonicRealmStone();
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                Lance.Account.GuideQuest.PrevGuideStep();

                Lance.Lobby.RefreshGuideQuestUI();
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                Lance.Account.Dungeon.SetBestStep(StageType.Gold, 72);
                Lance.Account.Dungeon.SetBestStep(StageType.Stone, 70);
                Lance.Account.Dungeon.SetBestStep(StageType.Reforge, 99);
                Lance.Account.Dungeon.SetBestStep(StageType.Ancient, 45);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                foreach (var key in Lance.GameData.AncientArtifactData.Keys)
                {
                    Lance.Account.AddArtifact(key, 1000);
                }

            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                Monster monster = Lance.GameManager.StageManager.GetFirstMonster();

                monster.OnDeath();
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                foreach (var key in Lance.GameData.WeaponData.Keys)
                {
                    Lance.Account.WeaponInventory.SetLevel(key, 1);
                    Lance.Account.WeaponInventory.SetReforge(key, 0);
                }

                foreach (var key in Lance.GameData.ArmorData.Keys)
                {
                    Lance.Account.ArmorInventory.SetLevel(key, 1);
                    Lance.Account.ArmorInventory.SetReforge(key, 0);
                }

                foreach (var key in Lance.GameData.GlovesData.Keys)
                {
                    Lance.Account.GlovesInventory.SetLevel(key, 1);
                    Lance.Account.GlovesInventory.SetReforge(key, 0);
                }

                foreach (var key in Lance.GameData.ShoesData.Keys)
                {
                    Lance.Account.ShoesInventory.SetLevel(key, 1);
                    Lance.Account.ShoesInventory.SetReforge(key, 0);
                }

                //Lance.Account.Essence.Reset();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                FastMode(5f);
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                mTestLangCode = !mTestLangCode;

                Lance.LocalSave.LangCode = mTestLangCode ? LangCode.KR : LangCode.US;

                Localize();
                Lance.Lobby.Localize();

                //Lance.Account.SkillInventory.Reset();

                //Lance.Account.ExpLevel.SetLimitBreak(0);
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                Lance.GameManager.StageManager.Player.Stat.OnDamage(Lance.GameManager.StageManager.Player.Stat.MaxHp);
                Lance.GameManager.StageManager.Player.OnDeath();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                TestIndex = (TestIndex + 1) % (int)ElementalType.Count;

                mTestStep++;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                mTestStep--;
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                foreach(var costumeData in Lance.GameData.BodyCostumeData.Values)
                {
                    Lance.Account.Costume.AddCostume(costumeData.id);
                }

                foreach (var costumeData in Lance.GameData.WeaponCostumeData.Values)
                {
                    Lance.Account.Costume.AddCostume(costumeData.id);
                }

                foreach (var costumeData in Lance.GameData.EtcCostumeData.Values)
                {
                    Lance.Account.Costume.AddCostume(costumeData.id);
                }

                for(int t = 0; t <= 45; ++t)
                {
                    AddEquipments(ItemType.Weapon, t);
                    AddEquipments(ItemType.Armor, t);
                    AddEquipments(ItemType.Gloves, t);
                    AddEquipments(ItemType.Shoes, t);
                }

                for(int tt = 0; tt <= 24; ++tt)
                {
                    AddAccessory(ItemType.Necklace, tt);
                    AddAccessory(ItemType.Earring, tt);
                    AddAccessory(ItemType.Ring, tt);
                }

                foreach(var artifactData in Lance.GameData.ArtifactData.Values)
                {
                    Lance.Account.AddArtifact(artifactData.id, 1, 100);
                }

                foreach(var ancientArtifact in Lance.GameData.AncientArtifactData.Values)
                {
                    Lance.Account.AddArtifact(ancientArtifact.id, 1, 200);
                }

                Lance.Account.ExpLevel.SetLimitBreak(12);
                Lance.Account.ExpLevel.SetUltimateLimitBreak(7);

                AddPetEquipments(63);

                Lance.Account.AddGold(50000000000000000000d);
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                Lance.Account.AddEquipment($"Weapon_{mTestEquipmentIndex}", 1);
                Lance.Account.AddEquipment($"Armor_{mTestEquipmentIndex}", 1);
                Lance.Account.AddEquipment($"Gloves_{mTestEquipmentIndex}", 1);
                Lance.Account.AddEquipment($"Shoes_{mTestEquipmentIndex}", 1);

                mTestEquipmentIndex++;

                if (mTestEquipmentIndex > 45)
                    mTestEquipmentIndex = 0;
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                mTestBestStage++;

                if (mTestBestStage > 4800)
                    mTestBestStage = 1;

                Lance.Account.StageRecords.SetBestStage(mTestBestStage);
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                mTestBestStage--;

                if (mTestBestStage <= 0)
                    mTestBestStage = 4800;

                Lance.Account.StageRecords.SetBestStage(mTestBestStage);
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                mTestBestStage += 20;

                if (mTestBestStage > 4800)
                    mTestBestStage = 1;

                Lance.Account.StageRecords.SetBestStage(mTestBestStage);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                Lance.GameManager.StageManager.PlayClearStage();
                //SetMaxLevelEquipments(ItemType.Weapon);
                //SetMaxLevelEquipments(ItemType.Armor);
                //SetMaxLevelEquipments(ItemType.Gloves);
                //SetMaxLevelEquipments(ItemType.Shoes);
            }

            void AddEquipments(ItemType itemType, int num)
            {
                for (int i = 0; i < num; ++i)
                {
                    string key = $"{itemType}_{i + 1}";

                    var data = DataUtil.GetEquipmentData(key);

                    Lance.Account.AddEquipment(key, 1);

                    var equipment = Lance.Account.GetEquipment(key);

                    equipment.SetReforgeStep(equipment.GetMaxReforge());

                    equipment.LevelUp(equipment.GetMaxLevel());
                }
            }

            void AddAccessory(ItemType itemType, int num)
            {
                for (int i = 0; i < num; ++i)
                {
                    string key = $"{itemType}_{i + 1}";

                    var data = DataUtil.GetAccessoryData(key);

                    Lance.Account.AddAccessory(key, 1);

                    var equipment = Lance.Account.GetAccessory(key);

                    equipment.SetReforgeStep(equipment.GetMaxReforge());
                    equipment.LevelUp(equipment.GetMaxLevel());
                }
            }

            void AddPetEquipments(int num)
            {
                for (int i = 0; i < num; ++i)
                {
                    string key = $"PetEquipment_{i + 1}";

                    var data = Lance.GameData.PetEquipmentData.TryGet(key);

                    Lance.Account.AddPetEquipment(key, 1);

                    var equipment = Lance.Account.GetPetEquipment(key);
                    equipment.SetReforgeStep(equipment.GetMaxReforge());
                    equipment.LevelUp(equipment.GetMaxLevel());
                }
            }
#endif
        }

        private void LateUpdate()
        {
            mStageManager.OnLateUpdate();
        }


#if DEVELOPMENT_BUILD || UNITY_EDITOR
        int mTestEquipmentIndex = 1;
        int mTestStep = 1;
        bool mIsAutoStep = false;
        int mTestBestStage = 1;

        bool mIsTestMode;
        public void FastMode(float timeScale)
        {
            mIsTestMode = !mIsTestMode;

            if (mIsTestMode)
            {
                Time.timeScale = timeScale;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
#endif

        public void OnChangeBitFlags()
        {
            Lance.SoundManager.OnChangeBitFlag();
        }

        void UpdateScaleTime(float dt)
        {
            mTimeUpdateInterval -= dt;
            if (mTimeUpdateInterval <= 0f)
            {
                mTimeUpdateInterval = TimeUpdateInterval;
            }

            mStageManager.OnUpdate(dt);
        }

        void UpdateUnscaledTime(float  dt)
        {
            UpdateRandomizeKey(dt);

            mSleepModeManager.OnUpdate(mStageManager.StageData, dt);

            if ( DOTween.timeScale != 1f )
            {
                DOTween.timeScale = 1f;
            }

            if ( DOTween.unscaledTimeScale != 1f)
            {
                DOTween.unscaledTimeScale = 1f;
            }

            //if (Time.time)
        }

        void UpdateRandomizeKey(float dt)
        {
            mRandomizeIntervalTime -= dt;
            if (mRandomizeIntervalTime <= 0)
            {
                mRandomizeIntervalTime = UnityEngine.Random.Range(1, 10);

                Lance.Account.RandomizeKey();

                mStageManager.RanodmizeKey();
            }
        }

        void OnBackButton()
        {
            if (Lance.PopupManager.Count > 0)
            {
                Lance.PopupManager.OnBackButton();
            }
            else
            {
                string title = StringTableUtil.Get("Title_Confirm");
                string desc = StringTableUtil.Get("Desc_ApplicationQuit");

                UIUtil.ShowConfirmPopup(title, desc, OnConfirm, null);

                void OnConfirm()
                {
                    // 화면을 가려주자
                    Lance.BackEnd.UpdateAllAccountInfos((callBack) =>
                    {
                        if (callBack.IsSuccess())
                        {
#if UNITY_EDITOR
                            EditorApplication.ExitPlaymode();
#else
                            Application.Quit();
#endif
                        }
                        else
                        {
                            string title = StringTableUtil.Get("Title_Error");
                            string desc = StringTableUtil.Get("Desc_SaveAccountInfosFail");

                            var popup = UIUtil.ShowConfirmPopup(title, desc, null, null);
                            popup.SetHideCancelButton();
                        }
                    });
                }
            }
        }

        public bool IsSleepMode()
        {
            return mSleepModeManager.IsSleepMode();
        }

        public void SetSleepMode(bool sleepMode)
        {
            mSleepModeManager.SetSleepMode(sleepMode);
        }

        public void PlayKnockback(float knockbackPower)
        {
            // 모든 몬스터들 넉백하고 배경도 슥
            foreach (var monster in mStageManager.GatherMonsters())
            {
                monster.Physics.PlayKnockback(Vector2.right * knockbackPower);
            }

            mStageManager.PlayKnockback();      //  배경
            mStageManager.Player.Action.PlayAction(ActionType.Knockback, true);
            mStageManager.Player.Action.AllCancelReservedAction();
        }

        public void OnBossAutoChallenge()
        {
            mStageManager.OnBossAutoChallenge();
        }

        private void OnEnable()
        {
            //Lance.AdManager.OnEnableEvent();
        }

        void OnApplicationPause(bool pause)
        {
            //Lance.AdManager.OnApplicationPauseEvent(pause);

            if (pause == false)
            {
                if (mLastUpdateTime > 0)
                {
                    // 마지막으로 업데이트한 UTC mLastUpdateTime
                    // 한국 시간을 기준으로 생각하기 위해 +9 hour 해준다
                    int lastUpdateKorTime = mLastUpdateTime + (TimeUtil.SecondsInHour * 9);
                    DateTime lastUpdateKorDateTime = TimeUtil.GetDateTime(lastUpdateKorTime);
                    DateTime nowKorDateTime = DateTime.UtcNow.AddHours(9);

                    // 확인할 요일과 시간대 한국 시간을 기준으로 오전 4시와 5시 사이가 갱신 시간이기 때문에
                    DayOfWeek targetDay = DayOfWeek.Monday; // 예: 월요일
                    TimeSpan startTime = new TimeSpan(4, 0, 0); // 04:00
                    TimeSpan endTime = new TimeSpan(5, 0, 0);   // 05:00

                    bool hasPassedRankTime = HasPassedTargetDayAndTime(lastUpdateKorDateTime, nowKorDateTime, targetDay, startTime, endTime);
                    if (hasPassedRankTime)
                    {
                        // 로컬에 저장된 랭킹관련 정보 0으로 바꿔주자
                        Lance.Account.Dungeon.SetRaidBossDamage(0);
                        Lance.Account.NewBeginnerRaidScore.SetRaidBossDamage(0);
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
                            {
                                SendQueue.Enqueue(Backend.RandomInfo.SetRandomData, RandomType.User, matchUrl, Lance.Account.JoustRankInfo.GetRankScore(), (bro) => { });
                            }
                        }
                    }

                    // 60분정도 지났다면
                    int elapsed = TimeUtil.Now - mLastUpdateTime;
                    int expire = TimeUtil.SecondsInMinute * 60;

                    // 로그인 화면으로 보내버리자
                    if (expire < elapsed )
                    {
                        ReLogin();
                    }
                }
            }
        }

        // 과거와 현재 사이에 특정 요일과 시간을 지났는지 확인하는 함수
        public bool HasPassedTargetDayAndTime(DateTime pastDate, DateTime currentDate, DayOfWeek targetDay, TimeSpan startTime, TimeSpan endTime)
        {
            // 과거 날짜부터 현재 날짜까지 하루씩 반복
            DateTime tempDate = pastDate;
            while (tempDate <= currentDate)
            {
                // 요일이 일치하는지 확인
                if (tempDate.DayOfWeek == targetDay)
                {
                    // 시간대 확인
                    if (tempDate.TimeOfDay >= startTime && tempDate.TimeOfDay <= endTime)
                    {
                        return true; // 특정 요일과 시간대를 지남
                    }
                }

                // 하루씩 증가
                tempDate = tempDate.AddHours(1); // 시간 단위로 비교 (필요 시 `AddDays(1)`로 조정)
            }

            return false; // 특정 요일과 시간대를 지나지 않음
        }

        void SyncServerTimeLongIdle()
        {
            string url = Lance.GameData.CommonData.timeCheatingDetectUrl;

            StartCoroutine(TimeCheatingDetector.GetOnlineTimeCoroutine(url, (result) =>
            {
                if (result.Success)
                    TimeUtil.SyncTo(result.OnlineDateTimeUtc);
            }));
        }

        void CheckConfirmMaintanceInfo()
        {
            foreach(var data in Lance.GameData.MaintanceInfoData.Values)
            {
                if (data.active && Lance.Account.UserInfo.IsConfirmedMaintanceInfo(data.id) == false)
                {
                    var popup = Lance.PopupManager.CreatePopup<Popup_ConfirmMaintanceUI>();

                    string title = StringTableUtil.Get("Title_AdvanceNotice");
                    string desc = StringTableUtil.Get("Desc_Maintance");

                    popup.Init(title, desc, () => Lance.Account.UserInfo.ConfirmMaintanceInfo(data.id));

                    break;
                }
            }
        }

        void CheckOfflineReward()
        {
            int lastUpdateOnlineTime = Lance.Account.UserInfo.GetLastUpdateOnlineTime();
            int nowOnlineTime = TimeUtil.Now;

            if (nowOnlineTime > lastUpdateOnlineTime && lastUpdateOnlineTime > 0)
            {
                int offlineTime = nowOnlineTime - lastUpdateOnlineTime;
                if (offlineTime >= Lance.GameData.OfflineRewardCommonData.minOfflineTimeForSecond)
                {
                    var popup = Lance.PopupManager.CreatePopup<Popup_OfflineRewardUI>();

                    popup.Init(offlineTime);

                    popup.SetOnCloseAction(() => mOnReceiveOfflineReward = true);
                }
                else
                {
                    mOnReceiveOfflineReward = true;
                }
            }
            else
            {
                mOnReceiveOfflineReward = true;
            }
        }

        void CheckAttendance()
        {
            if (Lance.Account.GuideQuest.GetCurrentStep() >= (int)ContentsLockType.Attendance)
            {
                if (Lance.Account.Attendance.Check())
                {
                    var popup = Lance.PopupManager.CreatePopup<Popup_AttendanceUI>();

                    popup.Init();

                    popup.SetOnCloseAction(() => Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.ConfirmAttendance));

                    Param param = new Param();
                    param.Add("nowDateNum", TimeUtil.NowDateNum());

                    Lance.BackEnd.InsertLog("Attendance", param, 7);
                }
            }
        }

        // 골드 훈련이 완료 되었을 때
        // 새로운 장비를 착용했을 때, 장비를 강화 완료 했을 때
        // 새로운 장비를 획득하고 보유 효과가 갱신되었을 때
        // 스킬 적용되었을 때, 스킬 강화 완료 했을 때
        // 스킬 패시브 적용되었을 때, 스킬 패시브 적용 끝났을 때
        // 프리셋이 변경되었을 때
        // 버프 적용되었을 때, 버프 적용 끝났을 때
        // To Do : 뭔가 캐릭터 강해지는거 추가되면 무조건 여기 수정해줘야함
        public void UpdatePlayerStat(bool updatePowerLevel = true)
        {
            mStageManager.UpdatePlayerStat();

            if (updatePowerLevel)
                UpdatePowerLevel(mStageManager.Player.Stat.PowerLevel);
        }

        public void UpdatePowerLevel(double newPowerLevel)
        {
            double prevPowerLevel = Lance.Account.UserInfo.GetPowerLevel();

            bool updateBestPower = Lance.Account.UserInfo.SetPowerLevel(newPowerLevel);

            double curPowerLevel = Lance.Account.UserInfo.GetPowerLevel();

            var popup = Lance.PopupManager.CreatePopup<Popup_UpdatePowerLevelUI>(showMotion:false);
            popup.Init(curPowerLevel, newPowerLevel >= prevPowerLevel);

            Lance.Lobby.UpdatePowerLevelUI();

            var popup2 = Lance.PopupManager.GetPopup<Popup_UserInfoUI>();
            popup2?.RefreshPowerLevel();

            if (updateBestPower)
            {
                Lance.Account.JoustBattleInfo.SetPowerLevel(newPowerLevel);
            }
        }

        public void OnChangeStage(StageData stageData)
        {
            mSleepModeManager?.OnChangeStage(stageData);
        }

        public void OnBossDamage(ObscuredDouble damage)
        {
            Lance.Lobby.UpdateBossHp();

            mStageManager.OnBossDamage(damage);
        }

        public void OnPlayerDeath()
        {
            mStageManager.OnPlayerDeath();
        }

        public void OnMonsterDeath(RewardResult reward, int level)
        {
            mStageManager.OnMonsterDeath(reward, level);

            mSleepModeManager.StackRewardInSleepMode(reward);

            Lance.Account.Buff.OnMonsterDeath();

            Lance.Lobby.RefreshBuffRedDot();

            Lance.Lobby.StackReward(reward);
        }

        public bool IsDungeonMaxStep()
        {
            return mStageManager.IsDungeonMaxStep();
        }

        public void OnAncientBossDeath(RewardResult reward)
        {
            mStageManager.OnAncientBossDeath(reward);

            mSleepModeManager.StackRewardInSleepMode(reward);

            Lance.Lobby.StackReward(reward);
        }

        public void OnPetBossDeath(RewardResult reward)
        {
            mStageManager.OnPetBossDeath(reward);

            mSleepModeManager.StackRewardInSleepMode(reward);

            Lance.Lobby.StackReward(reward);
        }

        public bool Train(StatType type, int trainCount)
        {
            if (DataUtil.HaveGoldTrainRequireType(type))
            {
                if (Lance.Account.GoldTrain.IsSatisfiedRequireType(type) == false)
                {
                    UIUtil.ShowSystemDefaultErrorMessage();

                    return false;
                }
            }

            // 골드가  충분한지 확인하고
            (bool canTrain, double totalRequireGold) result = Lance.Account.CanTrain(type, trainCount);
            if (result.canTrain == false)
            {
                // 데이터가 없어서 훈련을 할 수 없다는 뜻
                UIUtil.ShowSystemErrorMessage("IsMaxLevelTrain");

                return false;
            }

            if (Lance.Account.IsEnoughGold(result.totalRequireGold) == false)
            {
                // 골드가 부족하다.
                UIUtil.ShowSystemErrorMessage("IsNotEnoughGold");

                return false;
            }

            // 실제 훈련이 적용되는 부분
            Lance.Account.Train(type, trainCount);

            // LobbyUI를 통해서 처리할 것 처리
            Lance.Lobby.UpdateGoldUI();

            // 퀘스트
            Lance.GameManager.CheckQuest(QuestType.Train, trainCount);
            
            Lance.GameManager.CheckQuest(type.ChangeToQuestType(), trainCount);

            return true;
        }

        public bool ChangeSkillPreset(int preset)
        {
            if (mStageManager.IsBossStage)
            {
                UIUtil.ShowSystemErrorMessage("CanNotChangeSkillPresetWhileInBossStage");

                return false;
            }
            else
            {
                Lance.Account.SkillInventory.ChangePreset(preset);

                OnChangePreset();

                return true;
            }
        }

        public void OnChangePreset()
        {
            Lance.Lobby.OnChangePreset();

            mStageManager.Player.SkillManager.OnChangePreset();

            UpdatePlayerStat();

            mStageManager.Player.UpdateCostumes(Lance.Account.Costume.GetEquippedCostumeIds(), isJousting: mStageManager.IsJousting);

            Lance.Account.JoustBattleInfo.SetCostumes(Lance.Account.Costume.GetEquippedCostumeIds());

            Lance.BackEnd.ChattingManager.ReEntryChattinChannel();

            Lance.Lobby.UpdatePortrait();

            Lance.Lobby.RefreshTab();
        }

        public void EquipSkill(SkillType skillType, int slot, string id)
        {
            if (Lance.Account.IsUnlockSkillSlot(slot) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotUnlockSkillSlot");

                return;
            }

            if (Lance.Account.EquipSkill(skillType, slot, id))
            {
                mStageManager.Player.SkillManager.RefreshAllSlot();

                if (skillType == SkillType.Active)
                {
                    CheckQuest(QuestType.EquipActiveSkill, 1);
                }
                else
                {
                    CheckQuest(QuestType.EquipPassiveSkill, 1);
                }

                UpdatePlayerStat();

                Lance.Lobby.RefreshSkillCastUI();

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Skill);
            }
        }

        public void UnEquipSkill(SkillType skillType, string id)
        {
            if (Lance.Account.UnEquipSkill(skillType, id))
            {
                mStageManager.Player.ReleaseFX(id);
                mStageManager.Player.SkillManager.RefreshAllSlot();

                UpdatePlayerStat();

                Lance.Lobby.RefreshSkillCastUI();

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Skill);
            }
        }

        public void UnEquipSkill(SkillType skillType, int slot)
        {
            if (Lance.Account.UnEquipSkill(skillType, slot))
            {
                mStageManager.Player.SkillManager.RefreshAllSlot();

                UpdatePlayerStat();

                Lance.Lobby.RefreshSkillCastUI();

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Skill);
            }
        }

        // 유물 판매
        public void SellArtifact(string id)
        {
            int count = Lance.Account.Artifact.Sell(id);
            if (count > 0)
            {
                int totalSellGem = Lance.GameData.ShopCommonData.artifactSellPrice * count;
                int beforeTotalGem = (int)Lance.Account.Currency.GetGem();

                RewardResult rewardResult = new RewardResult();
                rewardResult.gem = totalSellGem;

                GiveReward(rewardResult, ShowRewardType.Popup);

                int afterTotalGem = (int)Lance.Account.Currency.GetGem();

                Param param = new Param();
                param.Add("id", id);
                param.Add("beforeTotalGem", beforeTotalGem);
                param.Add("totalSellCount", count);
                param.Add("totalSellGem", totalSellGem);
                param.Add("afterTotalGem", afterTotalGem);
                param.Add("count", count);

                Lance.BackEnd.InsertLog("SellArtifact", param, 7);
            }
        }

        public void SellAllArtifact()
        {
            int count = Lance.Account.Artifact.SellAll();
            if (count > 0)
            {
                int totalSellGem = Lance.GameData.ShopCommonData.artifactSellPrice * count;
                int beforeTotalGem = (int)Lance.Account.Currency.GetGem();

                RewardResult rewardResult = new RewardResult();
                rewardResult.gem = totalSellGem;

                GiveReward(rewardResult, ShowRewardType.Popup);

                int afterTotalGem = (int)Lance.Account.Currency.GetGem();

                Param param = new Param();
                param.Add("beforeTotalGem", beforeTotalGem);
                param.Add("totalSellCount", count);
                param.Add("totalSellGem", totalSellGem);
                param.Add("afterTotalGem", afterTotalGem);
                param.Add("count", count);

                Lance.BackEnd.InsertLog("SellAllArtifact", param, 7);
            }
        }

        // 유물 분해
        public void DismantleArtifact(string id)
        {
            int count = Lance.Account.Artifact.Dismantle(id);
            if (count > 0)
            {
                MinMaxRange minMaxRange = new MinMaxRange();
                minMaxRange.min = Lance.GameData.ArtifactCommonData.dismantleMinAmount;
                minMaxRange.max = Lance.GameData.ArtifactCommonData.dismantleMaxAmount;

                int totalAncientEssnce = 0;
                for(int i = 0; i < count; ++i)
                {
                    totalAncientEssnce += minMaxRange.SelectRandom();
                }

                RewardResult rewardResult = new RewardResult();
                rewardResult.ancientEssence = totalAncientEssnce;

                GiveReward(rewardResult, ShowRewardType.Popup);

                Param param = new Param();
                param.Add("id", id);
                param.Add("totalAncientEssence", totalAncientEssnce);
                param.Add("count", count);

                Lance.BackEnd.InsertLog("DismantleArtifact", param, 7);
            }
        }

        public void DismantleAllArtifact()
        {
            int count = Lance.Account.Artifact.DismantleAll();
            if (count > 0)
            {
                MinMaxRange minMaxRange = new MinMaxRange();
                minMaxRange.min = Lance.GameData.ArtifactCommonData.dismantleMinAmount;
                minMaxRange.max = Lance.GameData.ArtifactCommonData.dismantleMaxAmount;

                int totalAncientEssnce = 0;
                for (int i = 0; i < count; ++i)
                {
                    totalAncientEssnce += minMaxRange.SelectRandom();
                }

                RewardResult rewardResult = new RewardResult();
                rewardResult.ancientEssence = totalAncientEssnce;

                GiveReward(rewardResult, ShowRewardType.Popup);

                Param param = new Param();
                param.Add("totalAncientEssence", totalAncientEssnce);
                param.Add("count", count);

                Lance.BackEnd.InsertLog("DismantleAllArtifact", param, 7);
            }
        }

        // 고대 유물 분해
        public void DismantleAncientArtifact(string id)
        {
            int count = Lance.Account.AncientArtifact.Dismantle(id);
            if (count > 0)
            {
                MinMaxRange minMaxRange = new MinMaxRange();
                minMaxRange.min = Lance.GameData.ArtifactCommonData.ancientDismantleMinAmount;
                minMaxRange.max = Lance.GameData.ArtifactCommonData.ancientDismantleMaxAmount;

                int totalAncientEssnce = 0;
                for (int i = 0; i < count; ++i)
                {
                    totalAncientEssnce += minMaxRange.SelectRandom();
                }

                RewardResult rewardResult = new RewardResult();
                rewardResult.ancientEssence = totalAncientEssnce;

                GiveReward(rewardResult, ShowRewardType.Popup);

                Param param = new Param();
                param.Add("id", id);
                param.Add("totalAncientEssence", totalAncientEssnce);
                param.Add("count", count);

                Lance.BackEnd.InsertLog("DismantleAncientArtifact", param, 7);
            }
        }

        public void DismantleAllAncientArtifact()
        {
            int count = Lance.Account.AncientArtifact.DismantleAll();
            if (count > 0)
            {
                MinMaxRange minMaxRange = new MinMaxRange();
                minMaxRange.min = Lance.GameData.ArtifactCommonData.ancientDismantleMinAmount;
                minMaxRange.max = Lance.GameData.ArtifactCommonData.ancientDismantleMaxAmount;

                int totalAncientEssnce = 0;
                for (int i = 0; i < count; ++i)
                {
                    totalAncientEssnce += minMaxRange.SelectRandom();
                }

                RewardResult rewardResult = new RewardResult();
                rewardResult.ancientEssence = totalAncientEssnce;

                GiveReward(rewardResult, ShowRewardType.Popup);

                Param param = new Param();
                param.Add("totalAncientEssence", totalAncientEssnce);
                param.Add("count", count);

                Lance.BackEnd.InsertLog("DismantleAllAncientArtifact", param, 7);
            }
        }

        public void AddGem(double amount)
        {
            Lance.Account.AddGem(amount);

            Lance.Lobby.UpdateGemUI();
        }

        public void AddGold(double amount)
        {
            Lance.Account.AddGold(amount);

            Lance.Lobby.UpdateGoldUI();
        }

        public void AddUpgradeStone(double amount)
        {
            Lance.Account.AddUpgradeStone(amount);

            Lance.Lobby.UpdateUpgradeStoneUI();
        }

        public void StartLimitBreak()
        {
            //if ( mStageManager.IsBossStage )
            //{
            //    UIUtil.ShowSystemErrorMessage("CanNotEnterLimitBreakWhileInBossStage");

            //    return;
            //}

            if ( Lance.Account.ExpLevel.IsMaxLimitBreak() )
            {
                UIUtil.ShowSystemErrorMessage("IsMaxLimitBreak");

                return;
            }

            if ( Lance.Account.ExpLevel.IsEnoughLimitBreakLevel() == false )
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughLimitBreakLevel");

                return;
            }

            var stageData = DataUtil.GetLimitBreakStageData(Lance.Account.ExpLevel.GetLimitBreak() + 1);
            if (stageData == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            mStageManager.PlayStartStage(stageData);
        }

        public void StartUltimateLimitBreak()
        {
            //if (mStageManager.IsBossStage)
            //{
            //    UIUtil.ShowSystemErrorMessage("CanNotEnterUltimateLimitBreakWhileInBossStage");

            //    return;
            //}

            if (Lance.Account.ExpLevel.IsMaxLimitBreak() == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotMaxLimitBreak");

                return;
            }

            if (Lance.Account.ExpLevel.IsMaxUltimateLimitBreak())
            {
                UIUtil.ShowSystemErrorMessage("IsMaxUltimateLimitBreak");

                return;
            }

            if (Lance.Account.ExpLevel.IsEnoughUltimateLimitBreakLevel() == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughUltimateLimitBreakLevel");

                return;
            }

            var stageData = DataUtil.GetUltimateLimitBreakStageData(Lance.Account.ExpLevel.GetUltimateLimitBreak() + 1);
            if (stageData == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            mStageManager.PlayStartStage(stageData);
        }

        public void StartDungeon(StageData stageData, EntranceType type, Action callBack)
        {
            if (type == EntranceType.AD)
            {
                int remainWatchAdCount = Lance.Account.GetDungeonRemainWatchAdCount(stageData.type);

                // 광고 횟수가 충분한지 확인하자.
                if (remainWatchAdCount <= 0)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughWatchAdCount");

                    return;
                }

                bool isPurchasedRemoveAd = Lance.Account.PackageShop.IsPurchasedRemoveAD() || Lance.IAPManager.IsPurchasedRemoveAd();

                int watchAdCount = isPurchasedRemoveAd ? remainWatchAdCount : 1; 

                Lance.GameManager.ShowRewardedAd(stageData.type.ChangeToAdType(), () =>
                {
                    Lance.Account.Dungeon.StackWatchAdCount(stageData.type, watchAdCount);

                    RewardResult reward = new RewardResult();

                    reward.tickets = reward.tickets.AddTicket(stageData.type.ChangeToDungeonType(), watchAdCount);

                    Lance.GameManager.GiveReward(reward, ShowRewardType.Popup);

                    callBack.Invoke();
                });
            }
            else
            {
                // 티켓이 충분한지 확인하자.
                if (Lance.Account.IsEnoughDungeonTicket(stageData.type, Lance.GameData.DungeonCommonData.entranceRequireTicket) == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughDungeonTicket");

                    return;
                }

                int bestStep = Lance.Account.GetBestDungeonStep(stageData.type);
                int entranceStep = stageData.stage;
                // 도달 가능한 단계인지 확인하자.
                if (bestStep < entranceStep)
                {
                    UIUtil.ShowSystemErrorMessage("HaveNotReachedDungeonStep");

                    return;
                }

                //if (mStageManager.IsBossStage)
                //{
                //    UIUtil.ShowSystemErrorMessage("CanNotEnterDungeonWhileInBossStage");

                //    return;
                //}

                if (Lance.Account.UseDungeonTicket(stageData.type, Lance.GameData.DungeonCommonData.entranceRequireTicket))
                {
                    StartDungeon();

                    callBack.Invoke();
                }
            }

            void StartDungeon()
            {
                Lance.Lobby.ClearReservedRewardUI();

                if (stageData.type == StageType.Raid)
                    Lance.GameManager.CheckQuest(QuestType.TryRaidDungeon, 1);

                mStageManager.PlayStartStage(stageData);
                
                Lance.Lobby.RefreshTabRedDot(LobbyTab.Adventure);
            }
        }

        public void StartDemonicRealm(StageData stageData, EntranceType type)
        {
            if (type == EntranceType.AD)
            {
                int remainWatchAdCount = Lance.Account.GetDemonicRealmRemainWatchAdCount(stageData.type);

                // 광고 횟수가 충분한지 확인하자.
                if (remainWatchAdCount <= 0)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughWatchAdCount");

                    return;
                }

                bool isPurchasedRemoveAd = Lance.Account.PackageShop.IsPurchasedRemoveAD() || Lance.IAPManager.IsPurchasedRemoveAd();

                int watchAdCount = isPurchasedRemoveAd ? remainWatchAdCount : 1;

                Lance.GameManager.ShowRewardedAd(stageData.type.ChangeToAdType(), () =>
                {
                    Lance.Account.DemonicRealm.StackWatchAdCount(stageData.type, watchAdCount);

                    RewardResult reward = new RewardResult();

                    reward.demonicRealmStones = reward.demonicRealmStones.AddStone(stageData.type.ChangeToDemonicRealmType(), watchAdCount);

                    Lance.GameManager.GiveReward(reward, ShowRewardType.Popup);
                });
            }
            else
            {
                // 티켓이 충분한지 확인하자.
                if (Lance.Account.IsEnoughDemonicRealmStone(stageData.type, Lance.GameData.DemonicRealmCommonData.entranceRequireTicket) == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughDemonicRealmStone");

                    return;
                }

                int bestStep = Lance.Account.GetBestDemonicRealmStep(stageData.type);
                int entranceStep = stageData.stage;
                // 도달 가능한 단계인지 확인하자.
                if (bestStep < entranceStep)
                {
                    UIUtil.ShowSystemErrorMessage("HaveNotReachedDemonicRealmStep");

                    return;
                }

                //if (mStageManager.IsBossStage)
                //{
                //    UIUtil.ShowSystemErrorMessage("CanNotEnterDemonicRealmWhileInBossStage");

                //    return;
                //}

                if (Lance.Account.UseDemonicRealmStone(stageData.type, Lance.GameData.DemonicRealmCommonData.entranceRequireTicket))
                {
                    StartDemonicRealm();
                }
            }

            void StartDemonicRealm()
            {
                Lance.Lobby.ClearReservedRewardUI();

                mStageManager.PlayStartStage(stageData);

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Adventure);
            }
        }

        public void ChangeStage(StageData stageData)
        {
            // 현재 도달 가능한 스테이지 인지 확인하자.
            int bestTotalStage = Lance.Account.StageRecords.GetBestTotalStage();
            int entranceStage = StageRecordsUtil.CalcTotalStage(stageData.diff, stageData.chapter, stageData.stage);
            if (bestTotalStage < entranceStage)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotReachedStage");

                return;
            }

            mStageManager.PlayStartStage(stageData);
        }

        public void StartStageCoroutine(IEnumerator routine)
        {
            if (mStageCoroutine != null)
                StopCoroutine(mStageCoroutine);

            mStageCoroutine = StartCoroutine(routine);
        }

        public bool StartJousting(EntranceType entranceType)
        {
            if (mStageManager.InJoustingMotion)
                return false;

            // 티켓이 충분한가?
            if (entranceType == EntranceType.AD)
            {
                // 광고 횟수가 충분한지 확인하자.
                if (Lance.Account.JoustRankInfo.IsEnoughWatchAdCount() == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughWatchAdCount");

                    return false;
                }

                Lance.GameManager.ShowRewardedAd(StageType.Jousting.ChangeToAdType(), () =>
                {
                    Lance.Account.JoustRankInfo.StackWatchAdCount();

                    RewardResult reward = new RewardResult();

                    reward.joustTicket = 1;

                    Lance.GameManager.GiveReward(reward, ShowRewardType.Popup);
                });

                return true;
            }
            else
            {
                // 티켓이 충분한지 확인하자.
                if (Lance.Account.Currency.IsEnoughJoustingTicket(Lance.GameData.JoustingCommonData.entranceRequireTicket) == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughJoustingTicket");

                    return false;
                }

                if (Lance.Account.Currency.UseJoustingTicket(Lance.GameData.JoustingCommonData.entranceRequireTicket))
                {
                    Lance.Lobby.ClearReservedRewardUI();

                    var popup = Lance.PopupManager.GetPopup<Popup_JoustingUI>();
                    popup?.Close();

                    mStageManager.PlayJousting();

                    return true;
                }

                return false;
            }
        }

        public void CheckBountyQuest(MonsterType monsterType, string monsterId, int count)
        {
            Lance.Account.CheckBountyQuest(monsterType, monsterId, count);

            Lance.Lobby.RefreshBountyQuestRedDot();
        }

        public void CheckQuest(QuestType questTye, int count)
        {
            // 모든 퀘스트 ( 일일, 주간, 반복, 가이드 퀘스트 ) 확인
            Lance.Account.CheckQuest(questTye, count);

            Lance.Lobby.RefreshQuestRedDot();
            Lance.Lobby.RefreshEventRedDot();
            Lance.Lobby.RefreshFrameRedDot();
            Lance.Lobby.RefreshGuideQuestUI();

            if (questTye.IsImmediatelyGuideQuestCheck())
            {
                CheckGuideQuestReceiveReward(questTye);
            }
        }

        public void CheckGuideQuestReceiveReward(QuestType questTye)
        {
            if (Lance.GameManager.StageManager.InContents == false)
            {
                var guideData = DataUtil.GetGuideData(Lance.Account.GuideQuest.GetCurrentStep());
                if (guideData != null && guideData.autoAction.IsValid())
                {
                    var guideQuest = Lance.Account.GuideQuest.GetCurrentQuest();
                    if (guideQuest != null && questTye == guideQuest.GetQuestType())
                    {
                        if (Lance.Account.GuideQuest.CanReceiveReward() && Lance.Lobby.Guide.InGuide == false)
                        {
                            Lance.Lobby.Guide.StartGuide("GuideAction_GuideQuest");
                        }
                    }
                }
            }
        }

        public void GiveReward(RewardResult reward, ShowRewardType showType = ShowRewardType.None, string popupTitle = null, float popupCloseTime = 0f, bool ignoreUpdatePlayerStat = false)
        {
            Lance.Account.GiveReward(reward);

            if ((reward.equipments != null || 
                reward.petEquipments != null || 
                reward.accessorys != null) && ignoreUpdatePlayerStat == false)
            {
                UpdatePlayerStat();
            }

            if (reward.gold > 0)
                Lance.Lobby.UpdateGoldUI();
            if (reward.gem > 0)
                Lance.Lobby.UpdateGemUI();
            if (reward.stones > 0)
                Lance.Lobby.UpdateUpgradeStoneUI();
            if (reward.exp > 0)
                Lance.Lobby.UpdateExpUI();

            if (showType == ShowRewardType.Popup)
            {
                UIUtil.ShowRewardPopup(reward, popupTitle, popupCloseTime);
            }
            else if (showType == ShowRewardType.Banner)
            {
                if (mSleepModeManager.IsSleepMode() == false)
                    Lance.Lobby.ShowRewardUI(reward);
            }
            else if (showType == ShowRewardType.FirstStageClear)
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_StageClearUI>();

                popup.Init(reward);
            }
        }

        public RewardResult RewardDataChangeToRewardResult(string reward)
        {
            RewardResult rewardResult = new RewardResult();

            if (reward.IsValid())
            {
                var rewardData = Lance.GameData.RewardData.TryGet(reward);
                if (rewardData != null)
                {
                    rewardResult = rewardResult.AddReward(rewardData);
                }
            }

            return rewardResult;
        }

        public void GiveReward(string reward, string popupTitle = null, float popupCloseTime = 0f)
        {
            GiveReward(RewardDataChangeToRewardResult(reward), ShowRewardType.Popup, popupTitle, popupCloseTime);
        }

        public bool UpgradeCostume(string id)
        {
            CostumeInst costume = Lance.Account.Costume.GetCostume(id);
            if (costume == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotCostume");

                return false;
            }

            if (costume.IsMaxLevel())
            {
                UIUtil.ShowSystemErrorMessage("IsMaxLevelCostume");

                return false;
            }

            double requireStones = costume.GetUpgradeRequireStone();
            if (Lance.Account.IsEnoughUpgradeStones(requireStones) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughUpgradeStone");

                return false;
            }

            double require = costume.GetUpgradeRequire();
            if (Lance.Account.IsEnoughCostumeUpgrade(require) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughCostumeUpgrade");

                return false;
            }

            // 강화 적용
            Lance.Account.UpgradeCostume(id);

            Lance.Lobby.UpdateCurrencyUI();

            return true;
        }

        public bool UpgradeAccessory(string id, int upgradeCount)
        {
            AccessoryInst accessoryItem = Lance.Account.GetAccessory(id);
            if (accessoryItem == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotAccessory");

                return false;
            }

            if (accessoryItem.IsMaxLevel())
            {
                UIUtil.ShowSystemErrorMessage("IsMaxLevelAccessory");

                return false;
            }

            double requireStones = accessoryItem.GetUpgradeRequireStones(upgradeCount);
            if (Lance.Account.IsEnoughUpgradeStones(requireStones) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughUpgradeStone");

                return false;
            }

            // 강화 적용
            Lance.Account.UpgradeAccessory(id, upgradeCount);

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Inventory);

            return true;
        }

        public bool UpgradeEquipment(string id, int upgradeCount)
        {
            EquipmentInst equipItem = Lance.Account.GetEquipment(id);
            if (equipItem == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotEquipment");

                return false;
            }

            if (equipItem.IsMaxLevel())
            {
                UIUtil.ShowSystemErrorMessage("IsMaxLevelEquipment");

                return false;
            }

            double requireStones = equipItem.GetUpgradeRequireStones(upgradeCount);
            if (Lance.Account.IsEnoughUpgradeStones(requireStones) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughUpgradeStone");

                return false;
            }

            // 강화 적용
            Lance.Account.UpgradeEquipment(id, upgradeCount);

            // 퀘스트 확인
            CheckQuest(QuestType.UpgradeEquipment, upgradeCount);

            var data = DataUtil.GetEquipmentData(id);
            if (data.type == ItemType.Weapon)
            {
                CheckQuest(QuestType.UpgradeWeapon, upgradeCount);
            }

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Inventory);

            return true;
        }

        public bool UpgradePetEquipment(string id, int upgradeCount)
        {
            PetEquipmentInst equipItem = Lance.Account.GetPetEquipment(id);
            if (equipItem == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotPetEquipment");

                return false;
            }

            if (equipItem.IsMaxLevel())
            {
                UIUtil.ShowSystemErrorMessage("IsMaxLevelPetEquipment");

                return false;
            }

            double requireStones = equipItem.GetUpgradeRequireStones(upgradeCount);
            if (Lance.Account.IsEnoughUpgradeStones(requireStones) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughUpgradeStone");

                return false;
            }

            // 강화 적용
            Lance.Account.UpgradePetEquipment(id, upgradeCount);

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Pet);

            return true;
        }

        public (string id, int combineCount) CombineEquipment(string id)
        {
            var data = DataUtil.GetEquipmentData(id);
            if (data == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return (string.Empty, 0);
            }

            EquipmentInst equipItem = Lance.Account.GetEquipment(id);
            if (equipItem == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotEquipment");

                return (string.Empty, 0);
            }

            if (Lance.Account.IsEnoughCount(id, data.combineCount) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughEquipmentCombineRequireCount");

                return (string.Empty, 0);
            }

            (string id, int combineCount) result = Lance.Account.CombineEquipment(id);
            if (result.combineCount > 0)
            {
                UpdatePlayerStat(Lance.LocalSave.IsNewEquipment(result.id));

                // 퀘스트
                CheckQuest(QuestType.CombineEquipment, result.combineCount);

                if (data.type == ItemType.Weapon)
                {
                    CheckQuest(QuestType.CombineWeapon, result.combineCount);
                }

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Inventory);

                // 합성 결과 보여주기
                var resultData = DataUtil.GetEquipmentData(result.id);

                RewardResult rewardResult = new RewardResult();

                rewardResult = rewardResult.AddReward(new MultiReward(resultData.type, result.id, result.combineCount));

                UIUtil.ShowRewardPopup(rewardResult, StringTableUtil.Get("Title_CombineResult"));

                Param param = new Param();
                param.Add("id", id);
                param.Add("combineCount", result.combineCount);
                param.Add("resultItem", result.id);

                Lance.BackEnd.InsertLog("CombineEquipment", param, 7);
            }

            return result;
        }

        public (string id, int combineCount) CombineAccessory(string id)
        {
            var data = DataUtil.GetAccessoryData(id);
            if (data == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return (string.Empty, 0);
            }

            AccessoryInst accessotyItem = Lance.Account.GetAccessory(id);
            if (accessotyItem == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotAccessory");

                return (string.Empty, 0);
            }

            if (Lance.Account.IsEnoughAccessoryCount(id, data.combineCount) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughAccessoryCombineRequireCount");

                return (string.Empty, 0);
            }

            (string id, int combineCount) result = Lance.Account.CombineAccessory(id);
            if (result.combineCount > 0)
            {
                UpdatePlayerStat(Lance.LocalSave.IsNewAccessory(result.id));

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Inventory);

                // 합성 결과 보여주기
                var resultData = DataUtil.GetAccessoryData(result.id);

                RewardResult rewardResult = new RewardResult();

                rewardResult = rewardResult.AddReward(new MultiReward(resultData.type, result.id, result.combineCount));

                UIUtil.ShowRewardPopup(rewardResult, StringTableUtil.Get("Title_CombineResult"));

                Param param = new Param();
                param.Add("id", id);
                param.Add("combineCount", result.combineCount);
                param.Add("resultItem", result.id);

                Lance.BackEnd.InsertLog("CombineAccessory", param, 7);
            }

            return result;
        }

        public (string id, int combineCount) CombinePetEquipment(string id)
        {
            var data = Lance.GameData.PetEquipmentData.TryGet(id);
            if (data == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return (string.Empty, 0);
            }

            PetEquipmentInst equipItem = Lance.Account.GetPetEquipment(id);
            if (equipItem == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotPetEquipment");

                return (string.Empty, 0);
            }

            if (Lance.Account.IsEnoughPetEquipmentCount(id, data.combineCount) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPetEquipmentCombineRequireCount");

                return (string.Empty, 0);
            }

            (string id, int combineCount) result = Lance.Account.CombinePetEquipment(id);
            if (result.combineCount > 0)
            {
                UpdatePlayerStat(Lance.LocalSave.IsNewPetEquipment(result.id));

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Pet);

                // 합성 결과 보여주기
                var resultData = Lance.GameData.PetEquipmentData.TryGet(result.id);

                RewardResult rewardResult = new RewardResult();

                rewardResult = rewardResult.AddReward(new MultiReward(ItemType.PetEquipment, result.id, result.combineCount));

                UIUtil.ShowRewardPopup(rewardResult, StringTableUtil.Get("Title_CombineResult"));

                Param param = new Param();
                param.Add("id", id);
                param.Add("combineCount", result.combineCount);
                param.Add("resultItem", result.id);

                Lance.BackEnd.InsertLog("CombinePetEquipment", param, 7);
            }

            return result;
        }

        public int ReforgeEquipment(string id)
        {
            EquipmentInst equipItem = Lance.Account.GetEquipment(id);
            if (equipItem == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotEquipment");

                return -1;
            }

            if (equipItem.IsMaxReforge())
            {
                UIUtil.ShowSystemErrorMessage("IsMaxReforgeStepEquipment");

                return -1;
            }

            if (equipItem.IsMaxLevel() == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotMaxLevelEquipment");

                return -1;
            }

            double requireStones = equipItem.GetReforgeRequireStone();
            if (Lance.Account.IsEnoughReforgeStones(requireStones) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughReforgeStone");

                return -1;
            }

            // 재련 적용
            var result = Lance.Account.ReforgeEquipment(id);

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Inventory);

            Param param = new Param();
            param.Add("id", id);
            param.Add("remianReforgeStone", Lance.Account.Currency.GetReforgeStone());
            param.Add("result", result == 1 ? "성공" : "실패");
            param.Add("nowDateNum", TimeUtil.NowDateNum());

            Lance.BackEnd.InsertLog("ReforgeEquipment", param, 7);

            Lance.BackEnd.UpdateAllAccountInfos();

            return result;
        }

        public int ReforgeAccessory(string id)
        {
            AccessoryInst accessoryItem = Lance.Account.GetAccessory(id);
            if (accessoryItem == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotAccessory");

                return -1;
            }

            if (accessoryItem.IsMaxLevel() == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotMaxLevelAccessory");

                return -1;
            }

            if (accessoryItem.IsMaxReforge())
            {
                UIUtil.ShowSystemErrorMessage("IsMaxReforgeStepAccessory");

                return -1;
            }

            double requireStones = accessoryItem.GetReforgeRequireStone();
            if (Lance.Account.IsEnoughReforgeStones(requireStones) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughReforgeStone");

                return -1;
            }

            // 재련 적용
            var result = Lance.Account.ReforgeAccessory(id);

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Inventory);

            Param param = new Param();
            param.Add("id", id);
            param.Add("remianReforgeStone", Lance.Account.Currency.GetReforgeStone());
            param.Add("result", result == 1 ? "성공" : "실패");
            param.Add("nowDateNum", TimeUtil.NowDateNum());

            Lance.BackEnd.InsertLog("ReforgeAccessory", param, 7);

            return result;
        }

        public int ReforgePetEquipment(string id)
        {
            PetEquipmentInst equipItem = Lance.Account.GetPetEquipment(id);
            if (equipItem == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotPetEquipment");

                return -1;
            }

            if (equipItem.IsMaxLevel() == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotMaxLevelPetEquipment");

                return -1;
            }

            if (equipItem.IsMaxReforge())
            {
                UIUtil.ShowSystemErrorMessage("IsMaxReforgeStepPetEquipment");

                return -1;
            }

            double requireStones = equipItem.GetReforgeRequireStone();
            if (Lance.Account.IsEnoughReforgeStones(requireStones) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughReforgeStone");

                return -1;
            }

            int requireElementalStone = equipItem.GetReforgeRequireElementalStone();
            if (Lance.Account.IsEnoughElementalStones(requireElementalStone) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughElementalStone");

                return -1;
            }

            // 재련 적용
            var result = Lance.Account.ReforgePetEquipment(id);

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Inventory);

            Param param = new Param();
            param.Add("id", id);
            param.Add("remianReforgeStone", Lance.Account.Currency.GetReforgeStone());
            param.Add("result", result == 1 ? "성공" : "실패");
            param.Add("nowDateNum", TimeUtil.NowDateNum());

            Lance.BackEnd.InsertLog("ReforgePetEquipment", param, 7);

            return result;
        }

        public bool EquipEquipment(string id)
        {
            if (Lance.Account.HaveEquipment(id) == false)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotEquipment");

                return false;
            }

            if (Lance.Account.IsEquippedEquipment(id))
            {
                UIUtil.ShowSystemErrorMessage("AlreadyEquippedEquipment");

                return false;
            }

            if (Lance.Account.EquipEquipment(id))
            {
                UpdatePlayerStat();

                var data = DataUtil.GetEquipmentData(id);
                if (data.type == ItemType.Weapon)
                {
                    CheckQuest(QuestType.EquipWeapon, 1);
                }
                else if (data.type == ItemType.Armor)
                {
                    CheckQuest(QuestType.EquipArmor, 1);
                }
                else if (data.type == ItemType.Gloves)
                {
                    CheckQuest(QuestType.EquipGloves, 1);
                }
                else
                {
                    CheckQuest(QuestType.EquipShoes, 1);
                }

                return true;
            }

            return false;
        }

        public bool EquipAccessory(string id)
        {
            if (Lance.Account.HaveAccessory(id) == false)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotAccessory");

                return false;
            }

            if (Lance.Account.IsEquippedAccessory(id))
            {
                UIUtil.ShowSystemErrorMessage("AlreadyEquippedAccessory");

                return false;
            }

            if (Lance.Account.EquipAccessory(id))
            {
                UpdatePlayerStat();

                return true;
            }

            return false;
        }

        public bool UnEquipAccessory(string id)
        {
            if (Lance.Account.HaveAccessory(id) == false)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotAccessory");

                return false;
            }

            if (Lance.Account.IsEquippedAccessory(id) == false)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            Lance.Account.UnEquipAccessory(id);

            UpdatePlayerStat();

            return true;
        }

        public bool EquipPetEquipment(string id)
        {
            if (Lance.Account.HavePetEquipment(id) == false)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotEquipment");

                return false;
            }

            if (Lance.Account.IsEquippedPetEquipment(id))
            {
                UIUtil.ShowSystemErrorMessage("AlreadyEquippedPetEquipment");

                return false;
            }

            if (Lance.Account.EquipPetEquipment(id))
            {
                UpdatePlayerStat();

                return true;
            }

            return false;
        }

        public bool MakeArtifact()
        {
            return false;
        }

        public bool DismantleArtifact()
        {
            return false;
        }

        public bool UpgradeExcalibur()
        {
            // 엑스칼리버가 최고 단계인지 확인
            if (Lance.Account.Excalibur.IsMaxStepExcalibur())
            {
                UIUtil.ShowSystemErrorMessage("IsMaxStepExcalibur");

                return false;
            }

            // 모든 포스가 최고 레벨인지 확인
            if (Lance.Account.Excalibur.IsAllMaxLevelExcaliburForces() == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotSatisfiedAllMaxLevelExcaliburForces");

                return false;
            }

            // 극 한계돌파 조건을 만족했는지 확인
            if (Lance.Account.IsSatisfiedExcaliburUltimateLimitBreak() == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotSatisfiedExcaliburUltimateLimitBreak");

                return false;
            }

            // 강화
            return Lance.Account.UpgradeExcalibur();
        }

        public bool UpgradeExcaliburForce(ExcaliburForceType type)
        {
            // 최고 레벨이라 강화 불가능
            if (Lance.Account.Excalibur.IsMaxLevelExcaliburForce(type))
            {
                UIUtil.ShowSystemErrorMessage("IsMaxLevelExcaliburForce");

                return false;
            }

            // 재화가 충분한지 확인
            int require = Lance.Account.Excalibur.GetUpgradeRequire(type);
            if (Lance.Account.IsEnoughAncientEssence(require) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughAncientEssence");

                return false;
            }

            // 엑스칼리버 힘 강화
            return Lance.Account.UpgradeExcaliburForce(type);
        }

        public ArtifactLevelUpResult LevelUpArtifact(string id)
        {
            // 최고 레벨인지 확인한다.
            
            if (Lance.Account.IsMaxLevelArtifact(id))
            {
                UIUtil.ShowSystemErrorMessage("IsMaxLevelArtifact");

                return ArtifactLevelUpResult.Error;
            }

            int level = Lance.Account.GetLevelArtifact(id);
            var levelUpData = DataUtil.GetArtifactLevelUpData(id, level);
            int requireCount = levelUpData?.requireCount ?? 1;
            // 갯수가 충분한지 확인한다.
            if (Lance.Account.IsEnoughArtifactCount(id, requireCount) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughUpgradeArtifact");

                return ArtifactLevelUpResult.Error;
            }

            // 퀘스트 확인
            CheckQuest(QuestType.TryUpgradeArtifact, 1);

            // 유물 강화
            if (Lance.Account.LevelUpArtifact(id))
            {
                return ArtifactLevelUpResult.Success;
            }
            else
            {
                return ArtifactLevelUpResult.Fail;
            }
        }

        public bool UpgradeGloryOrb()
        {
            // 재화가 충분한지 확인
            int require = Lance.Account.JoustGloryOrb.GetUpgradeRequire();
            if (require <= 0)
            {
                UIUtil.ShowSystemErrorMessage("IsMaxStepJoustGloryOrb");

                return false;
            }

            if (Lance.Account.IsEnoughJoustGloryToken(require) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughGloryToken");

                return false;
            }

            // 보주 강화
            if (Lance.Account.UpgradeJoustGloryOrb())
            {
                Param param = new Param();
                param.Add("curStep", Lance.Account.JoustGloryOrb.GetStep());
                param.Add("remainGloryToken", Lance.Account.Currency.GetGloryToken());

                Lance.BackEnd.InsertLog("UpgradeGloryOrb", param, 7);

                Lance.Lobby.RefreshJoustingRedDot();

                return true;
            }

            return false;
        }

        public bool DismantleSkill(string id, int count)
        {
            //if (Lance.Account.HaveLimitBreakSkill() == false)
            //{
            //    UIUtil.ShowSystemErrorMessage("HaveNotLimitBreakSkill");

            //    return false;
            //}

            if (count <= 0)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughSkillDismantleRequireCount");

                return false;
            }

            var data = DataUtil.GetSkillData(id);
            if (data == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            if (Lance.Account.HaveSkill(data.type, id) == false)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotSkill");

                return false;
            }

            int skillCount = Lance.Account.SkillInventory.GetSkillCount(data.type, id);
            if (count == 0 || skillCount < count)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughSkillDismantleRequireCount");

                return false;
            }

            if (Lance.Account.DismantleSkill(data.type, id, count))
            {
                int skillPieceAmount = DataUtil.GetSkillPieceAmount(id, count);

                RewardResult rewardResult = new RewardResult();
                rewardResult.skillPiece = skillPieceAmount;

                GiveReward(rewardResult, showType: ShowRewardType.Popup);

                Param param = new Param();
                param.Add("id", id);
                param.Add("count", count);
                param.Add("dismantleAmount", skillPieceAmount);

                Lance.BackEnd.InsertLog("DismantleSkill", param, 7);

                return true;
            }

            return false;
        }

        public bool LevelUpSkill(string id, int count)
        {
            var data = DataUtil.GetSkillData(id);
            if (data == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            if (Lance.Account.HaveSkill(data.type, id) == false)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotSkill");

                return false;
            }
                
            if (Lance.Account.IsMaxLevelSkill(data.type, id))
            {
                UIUtil.ShowSystemErrorMessage("IsMaxLevelSkill");

                return false;
            }

            if (Lance.Account.IsEnoughSkillUpgradeRequireCount(data.type, id, count) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughSkillUpgradeRequireCount");

                return false;
            }

            if (Lance.Account.UpgradeSkill(data.type, id, count))
            {
                CheckQuest(QuestType.UpgradeSkill, 1);

                // 퀘스트 확인
                if (data.type == SkillType.Active)
                    CheckQuest(QuestType.UpgradeActiveSkill, 1);
                else
                    CheckQuest(QuestType.UpgradePassiveSkill, 1);

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Skill);

                Param param = new Param();
                param.Add("id", id);
                param.Add("levelUpCount", count);
                param.Add("remainSkillPiece", Lance.Account.Currency.GetSkillPiece());
                param.Add("curLevel", Lance.Account.GetSkillLevel(data.type, id));

                Lance.BackEnd.InsertLog("UpgradeSkill", param, 7);

                return true;
            }

            return false;
        }

        public bool LevelUpAbility(string id)
        {
            // 조건을 만족했는지 확인
            if (Lance.Account.Ability.IsMeetRequirements(id) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotMeetRequirementsAbility");

                return false;
            }

            // 최고레벨인지 확인
            if (Lance.Account.Ability.IsMaxLevel(id))
            {
                UIUtil.ShowSystemErrorMessage("IsMaxLevelAbility");

                return false;
            }

            var data = Lance.GameData.AbilityData.TryGet(id);
            if (data == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            int level = Lance.Account.Ability.GetAbilityLevel(id);
            int requierAP = AbilityUtil.CalcRequireAP(id, level);

            // AP가 충분한지 확인
            if (Lance.Account.ExpLevel.IsEnoughAP(requierAP) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughAP");

                return false;
            }

            Lance.Account.LevelUpAbility(id);

            CheckQuest(QuestType.LevelUpAbility, 1);

            UpdatePlayerStat(
                data.statType != StatType.ExpAmount && 
                data.statType != StatType.GoldAmount &&
                data.statType != StatType.MoveSpeed &&
                data.statType != StatType.MoveSpeedRatio);

            Param param = new Param();
            param.Add("id", id);
            param.Add("curLevel", Lance.Account.Ability.GetAbilityLevel(id));

            Lance.BackEnd.InsertLog("LevelUpAbility", param, 7);

            return true;
        }

        public void ActiveSpeedMode()
        {
            if (ContentsLockUtil.IsLockContents(ContentsLockType.SpeedMode))
                return;

            if (Lance.Account.SpeedMode.GetDuration() > 0)
            {
                Lance.Account.SpeedMode.ToggleActive();

                CheckQuest(QuestType.ActiveSpeedMode, 1);

                OnChangeSpeedMode();
            }
            else
            {
                if (Lance.Account.SpeedMode.GetFirstActive() == false)
                {
                    ActiveSpeedMode();
                }
                else
                {
                    if (Lance.Account.PackageShop.IsPurchasedRemoveAD() || Lance.IAPManager.IsPurchasedRemoveAd())
                    {
                        OnConfirm();
                    }
                    else
                    {
                        string title = StringTableUtil.Get("Title_Confirm");
                        string desc = StringTableUtil.Get("Desc_SpeedModeActive");

                        UIUtil.ShowConfirmPopup(title, desc, OnConfirm, null);
                    }

                    void OnConfirm()
                    {
                        Lance.GameManager.ShowRewardedAd(AdType.SpeedMode, () =>
                        {
                            ActiveSpeedMode();
                        });
                    }
                }

                void ActiveSpeedMode()
                {
                    // 스피드 모드 적용
                    if ( Lance.Account.SpeedMode.ActiveSpeedMode() )
                    {
                        SoundPlayer.PlayBuffActive();

                        OnChangeSpeedMode();

                        CheckQuest(QuestType.ActiveSpeedMode, 1);
                    }
                }
            }
        }

        public void OnChangeSpeedMode()
        {
            Time.timeScale = Lance.Account.SpeedMode.InSpeedMode() ? Lance.GameData.CommonData.speedModeValue : 1f;

            bool purchasedRemovedAD = Lance.Account.PackageShop.IsPurchasedRemoveAD() || Lance.IAPManager.IsPurchasedRemoveAd();

            Lance.Lobby.UpdateSpeedModeUI(purchasedRemovedAD);
        }

        public bool ActiveBuff(string id, StatType type)
        {
            // 버프 횟수가 남아 있는지 확인한다.
            if (Lance.Account.Buff.IsEnoughActiveCount(id) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughBuffRemainCount");

                return false;
            }

            // 버프가 이미 적용중인지 확인한다.
            if (Lance.Account.Buff.IsAlreadyActiveBuff(id))
            {
                UIUtil.ShowSystemErrorMessage("AlreadyActiveBuff");

                return false;
            }

            // 처음에는 무료
            if (Lance.Account.Buff.GetStackedActiveCount(id) == 0)
            {
                ActiveBuff();

                return true;
            }
            else
            {
                Lance.GameManager.ShowRewardedAd(type.ChangeToAdType(), ActiveBuff);

                return true;
            }

            void ActiveBuff()
            {
                SoundPlayer.PlayBuffActive();

                // 버프를 적용한다.
                Lance.Account.Buff.ActiveBuff(id);

                CheckQuest(QuestType.ActiveBuff, 1);

                UpdatePlayerStat(type == StatType.AtkRatio);
            }
        }

        public bool LevelUpBuff(string id, StatType statType)
        {
            // 최고 레벨인지 확인
            if (Lance.Account.Buff.IsMaxLevel(id))
            {
                UIUtil.ShowSystemErrorMessage("IsMaxLevelBuff");

                return false;
            }

            // 레벨업이 가능한지 확인
            if (Lance.Account.Buff.CanLevelUpBuff(id) == false)
            {
                UIUtil.ShowSystemErrorMessage("NotEnoughMonsterKillCount");

                return false;
            }

            int prevLevel = Lance.Account.Buff.GetLevel(id);
            
            if (Lance.Account.Buff.LevelUpBuff(id))
            {
                int nextLevel = Lance.Account.Buff.GetLevel(id);
                Param param = new Param();
                param.Add("id", id);
                param.Add("nowDateNum", TimeUtil.NowDateNum());
                param.Add("prevLevel", prevLevel);
                param.Add("nextLevel", nextLevel);

                Lance.BackEnd.InsertLog("LevelUpBuff", param, 7);

                UpdatePlayerStat(statType == StatType.AddDmg);

                return true;
            }

            return false;
        }

        public void ReceiveSpawnReward(ItemType itemType, int level)
        {
            var result = Lance.Account.ReceiveSpawnReward(itemType, level);
            if (result.IsEmpty() == false)
            {
                Lance.Lobby.UpdateCurrencyUI();
                
                Lance.Lobby.RefreshTab();

                if (itemType.IsEquipment() || itemType.IsAccessory())
                {
                    Lance.Lobby.RefreshTabRedDot(LobbyTab.Inventory);
                }
                else if (itemType.IsArtifact())
                {
                    Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
                }
                else if (itemType.IsSkill())
                {
                    Lance.Lobby.RefreshTabRedDot(LobbyTab.Skill);
                }

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Spawn);
                Lance.Lobby.RefreshNavBarRedDot();

                UIUtil.ShowRewardPopup(result);

                Param param = new Param();
                param.Add("itemType", itemType);
                param.Add("level", level);
                param.Add("reward", result);

                Lance.BackEnd.InsertLog("ReceiveSpawnReward", param, 7);
            }
        }

        public void ReceiveQuestReward(string id, Action onFinish)
        {
            QuestInfo questInfo = Lance.Account.GetQuestInfo(id);
            if (questInfo.GetIsReceived())
            {
                UIUtil.ShowSystemErrorMessage("AlreadyReceivedQuestReward");

                return;
            }

            if (questInfo.IsSatisfied() == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotSatisfiedQuest");

                return;
            }

            RewardResult result = Lance.Account.ReceiveQuestReward(id);
            if (result.IsEmpty() == false)
            {
                Lance.Lobby.UpdateCurrencyUI();

                UIUtil.ShowRewardPopup(result);

                var questData = DataUtil.GetQuestData(id);
                if (questData.updateType == QuestUpdateType.Daily)
                {
                    if (questData.type != QuestType.DailyquestClear)
                        CheckQuest(QuestType.DailyquestClear, 1);
                }
                else if (questData.updateType == QuestUpdateType.Weekly)
                {
                    if (questData.type != QuestType.WeeklyquestClear)
                        CheckQuest(QuestType.WeeklyquestClear, 1);
                }
                else
                {
                    if (questData.type != QuestType.RepeatquestClear)
                        CheckQuest(QuestType.RepeatquestClear, 1);
                }

                onFinish?.Invoke();

                UIUtil.PopupRefreshRedDots<Popup_QuestUI>();
                Lance.Lobby.RefreshTab();
                Lance.Lobby.RefreshQuestRedDot();

                Param param = new Param();
                param.Add("id", id);
                param.Add("reward", result);

                Lance.BackEnd.InsertLog("ReceiveQuestReward", param, 7);
            }
        }

        public void ReceiveQuestBonusReward(string id, Action onFinish)
        {
            QuestInfo questInfo = Lance.Account.GetQuestInfo(id);
            if (questInfo.GetIsReceivedBonus())
            {
                UIUtil.ShowSystemErrorMessage("AlreadyReceivedQuestReward");

                return;
            }

            if (questInfo.IsSatisfiedBonus() == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotSatisfiedQuest");

                return;
            }

            ShowRewardedAd(AdType.Daily_QuestBonus, () =>
            {
                RewardResult result = Lance.Account.ReceiveQuestBonusReward(id);
                if (result.IsEmpty() == false)
                {
                    Lance.Lobby.UpdateCurrencyUI();

                    UIUtil.ShowRewardPopup(result);

                    onFinish?.Invoke();

                    UIUtil.PopupRefreshRedDots<Popup_QuestUI>();
                    Lance.Lobby.RefreshTab();
                    Lance.Lobby.RefreshQuestRedDot();

                    Param param = new Param();
                    param.Add("id", id);
                    param.Add("reward", result);

                    Lance.BackEnd.InsertLog("ReceiveQuestBonusReward", param, 7);
                }
            });
        }

        public void ReceiveAllQuestReward(QuestUpdateType updateType, Action onFinish)
        {
            var result = Lance.Account.ReceiveAllQuestReward(updateType);
            if (result.totalReward.IsEmpty() == false)
            {
                Lance.Lobby.UpdateCurrencyUI();
                Lance.Lobby.RefreshTab();
                Lance.Lobby.RefreshQuestRedDot();
                UIUtil.ShowRewardPopup(result.totalReward);

                if (updateType == QuestUpdateType.Daily)
                {
                    CheckQuest(QuestType.DailyquestClear, result.totalCount);
                }
                else if (updateType == QuestUpdateType.Weekly)
                {
                    CheckQuest(QuestType.WeeklyquestClear, result.totalCount);
                }
                else
                {
                    CheckQuest(QuestType.RepeatquestClear, result.totalCount);
                }

                onFinish?.Invoke();

                Param param = new Param();
                param.Add("result", result.Item1);

                Lance.BackEnd.InsertLog("AllReceiveQuestReward", param, 7);
            }
        }

        public void ReceiveGuideQuestReward(Action onFinish)
        {
            int currentStep = Lance.Account.GuideQuest.GetCurrentStep();

            QuestInfo questInfo = Lance.Account.GuideQuest.GetCurrentQuest();
            if (questInfo.GetIsReceived())
            {
                UIUtil.ShowSystemErrorMessage("AlreadyReceivedQuestReward");

                return;
            }

            if (questInfo.IsSatisfied() == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotSatisfiedQuest");

                return;
            }

            RewardResult result = Lance.Account.ReceiveGuideQuestReward();
            if (result.IsEmpty() == false)
            {
                Lance.Lobby.OnReceiveGuideQuestReward();

                var popup = UIUtil.ShowRewardPopup(result);
                popup.SetOnCloseAction(() =>
                {
                    Lance.Lobby.StartGuideAction(isAuto: true);
                });

                Lance.Lobby.RefreshTab();

                onFinish?.Invoke();
#if !UNITY_EDITOR
                Param param = new Param();
                param.Add("step", currentStep);
                param.Add("reward", result);

                Lance.BackEnd.InsertLog("ReceiveGuideQuestReward", param, 7);

                Lance.Firebase.LogEvent($"ClearGuideQuest_Step_{currentStep}");
#endif
            }
        }

        public bool ReceiveAttendanceReward(string id, int day)
        {
            if (Lance.Account.Attendance.IsReceivedReward(id, day))
            {
                UIUtil.ShowSystemErrorMessage("IsReceivedAttendanceReward");

                return false;
            }

            if (Lance.Account.Attendance.CanReceiveReward(id, day) == false)
            {
                UIUtil.ShowSystemErrorMessage("CanNotReceiveAttendanceReward");

                return false;
            }

            if (Lance.Account.Attendance.ReceiveReward(id, day))
            {
                var data = DataUtil.GetAttendanceDayData(id, day);
                var rewardData = Lance.GameData.RewardData.TryGet(data.reward);
                if (rewardData != null)
                {
                    GiveReward(data.reward);

                    Param param = new Param();
                    param.Add("day", day);
                    param.Add("reward", data.reward);
                    param.Add("totalGem", Lance.Account.Currency.GetGem());
                    param.Add("nowDateNum", TimeUtil.NowDateNum());

                    Lance.BackEnd.InsertLog("ReceiveAttendanceReward", param, 7);
                }

                return true;
            }

            return false;
        }

        public void ReceiveBountyQuestReward(string id, Action onFinish)
        {
            BountyQuestInfo questInfo = Lance.Account.GetBountyQuestInfo(id);
            if (questInfo.GetIsReceived())
            {
                UIUtil.ShowSystemErrorMessage("AlreadyReceivedQuestReward");

                return;
            }

            if (questInfo.IsSatisfied() == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotSatisfiedQuest");

                return;
            }

            RewardResult result = Lance.Account.ReceiveBountyQuestReward(id);
            if (result.IsEmpty() == false)
            {
                Lance.Lobby.UpdateCurrencyUI();

                UIUtil.ShowRewardPopup(result);

                onFinish?.Invoke();

                CheckQuest(QuestType.ClearBountyQuest, 1);

                Lance.Lobby.RefreshTab();
                Lance.Lobby.RefreshQuestRedDot();
                Lance.Lobby.RefreshEventRedDot();
                Lance.Lobby.RefreshBountyQuestRedDot();

                Param param = new Param();
                param.Add("id", id);
                param.Add("reward", result);

                Lance.BackEnd.InsertLog("ReceiveBountyQuestReward", param, 7);
            }
        }

        

        public (List<(string questId, string rewardId, int count)>, RewardResult totalReward, int totalCount) ReceiveAllNewEventQuestReward(string eventId)
        {
            (List<(string questId, string rewardId, int count)>, RewardResult totalReward, int totalCount) result = Lance.Account.ReceiveAllNewEventQuestReward(eventId);
            if (result.totalReward.IsEmpty() == false)
            {
                Lance.Lobby.UpdateCurrencyUI();
                Lance.Lobby.RefreshTab();
                Lance.Lobby.RefreshEventRedDot();

                UIUtil.ShowRewardPopup(result.totalReward);

                Param param = new Param();
                param.Add("eventId", eventId);
                param.Add("result", result.Item1);

                Lance.BackEnd.InsertLog("AllReceiveEventQuestReward", param, 7);
            }

            return result;
        }

        public bool ReceiveNewEventQuestReward(string eventId, string id)
        {
            QuestInfo questInfo = Lance.Account.Event.GetQuestInfo(eventId, id);
            if (questInfo == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            if (questInfo.GetIsReceived())
            {
                UIUtil.ShowSystemErrorMessage("AlreadyReceivedQuestReward");

                return false;
            }

            if (questInfo.GetOpenDay() > 0)
            {
                if (questInfo.GetOpenDay() > Lance.Account.Event.CalcEventQuestPassDay(eventId))
                {
                    UIUtil.ShowSystemErrorMessage("IsNotSatisfiedPassDay");

                    return false;
                }
            }

            if (questInfo.IsSatisfied() == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotSatisfiedQuest");

                return false;
            }

            RewardResult result = Lance.Account.ReceiveNewEventQuestReward(eventId, id);
            if (result.IsEmpty() == false)
            {
                UIUtil.ShowRewardPopup(result);

                Lance.Lobby.UpdateCurrencyUI();
                Lance.Lobby.RefreshTab();
                Lance.Lobby.RefreshEventRedDot();

                Param param = new Param();
                param.Add("eventId", eventId);
                param.Add("id", id);
                param.Add("reward", result);

                Lance.BackEnd.InsertLog("ReceiveEventQuestReward", param, 7);

                return true;
            }

            return false;
        }

        public List<string> AllCompleteCollection(ItemType itemtype)
        {
            var result = Lance.Account.AllCompleteCollect(itemtype);
            if (result.Count <= 0)
            {
                UIUtil.ShowSystemErrorMessage("IsNotAnySatisfiedCollection");

                return result;
            }
            else
            {
                Param param = new Param();
                param.Add("result", result.ToArray());
                param.Add("nowDateNum", TimeUtil.NowDateNum());

                UpdatePlayerStat();

                Lance.Lobby.RefreshCollectionRedDot();

                Lance.BackEnd.InsertLog("AllCompleteCollect", param, 7);

                return result;
            }
        }

        public bool CompleteCollection(string id)
        {
            if (Lance.Account.Collection.IsAlreadyCollect(id))
            {
                UIUtil.ShowSystemErrorMessage("IsAlreadyCollect");

                return false;
            }

            if (Lance.Account.IsSatisfiedCollection(id) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotSatisfiedCollection");

                return false;
            }

            if (Lance.Account.CompleteCollect(id))
            {
                Param param = new Param();
                param.Add("id", id);
                param.Add("nowDateNum", TimeUtil.NowDateNum());

                UpdatePlayerStat();

                Lance.Lobby.RefreshCollectionRedDot();

                Lance.BackEnd.InsertLog("CompleteCollect", param, 7);

                return true;
            }

            return false;
        }

        public bool SkipBattle()
        {
            // 최고 스테이지를 기준으로 SkipBattle을 진행한다.
            int skipBattleStackedCount = Lance.Account.StageRecords.GetSkipBattleStackedCount();
            int price = DataUtil.GetSkipBattlePrice(skipBattleStackedCount);

            // 잼이 충분한가?
            if (Lance.Account.IsEnoughGem(price) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughGem");

                return false;
            }
                
            // 횟수가 충분한가?
            if (Lance.Account.StageRecords.GetSkipBattleRemainCount() <= 0)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughSkipBattleCount");

                return false;
            }

            if (Lance.Account.UseGem(price))
            {
                if (Lance.Account.StageRecords.SkipBattle())
                {
                    var reward = Lance.Account.GetSkipBattleRewards();

                    int monsterKillCalcMinute = DataUtil.GetSkipBattleMonsterKillCount(Lance.Account.StageRecords.GetBestTotalStage());
                    int timeForMinute = Lance.GameData.SkipBattleCommonData.maxTimeForMinute;
                    int monsterKillCount = monsterKillCalcMinute * timeForMinute;

                    Lance.Account.Buff.OnMonsterDeath(monsterKillCount);
                    Lance.Lobby.RefreshBuffRedDot();
                    Lance.Account.ExpLevel.StackMonsterKillCount(monsterKillCount);
                    Lance.GameManager.CheckQuest(QuestType.KillMonster, monsterKillCount);
                    Lance.GameManager.GiveReward(reward, ShowRewardType.Popup);

                    Param param = new Param();
                    param.Add("reward", reward);
                    param.Add("remainCount", Lance.Account.StageRecords.GetSkipBattleRemainCount());
                    param.Add("nowDateNum", TimeUtil.NowDateNum());

                    Lance.BackEnd.InsertLog("SkipBattle", param, 7);

                    Lance.Lobby.RefreshSkipBattleRedDot();

                    return true;
                }
            }

            return false;
        }

        public void ShowGameOverPopup()
        {
            var popup = Lance.PopupManager.CreatePopup<Popup_GameOverUI>();

            popup.Init();
        }

        public bool PurchaseDemonicRealmSpoilsItem(string id, int count)
        {
            var data = Lance.GameData.DemonicRealmSpoilsData.TryGet(id);
            if (data == null || count <= 0)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            int totalPrice = data.price * count;

            if (Lance.Account.IsEnoughSoulStone(totalPrice) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughSoulStone");

                return false;
            }

            if (Lance.Account.DemonicRealmSpoils.IsEnoughPurchaseCount(id, count) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPurchaseCount");

                return false;
            }

            if (Lance.Account.DemonicRealmSpoils.GetRequireFirendShipLevel(data.id) > data.requireFriendShipLevel)
            {
                UIUtil.ShowSystemErrorMessage("IsNotSatisfiedFriendShipLevel");

                return false;
            }

            if (Lance.Account.PurchaseDemonicRealmSpoilsItem(data.id, count))
            {
                if (data.reward.IsValid())
                {
                    RewardResult totalReward = new RewardResult();

                    for (int i = 0; i < count; ++i)
                    {
                        var reward = RewardDataChangeToRewardResult(data.reward);

                        totalReward = totalReward.AddReward(reward);
                    }

                    GiveReward(totalReward, ShowRewardType.Popup);

                    Param param = new Param();
                    param.Add("id", id);
                    param.Add("purchaseCount", count);
                    param.Add("remainSoulStone", Lance.Account.Currency.GetSoulStone());
                    param.Add("reward", totalReward);

                    Lance.BackEnd.InsertLog("PurchaseDemonicRealmSpoilsItem", param, 7);

                    // 보상을 다 얻고 난 다음에는 저장
                    Lance.BackEnd.UpdateAllAccountInfos();

                    return true;
                }
            }

            return false;
        }

        public bool PurchaseJoustItem(string id, int count)
        {
            var data = Lance.GameData.JoustShopData.TryGet(id);
            if (data == null || count <= 0)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            int totalPrice = data.price * count;

            if (Lance.Account.IsEnoughJoustCoin(totalPrice) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughJoustCoin");

                return false;
            }

            if (Lance.Account.JoustShop.GetRemainPurchaseCount(data.id) < count)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPurchaseCount");

                return false;
            }

            if (Lance.Account.PurchaseJoustItem(data.id, count))
            {
                if (data.reward.IsValid())
                {
                    RewardResult totalReward = new RewardResult();

                    for (int i = 0; i < count; ++i)
                    {
                        var reward = RewardDataChangeToRewardResult(data.reward);

                        totalReward = totalReward.AddReward(reward);
                    }

                    GiveReward(totalReward, ShowRewardType.Popup);

                    var rewardData = Lance.GameData.RewardData.TryGet(data.reward);
                    if (rewardData != null)
                    {
                        if (rewardData.dungeonTicket.Any(x => x > 0))
                        {
                            CheckQuest(QuestType.PurchaseTicket, count);
                        }
                    }

                    Param param = new Param();
                    param.Add("id", id);
                    param.Add("remainJoustCoin", Lance.Account.Currency.GetJoustingCoin());
                    param.Add("reward", totalReward);

                    Lance.BackEnd.InsertLog("PurchaseJoustItem", param, 7);

                    // 보상을 다 얻고 난 다음에는 저장
                    Lance.BackEnd.UpdateAllAccountInfos();

                    return true;
                }
            }

            return false;
        }

        public bool PurchaseNormalItem(string id, int count)
        {
            var data = Lance.GameData.NormalShopData.TryGet(id);
            if (data == null || count <= 0)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            int totalPrice = data.price * count;

            if (Lance.Account.IsEnoughGem(totalPrice) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughGem");

                return false;
            }

            if (Lance.Account.NormalShop.GetRemainPurchaseCount(data.id) < count)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPurchaseCount");

                return false;
            }

            if (Lance.Account.PurchaseNormalItem(data.id, count))
            {
                if (data.reward.IsValid())
                {
                    RewardResult totalReward = new RewardResult();

                    for (int i = 0; i < count; ++i)
                    {
                        var reward = RewardDataChangeToRewardResult(data.reward);

                        totalReward = totalReward.AddReward(reward);
                    }

                    GiveReward(totalReward, ShowRewardType.Popup);

                    var rewardData = Lance.GameData.RewardData.TryGet(data.reward);
                    if (rewardData != null)
                    {
                        if (rewardData.dungeonTicket.Any(x => x > 0))
                        {
                            CheckQuest(QuestType.PurchaseTicket, count);
                        }
                    }

                    Param param = new Param();
                    param.Add("id", id);
                    param.Add("remainGem", Lance.Account.Currency.GetGem());
                    param.Add("reward", totalReward);

                    Lance.BackEnd.InsertLog("PurchaseNormalItem", param, 7);

                    Lance.Lobby.UpdateGemUI();

                    // 보상을 다 얻고 난 다음에는 저장
                    Lance.BackEnd.UpdateAllAccountInfos();

                    return true;
                }
            }

            return false;
        }

        public bool PurchaseMileageItem(string id, int count)
        {
            var data = Lance.GameData.MileageShopData.TryGet(id);
            if (data == null || count <= 0)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            int totalPrice = data.price * count;
                
            if (Lance.Account.MileageShop.IsEnoughMileage(totalPrice) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughMileage");

                return false;
            }

            if (Lance.Account.MileageShop.GetRemainPurchaseCount(data.id) < count)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPurchaseCount");

                return false;
            }

            if (Lance.Account.MileageShop.Purchase(data.id, count))
            {
                if (data.reward.IsValid())
                {
                    RewardResult totalReward = new RewardResult();

                    for (int i = 0; i < count; ++i)
                    {
                        var reward = RewardDataChangeToRewardResult(data.reward);

                        totalReward = totalReward.AddReward(reward);
                    }

                    GiveReward(totalReward, ShowRewardType.Popup);

                    Param param = new Param();
                    param.Add("id", id);
                    param.Add("remainMileage", Lance.Account.Currency.GetMileage());
                    param.Add("reward", totalReward);

                    Lance.BackEnd.InsertLog("PurchaseMileageItem", param, 7);

                    // 보상을 다 얻고 난 다음에는 저장
                    Lance.BackEnd.UpdateAllAccountInfos();

                    return true;
                }
            }

            return false;
        }

        public bool PurchaseEventItem(string eventId, string id, int count)
        {
            var data = Lance.GameData.EventShopData.TryGet(id);
            if (data == null || count <= 0)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            int totalPrice = data.price * count;

            if (Lance.Account.Event.IsEnoughEventCurrency(eventId, totalPrice) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughEventCurrency");

                return false;
            }

            if (Lance.Account.Event.IsEnoughPurchaseCount(eventId, data.id, count) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPurchaseCount");

                return false;
            }

            if (Lance.Account.Event.PurchaseEventItem(eventId, data.id, count))
            {
                Lance.Account.Event.SetIsChangedData(true);

                if (data.reward.IsValid())
                {
                    RewardResult totalReward = new RewardResult();

                    for(int i = 0; i < count; ++i)
                    {
                        var reward = RewardDataChangeToRewardResult(data.reward);

                        totalReward = totalReward.AddReward(reward);
                    }

                    GiveReward(totalReward, ShowRewardType.Popup);

                    Param param = new Param();
                    param.Add("eventId", eventId);
                    param.Add("id", id);
                    param.Add("reward", totalReward);

                    Lance.BackEnd.InsertLog("PurchaseEventItem", param, 7);

                    // 보상을 다 얻고 난 다음에는 저장
                    Lance.BackEnd.UpdateAllAccountInfos();

                    return true;
                }
            }

            return false;
        }

        public void PurchasePackageItem(PackageShopData data, Action onFinishPurchased)
        {
            if (Lance.Account.PackageShop.IsEnoughPurchaseCount(data.id) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPurchaseCount");

                return;
            }

            if (data.productId.IsValid())
            {
                string name = string.Empty;
                if (data.type == PackageType.StepReward)
                {
                    int step = Lance.Account.PackageShop.GetStep(data.id);

                    name = StringTableUtil.GetName($"{data.id}{step}");
                }
                else
                {
                    name = StringTableUtil.GetName(data.id);
                }

                if (data.monthlyFeeDailyReward.IsValid())
                {
                    List<ItemInfo> immediatelyRewards = new List<ItemInfo>();
                    
                    if (data.reward.IsValid())
                    {
                        RewardData immediatelyReward = Lance.GameData.RewardData.TryGet(data.reward);
                        if (immediatelyReward != null)
                        {
                            immediatelyRewards = ItemInfoUtil.CreateItemInfos(immediatelyReward);
                        }

                        if (data.mileage > 0)
                        {
                            var mileageReward = new ItemInfo(ItemType.Mileage, data.mileage);

                            immediatelyRewards.Add(mileageReward);
                        }
                    }

                    List<ItemInfo> dailyRewards = new List<ItemInfo>();

                    RewardData dailyReward = Lance.GameData.RewardData.TryGet(data.monthlyFeeDailyReward);
                    if (dailyReward != null)
                    {
                        dailyRewards = ItemInfoUtil.CreateItemInfos(dailyReward);
                    }

                    var popup = Lance.PopupManager.CreatePopup<Popup_MonthlyFeeConfirmPurchaseUI>();
                    popup.Init(name, StringTableUtil.GetDesc("MonthlyFeePacakge"), immediatelyRewards.ToArray(), dailyRewards.ToArray(), () => Lance.IAPManager.Purchase(data.productId, OnFinishPurchased));
                }
                else
                {
                    List<ItemInfo> rewards = new List<ItemInfo>();

                    if (data.reward.IsValid())
                    {
                        RewardData reward = Lance.GameData.RewardData.TryGet(data.reward);
                        if (reward != null)
                        {
                            rewards = ItemInfoUtil.CreateItemInfos(reward);
                        }

                        if (data.mileage > 0)
                        {
                            var mileageReward = new ItemInfo(ItemType.Mileage, data.mileage);

                            rewards.Add(mileageReward);
                        }

                        var popup = Lance.PopupManager.CreatePopup<Popup_ConfirmPurchaseUI>();
                        popup.Init(name, rewards.ToArray(), () => Lance.IAPManager.Purchase(data.productId, OnFinishPurchased));
                    }
                }
            }
            else
            {
                OnFinishPurchased();
            }

            void OnFinishPurchased()
            {
                if (Lance.Account.PackageShop.Purchase(data.id))
                {
                    Lance.Account.PackageShop.SetIsChangedData(true);

                    if (data.type == PackageType.MonthlyFee && data.monthlyFeeDailyReward.IsValid())
                    {
                        AnyCanReceiveMonthlyFeeDailyReward();
                    }

                    if (data.reward.IsValid())
                    {
                        RewardResult rewardResult = RewardDataChangeToRewardResult(data.reward);

                        // 마일리지 포함
                        if (data.mileage > 0)
                            rewardResult = rewardResult.AddReward(new ItemInfo(ItemType.Mileage, data.mileage));

                        GiveReward(rewardResult, ShowRewardType.Popup);

                        Lance.Account.UserInfo.StackPayments(data.price);

                        CheckQuest(QuestType.Payments, data.price);

                        Lance.Lobby.RefreshEventRedDot();
                        Lance.Lobby.RefreshQuestRedDot();

                        Param param = new Param();
                        param.Add("id", data.id);
                        param.Add("reward", data.reward);
                        param.Add("stackedPayments", Lance.Account.UserInfo.GetStackedPayments());
                        param.Add("mileage", data.mileage);

                        Lance.BackEnd.InsertLog("PurchasePackageItem", param, 7);

                        if (data.type == PackageType.RemoveAD)
                        {
                            Lance.Account.Buff.OnPurchasedRemoveAd();

                            UpdatePlayerStat();
                        }

                        // 보상을 다 얻고 난 다음에는 저장
                        Lance.BackEnd.UpdateAllAccountInfos();
                    }

                    onFinishPurchased?.Invoke();
                }
            }
        }

        void AnyCanReceiveMonthlyFeeDailyReward()
        {
            if (Lance.Account.PackageShop.AnyCanReceiveMonthlyFeeDailyReward())
            {
                if (Lance.PopupManager.GetPopup<Popup_MonthlyFeeRewardResultUI>() != null)
                    return;

                var popup = Lance.PopupManager.CreatePopup<Popup_MonthlyFeeRewardResultUI>();
                popup.Init();
            }
        }

        public void OnPlayerLevelUp(int levelUpCount, int testLevel = 0)
        {
            SoundPlayer.PlayPlayerLevelUp();

            UIUtil.ShowLevelUpText(mStageManager.Player);

            int curLevel = testLevel > 0 ? testLevel : Lance.Account.ExpLevel.GetLevel();

            bool isUnlockSkillSlotLevel = DataUtil.IsUnlockSkillSlotLevel(curLevel);
            bool canTryLimitBreakLevel = DataUtil.CanTryLimitBreakLevel(curLevel);

            if (isUnlockSkillSlotLevel || canTryLimitBreakLevel)
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_LevelUpUI>();
                popup.Init(curLevel, isUnlockSkillSlotLevel, canTryLimitBreakLevel);
                popup.SetOnCloseAction(() => Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.LimitBreak));
            }

            Lance.GameManager.CheckQuest(QuestType.LevelUpCharacter, levelUpCount);

            Lance.Account.UpdatePassRewardValue(PassType.Level);
            Lance.Account.JoustBattleInfo.SetLevel(Lance.Account.ExpLevel.GetLevel());

            Lance.Lobby.RefreshSkillCastUI();

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Skill);
            Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
        }

        public int FeedPet(string id, int petFood)
        {
            // 최고 레벨인지 확인
            if (Lance.Account.Pet.IsMaxLevel(id))
            {
                UIUtil.ShowSystemErrorMessage("IsMaxLevelPet");

                return -1;
            }

            // 펫 먹이가 충분한지 확인
            if (Lance.Account.Currency.IsEnoughPetFood(petFood) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPetFood");

                return -1;
            }

            // 펫 레벨업
            var result = Lance.Account.FeedPet(id, petFood);
            if (result.Item1)
            {
                CheckQuest(QuestType.LevelUpPet, result.Item2);

                return 1;
            }
            else
            {
                return 0;
            }
        }

        public bool EvolutionPet(string id)
        {
            // 최고 단계인지 확인
            if (Lance.Account.Pet.IsMaxStep(id))
            {
                UIUtil.ShowSystemErrorMessage("IsMaxStepPet");

                return false;
            }

            // 최고 레벨이 아니면 진화 불가능
            if (Lance.Account.Pet.IsMaxLevel(id) == false)
            {
                UIUtil.ShowSystemErrorMessage("NeedMaxLevelPet");

                return false;
            }

            // 속성석이 충분한지 확인
            var require = Lance.Account.Pet.GetRequireElementalStone(id);
            if (Lance.Account.Currency.IsEnoughElementalStone(require) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughElementalStone");

                return false;
            }

            if (Lance.Account.EvolutionPet(id) )
            {
                UpdatePlayerStat();

                mStageManager.Player.OnChangePet();

                CheckQuest(QuestType.EvolutionPet, 1);

                return true;
            }

            return false;
        }

        public bool EquipPet(string id)
        {
            if (Lance.Account.Pet.IsEquipped(id))
            {
                // 이미 착용중이다.
                UIUtil.ShowSystemErrorMessage("AlreadyEquippedPet");

                return false;
            }

            if (Lance.Account.Pet.EquipPet(id))
            {
                mStageManager.Player.OnChangePet();

                UpdatePlayerStat();

                CheckQuest(QuestType.EquipPet, 1);

                return true;
            }

            return false;
        }

        public void ChangePetEvolutionStatPreset(string id, int preset)
        {
            Lance.Account.Pet.ChangeEvolutionStatPreset(id, preset);

            UpdatePlayerStat();
        }

        public bool ChangePetEvolutionStat(string id)
        {
            int petEvolutionStep = Lance.Account.Pet.GetStep(id);
            if (petEvolutionStep == 0)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughEvolutionStep");

                return false;
            }

            if (Lance.Account.Pet.IsAllEvolutionStatLocked(id))
            {
                UIUtil.ShowSystemErrorMessage("AllLockedStats");

                return false;
            }

            int statLockCount = Lance.Account.Pet.GetEvolutionStatLockCount(id);
            int changePrice = DataUtil.GetPetEvolutionChangePrice(statLockCount);
            if (Lance.Account.Currency.IsEnoughElementalStone(changePrice) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughElementalStone");

                return false;
            }
                

            if (Lance.Account.ChangePetEvolutionStat(id))
            {
                return true;
            }

            return false;
        }

        public void ChangeEquipmentOptionStatPreset(string id, int preset)
        {
            Lance.Account.ChangeEquipmentOptionPreset(id, preset);

            UpdatePlayerStat();
        }

        public bool ChangeEquipmentOptionStat(string id)
        {
            var equipmentInst = Lance.Account.GetEquipment(id);
            if (equipmentInst == null)
            {
                // 획득하지 못한 장비입니다.
                UIUtil.ShowSystemErrorMessage("HaveNotEquipment");

                return false;
            }

            if (Lance.Account.IsAllEquipmentOptionStatLocked(id))
            {
                // 모든 장비 옵션이 잠겨있습니다.
                UIUtil.ShowSystemErrorMessage("AllLockedEquipmentOptionStats");

                return false;
            }

            double changePrice = Lance.Account.GetEquipmentOptionChangePrice(id);

            if (Lance.Account.Currency.IsEnoughReforgeStone(changePrice) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughReforgeStone");

                return false;
            }

            if (Lance.Account.ChangeEquipmentOptionStat(id))
            {
                return true;
            }

            return false;
        }


        public void ShowRewardedAd(AdType adType, Action onRewardedEvent)
        {
#if UNITY_EDITOR
            onRewardedEvent?.Invoke();

            Lance.Account.UserInfo.StackWatchAdCount(1);

            CheckQuest(QuestType.WatchAd, 1);

#else
            if (Lance.Account.PackageShop.IsPurchasedRemoveAD() || Lance.IAPManager.IsPurchasedRemoveAd())
            {
                onRewardedEvent?.Invoke();

                Param param = new Param();
                param.Add("adType", $"{adType}");

                Lance.BackEnd.InsertLog("SkipAd", param, 7);

                CheckQuest(QuestType.WatchAd, 1);

                Lance.Account.UserInfo.StackWatchAdCount(1);
            }
            else
            {
                if (Lance.AdManager.IsRewardedVideoAvailable() == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsRewardedVieidoUnavailable");

                    return;
                }

                Lance.AdManager.ShowRewardedAd(adType, () =>
                {
                    Lance.MainThreadDispatcher.QueueOnMainThread(OnRewardedEvet);
                });

                void OnRewardedEvet()
                {
                    onRewardedEvent.Invoke();

                    Param param = new Param();
                    param.Add("adType", $"{adType}");

                    Lance.BackEnd.InsertLog("WatchAd", param, 7);

                    CheckQuest(QuestType.WatchAd, 1);

                    Lance.Account.UserInfo.StackWatchAdCount(1);
                }
            }
#endif
        }

        public void SetActiveJoustingPlayerPreview(bool isActive)
        {
            mJoustingPlayerPreview.SetActive(isActive);
        }

        public RenderTexture GetJoustingPlayerPreivewRenderTexture()
        {
            return mJoustingPlayerPreview.GetRenderTexture();
        }

        public void RefreshJoustingPlayerPreview(string[] costumes)
        {
            mJoustingPlayerPreview.Refresh(costumes);
        }

        public void SetActiveJoustingOpponentPreview(bool isActive)
        {
            mJoustingOpponentPreview.SetActive(isActive);
        }

        public RenderTexture GetJoustingOpponentPreviewRenderTexture()
        {
            return mJoustingOpponentPreview.GetRenderTexture();
        }

        public void RefreshJoustingOpponentPlayerPreview(string[] costumes)
        {
            mJoustingOpponentPreview.Refresh(costumes);
        }

        public void SetActiveCostumePreview(bool isActive)
        {
            mCostumePreview.SetActive(isActive);
        }

        public RenderTexture GetCostumePreivewRenderTexture()
        {
            return mCostumePreview.GetRenderTexture();
        }

        public void OnSelectBodyCostume(string id)
        {
            mCostumePreview.OnSelectBodyCostume(id);
        }

        public void OnSelectEtcCostume(string id)
        {
            mCostumePreview.OnSelectEtcCostume(id);
        }

        public void OnSelectWeaponCostume(string id)
        {
            mCostumePreview.OnSelectWeaponCostume(id);
        }

        public bool EquipCostume(string id)
        {
            // 보유한 코스튬인지 확인
            if (Lance.Account.Costume.HaveCostume(id) == false)
            {
                // 아직 획득하지 못한 코스튬 입니다.
                UIUtil.ShowSystemErrorMessage("HaveNotCostume");

                return false;
            }

            if (Lance.Account.Costume.IsEquipped(id))
            {
                // 이미 착용중인 코스튬 입니다.
                UIUtil.ShowSystemErrorMessage("AlreadyEquippedCostume");

                return false;
            }

            if (Lance.Account.Costume.EquipCostume(id))
            {
                bool isBodyCostume = DataUtil.GetCostumeData(id)?.type == CostumeType.Body;

                mStageManager.Player.UpdateCostumes(Lance.Account.Costume.GetEquippedCostumeIds(), isJousting:mStageManager.IsJousting);

                Lance.Account.JoustBattleInfo.SetCostumes(Lance.Account.Costume.GetEquippedCostumeIds());

                if (isBodyCostume)
                {
                    Lance.BackEnd.ChattingManager.ReEntryChattinChannel();
                }

                return true;
            }

            return false;
        }

        public bool PurchaseShopCostume(string id)
        {
            CostumeShopData costumeShopData = Lance.GameData.CostumeShopData.TryGet(id);
            if (costumeShopData == null || costumeShopData.reward.IsValid() == false)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            CostumeData costumeData = DataUtil.GetCostumeData(costumeShopData.reward);
            if (costumeData == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            if (Lance.Account.Costume.HaveCostume(id))
            {
                UIUtil.ShowSystemErrorMessage("AlreadyHaveCostume");

                return false;
            }

            if (costumeData.requireLimitBreak > 0)
            {
                if (Lance.Account.ExpLevel.GetLimitBreak() < costumeData.requireLimitBreak)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotSatisfiedLimitBreak");

                    return false;
                }
            }

            // 이미 구매한 코스튬인가?
            if (Lance.Account.Costume.HaveCostume(id))
            {
                // 이미 보유한 코스튬입니다!
                UIUtil.ShowSystemErrorMessage("AlreadyHaveCostume");

                return false;
            }

            if (Lance.Account.IsEnoughCostumeUpgrade(costumeShopData.price) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughCostumeUpgrade");

                return false;
            }

            if (Lance.Account.UseCostumeUpgrade(costumeShopData.price))
            {
                // 로그 기록
                Param param = new Param();
                param.Add("id", costumeData.id);
                param.Add("usedCostumeUpgrade", costumeShopData.price);
                param.Add("remainCostumeUpgrade", Lance.Account.Currency.GetCostumeUpgrade());

                Lance.BackEnd.InsertLog("PurchaseCostumeShop", param, 7);

                var reward = new RewardResult();

                reward.costumes = new MultiReward[] { new MultiReward() { Id = costumeData.id, ItemType = ItemType.Costume, Count = 1 } };

                if (reward.IsEmpty() == false)
                {
                    GiveReward(reward, ShowRewardType.Popup);

                    UpdatePlayerStat(true);

                    return true;
                }
            }

            return false;
        }

        public void PurchaseCostume(string id, Action onFinishPurchase)
        {
            CostumeData costumeData = DataUtil.GetCostumeData(id);
            if (costumeData == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (costumeData.requireLimitBreak > 0)
            {
                if (Lance.Account.ExpLevel.GetLimitBreak() < costumeData.requireLimitBreak)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotSatisfiedLimitBreak");

                    return;
                }
            }

            if (costumeData.eventId.IsValid())
            {
                StringParam passNameParam = new StringParam("passName", StringTableUtil.GetName(costumeData.eventId));

                UIUtil.ShowSystemErrorMessage("IsOnlyObtainEvent", param: passNameParam);

                return;
            }

            // 이미 구매한 코스튬인가?
            if (Lance.Account.Costume.HaveCostume(id))
            {
                // 이미 보유한 코스튬입니다!
                UIUtil.ShowSystemErrorMessage("AlreadyHaveCostume");

                return;
            }

            // 잼으로 구매
            if (costumeData.gemPrice > 0)
            {
                if (Lance.Account.IsEnoughGem(costumeData.gemPrice) == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughGem");

                    return;
                }

                // 코스튬 구매 확인 팝업
                string title = StringTableUtil.Get("Title_Confirm");

                StringParam param = new StringParam("gem", costumeData.gemPrice);

                string desc = StringTableUtil.GetDesc(costumeData.id, param);

                UIUtil.ShowConfirmPopup(title, desc, OnConfirm, null);

                void OnConfirm()
                {
                    // 잼 소모
                    if (Lance.Account.UseGem(costumeData.gemPrice))
                    {
                        var reward = new RewardResult();

                        reward.costumes = new MultiReward[] { new MultiReward() { Id = costumeData.id, ItemType = ItemType.Costume, Count = 1 } };
                        if (reward.IsEmpty() == false)
                        {
                            // 코스튬 획득
                            GiveReward(reward, ShowRewardType.Popup);

                            UpdatePlayerStat(true);
                        }

                        // 로그 기록
                        Param param = new Param();
                        param.Add("id", costumeData.id);
                        param.Add("usedGem", costumeData.gemPrice);
                        param.Add("remainGem", Lance.Account.Currency.GetGem());

                        Lance.BackEnd.InsertLog("PurchaseCostume", param, 7);

                        // 계정 정보 바로 저장
                        Lance.BackEnd.UpdateAllAccountInfos();

                        onFinishPurchase.Invoke();
                    }
                }
            }
            // 현금 구매
            else if (costumeData.productId.IsValid())
            {
                string name = StringTableUtil.GetName(costumeData.id);

                var mileageReward = new ItemInfo(ItemType.Mileage, costumeData.mileage);
                var costumeReward = new ItemInfo(ItemType.Costume, 1).SetId(costumeData.id);
                ItemInfo[] rewards = new ItemInfo[] { mileageReward, costumeReward };

                var popup = Lance.PopupManager.CreatePopup<Popup_ConfirmPurchaseUI>();
                popup.Init(name, rewards, OnConfirm);

                void OnConfirm()
                {
                    Lance.IAPManager.Purchase(costumeData.productId, OnFinishPurchased);

                    void OnFinishPurchased()
                    {
                        // 코스튬 획득
                        var reward = new RewardResult();

                        reward.mileage = costumeData.mileage;
                        reward.costumes = new MultiReward[] { new MultiReward() { Id = costumeData.id, ItemType = ItemType.Costume, Count = 1 } };
                        if (reward.IsEmpty() == false)
                        {
                            GiveReward(reward, ShowRewardType.Popup);

                            UpdatePlayerStat(true);
                        }

                        Lance.Account.UserInfo.StackPayments(costumeData.price);

                        CheckQuest(QuestType.Payments, costumeData.price);

                        Lance.Lobby.RefreshEventRedDot();
                        Lance.Lobby.RefreshQuestRedDot();

                        Param param = new Param();
                        param.Add("id", costumeData.id);
                        param.Add("stackedPayments", Lance.Account.UserInfo.GetStackedPayments());
                        param.Add("mileage", costumeData.mileage);

                        Lance.BackEnd.InsertLog("PurchaseCostume", param, 7);

                        Lance.BackEnd.UpdateAllAccountInfos();

                        onFinishPurchase.Invoke();
                    }
                }
            }
        }

        public void GiveLimitBreakSkill()
        {
            RewardResult rewardResult = new RewardResult();

            int myLimitBreak = Lance.Account.ExpLevel.GetLimitBreak();

            foreach(SkillData data in DataUtil.GetSkillDatas(SkillType.Active))
            {
                if (data.requireLimitBreak > 0)
                {
                    if (myLimitBreak >= data.requireLimitBreak)
                    {
                        if (Lance.Account.HaveSkill(SkillType.Active, data.id) == false)
                        {
                            rewardResult.skills = rewardResult.skills.AddArray(new MultiReward[] { new MultiReward(ItemType.Skill, data.id, 1) });
                        }
                    }
                }
            }

            if (rewardResult.IsEmpty() == false)
            {
                GiveReward(rewardResult, ShowRewardType.Popup);
            }
        }

        public bool UpgradeEssence(EssenceType essenceType)
        {
            if (essenceType == EssenceType.Central)
            {
                int step = Lance.Account.Essence.GetStep(essenceType);

                bool isMaxStep = Lance.Account.Essence.IsMaxStep(essenceType);
                if (isMaxStep)
                {
                    UIUtil.ShowSystemErrorMessage("IsMaxStepEssence");

                    return false;
                }

                CentralEssenceStepData essenceStepData = Lance.GameData.CentralEssenceStepData.TryGet(step);
                if (essenceStepData == null)
                    return false;

                if (essenceStepData.requireAllEssenceLevel > 0)
                {
                    for (int i = 0; i < (int)EssenceType.Count; ++i)
                    {
                        var type = (EssenceType)i;

                        var essence = Lance.Account.Essence.GetEssence(type);

                        int level = essence?.GetLevel() ?? 0;

                        if (level < essenceStepData.requireAllEssenceLevel)
                        {
                            UIUtil.ShowSystemErrorMessage("IsNotSatisfiedEssenceLevel");

                            return false;
                        }
                    }
                }
            }
            else
            {
                // 최고레벨인가?
                bool isMaxStep = Lance.Account.Essence.IsMaxStep(essenceType);
                bool isMaxLevel = Lance.Account.Essence.IsMaxLevel(essenceType);
                if (isMaxStep && isMaxLevel)
                {
                    UIUtil.ShowSystemErrorMessage("IsMaxStepEssence");

                    return false;
                }

                if (isMaxLevel)
                {
                    UIUtil.ShowSystemErrorMessage("IsMaxLevelEssenceRequireCentral");

                    return false;
                }
                
                // 재화가 충분한가?
                int myEssenceElement = Lance.Account.Currency.GetEssence(essenceType);
                int requireEssenceElement = Lance.Account.Essence.GetUpgradeRequireElements(essenceType);
                bool isEnough = myEssenceElement >= requireEssenceElement;
                if (isEnough == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughEssenceElements");

                    return false;
                }

                // 강화 조건을 만족했나?
                int step = Lance.Account.Essence.GetStep(essenceType);

                EssenceStepData essenceStepData = DataUtil.GetEssenceStepData(essenceType, step);
                if (essenceStepData == null)
                    return false;

                if (essenceStepData.requireCentralStep > 0)
                {
                    var centralEssence = Lance.Account.Essence.GetEssence(EssenceType.Central);
                    if (centralEssence == null)
                        return false;

                    if (essenceStepData.requireCentralStep > centralEssence.GetStep())
                    {
                        UIUtil.ShowSystemErrorMessage("IsMaxLevelEssenceRequireCentral");

                        return false;
                    }
                }
            }

            if (Lance.Account.UpgradeEssence(essenceType))
            {
                if (essenceType == EssenceType.Central)
                {
                    CheckQuest(QuestType.LevelUpCentralEssence, 1);

                    Lance.Lobby.RefreshEventRedDot();
                    Lance.Lobby.RefreshQuestRedDot();
                }

                Lance.Lobby.RefreshEssenceRedDot();

                UpdatePlayerStat(essenceType != EssenceType.Chapter1 && essenceType != EssenceType.Chapter2);

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ActiveCentralEssence()
        {
            var essence = Lance.Account.Essence.GetEssence(EssenceType.Central);
            if (essence == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            if (Lance.Account.Essence.IsActiveCentralEssence())
            {
                // 이미 태초의 정수가 활성화중입니다.
                UIUtil.ShowSystemErrorMessage("AlreadyActiveCentralEssence");

                return false;
            }

            // 태초의 정수 단계가 충분한가?
            int step = essence.GetStep();
            var data = Lance.GameData.CentralEssenceActiveRequireData.TryGet(step);
            if (data == null || data.requireAllEssenceAmount <= 0)
            {
                // 최소 태초의 정수 1단계가 필요합니다.
                UIUtil.ShowSystemErrorMessage("IsNotSatisfiedCentralEssenceStep");

                return false;
            }

            int remainCount = Lance.Account.Essence.GetRemainActiveCount();
            if (remainCount <= 0)
            {
                // 태초의 정수 활성화 횟수를 모두 소진하였습니다.
                UIUtil.ShowSystemErrorMessage("IsNotEnoughActiveCount");

                return false;
            }

            // 정수를 충분히 가지고 있는가?
            for(int i = 0; i < (int)EssenceType.Count; ++i)
            {
                EssenceType type = (EssenceType)i;

                if (Lance.Account.Currency.IsEnoughEssence(type, data.requireAllEssenceAmount) == false)
                {
                    // 정수가 부족합니다.
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughEssenceElements");

                    return false;
                }
            }

            // 정수를 소모
            for (int i = 0; i < (int)EssenceType.Count; ++i)
            {
                EssenceType type = (EssenceType)i;

                Lance.Account.Currency.UseEssence(type, data.requireAllEssenceAmount);
            }

            // 태초의 정수 활성화
            Lance.Account.Essence.ActiveCentralEssence();

            UpdatePlayerStat();

            Lance.Lobby.UpdateCentralEssenceUI();

            SoundPlayer.PlayActiveCentralEssence();

            var popup = Lance.PopupManager.GetPopup<Popup_EssenceUI>();
            popup?.Refresh();

            Param param = new Param();
            param.Add("nowDateNum", TimeUtil.NowDateNum());
            param.Add("remainCount", Lance.Account.Essence.GetRemainActiveCount());

            Lance.BackEnd.InsertLog("ActiveCentralEssence", param, 7);

            return true;
        }

        public void DrawLotto()
        {
            int remainDailyCount = Lance.Account.Lotto.GetRemainDailyCount();
            if (remainDailyCount <= 0)
            {
                UIUtil.ShowSystemErrorMessage("NotEnoughLottoDailyCount");

                return;
            }

            if (Lance.Account.Lotto.InCoolTime())
            {
                UIUtil.ShowSystemErrorMessage("InCoomTimeLotto");

                return;
            }

            var reward = Lance.Account.Lotto.DrawLotto();
            if (reward.IsValid())
            {
                GiveReward(reward);

                Param param = new Param();
                param.Add("nowDateNum", TimeUtil.NowDateNum());
                param.Add("lottoReward", reward);
                param.Add("remainCount", Lance.Account.Lotto.GetRemainDailyCount());

                Lance.Lobby.RefreshMenuRedDots();

                Lance.BackEnd.InsertLog("DrawLotto", param, 7);
            }
        }

        public void ReceiveMonthlyFeeDailyReward(string id, Action onFinish)
        {
            if (Lance.Account.PackageShop.CanReceiveMonthlyFeeDailyReward(id) == false)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                onFinish?.Invoke();

                return;
            }

            var packageShopData = DataUtil.GetPackageShopData(id, 1);
            if (packageShopData == null || packageShopData.monthlyFeeDailyReward.IsValid() == false)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                onFinish?.Invoke();

                return;
            }

            var reward = Lance.GameData.RewardData.TryGet(packageShopData.monthlyFeeDailyReward);
            if (reward == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (Lance.Account.PackageShop.ReceiveMonthlyFeeDailyReward(id))
            {
                Lance.Account.PackageShop.SetIsChangedData(true);

                var rewardResult = RewardDataChangeToRewardResult(packageShopData.monthlyFeeDailyReward);

                GiveReward(rewardResult);

                Lance.Lobby.UpdateCurrencyUI();

                onFinish?.Invoke();

                Lance.Lobby.RefreshTab();
                Lance.Lobby.RefreshQuestRedDot();

                Param param = new Param();
                param.Add("id", id);
                param.Add("nowDateNum", TimeUtil.NowDateNum());
                param.Add("reward", rewardResult);

                Lance.BackEnd.InsertLog("ReceiveMonthlyFeeDailyReward", param, 7);
            }
        }

        public void ResetSkill(SkillType skillType, string id)
        {
            var skillInst = Lance.Account.GetSkill(skillType, id);
            if (skillInst == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotSkill");

                return;
            }

            int level = skillInst.GetLevel();
            if (level <= 1)
            {
                UIUtil.ShowSystemErrorMessage("CanNotResetSkill");

                return;
            }

            // 레벨 초기화
            skillInst.SetLevel(1);

            // 스킬 조각 돌려주기
            int totalSkillPiece = DataUtil.CalcTotalUsedSkillPiece(id, level);

            RewardResult skillPiece = new RewardResult();

            skillPiece.skillPiece = totalSkillPiece;

            GiveReward(skillPiece, ShowRewardType.Popup);

            // 만족 못하는 도감은 다시 삭제처리
            if (Lance.Account.DeleteDissatisfyingCollection(ItemType.Skill))
            {
                UpdatePlayerStat();
            }

            Param param = new Param();

            param.Add("skillType", $"{skillType}");
            param.Add("skillId", id);
            param.Add("totalSkillPiece", totalSkillPiece);
            param.Add("nowDateNum", TimeUtil.NowDateNum());

            Lance.BackEnd.InsertLog("ResetSkill", param, 7);
        }

        public void ResetSkills(SkillType skillType)
        {
            var resetList = Lance.Account.ResetSkills(skillType);
            if (resetList.Count <= 0)
            {
                UIUtil.ShowSystemErrorMessage("CanNotResetSkillList");
            }
            else
            {
                int totalSkillPiece = 0;

                foreach(var resetSkill in resetList)
                {
                    totalSkillPiece += DataUtil.CalcTotalUsedSkillPiece(resetSkill.Item1, resetSkill.Item2);
                }

                RewardResult skillPiece = new RewardResult();

                skillPiece.skillPiece = totalSkillPiece;

                GiveReward(skillPiece, ShowRewardType.Popup);

                // 만족 못하는 도감은 다시 삭제처리
                if (Lance.Account.DeleteDissatisfyingCollection(ItemType.Skill))
                {
                    UpdatePlayerStat();
                }

                Param param = new Param();

                param.Add("skillType", $"{skillType}");
                param.Add("resetList", resetList);
                param.Add("totalSkillPiece", totalSkillPiece);
                param.Add("nowDateNum", TimeUtil.NowDateNum());

                Lance.BackEnd.InsertLog("ResetSkills", param, 7);
            }
        }

        public bool EquipAchievement(string id)
        {
            if (Lance.Account.Achievement.IsCompleteAchievement(id) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotCompleteAchievement");

                return false;
            }

            if (Lance.Account.Achievement.IsReceivedAchievement(id) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotReceivedAchievement");

                return false;
            }

            if (Lance.Account.Achievement.IsEquippedAchievement(id))
            {
                UIUtil.ShowSystemErrorMessage("IsAlreadyEquippedAchievement");

                return false;
            }

            if (Lance.Account.Achievement.EquipAchievement(id))
            {
                Lance.Account.UpdateJoustingRankExtraInfo();

                Lance.Lobby.UpdateAchievement();

                // 채팅방 재접속
                Lance.BackEnd.ChattingManager.ReEntryChattinChannel();

                return true;
            }

            return false;
        }

        public bool ReceiveAchievement(string id)
        {
            if (Lance.Account.Achievement.IsCompleteAchievement(id) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotCompleteAchievement");

                return false;
            }

            if (Lance.Account.Achievement.IsReceivedAchievement(id))
            {
                UIUtil.ShowSystemErrorMessage("IsNotReceivedAchievement");

                return false;
            }

            if (Lance.Account.Achievement.ReceiveAchievement(id))
            {
                UpdatePlayerStat();

                Lance.Lobby.UpdateAchievement();

                return true;
            }

            return false;
        }

        public void OnRelease()
        {
            mInit = false;
        }

        public void ReLogin()
        {
            Lance.Lobby?.OnRelease();
            Lance.GameManager?.OnRelease();
            Lance.SoundManager?.OnRelease();

            SceneManager.LoadScene("Login");
        }

        public void Localize()
        {
            mSleepModeManager.Localize();
        }

        public void UpdateMyRankProfile()
        {
            mRankProfileManager.UpdateMyRankProfile();
        }

        public void GetRankProfile(string nickname, GetRankProfileFunc getRankProfileFunc)
        {
            mRankProfileManager.GetRankProfile(nickname, getRankProfileFunc);
        }

        public void SetActiveRankProfilePreview(bool isActive)
        {
            mRankProfileManager.SetActiveRankProfile(isActive);
        }

        public void RefreshRankProfilePreview(string[] costumes)
        {
            mRankProfileManager.RefreshRankProfilePreview(costumes);
        }

        public RenderTexture GetRankProfilePreviewRenderTexture()
        {
            return mRankProfileManager.GetRankProfilePreviewRenderTexture();
        }

        public bool UpgradeManaHeart()
        {
            int manaHeartStep = Lance.Account.ManaHeart.GetStep();
            if (manaHeartStep == DataUtil.GetManaHeartMaxStep())
            {
                UIUtil.ShowSystemErrorMessage("IsMaxStepManaHeart");

                return false;
            }

            var manaHeartStepData = Lance.GameData.ManaHeartStepData.TryGet(manaHeartStep);
            if (manaHeartStepData == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            int upgradeStep = Lance.Account.ManaHeart.GetUpgradeStep();
            if (manaHeartStepData.maxUpgradeStep > upgradeStep)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughManaHeartRequireStep");

                return false;
            }

            int level = Lance.Account.ExpLevel.GetLevel();

            if (manaHeartStepData.requireLevel > level)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughManaHeartRequireLevel");

                return false;
            }

            // 마나 하트 강화
            if (Lance.Account.UpgradeManaHeart())
            {
                Param param = new Param();
                param.Add("manaHeartStep", manaHeartStep);
                param.Add("manaHearUpgradeStep", upgradeStep);
                param.Add("currentLevel", level);

                Lance.BackEnd.InsertLog("UpgradeManaHeart", param, 7);

                return true;
            }

            return false;
        }

        public bool UpgradeManaHeartInst()
        {
            int manaHeartStep = Lance.Account.ManaHeart.GetStep();
            var manaHeartStepData = Lance.GameData.ManaHeartStepData.TryGet(manaHeartStep);
            if (manaHeartStepData == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            int upgradeStep = Lance.Account.ManaHeart.GetUpgradeStep();
            var stepData = Lance.GameData.ManaHeartUpgradeStepData.TryGet(upgradeStep);
            if (stepData == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return false;
            }

            if (manaHeartStepData.maxUpgradeStep < upgradeStep)
            {
                UIUtil.ShowSystemErrorMessage("IsMaxStepManaHeart");

                return false;
            }

            // 재화가 충분한지 확인
            int require = Lance.Account.ManaHeart.GetUpgradeRequire();
            if (Lance.Account.IsEnoughManaEssence(require) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughManaEssence");

                return false;
            }

            // 마나 하트 강화
            if (Lance.Account.UpgradeManaHeartInst())
            {
                Param param = new Param();
                param.Add("manaHeartStep", manaHeartStep);
                param.Add("manaHearUpgradeStep", upgradeStep);
                param.Add("remainManaEssence", Lance.Account.Currency.GetManaEssence());

                Lance.BackEnd.InsertLog("UpgradeManaHeartInst", param, 7);

                return true;
            }

            return false;
        }
    }
}