using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEndChat;

namespace Lance
{
    class LobbyUI : MonoBehaviour
    {
        bool mInit;
        LobbyTabUIManager mTabUIManager;    // 하단 탭 관련 모두

        Lobby_UserInfoUI mUserInfoUI;
        Lobby_CurrencyUI mCurrencyUI;
        Lobby_BuffUI mBuffUI;

        Lobby_WeekendDoubleUI mWeekendDoubleUI;
        Lobby_SettingMenuUI mSettingMenuUI; // 설정
        Lobby_MenuLUI mMenuLUI;
        Lobby_MenuUI mMenuUI;               // 랭킹, 퀘스트, 스테이지, 저장
        Lobby_StageInfoUI mStageInfoUI;
        Lobby_RewardListUI mRewardListUI;
        Lobby_SkillCastUI mSkillCastUI;
        Lobby_ChattingUI mChattingUI;

        Lobby_SkillAutoCastUI mSkillAutoCastUI;
        Lobby_SpeedModeUI mSpeedModeUI;
        Lobby_SkipBattleUI mSkipBattleUI;
        Lobby_CentralEssenceUI mCentralEssenceUI;

        Lobby_DungeonModeUI mDungeonModeUI;
        Lobby_DemonicRealmModeUI mDemonicRealmModeUI;
        Lobby_JoustingModeUI mJoustingModeUI;
        Lobby_BossAppearUI mLobbyBossAppearUI;

        GuideActionManager mGuideActionManager;
        CharacterMessageManager mCharacterMessageManager;
        public GuideActionManager Guide => mGuideActionManager;
        public bool IsInit => mInit;
        public void Init()
        {
            GameObject tabUIManagerObj = Find("LobbyTabUIList");

            mTabUIManager = new LobbyTabUIManager();
            StartCoroutine(mTabUIManager.AsyncInit(tabUIManagerObj));

            var userInfoObj = Find("UserInfo");

            mUserInfoUI = userInfoObj.GetOrAddComponent<Lobby_UserInfoUI>();
            mUserInfoUI.Init();

            var currencyObj = Find("Currency");

            mCurrencyUI = currencyObj.GetOrAddComponent<Lobby_CurrencyUI>();
            mCurrencyUI.Init();

            var buffObj = Find("Buffs");

            mBuffUI = buffObj.GetOrAddComponent<Lobby_BuffUI>();
            mBuffUI.Init();

            var weekendDoubleObj = Find("WeekendDouble");

            mWeekendDoubleUI = weekendDoubleObj.GetOrAddComponent<Lobby_WeekendDoubleUI>();
            mWeekendDoubleUI.Init();

            var menusLObj = Find("MenusL");

            mMenuLUI = menusLObj.GetOrAddComponent<Lobby_MenuLUI>();
            mMenuLUI.Init();

            var menusObj = Find("Menus");

            mMenuUI = menusObj.GetOrAddComponent<Lobby_MenuUI>();
            mMenuUI.Init();

            var settingMenuObj = gameObject.FindGameObject("SettingMenu");
            mSettingMenuUI = settingMenuObj.GetOrAddComponent<Lobby_SettingMenuUI>();
            mSettingMenuUI.Init();

            var rewardObj = Find("RewardList");

            mRewardListUI = rewardObj.GetOrAddComponent<Lobby_RewardListUI>();
            mRewardListUI.Init();

            var skillObj = Find("SkillList");

            mSkillCastUI = skillObj.GetOrAddComponent<Lobby_SkillCastUI>();
            mSkillCastUI.Init();

            var skillAutoCastObj = Find("AutoCastSkill");

            mSkillAutoCastUI = skillAutoCastObj.GetOrAddComponent<Lobby_SkillAutoCastUI>();
            mSkillAutoCastUI.Init();

            var speedModeObj = Find("SpeedMode");

            mSpeedModeUI = speedModeObj.GetOrAddComponent<Lobby_SpeedModeUI>();
            mSpeedModeUI.Init();

            var skipBattleObj = Find("SkipBattle");

            mSkipBattleUI = skipBattleObj.GetOrAddComponent<Lobby_SkipBattleUI>();
            mSkipBattleUI.Init();

            var centralEssenceObj = Find("CentralEssence");

            mCentralEssenceUI = centralEssenceObj.GetOrAddComponent<Lobby_CentralEssenceUI>();
            mCentralEssenceUI.Init();

            var stageInfoObj = Find("StageInfo");

            mStageInfoUI = stageInfoObj.GetOrAddComponent<Lobby_StageInfoUI>();
            mStageInfoUI.Init();

            var dungeonModeUI = Find("DungeonModeUI");

            mDungeonModeUI = dungeonModeUI.GetOrAddComponent<Lobby_DungeonModeUI>();
            mDungeonModeUI.Init();

            var demonicRealmModeUI = Find("DemonicRealmModeUI");

            mDemonicRealmModeUI = demonicRealmModeUI.GetOrAddComponent<Lobby_DemonicRealmModeUI>();
            mDemonicRealmModeUI.Init();

            var joustingModeUI = Find("JoustingModeUI");

            mJoustingModeUI = joustingModeUI.GetOrAddComponent<Lobby_JoustingModeUI>();
            mJoustingModeUI.Init();

            var bossAppearObj = Find("BossAppearUI");

            mLobbyBossAppearUI = bossAppearObj.GetOrAddComponent<Lobby_BossAppearUI>();
            mLobbyBossAppearUI.Init();

            mGuideActionManager = new GuideActionManager();
            mGuideActionManager.Init();

            mCharacterMessageManager = new CharacterMessageManager();
            mCharacterMessageManager.Init(mGuideActionManager);

            var chattingUIObj = gameObject.FindGameObject("Chatting");

            mChattingUI = chattingUIObj.GetOrAddComponent<Lobby_ChattingUI>();
            mChattingUI.Init();

            RefreshRedDots();
            RefreshContentsLockUI();

            mInit = true;
        }

