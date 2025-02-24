using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;


namespace Lance
{
    class StageManager
    {
        bool mIsPlay;
        bool mIsClearStage;
        bool mIsFinishJousting;
        bool mIsBossStage;
        bool mInBossSpawnMotion;
        bool mInJoustingMotion;
        ObscuredInt mMonsterLimitCount;
        ObscuredInt mMonsterKillCount;

        ObscuredInt mJoustingComboAtkCount;
        ObscuredInt mJoustingFirstSelectAtkType;

        ObscuredDouble mStackedDamage;
        ObscuredFloat mTimeout;

        Image mFieldFade;
        StageData mStageData;
        RewardResult mStackedReward;
        CharacterManager mCharacterManager;
        StageBackgroundManager mBackgroundManager;
        public StageData StageData => mStageData;
        public double StackedDamage => mStackedDamage;
        public float Timeout => mTimeout;
        public bool InContents => IsBossStage || IsDungeon || IsLimitBreak || IsUltimateLimitBreak;
        public bool IsBossStage => IsDungeon == false && mIsBossStage;
        public bool IsDungeon => mStageData.type.IsDungeon();
        public bool IsDemonicRealm => mStageData.type.IsDemonicRealm();
        public bool IsJousting => mStageData.type.IsJousting();
        public bool InJoustingMotion => mInJoustingMotion;
        public bool IsLimitBreak => mStageData.type.IsLimitBreak();
        public bool IsUltimateLimitBreak => mStageData.type == StageType.UltimateLimitBreak;
        public bool IsPlay => mIsPlay;
        public int MonsterKillCount => mMonsterKillCount;
        public Boss Boss => mCharacterManager.GetBoss();
        public Player Player => mCharacterManager.Player;
        public JoustOpponent JoustOpponent => mCharacterManager.FindJoustOpponent();
        public void Init()
        {
            mCharacterManager = new CharacterManager();
            mCharacterManager.Init();

            mBackgroundManager = new StageBackgroundManager();
            mBackgroundManager.Init(this);

            var fieldFadeObj = GameObject.Find("Field_Fade");

            mFieldFade = fieldFadeObj.GetOrAddComponent<Image>();

            var stageData = Lance.Account.StageRecords.GetCurrentStageData();

            OnStartStage(stageData);

            if (stageData.type.IsNormal() && SaveBitFlags.BossBreakThrough.IsOn() && mIsBossStage == false)
            {
                // 보스 몬스터 바로 소환 및 연출
                TryBossChallenge();
            }
        }

        float mMonsterRespawnTime;
        public void OnUpdate(float dt)
        {
            if (mIsPlay == false)
                return;

            mCharacterManager.OnUpdate(dt);
            mBackgroundManager.OnUpdate(dt);

            if (mStageData.type == StageType.Jousting)
            {
                UpdateJousting(dt);
            }
            else
            {
                UpdateTimeout(dt);

                // 일반적인 경우에 플레이어가 몬스터를 만나지 못하고
                // 계속 전진하는 경우는 없지만 만약을 위한 코드
                if (mCharacterManager.Player.IsMoving)
                {
                    // 약 10초동안 달리고만 있었다면
                    // 몬스터를 다시 소환해주자
                    mMonsterRespawnTime += dt;
                    if (mMonsterRespawnTime >= 10f)
                    {
                        mMonsterRespawnTime = 0f;
                        if (mIsBossStage)
                        {
                            PlayStartBossStage();
                        }
                        else
                        {
                            PlayRespawnMonsters();
                        }
                    }
                }
                else
                {
                    mMonsterRespawnTime = 0f;
                }
            }
        }

        public void OnAnyScreenTouch()
        {
            //mRestCount = 0;
        }

        public void OnLateUpdate()
        {
            if (mIsPlay == false)
                return;

            mCharacterManager.OnLateUpdate();
        }

        public void PlayKnockback()
        {
            mBackgroundManager.PlayKnockback();
        }

        void UpdateTimeout(float dt)
        {
            if ((mStageData.type.IsDungeon() || mStageData.type.IsDemonicRealm() || mIsBossStage) && mInBossSpawnMotion == false)
            {
                if (mTimeout > 0)
                {
                    mTimeout -= dt;
                    if (mTimeout <= 0)
                    {
                        if (mStageData.type != StageType.Raid)
                        {
                            if (mStageData.type != StageType.Pet && mStageData.type != StageType.Ancient)
                            {
                                // 타임오버
                                PlayGameOverStage();
                            }
                            else
                            {
                                ClearDungeon();
                            }
                        }
                        else
                        {
                            // 레이드 던전 끝
                            PlayFinishRaidDungeon();
                        }
                    }

                    Lance.Lobby.UpdateStageTimerUI();
                }
            }
        }
        void ChangeBackground()
        {
            mBackgroundManager.ChangeBackground(mStageData.type, mStageData.chapter);
        }

        public void RanodmizeKey()
        {
            mTimeout.RandomizeCryptoKey();
            mMonsterLimitCount.RandomizeCryptoKey();
            mMonsterKillCount.RandomizeCryptoKey();
            mStackedDamage.RandomizeCryptoKey();

            mJoustingComboAtkCount.RandomizeCryptoKey();
            mJoustingFirstSelectAtkType.RandomizeCryptoKey();
        }

        public Monster GetFirstMonster()
        {
            return mCharacterManager.GetAllMonsters().FirstOrDefault();
        }

        public void OnPlayerDeath()
        {
            if (mCharacterManager.Player.IsDeath)
            {
                if (mStageData.type == StageType.Raid)
                {
                    // 레이드 던전 끝난것임
                    PlayFinishRaidDungeon();
                }
                else if (mStageData.type == StageType.Ancient || mStageData.type == StageType.Pet)
                {
                    PlayFinishDungeon();
                }
                else
                {
                    PlayGameOverStage();
                }
            }
        }

        public void OnAncientBossDeath(RewardResult reward)
        {
            mTimeout += mStageData.increaseTimeout;
            mTimeout = Mathf.Min(mTimeout, mStageData.stageTimeout);

            mStackedReward = mStackedReward.AddReward(reward);

            int bestStep = Lance.Account.Dungeon.GetBestStep(mStageData.type);
            int curStep = mStageData.stage;

            if (bestStep < curStep)
            {
                Lance.Account.Dungeon.SetBestStep(mStageData.type, curStep);
            }

            int nextStep = curStep + 1;

            var newStageData = DataUtil.GetDungeonStageData(mStageData.type, nextStep);
            if (newStageData == null)
                newStageData = mStageData;
            else
                mStageData = newStageData;

            // 보스 능력치와 보스 재설정
            MonsterData bossData = DataUtil.GetBossData(mStageData);

            Boss.SetMonsterInfo(bossData, mStageData.type, mStageData.diff, mStageData.chapter, mStageData.stage, mStageData.bossLevel, mStageData.bossDropReward);

            Lance.Lobby.RefreshStageInfoUI();

            string title = StringTableUtil.Get($"Title_{mStageData.type}Dungeon");
            StringParam param = new StringParam("step", curStep);
            string step = StringTableUtil.Get("UIString_Step", param);

            Lance.Lobby.SetDungeonName($"{title} {step}");
        }

        public void OnPetBossDeath(RewardResult reward)
        {
            mTimeout += mStageData.increaseTimeout;
            mTimeout = Mathf.Min(mTimeout, mStageData.stageTimeout);

            mStackedReward = mStackedReward.AddReward(reward);

            int bestStep = Lance.Account.Dungeon.GetBestStep(mStageData.type);
            int curStep = mStageData.stage;

            if (bestStep < curStep)
            {
                Lance.Account.Dungeon.SetBestStep(mStageData.type, curStep);
            }

            int nextStep = curStep + 1;

            var newStageData = DataUtil.GetDungeonStageData(mStageData.type, nextStep);
            if (newStageData == null)
                newStageData = mStageData;
            else
                mStageData = newStageData;

            // 보스 능력치와 보스 재설정
            MonsterData bossData = DataUtil.GetBossData(mStageData);

            Boss.SetMonsterInfo(bossData, mStageData.type, mStageData.diff, mStageData.chapter, mStageData.stage, mStageData.bossLevel, mStageData.bossDropReward);

            Lance.Lobby.RefreshStageInfoUI();

            string title = StringTableUtil.Get($"Title_{mStageData.type}Dungeon");
            StringParam param = new StringParam("step", curStep);
            string step = StringTableUtil.Get("UIString_Step", param);

            Lance.Lobby.SetDungeonName($"{title} {step}");
        }

        public void OnMonsterDeath(RewardResult reward, int level)
        {
            if (mIsPlay == false)
                return;

            mMonsterLimitCount--;
            mMonsterKillCount++;
            mMonsterRespawnTime = 0f;
            mStackedReward = mStackedReward.AddReward(reward);

            if (IsDungeon)
            {
                //if ()
                //{
                //    int bestStep = Lance.Account.Dungeon.GetBestStep(mStageData.type);
                //    int curStep = level / mStageData.increaseLevel;

                //    if (bestStep < curStep)
                //    {
                //        Lance.Account.Dungeon.SetBestStep(mStageData.type, curStep);
                //    }
                //}
                if (mStageData.type == StageType.Ancient || mStageData.type == StageType.Pet)
                {
                    int bestStep = Lance.Account.Dungeon.GetBestStep(mStageData.type);
                    int curStep = mStageData.stage;

                    if (bestStep < curStep)
                    {
                        Lance.Account.Dungeon.SetBestStep(mStageData.type, curStep);
                    }
                }

                OnMonsterDeath_Dungeon();
            }
            else if (mStageData.type.IsDemonicRealm())
            {
                OnMonsterDeath_DemonicRealm();
            }
            else if (mStageData.type.IsNormal())
            {
                OnMonsterDeath_NormalStage();
            }
            else
            {
                OnMonsterDeath_LimitBreakStage();
            }

            Lance.Lobby.UpdateMonsterKillUI();
        }

        public void OnGiveupButton()
        {
            PlayGameOverStage();
        }

        public void OnGiveupJoustingButton()
        {
            PlayGiveupJousting();
        }

        public void OnExitJoustingButton()
        {
            PlayExitJousting();
        }

        void OnMonsterDeath_Dungeon()
        {
            if (mCharacterManager.IsAllMonsterDeath())
            {
                // 몬스터를 모두 잡았다. 던전을 클리어한 것
                if (mMonsterLimitCount <= 0)
                {
                    ClearDungeon();
                }
                // 아직 잡을 몬스터가 남았다.
                else
                {
                    PlayRespawnMonsters();
                }
            }
        }

        void ClearDungeon()
        {
            Param param = new Param();
            param.Add("atk", (double)Player.Stat.Atk);
            param.Add("hp", (double)Player.Stat.MaxHp);
            param.Add("type", $"{mStageData.type}");
            param.Add("remainTicket", Lance.Account.Currency.GetDungeonTicket(mStageData.type));
            param.Add("nowDateNum", TimeUtil.NowDateNum());

            Lance.BackEnd.InsertLog("ClearDungeon", param, 7);

            // 최고 단계를 올려준다.
            if (mStageData.type.HaveNextStepDungeon())
            {
                int bestStep = Lance.Account.Dungeon.GetBestStep(mStageData.type);
                int curStep = mStageData.stage;

                if (bestStep == curStep)
                {
                    Lance.Account.Dungeon.NextStep(mStageData.type);
                }
            }

            Lance.Account.Dungeon.StackClearCount(mStageData.type);

            Lance.GameManager.CheckQuest(QuestType.ClearDungeon, 1);
            Lance.GameManager.CheckQuest(mStageData.type.StageTypeChangeToQuestType(), 1);

            // 만약 자동으로 도전을 활성화 해두었다면
            // 티켓이 충분한지 확인하고 자동으로 도전한다.
            if (SaveBitFlags.DungeonAutoChallenge.IsOn())
            {
                // 티켓이 충분한지 확인하고
                if (Lance.Account.IsEnoughDungeonTicket(mStageData.type, Lance.GameData.DungeonCommonData.entranceRequireTicket) == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughDungeonTicket");

                    // 충분하지 않으면 일반 스테이지로 돌아오기
                    PlayFinishDungeon();

                    return;
                }
                else
                {
                    if (Lance.Account.UseDungeonTicket(mStageData.type, Lance.GameData.DungeonCommonData.entranceRequireTicket))
                    {
                        int newStep = mStageData.type.HaveNextStepDungeon() ? mStageData.stage + 1 : 1;

                        var newStageData = DataUtil.GetDungeonStageData(mStageData.type, newStep);
                        if (newStageData == null)
                            newStageData = mStageData;

                        PlayStartStage(newStageData, ignoreStackedRewardReset: true);

                        Lance.Lobby.RefreshTabRedDot(LobbyTab.Adventure);
                    }
                }
            }
            else
            {
                PlayFinishDungeon();
            }
        }

        void OnMonsterDeath_DemonicRealm()
        {
            if (mCharacterManager.IsAllMonsterDeath())
            {
                // 몬스터를 모두 잡았다. 던전을 클리어한 것
                if (mMonsterLimitCount <= 0)
                {
                    ClearDemonicRealm();
                }
                // 아직 잡을 몬스터가 남았다.
                else
                {
                    PlayRespawnMonsters();
                }
            }
        }