        public void RefreshContentsLockUI()
        {
            mTabUIManager.RefreshContentsLockUI();
            mStageInfoUI.RefreshContentsLockUI();
            mSkillAutoCastUI.RefreshContentsLockUI();
            mSpeedModeUI.RefreshContentsLockUI();
            mSkipBattleUI.RefreshContentsLockUI();
            mCentralEssenceUI.RefreshContentsLockUI();
            mSettingMenuUI.RefreshContentsLockUI();
            mMenuUI.RefreshContentsLockUI();
            mMenuLUI.RefreshContentsLockUI();
            mChattingUI.RefreshContentsLockUI();
        }

        public void StartGuideAction(bool isAuto)
        {
            if (mGuideActionManager.InGuide == false)
            {
                mGuideActionManager.StartGuide(Lance.Account.GuideQuest.GetCurrentStep(), isAuto);
            }
        }

        public void StartCharacterMessage(string id)
        {
            mCharacterMessageManager.StartMessage(id);
        }

        public void StartTestGuideAction(int step, bool isAuto)
        {
            mGuideActionManager.EndGuide();
            mGuideActionManager.StartGuide(step, isAuto);
        }

        public GameObject Find(string name)
        {
            return gameObject.FindGameObject(name);
        }

        public void ChangeTab(LobbyTab tab)
        {
            mTabUIManager.ChangeTab(tab);
            mTabUIManager.RefreshTabFrame();
        }

        public T GetLobbyTabUI<T>() where T : LobbyTabUI
        {
            return mTabUIManager.GetTab<T>();
        }

        public void UpdateMonsterKillUI()
        {
            mStageInfoUI.UpdateMonsterKillCount();
        }

        public void RefreshStageInfoUI()
        {
            mStageInfoUI.Refresh();
        }

        public void RefreshGuideQuestUI()
        {
            mMenuLUI.RefreshGuideQuestUI();
        }

        public void UpdateBossHp()
        {
            mStageInfoUI.UpdateBossHp();
        }

        public void UpdateStageTimerUI()
        {
            mStageInfoUI.UpdateTimer();
        }

        public void UpdateJoustingDistanceUI()
        {
            mStageInfoUI.UpdateJoustingDistance();
        }

        public void UpdateSkillCastUI(Player player)
        {
            mSkillCastUI.OnUpdate(player);
            mDungeonModeUI.OnUpdate(player);
            mDemonicRealmModeUI.OnUpdate(player);
        }

        public void OnReceiveGuideQuestReward()
        {
            UpdateCurrencyUI();

            RefreshContentsLockUI();
        }

        public void UpdateCurrencyUI()
        {
            mCurrencyUI.Refresh();
        }

        public void UpdateGoldUI()
        {
            mCurrencyUI.UpdateGold();
        }

        public void UpdateStatureTabUI(StatureTab tab)
        {
            var statureTabUI = mTabUIManager.GetTab<Lobby_StatureUI>();

            statureTabUI.RefreshTab(tab);
        }

        public void UpdateUpgradeStoneUI()
        {
            mCurrencyUI.UpdateUpgradeStones();
        }

        public void UpdateGemUI()
        {
            mCurrencyUI.UpdateGem();
        }

        public void UpdateBuffUI(bool purchasedRemovedAD)
        {
            mBuffUI.OnUpdate(purchasedRemovedAD);

            var popup = Lance.PopupManager.GetPopup<Popup_BuffUI>();

            popup?.Refresh(purchasedRemovedAD);

            RefreshBuffRedDot();
        }