        void ClearDemonicRealm()
        {
            Param param = new Param();
            param.Add("atk", (double)Player.Stat.Atk);
            param.Add("hp", (double)Player.Stat.MaxHp);
            param.Add("type", $"{mStageData.type}");
            param.Add("remainStone", Lance.Account.Currency.GetDemonicRealmStone(mStageData.type));
            param.Add("nowDateNum", TimeUtil.NowDateNum());

            Lance.BackEnd.InsertLog("ClearDemonicRealm", param, 7);

            // 최고 단계를 올려준다.
            if (mStageData.type.HaveNextStepDemonicRealm())
            {
                int bestStep = Lance.Account.DemonicRealm.GetBestStep(mStageData.type);
                int curStep = mStageData.stage;

                if (bestStep == curStep)
                {
                    Lance.Account.DemonicRealm.NextStep(mStageData.type);
                }
            }

            Lance.Account.DemonicRealm.StackClearCount(mStageData.type);

            // 만약 자동으로 도전을 활성화 해두었다면
            // 티켓이 충분한지 확인하고 자동으로 도전한다.
            if (SaveBitFlags.DemonicRealmAutoChallenge.IsOn())
            {
                // 티켓이 충분한지 확인하고
                if (Lance.Account.IsEnoughDemonicRealmStone(mStageData.type, Lance.GameData.DemonicRealmCommonData.entranceRequireTicket) == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughDemonicRealmStone");

                    // 충분하지 않으면 일반 스테이지로 돌아오기
                    PlayFinishDemonicRealm();

                    return;
                }
                else
                {
                    if (Lance.Account.UseDemonicRealmStone(mStageData.type, Lance.GameData.DemonicRealmCommonData.entranceRequireTicket))
                    {
                        int newStep = mStageData.type.HaveNextStepDemonicRealm() ? mStageData.stage + 1 : 1;

                        var newStageData = DataUtil.GetDemonicRealmStageData(mStageData.type, newStep);
                        if (newStageData == null)
                            newStageData = mStageData;

                        PlayStartStage(newStageData, ignoreStackedRewardReset: true);

                        Lance.Lobby.RefreshTabRedDot(LobbyTab.Adventure);
                    }
                }
            }
            else
            {
                PlayFinishDemonicRealm();
            }
        }

        public int GetJoustingComboAtkCount()
        {
            return mJoustingComboAtkCount;
        }

        void OnMonsterDeath_NormalStage()
        {
            if (mIsBossStage == false)
            {
                if (SaveBitFlags.BossBreakThrough.IsOn())
                {
                    // 도전이 가능하면 보스 바로 도전하자
                    if (TryBossChallenge())
                        return;
                }
            }

            if (mCharacterManager.IsAllMonsterDeath())
            {
                if (mIsBossStage)
                {
                    PlayClearStage();
                }
                else
                {
                    PlayRespawnMonsters();
                }
            }
        }

        void OnMonsterDeath_LimitBreakStage()
        {
            if (mCharacterManager.IsAllMonsterDeath())
            {
                if (mStageData.type == StageType.UltimateLimitBreak)
                {
                    PlayFinishUltimateLimitBreak();
                }
                else
                {
                    PlayFinishLimitBreak();
                }
            }
        }

        void OnStartStage(StageData stageData, bool ignoreStackedRewardReset = false)
        {
            Lance.CameraManager.SetMovePower(Const.CameraDefaultMovePower);

            if (mStageData == null)
            {
                PlayBGM(stageData);
            }
            else
            {
                if (mStageData.chapter != stageData.chapter ||
                    mStageData.type != stageData.type)
                {
                    PlayBGM(stageData);
                }
            }

            mStageData = stageData;

            ChangeBackground();

            // 플레이어와 몬스터를 새롭게 생성한다.
            mCharacterManager.OnStartStage(stageData);

            mBackgroundManager.SetAdjSpeed(mCharacterManager.Player.GetMoveSpeedRatio());

            if (ignoreStackedRewardReset == false)
                mStackedReward = new RewardResult();

            mStackedDamage = 0;
            mMonsterKillCount = 0;
            mMonsterLimitCount = stageData.monsterLimitCount;
            mTimeout = stageData.stageTimeout;

            DoFadeIn(1.25f);

            mIsClearStage = false;
            mIsPlay = true;
            mIsBossStage = false;
            mInBossSpawnMotion = false;

            mBackgroundManager.SetActiveBossBreakThrough(false);
            Lance.GameManager.OnChangeStage(stageData);
            Lance.Lobby.OnStartStage(stageData);

            Param param = new Param();
            param.Add("atk", (double)Player.Stat.Atk);
            param.Add("hp", (double)Player.Stat.MaxHp);
            param.Add("type", $"{stageData.type}");
            param.Add("diff", $"{stageData.diff}");
            param.Add("chapter", stageData.chapter);
            param.Add("stage", stageData.stage);
            param.Add("nowDateNum", TimeUtil.NowDateNum());

            Lance.BackEnd.InsertLog("StartStage", param, 7);
        }

        void PlayBGM(StageData stageData)
        {
            if (stageData != null)
            {
                if (stageData.type.IsDungeon())
                {
                    SoundPlayer.PlayDungeonBGM(stageData.id, stageData.type);
                }
                else if (stageData.type.IsDemonicRealm())
                {
                    SoundPlayer.PlayDemonicRealmBGM(stageData.id, stageData.type);
                }
                else if (stageData.type.IsNormal())
                {
                    SoundPlayer.PlayChapterBGM(stageData.chapter);
                }
                else if (stageData.type.IsJousting())
                {
                    SoundPlayer.PlayJoustingBGM();
                }
                else
                {
                    SoundPlayer.PlayLimitBreakBGM();
                }
            }
        }

        public void UpdatePlayerStat()
        {
            mCharacterManager.Player.UpdatePlayerStat();

            mBackgroundManager.SetAdjSpeed(mCharacterManager.Player.GetMoveSpeedRatio());
        }

        public void OnBossDamage(ObscuredDouble damage)
        {
            mStackedDamage += damage;
        }

        public void PlayStartStage(StageData stageData, bool ignoreStackedRewardReset = false)
        {
            mIsPlay = false;

            Lance.GameManager.StartStageCoroutine(StartStage(stageData, ignoreStackedRewardReset));
        }

        IEnumerator StartStage(StageData stageData, bool ignoreStackedRewardReset = false)
        {
            DoFadeOut(0.5f);

            yield return new WaitForSeconds(0.5f);

            OnStartStage(stageData, ignoreStackedRewardReset);

            if (stageData.type.IsOnlyBoss())
            {
                // 보스 몬스터 바로 소환 및 연출
                yield return StartBossStage();
            }

            //if (stageData.type.IsNormal() && SaveBitFlags.BossAutoChallenge.IsOn() && mIsBossStage == false)
            //{
            //    // 보스 몬스터 바로 소환 및 연출
            //    yield return StartBossStage();
            //}
        }

        void PlayRespawnMonsters()
        {
            Lance.GameManager.StartStageCoroutine(RespawnMonsters());
        }