        public void UpdateWeekendDoubleUI()
        {
            mWeekendDoubleUI.RefreshUI();
        }

        public void UpdateCentralEssenceUI()
        {
            mCentralEssenceUI.RefreshCentralEssenceUI();
        }

        public void UpdadteUserInfo_Nickname()
        {
            mUserInfoUI.UpdateNickname();
        }

        public void OnChangePreset()
        {
            mSkillCastUI.Refresh();
            mDungeonModeUI.Refresh();
            mDemonicRealmModeUI.Refresh();

            RefreshTabRedDot(LobbyTab.Skill);
        }

        public void RefreshSkillCastUI()
        {
            mSkillCastUI.Refresh();
            mDungeonModeUI.Refresh();
            mDemonicRealmModeUI.Refresh();
        }

        public void StackReward(RewardResult reward)
        {
            mMenuLUI.StackReward(reward);
        }

        public void StackDamage(string id, double damage)
        {
            mMenuLUI.StackDamage(id, damage);
        }

        public void ShowRewardUI(RewardResult reward)
        {
            mRewardListUI.ReserveReward(reward);
        }

        public void ClearReservedRewardUI()
        {
            mRewardListUI.ClearReservedReward();
        }

        public void SetActiveRewardUI(bool active)
        {
            mRewardListUI.SetActive(active);
        }

        public void OnStartStage(StageData stageData)
        {
            mUserInfoUI.OnStartStage(stageData);
            mBuffUI.OnStartStage(stageData);
            mCurrencyUI.OnStartStage(stageData);

            mStageInfoUI.OnStartStage(stageData);

            mSettingMenuUI.OnStartStage(stageData);
            mMenuUI.OnStartStage(stageData);
            mMenuLUI.OnStartStage(stageData);

            mSkillAutoCastUI.OnStartStage(stageData);
            mSkipBattleUI.OnStatStage(stageData);
            mCentralEssenceUI.OnStartStage(stageData);
            mSpeedModeUI.OnStartStage(stageData);
            mSkillCastUI.OnStartStage(stageData);
            mDungeonModeUI.OnStartStage(stageData);
            mDemonicRealmModeUI.OnStartStage(stageData);
            mJoustingModeUI.OnStartStage(stageData);
        }

        public void OnStartJousting(JoustBattleInfo myBattleInfo, JoustBattleInfo opponentBattleInfo)
        {
            OnStartStage(Lance.GameData.JoustingStageData);

            mStageInfoUI.RefreshJoustingInfo(myBattleInfo, opponentBattleInfo);
            mStageInfoUI.UpdateJoustingDistance();
        }

        public void SetDungeonName(string dungeonName)
        {
            mDungeonModeUI.SetDungeonName(dungeonName);
        }

        public void SetDemonicRealName(string demonicRealmName)
        {
            mDemonicRealmModeUI.SetDemonicRealmName(demonicRealmName);
        }

        public InventoryTabUI GetInventoryTabUI(ItemType itemType)
        {
            var tab = mTabUIManager.GetTab<Lobby_InventoryUI>();

            return tab.GetTab(itemType.ChangeToInventoryTab());
        }

        public void UpdateExpUI()
        {
            mUserInfoUI.UpdateExp();
            mUserInfoUI.UpdateLevel();

            var popup = Lance.PopupManager.GetPopup<Popup_UserInfoUI>();

            popup?.RefreshExp();
        }

        public void UpdatePowerLevelUI()
        {
            mUserInfoUI.UpdatePowerLevel();
        }

        public void UpdatePortrait()
        {
            mUserInfoUI.UpdatePortrait();
        }

        public void UpdateAchievement()
        {
            mUserInfoUI.UpdateAchievement();
        }

        public void RefreshFrameRedDot()
        {
            mUserInfoUI.RefreshFrameRedDot();
        }

        private void Update()
        {
            if (mInit)
            {
                float dt = Time.unscaledDeltaTime;

                mGuideActionManager.OnUpdate(dt);
                mCharacterMessageManager.OnUpdate(dt);
                mRewardListUI.OnUpdate(dt);
                mTabUIManager.OnUpdate();
                mLobbyBossAppearUI.OnUpdate();

                mChattingUI.OnUpdate(dt);
                mMenuLUI.OnUpdate(dt);
            }
        }

        // ========================================
        // ########  레드닷 관련  ########
        // ========================================
        public void RefreshRedDots()
        {
            RefreshMenuRedDots();
            RefreshAllTabRedDots();
        }

        public void RefreshAllTabRedDots()
        {
            mTabUIManager.RefreshRedDots();
        }