        IEnumerator RespawnMonsters()
        {
            yield return new WaitForSeconds(0.5f);

            mCharacterManager.ClearAllMonsters();

            if (mStageData != null)
            {
                MonsterData monsterData = DataUtil.GetRandomStageMonster(mStageData);
                if (monsterData != null)
                    mCharacterManager.SpawnMonsters(monsterData, mStageData);
            }
        }

        IEnumerator ReStartNormalStage()
        {
            DoFadeOut(0.5f);

            yield return new WaitForSeconds(1f);

            OnStartStage(Lance.Account.StageRecords.GetCurrentStageData());
        }

        void PlayFinishDungeon()
        {
            Lance.GameManager.StartStageCoroutine(FinishDungeon());
        }

        IEnumerator FinishDungeon()
        {
            StageType stageType = mStageData.type;

            var stackedReward = mStackedReward;

            yield return ReStartNormalStage();

            Lance.Lobby.RefreshTab();

            // 던전 결과 팝업 보여주자
            var popup = UIUtil.ShowRewardPopup(stackedReward, StringTableUtil.Get("Title_DungeonResult"));
            popup.SetOnCloseAction(() =>
            {
                QuestType questType = QuestType.ClearStage;

                if (stageType == StageType.Gold)
                {
                    questType = QuestType.ClearGoldDungeon;
                }
                else if (stageType == StageType.Stone)
                {
                    questType = QuestType.ClearStoneDungeon;
                }
                else if (stageType == StageType.Pet)
                {
                    questType = QuestType.ClearPetDungeon;
                }
                else if (stageType == StageType.Reforge)
                {
                    questType = QuestType.ClearReforgeDungeon;
                }
                else if (stageType == StageType.Growth)
                {
                    questType = QuestType.ClearGrowthDungeon;
                }
                else if (stageType == StageType.Ancient)
                {
                    questType = QuestType.ClearAncientDungeon;
                }

                Lance.GameManager.CheckGuideQuestReceiveReward(questType);
            });
        }

        void PlayFinishDemonicRealm()
        {
            Lance.GameManager.StartStageCoroutine(FinishDemonicRealm());
        }

        IEnumerator FinishDemonicRealm()
        {
            StageType stageType = mStageData.type;

            var stackedReward = mStackedReward;

            yield return ReStartNormalStage();

            // 던전 결과 팝업 보여주자
            var popup = UIUtil.ShowRewardPopup(stackedReward, StringTableUtil.Get("Title_DemonicRealmResult"));
            //popup.SetOnCloseAction(() =>
            //{
            //    QuestType questType = QuestType.ClearStage;

            //    if (stageType == StageType.Gold)
            //    {
            //        questType = QuestType.ClearGoldDungeon;
            //    }
            //    else if (stageType == StageType.Stone)
            //    {
            //        questType = QuestType.ClearStoneDungeon;
            //    }
            //    else if (stageType == StageType.Pet)
            //    {
            //        questType = QuestType.ClearPetDungeon;
            //    }
            //    else if (stageType == StageType.Reforge)
            //    {
            //        questType = QuestType.ClearReforgeDungeon;
            //    }
            //    else if (stageType == StageType.Growth)
            //    {
            //        questType = QuestType.ClearGrowthDungeon;
            //    }
            //    else if (stageType == StageType.Ancient)
            //    {
            //        questType = QuestType.ClearAncientDungeon;
            //    }

            //    Lance.GameManager.CheckGuideQuestReceiveReward(questType);
            //});
        }

        public void PlayClearStage()
        {
            if (mIsPlay == false)
                return;

            Lance.GameManager.StartStageCoroutine(ClearStage());
        }

        IEnumerator ClearStage()
        {
            mIsClearStage = true;

            // 최고 단계보다 아래라면 보스 도전 불가능
            int curTotalStage = StageRecordsUtil.CalcTotalStage(mStageData.diff, mStageData.chapter, mStageData.stage);
            int bestTotalStage = Lance.Account.StageRecords.GetBestTotalStage();
            if (curTotalStage == bestTotalStage)
            {
                Lance.Account.StageRecords.NextStage();

                Param param = new Param();
                param.Add("atk", (double)Player.Stat.Atk);
                param.Add("hp", (double)Player.Stat.MaxHp);
                param.Add("type", mStageData.type);
                param.Add("diff", mStageData.diff);
                param.Add("chapter", mStageData.chapter);
                param.Add("stage", mStageData.stage);
                param.Add("nowDateNum", TimeUtil.NowDateNum());

                Lance.BackEnd.InsertLog("ClearStage", param, 7);

                string rewardId = mStageData.stageFirstClearReward;

                if (rewardId.IsValid())
                {
                    RewardResult reward = Lance.GameManager.RewardDataChangeToRewardResult(rewardId);

                    Lance.GameManager.GiveReward(reward, ShowRewardType.FirstStageClear);
                }

                if (SaveBitFlags.BossBreakThrough.IsOn())
                {
                    yield return new WaitForSeconds(2f);

                    var newStageData = Lance.Account.StageRecords.GetCurrentStageData();

                    if (mStageData == null)
                    {
                        PlayBGM(newStageData);
                    }
                    else
                    {
                        if (mStageData.chapter != newStageData.chapter ||
                            mStageData.type != newStageData.type)
                        {
                            PlayBGM(newStageData);
                        }
                    }

                    mStageData = newStageData;

                    ChangeBackground();

                    mIsClearStage = false;
                    mIsPlay = true;
                    mIsBossStage = false;
                    mInBossSpawnMotion = false;

                    yield return StartBossStage();
                }
                else
                {
                    yield return new WaitForSeconds(2f);

                    yield return StartNextStage();
                }

                Lance.GameManager.CheckQuest(QuestType.ClearStage, 1);

                Lance.Account.UpdatePassRewardValue(PassType.Stage);

                Lance.Lobby.RefreshPassRedDot();
                Lance.Lobby.RefreshContentsLockUI();
                Lance.Lobby.RefreshEssenceRedDot();
            }
            else
            {
                if (SaveBitFlags.BossBreakThrough.IsOn())
                {
                    yield return new WaitForSeconds(2f);

                    mIsClearStage = false;
                    mIsPlay = true;
                    mIsBossStage = false;
                    mInBossSpawnMotion = false;

                    yield return StartBossStage();
                }
                else
                {
                    yield return new WaitForSeconds(1.5f);

                    yield return ReStartNormalStage();
                }
            }
        }

        IEnumerator StartNextStage()
        {
            DoFadeOut(0.5f);

            yield return new WaitForSeconds(0.5f);

            OnStartStage(Lance.Account.StageRecords.GetCurrentStageData());
        }

        void PlayFinishRaidDungeon()
        {
            if (mIsPlay == false)
                return;

            Lance.GameManager.StartStageCoroutine(FinishRaidDungeon());
        }