        public void RefreshNavBarRedDot()
        {
            mTabUIManager.RefreshNavBarRedDot();
        }

        public void RefreshTabRedDot(LobbyTab tab)
        {
            mTabUIManager.RefreshRedDot(tab);
        }

        public void RefreshMenuRedDots()
        {
            mSettingMenuUI.RefreshRedDots();
            mMenuUI.RefreshRedDots();
            mMenuLUI.RefreshRedDots();
            mSkipBattleUI.Refresh();
        }

        public void RefreshSkipBattleRedDot()
        {
            mSkipBattleUI.Refresh();
        }

        public void OpenMenuUI()
        {
            mMenuUI.OnButtonOpenMenu();
        }

        public bool IsOpenedMenuUI()
        {
            return mMenuUI.IsActiveOpenMenu;
        }

        public void RefreshPostRedDot()
        {
            mSettingMenuUI.RefreshRedDots();
        }

        public void RefreshAttendanceRedDot()
        {
            mSettingMenuUI.RefreshRedDots();
        }

        public void RefreshEventRedDot()
        {
            mMenuLUI.RefreshEventRedDot();
        }

        public void RefreshLottoRedDot()
        {
            mMenuLUI.RefreshLottoRedDot();
        }

        public void RefreshPassRedDot()
        {
            mMenuLUI.RefreshPassRedDot();
        }

        //public void RefreshChattingRedDot()
        //{
        //    mMenuUI.RefreshChattingRedDot();
        //}

        public void RefreshQuestRedDot()
        {
            mMenuUI.RefreshQuestRedDot();
        }

        public void RefreshBountyQuestRedDot()
        {
            mMenuUI.RefreshBountyQuestRedDot();
        }

        public void RefreshCollectionRedDot()
        {
            mMenuUI.RefreshCollectionRedDot();
        }

        public void RefreshEssenceRedDot()
        {
            mMenuUI.RefreshEssenceRedDot();
        }

        public void RefreshJoustingRedDot()
        {
            mMenuUI.RefreshJoustingRedDot();
        }

        public void RefreshBuffRedDot()
        {
            mMenuUI.RefreshBuffRedDot();
        }

        public void ShowBossAppearUI(StageType stageType)
        {
            mLobbyBossAppearUI.Show(stageType);   
        }

        public void RefreshTab()
        {
            mTabUIManager.Refresh();
        }

        public void RefreshAllTab(LobbyTab tab)
        {
            mTabUIManager.RefreshAllTab(tab);
        }

        public void UpdateSpeedModeUI(bool purchasedRemovedAD)
        {
            mSpeedModeUI.RefreshSpeedModeUI(purchasedRemovedAD);
        }

        public bool InMessageMode()
        {
            return mCharacterMessageManager?.InMessage ?? false;
        }

        public void OnSelectJoustingAtkType(JoustingAttackType atkType)
        {
            mJoustingModeUI.OnSelectAtkType(atkType);
        }

        public void RefreshChattingUI(MessageInfo messageInfo)
        {
            mChattingUI.RefreshChatMessage(messageInfo);
        }

        public void OnRelease()
        {
            mInit = false;
        }

        public void Localize()
        {
            var readerUIs = gameObject.GetComponentsInChildren<StringTableUIReader>(true);
            foreach(var readerUI in readerUIs)
            { 
                readerUI.Localize(); 
            }

            mTabUIManager.Localize();
            mMenuLUI.Localize();
            mChattingUI.Localize();
            mDungeonModeUI.Localize();
            mDemonicRealmModeUI.Localize();
            mJoustingModeUI.Localize();
            mGuideActionManager.Localize();
        }

        bool mIsMovieMode;
        public void ToggleMovieMode()
        {
            mIsMovieMode = !mIsMovieMode;

            var userInfoObj = Find("UserInfo");
            var currencyObj = Find("Currency");
            var buffObj = Find("Buffs");
            var stageInfoObj = Find("StageInfo");
            var menusLObj = Find("MenusL");
            var menusObj = Find("Menus");
            var settingMenuObj = gameObject.FindGameObject("SettingMenu");
            var anChorCenterObj = Find("Anchor_Center");

            userInfoObj.SetActive(!mIsMovieMode);
            currencyObj.SetActive(!mIsMovieMode);
            buffObj.SetActive(!mIsMovieMode);
            stageInfoObj.SetActive(!mIsMovieMode);
            menusLObj.SetActive(!mIsMovieMode);
            menusObj.SetActive(!mIsMovieMode);
            settingMenuObj.SetActive(!mIsMovieMode);
            anChorCenterObj.SetActive(!mIsMovieMode);
        }
    }
}