        IEnumerator FinishRaidDungeon()
        {
            mIsPlay = false;

            if (mCharacterManager.Player.IsAlive)
            {
                mCharacterManager.Player.Action.PlayAction(ActionType.Idle, true);
            }

            Lance.GameManager.CheckQuest(QuestType.ClearDungeon, 1);

            Param param = new Param();
            param.Add("atk", (double)Player.Stat.Atk);
            param.Add("hp", (double)Player.Stat.MaxHp);
            param.Add("type", mStageData.type);
            param.Add("stackedDamage", (double)mStackedDamage);
            param.Add("nowDateNum", TimeUtil.NowDateNum());

            Lance.BackEnd.InsertLog("FinishRaidDungeon", param, 7);

            Boss raidBoss = mCharacterManager.GetBoss();

            ElementalType type = raidBoss?.Data.elementalType ?? (ElementalType)DataUtil.GetNowRaidBossElementalTypeIndex(); ;

            double bestDamage = Lance.Account.Dungeon.GetRaidBossBestDamage(type);

            // 누적시킨 데미지를 저장
            Lance.Account.UpdateRaidBossDamage(type, mStackedDamage);

            // 랭킹 정보 등록
            Lance.BackEnd.UpdateUserRankScores();

            Lance.GameManager.UpdateMyRankProfile();

            // 보상 지급
            var raidRewardData = DataUtil.GetRaidRewardData(type, mStackedDamage);
            if (raidRewardData != null)
            {
                if (raidRewardData.reward.IsValid())
                {
                    var rewardData = Lance.GameData.RewardData.TryGet(raidRewardData.reward);
                    if (rewardData != null)
                    {
                        var rewardResult = new RewardResult();

                        rewardResult = rewardResult.AddReward(rewardData);

                        Lance.GameManager.GiveReward(rewardResult);
                    }
                }
            }

            yield return new WaitForSeconds(2f);

            // 스테이지 다시 시작하면서 누적된 데미지 초기화되기 때문에
            double stackedDamage = mStackedDamage;
            
            yield return ReStartNormalStage();

            var popup = Lance.PopupManager.CreatePopup<Popup_RaidResultUI>();
            popup.Init(type, stackedDamage, bestDamage);
            popup.SetOnCloseAction(() => Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.TryRaidDungeon));
        }

        public double GetRaidBossBestScore()
        {
            Boss raidBoss = mCharacterManager.GetBoss();

            ElementalType type = raidBoss?.Data.elementalType ?? (ElementalType)DataUtil.GetNowRaidBossElementalTypeIndex(); ;

            return Lance.Account.Dungeon.GetRaidBossBestDamage(type);
        }

        void PlayFinishLimitBreak()
        {
            if (mInBossSpawnMotion)
                return;

            if (mIsPlay == false)
                return;

            Lance.GameManager.StartStageCoroutine(FinishLimitBreak());
        }

        IEnumerator FinishLimitBreak()
        {
            mIsPlay = false;

            if (mCharacterManager.Player.IsAlive)
            {
                mCharacterManager.Player.Action.PlayAction(ActionType.Idle, true);
            }

            Param param = new Param();
            param.Add("atk", (double)Player.Stat.Atk);
            param.Add("hp", (double)Player.Stat.MaxHp);
            param.Add("type", mStageData.type);
            param.Add("limitBreak", Lance.Account.ExpLevel.GetLimitBreak());

            Lance.BackEnd.InsertLog("FinishLimitBreak", param, 7);

            Lance.Account.ExpLevel.NextLimitBreak();

            yield return new WaitForSeconds(2f);

            yield return ReStartNormalStage();

            Lance.Lobby.RefreshTab();

            var popup = Lance.PopupManager.CreatePopup<Popup_LimitBreakUI>();
            popup.Init();
            popup.SetOnCloseAction(() =>
            {
                if (Lance.Account.ExpLevel.GetLimitBreak() == 1)
                {
                    try
                    {
                        string title = StringTableUtil.Get("Title_Confirm");
                        string desc = StringTableUtil.Get("Desc_RequireReview");

                        var popup = UIUtil.ShowConfirmPopup(title, desc, () => {

                            Time.timeScale = Lance.Account.SpeedMode.InSpeedMode() ? Lance.GameData.CommonData.speedModeValue : 1f;

                            var inAppReviewManager = new InAppReviewManager();

                            inAppReviewManager.LaunchReview(RefreshLimitBreak);
                        }, () => { 
                            Time.timeScale = Lance.Account.SpeedMode.InSpeedMode() ? Lance.GameData.CommonData.speedModeValue : 1f;

                            RefreshLimitBreak();

                        }, ignoreModalTouch:true,ignoreBackButton:true);

                        Time.timeScale = 0f;
                    }
                    catch
                    {
                        RefreshLimitBreak();

                        Time.timeScale = Lance.Account.SpeedMode.InSpeedMode() ? Lance.GameData.CommonData.speedModeValue : 1f;
                    }
                }
                else
                {
                    RefreshLimitBreak();
                }

                void RefreshLimitBreak()
                {
                    Lance.Lobby.RefreshAllTab(LobbyTab.Stature);

                    Lance.GameManager.CheckQuest(QuestType.LimitBreak, 1);

                    Lance.GameManager.UpdatePlayerStat();

                    Lance.GameManager.GiveLimitBreakSkill();
                }
            });
        }

        void PlayFinishUltimateLimitBreak()
        {
            if (mInBossSpawnMotion)
                return;

            if (mIsPlay == false)
                return;

            Lance.GameManager.StartStageCoroutine(FinishUltimateLimitBreak());
        }

        IEnumerator FinishUltimateLimitBreak()
        {
            mIsPlay = false;

            if (mCharacterManager.Player.IsAlive)
            {
                mCharacterManager.Player.Action.PlayAction(ActionType.Idle, true);
            }

            Param param = new Param();
            param.Add("atk", (double)Player.Stat.Atk);
            param.Add("hp", (double)Player.Stat.MaxHp);
            param.Add("type", mStageData.type);
            param.Add("ultimateLimitBreak", Lance.Account.ExpLevel.GetUltimateLimitBreak());

            Lance.BackEnd.InsertLog("FinishUltimateLimitBreak", param, 7);

            Lance.Account.ExpLevel.NextUltimateLimitBreak();

            yield return new WaitForSeconds(2f);

            yield return ReStartNormalStage();

            Lance.Lobby.RefreshTab();

            var popup = Lance.PopupManager.CreatePopup<Popup_UltimateLimitBreakUI>();
            popup.Init();
            popup.SetOnCloseAction(() =>
            {
                Lance.Lobby.RefreshAllTab(LobbyTab.Stature);

                Lance.GameManager.UpdatePlayerStat();
            });
        }

        public bool IsDungeonMaxStep()
        {
            return mStageData.stage == DataUtil.GetDungeonBestStage(mStageData.type);
        }

        void PlayGameOverStage()
        {
            //if (mInBossSpawnMotion)
            //    return;

            if (mIsPlay == false || mIsClearStage)
                return;

            Lance.GameManager.StartStageCoroutine(GameOverStage());
        }

        IEnumerator GameOverStage()
        {
            mIsPlay = false;

            if (mIsBossStage && mStageData.type == StageType.Normal)
            {
                SaveBitFlags.BossBreakThrough.Set(false);

                Lance.Lobby.RefreshStageInfoUI();
            }

            bool isAlive = mCharacterManager.Player.IsAlive;

            if (isAlive)
            {
                mCharacterManager.Player.Action.PlayAction(ActionType.Idle, true);
            }

            SoundPlayer.PlayGameOver();

            yield return new WaitForSeconds(2.5f);

            StageType type = mStageData.type;
            var stackedReward = mStackedReward;

            yield return ReStartNormalStage();

            if (isAlive)
            {
                if ((type.IsDungeon() || type.IsDemonicRealm()) && stackedReward.IsEmpty() == false)
                    UIUtil.ShowRewardPopup(stackedReward);
            }
            else
            {
                Lance.GameManager.ShowGameOverPopup();
            }
        }

        void PlayStartBossStage()
        {
            if (mIsPlay == false)
                return;

            if (mInBossSpawnMotion)
                return;

            Lance.GameManager.StartStageCoroutine(StartBossStage());
        }

        IEnumerator StartBossStage()
        {
            mIsBossStage = true;
            mInBossSpawnMotion = true;

            mCharacterManager.Player.OnStartBossSpawnMotion();

            mBackgroundManager.SetActiveBossBreakThrough(SaveBitFlags.BossBreakThrough.IsOn() && mStageData.type.IsNormal());

            yield return new WaitForSeconds(0.1f);

            mCharacterManager.ClearAllMonsters();

            mTimeout = mStageData.stageTimeout;

            MonsterData bossData = DataUtil.GetBossData(mStageData);
            Boss boss = mCharacterManager.SpawnBoss(bossData, mStageData.type, mStageData.diff, mStageData.chapter, mStageData.stage, mStageData.bossDropReward, mStageData.bossLevel);
            if (boss != null)
            {
                boss.OnSpawn();

                Lance.Lobby.ShowBossAppearUI(mStageData.type);
            }

            Lance.Lobby.RefreshStageInfoUI();

            yield return new WaitForSeconds(1f);

            mCharacterManager.Player.OnFinishBossSpawnMotion();

            boss.OnSpawnFinish();

            mInBossSpawnMotion = false;
        }

        public IEnumerable<Character> GatherMonsters()
        {
            foreach (var monster in mCharacterManager.GetAllMonsters())
            {
                yield return monster;
            }
        }

        public IEnumerable<Character> GatherInAttackRangeOpponents(Character character, float range)
        {
            // 확인하려고 한 본인이 만약 죽었으면 의미 없다.
            if (character.IsDeath)
                yield return null;

            foreach (var opponent in mCharacterManager.GetOpponents(character.IsPlayer))
            {
                if (opponent.IsDeath)
                    continue;

                if (character.Physics.IsInRange(opponent.Physics, range))
                    yield return opponent;
            }
        }

        public (bool any, Character target) AnyInAttackRangeOpponent(Character character, float range)
        {
            // 확인하려고 한 본인이 만약 죽었으면 의미 없다.
            if (character.IsDeath)
                return (false, null);

            IEnumerable<Character> opponents = mCharacterManager.GetOpponents(character.IsPlayer);

            foreach (var opponent in opponents)
            {
                if (opponent.IsDeath)
                    continue;

                if (character.Physics.IsInRange(opponent.Physics, range))
                {
                    opponents = null;

                    return (true, opponent);
                }
            }

            return (false, null);
        }

        public void SetActiveWindlines(bool isActive)
        {
            mBackgroundManager.SetActiveWindlines(isActive);
        }

        public void OnBossChallengeButton()
        {
            // 현재 보스에 도전이 가능한지 확인하자
            if (TryBossChallenge() == false)
            {
                // 시스템 메세지 보여주자
                UIUtil.ShowSystemErrorMessage("CanNotChallengeBoss");
            }
        }

        public void OnBossAutoChallenge()
        {
            TryBossChallenge();
        }

        public bool CanTryBossChallenge()
        {
            return mMonsterKillCount >= Lance.GameData.StageCommonData.bossChallengeKillCount && mIsBossStage == false;
        }

        public void SetPlayerMoving(bool move)
        {
            Physics.autoSyncTransforms = move;

            mBackgroundManager.SetPlayerMoving(move);

            if (!move)
                SetActiveWindlines(false);
        }

        bool TryBossChallenge()
        {
            if (CanTryBossChallenge())
            {
                PlayStartBossStage();

                return true;
            }

            return false;
        }

        void DoFade(float startValue, float endValue, float duration)
        {
            Color startColor = new Color(0, 0, 0, startValue);

            mFieldFade.color = startColor;
            mFieldFade.DOFade(endValue, duration);
        }

        void DoFadeOut(float duration)
        {
            DoFade(0f, 1f, duration);
        }

        void DoFadeIn(float duration)
        {
            DoFade(1f, 0f, duration);
        }


        public void PlayJousting()
        {
            mIsPlay = false;

            Lance.GameManager.StartStageCoroutine(StartMatchJousting());
        }

        JoustBattleInfo mOpponentInfo;
        IEnumerator StartMatchJousting()
        {
            mInJoustingMotion = true;

            Time.timeScale = 1f;

            DoFadeOut(0.5f);

            yield return new WaitForSeconds(0.5f);

            var stageData = Lance.GameData.JoustingStageData;

            if (mStageData.chapter != stageData.chapter ||
                    mStageData.type != stageData.type)
            {
                PlayBGM(stageData);
            }

            mStageData = Lance.GameData.JoustingStageData;

            ChangeBackground();

            // 데이터에 저장되어 있는 내 데이터를 기반으로 뽑아와서 테이블에서 조회한다..?
            int myRankScore = Lance.Account.JoustRankInfo.GetRankScore(); // 내 전투력

            string matchUrl = Lance.GameData.JoustingCommonData.matchingUuid_kor;

            var countryCode = Lance.Account.CountryCode;
            if (countryCode != "KR")
                matchUrl = Lance.GameData.JoustingCommonData.matchingUuid_glb;

            // 1. 랜덤 테이블 조회를 통해 랜덤 유저 inDate 가져오기
            SendQueue.Enqueue(Backend.RandomInfo.GetRandomData, RandomType.User, matchUrl, myRankScore, 10, 10, bro =>
            {
                try
                {
                    if (bro.IsSuccess() == false)
                    {
                        // 오류가 발생했지만 더미 데이터로 일단 시작하자
                        CreateDummy();

                        StartPlayJousting();
                    }
                    else
                    {
                        // 리턴된  중 1개를 다시 랜덤하게 선택해 PVP 시작하기.
                        int max = bro.FlattenRows().Count;
                        if (max > 0)
                        {
                            int randomUser = UnityEngine.Random.Range(0, max);
                            string opponentUserIndate = bro.FlattenRows()[randomUser]["gamerInDate"].ToString();

                            SendQueue.Enqueue(Backend.PlayerData.GetOtherData, "JoustBattleInfo", opponentUserIndate, bro =>
                            {
                                if (bro.IsSuccess() == false)
                                {
                                    // 오류가 발생했지만 더미 데이터로 일단 시작하자
                                    CreateDummy();

                                    StartPlayJousting();
                                }
                                else
                                {
                                    mOpponentInfo = new JoustBattleInfo();
                                    mOpponentInfo.ExternalSetServerDataToLocal(bro.FlattenRows()[0]);

                                    StartPlayJousting();
                                }
                            });
                        }
                        else
                        {
                            // 오류가 발생했지만 더미 데이터로 일단 시작하자
                            CreateDummy();

                            StartPlayJousting();
                        }
                    }
                }
                catch (Exception e)
                {
                    Lance.BackEnd.SendBugReport("GetRandomData", "StartJousting", e.ToString());

                    // 오류가 발생했지만 더미 데이터로 일단 시작하자
                    CreateDummy();

                    StartPlayJousting();
                }
            });

            void CreateDummy()
            {
                mOpponentInfo = new JoustBattleInfo();
                mOpponentInfo.SetNickName("방랑기사돌탄");
                mOpponentInfo.SetPowerLevel(Lance.Account.JoustBattleInfo.GetPowerLevel() * 0.8d);
                mOpponentInfo.SetLevel(2000);
                mOpponentInfo.SetCostumes(new string[] { "BodyCostume_03", "WeaponCostume_03", "EtcCostume_02" });
            }
        }

        void StartPlayJousting()
        {
            Lance.GameManager.StartStageCoroutine(StartPlayJousting(mOpponentInfo));
        }

        IEnumerator StartPlayJousting(JoustBattleInfo opponentBattleInfo)
        {
            Lance.Lobby.OnStartJousting(Lance.Account.JoustBattleInfo, opponentBattleInfo);

            Lance.CameraManager.SetMovePower(Const.CameraJoustingMovePower);

            // 뽑아온 상대의 정보로 적을 생성한다.
            var costumes = opponentBattleInfo.GetCostumes();

            // 플레이어와 몬스터를 새롭게 생성한다.
            mCharacterManager.OnStartJousting(costumes);
            mStackedReward = new RewardResult();
            mStackedDamage = 0;
            mMonsterKillCount = 0;
            mMonsterLimitCount = 0;
            mTimeout = mStageData.stageTimeout;

            mBackgroundManager.SetAdjSpeed(mCharacterManager.Player.GetMoveSpeedRatio());
            mBackgroundManager.SetActiveBossBreakThrough(false);

            DoFadeIn(1.25f);

            mIsClearStage = false;
            mIsFinishJousting = false;
            mIsPlay = true;
            mIsBossStage = false;
            mInBossSpawnMotion = false;
            mSelectedAtkType = JoustingAttackType.None;

            // vs 보여주기
            var popupVSUI = Lance.PopupManager.CreatePopup<Popup_JoustingVSUI>();
            popupVSUI.Init(Lance.Account.JoustBattleInfo, opponentBattleInfo);

            SoundPlayer.PlayJoustingStartCheer();

            yield return new WaitForSeconds(2f);

            mCharacterManager.Player.Action.PlayAction(ActionType.Move, true);

            Param param = new Param();
            param.Add("myPowerLevel", mCharacterManager.Player.Stat.PowerLevel);
            param.Add("opponentName", opponentBattleInfo.GetNickName());
            param.Add("opponentCosutmes", opponentBattleInfo.GetCostumes());
            param.Add("opponentPowerLevel", opponentBattleInfo.GetPowerLevel());
            param.Add("opponentLevel", opponentBattleInfo.GetLevel());
            param.Add("nowDateNum", TimeUtil.NowDateNum());

            Lance.BackEnd.InsertLog("StartJousting", param, 7);

            mInJoustingMotion = false;
        }

        void UpdateJousting(float dt)
        {
            if (mIsFinishJousting)
                return;

            if (mTimeout > 0)
            {
                mTimeout -= dt;
                if (mTimeout <= 0)
                {
                    // 데이터에 지정해둔 시간이 다되었는데 아직도 충돌을 못한건 뭔가 잘 못된 것
                    // 마무리 연출 시작해주자
                    PlayFinishJousting();
                }
                else
                {
                    // 충돌이 발생하는지 체크를 계속해주고 있자
                    var player = mCharacterManager.Player;
                    var opponent = mCharacterManager.FindJoustOpponent();

                    float distance = Mathf.Abs(player.GetPosition().x - opponent.GetPosition().x);
                    if (distance <= 0.1f)
                    {
                        // 충돌이 감지되었다면 FX와 사운드 터트려주고
                        Lance.ParticleManager.Aquire("JoustingImpact", player.transform);
                        Lance.ParticleManager.Aquire("JoustingImpact", opponent.transform);

                        Lance.CameraManager.Shake(duration:0.75f, strength: 0.5f);

                        // 카메라를 슬쩍 미뤄주자 가운데로 가고 멈추자
                        Lance.CameraManager.SetFollowInfo(new FollowInfo()
                        {
                            FollowType = FollowType.None,
                            FollowTm = null,
                        });

                        SoundPlayer.PlayImpactJousting(mSelectedAtkType);
                        SoundPlayer.PlayJoustingImpactCheer();

                        // 마무리 연출 시작해주자
                        PlayFinishJousting();
                    }
                    else if (distance <= 15f)
                    {
                        if (mSelectedAtkType == JoustingAttackType.None)
                        {
                            mSelectedAtkType = GetRandomJoustingAttackType();

                            Lance.Lobby.OnSelectJoustingAtkType(mSelectedAtkType);

                            OnSelectJoustingAttackType(mSelectedAtkType);
                        }
                    }
                }

                // 나와 상대의 거리를 계속 측정하고 있는다.
                Lance.Lobby.UpdateJoustingDistanceUI();
            }
        }

        void PlayFinishJousting()
        {
            if (mIsFinishJousting)
                return;

            Lance.GameManager.StartStageCoroutine(FinishJousting());
        }

        IEnumerator FinishJousting()
        {
            mIsFinishJousting = true;
            mInJoustingMotion = false;  // 마상시합 버튼을 연속으로 눌렀을 때 방어 코드

            yield return new WaitForSeconds(1.5f);

            // 결과를 산출
            var myPowerLevel = Lance.Account.JoustBattleInfo.GetPowerLevel();
            var opponentPowerLevel = mOpponentInfo.GetPowerLevel();

            double totalPowerLevel = myPowerLevel + opponentPowerLevel;

            float myWineRate = (float)(myPowerLevel / totalPowerLevel);

            myWineRate = Mathf.Clamp(myWineRate, 0.25f, 0.99f);

            var opponentAtkType = GetRandomJoustingAttackType();

            float atkSelectWineRate = GetJoustingAttackSelectWineRate(opponentAtkType);

            if ((JoustingAttackType)(int)mJoustingFirstSelectAtkType == JoustingAttackType.None)
                mJoustingFirstSelectAtkType = (int)mSelectedAtkType;

            if ((JoustingAttackType)(int)mJoustingFirstSelectAtkType == mSelectedAtkType)
            {
                mJoustingComboAtkCount++;
            }
            else
            {
                mJoustingFirstSelectAtkType = (int)mSelectedAtkType;

                mJoustingComboAtkCount = 0;
            }

            myWineRate += atkSelectWineRate;

            bool win = Util.Dice(myWineRate);

            int prevRankScore = Lance.Account.JoustRankInfo.GetRankScore();

            // 랭킹 스코어 반영
            Lance.Account.JoustRankInfo.AddRankScore(win ? Lance.GameData.JoustingCommonData.winRankingScore : Lance.GameData.JoustingCommonData.loseRankingScore);

            Lance.Account.UpdateJoustingRankExtraInfo();

            int curRankScore = Lance.Account.JoustRankInfo.GetRankScore();

            // 계정 정보 저장 및 랭킹 정보 최신화
            Lance.BackEnd.UpdateAllAccountInfos((bro) => Lance.BackEnd.UpdateUserRankScores());

            string matchUrl = Lance.GameData.JoustingCommonData.matchingUuid_kor;

            var countryCode = Lance.Account.CountryCode;
            if (countryCode != "KR")
                matchUrl = Lance.GameData.JoustingCommonData.matchingUuid_glb;

            bool ignoreUpdate = false;

            if (Lance.GameData.RankUpdateIgnoreData != null)
            {
                foreach(var ignore in Lance.GameData.RankUpdateIgnoreData.ignores)
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
                SendQueue.Enqueue(Backend.RandomInfo.SetRandomData, RandomType.User, matchUrl, curRankScore, (bro) => { });

            // 보상 지급
            var rewardResult = new RewardResult();

            rewardResult.joustCoin = Lance.GameData.JoustingCommonData.resultCoinReward;
            rewardResult.gloryToken = Lance.GameData.JoustingCommonData.resultGloryTokenReward;

            Lance.GameManager.GiveReward(rewardResult);

            var resultPopup = Lance.PopupManager.CreatePopup<Popup_JoustingResultUI>();
            resultPopup.Init(win, prevRankScore, curRankScore, mSelectedAtkType, opponentAtkType);

            Param param = new Param();
            param.Add("selectedAtkType", $"{mSelectedAtkType}");
            param.Add("opponentAtkType", $"{opponentAtkType}");
            param.Add("result", win ? "WIN" : "LOSE");
            param.Add("myWinRate", myWineRate);
            param.Add("myPowerLevel", myPowerLevel.ToAlphaString());
            param.Add("opponentPowerLevel", opponentPowerLevel.ToAlphaString());

            Lance.BackEnd.InsertLog("FinishJousting", param, 7);
        }

        JoustingAttackType mSelectedAtkType;
        public void OnSelectJoustingAttackType(JoustingAttackType selectType)
        {
            mSelectedAtkType = selectType;

            SoundPlayer.PlayJoustingAtkReady();
        }

        float GetJoustingAttackSelectWineRate(JoustingAttackType opponentAtkType)
        {
            var data = Lance.GameData.JoustingCompatibilityData.TryGet(mSelectedAtkType);
            if (data == null)
                return 0f;

            if (data.strongType == opponentAtkType)
                return Lance.GameData.JoustingCommonData.strongTypeAtkWinRate;
            else if (data.weakType == opponentAtkType)
                return Lance.GameData.JoustingCommonData.weakTypeAtkWinRate;
            else
                return 0f;
        }

        JoustingAttackType GetRandomJoustingAttackType()
        {
            return (JoustingAttackType)UnityEngine.Random.Range((int)JoustingAttackType.Head, (int)JoustingAttackType.Count);
        }

        void PlayGiveupJousting()
        {
            if (mIsPlay == false)
                return;

            Lance.GameManager.StartStageCoroutine(GiveupJousting());
        }

        IEnumerator GiveupJousting()
        {
            mIsPlay = false;

            mCharacterManager.Player.Action.PlayAction(ActionType.Idle, true);

            SoundPlayer.PlayGameOver();

            yield return new WaitForSeconds(2.5f);

            var popup = Lance.PopupManager.CreatePopup<Popup_JoustingUI>();

            popup.Init();

            Time.timeScale = Lance.Account.SpeedMode.InSpeedMode() ? Lance.GameData.CommonData.speedModeValue : 1f;

            yield return ReStartNormalStage();

            //int prevRankScore = Lance.Account.JoustRankInfo.GetRankScore();

            //// 랭킹 스코어 반영
            //Lance.Account.JoustRankInfo.AddRankScore(win ? Lance.GameData.JoustingCommonData.winRankingScore : Lance.GameData.JoustingCommonData.loseRankingScore);

            //int curRankScore = Lance.Account.JoustRankInfo.GetRankScore();

            //// 랭킹 정보 최신화
            //Lance.BackEnd.UpdateUserRankScores();

            //// 보상 지급
            //var rewardResult = new RewardResult();

            //rewardResult.joustCoin = Lance.GameData.JoustingCommonData.resultCoinReward;

            //Lance.GameManager.GiveReward(rewardResult);

            //var resultPopup = Lance.PopupManager.CreatePopup<Popup_JoustingResultUI>();
            //resultPopup.Init(win, prevRankScore, curRankScore);

            //Param param = new Param();
            //param.Add("selectedAtkType", $"{mSelectedAtkType}");
            //param.Add("result", win ? "WIN" : "LOSE");
            //param.Add("myWinRate", myWineRate);
            //param.Add("myPowerLevel", myPowerLevel.ToAlphaString());
            //param.Add("opponentPowerLevel", opponentPowerLevel.ToAlphaString());

            //Lance.BackEnd.InsertLog("FinishJousting", param, 7);
        }

        void PlayExitJousting()
        {
            if (mIsPlay == false)
                return;

            Lance.GameManager.StartStageCoroutine(ExitJousting());
        }

        IEnumerator ExitJousting()
        {
            var popup = Lance.PopupManager.CreatePopup<Popup_JoustingUI>();
            popup.Init();

            Time.timeScale = Lance.Account.SpeedMode.InSpeedMode() ? Lance.GameData.CommonData.speedModeValue : 1f;

            Lance.GameManager.CheckQuest(QuestType.JoustingComboAtk, mJoustingComboAtkCount);

            mJoustingFirstSelectAtkType = (int)JoustingAttackType.None;
            mJoustingComboAtkCount = 0;

            yield return ReStartNormalStage();
        }
    }
}