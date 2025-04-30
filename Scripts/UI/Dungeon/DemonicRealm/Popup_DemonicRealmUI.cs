using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Popup_DemonicRealmUI : PopupBase
    {
        int mSelectedStep;
        GameObject mRedDotObj;
        DemonicRealmItemUI mParent;
        DemonicRealmData mData;
        TextMeshProUGUI mTextTicketCount;
        TextMeshProUGUI mTextAdRemainCount;
        TextMeshProUGUI mTextSelectedStep;
        Image mImageAutoChallengeCheck;
        Button mButtonEntranceByAd;
        Button mButtonEntranceByTicket;
        Button mButtonSweep;
        List<ItemSlotUI> mRewardSlotUIs;
        int SweepRequireTicket => Lance.GameData.DemonicRealmCommonData.sweepRequireTicket;
        int EntranceRequireTicket => Lance.GameData.DemonicRealmCommonData.entranceRequireTicket;
        public void Init(DemonicRealmItemUI parent, DemonicRealmData data)
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get($"Title_{data.type}DemonicRealm"));

            mParent = parent;
            mData = data;
            mRewardSlotUIs = new List<ItemSlotUI>();

            // ========================================
            // 내 소유 티켓/정보 버튼 초기화
            // ========================================
            Image imgTicket = gameObject.FindComponent<Image>("Image_Ticket");
            imgTicket.sprite = Lance.Atlas.GetDemonicRealmStone(mData.type);

            mTextTicketCount = gameObject.FindComponent<TextMeshProUGUI>("Text_TicketCount");

            // ========================================
            // 난이도 설정 초기화
            // ========================================
            mTextAdRemainCount = gameObject.FindComponent<TextMeshProUGUI>("Text_AdRemainCount");
            mTextSelectedStep = gameObject.FindComponent<TextMeshProUGUI>("Text_Step");

            int maxStep = DataUtil.GetDemonicRealmBestStage(data.type);
            mSelectedStep = Mathf.Clamp(Lance.Account.GetBestDemonicRealmStep(data.type), 1, maxStep);
            if (data.type.HaveNextStepDemonicRealm())
            {
                var buttonPrevStep = gameObject.FindComponent<Button>("Button_PrevStep", ignoreAssert: true);
                buttonPrevStep.SetButtonAction(() => OnChangeStep(isPrev: true));
                var buttonNextStep = gameObject.FindComponent<Button>("Button_NextStep", ignoreAssert: true);
                buttonNextStep.SetButtonAction(() => OnChangeStep(isPrev: false));
            }

            var textAutoChallenge = gameObject.FindComponent<TextMeshProUGUI>("Text_AutoChallenge");
            textAutoChallenge.text = data.type.HaveNextStepDemonicRealm() ? StringTableUtil.Get("UIString_AutoChallengeNextStep") : StringTableUtil.Get("UIString_AutoChallenge");

            InitButtons();

            Refresh();
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            if (mRewardSlotUIs != null)
            {
                foreach (ItemSlotUI itemSlotUI in mRewardSlotUIs)
                {
                    itemSlotUI.OnRelease();

                    Lance.ObjectPool.ReleaseUI(itemSlotUI.gameObject);
                }

                mRewardSlotUIs = null;
            }

            base.Close(immediate, hideMotion);
        }

        void OnChangeStep(bool isPrev)
        {
            int bestStep = DataUtil.GetDemonicRealmBestStage(mData.type);
            if (isPrev == false)
            {
                int changeStep = mSelectedStep + 1;
                if (changeStep > bestStep)
                {
                    // 최고 도달 스테이지 보다 높은 스테이지 선택할 수 없어
                    UIUtil.ShowSystemErrorMessage("IsMaxStepDemonicRealm");

                    return;
                }

                mSelectedStep = mSelectedStep + 1;
            }
            else
            {
                mSelectedStep = mSelectedStep - 1;
                mSelectedStep = Math.Max(1, mSelectedStep);
            }

            Refresh();
        }

        public void Refresh()
        {
            RefreshSelectedStep();
            RefreshMonsterRewards();
            RefreshButtons();

            mTextTicketCount.text = $"{Lance.Account.GetDemonicRealmStone(mData.type)} / {EntranceRequireTicket}";
            mTextAdRemainCount.text = $"{Lance.Account.GetDemonicRealmRemainWatchAdCount(mData.type)} / {mData.dailyWatchAdCount}";
        }

        void RefreshSelectedStep()
        {
            StringParam param = new StringParam("step", mSelectedStep);

            mTextSelectedStep.text = StringTableUtil.Get("UIString_Step", param);
        }

        void RefreshMonsterRewards()
        {
            var stageData = GetStageData();
            if (stageData == null)
                return;

            var rewardsObj = gameObject.FindGameObject("Rewards");

            rewardsObj.AllChildObjectOff();

            var soulStoneRewardData = DataUtil.GetDemonicRealmSoulStoneRewardData(stageData.type, stageData.stage);
            if (soulStoneRewardData != null)
            {
                if (soulStoneRewardData.dropProb > 0)
                {
                    ItemInfo itemInfo = new ItemInfo(ItemType.SoulStone).SetShowStr($"{soulStoneRewardData.dropProb * 100f:f2}%");

                    CreateItemSlot().Init(itemInfo);
                }
            }

            var manaEssenceRewardData = DataUtil.GetDemonicRealmManaEssenceRewardData(stageData.type, stageData.stage);
            if (manaEssenceRewardData != null)
            {
                if (manaEssenceRewardData.dropProb > 0)
                {
                    ItemInfo itemInfo = new ItemInfo(ItemType.ManaEssence).SetShowStr($"{manaEssenceRewardData.dropProb * 100f:f2}%");

                    CreateItemSlot().Init(itemInfo);
                }
            }

            if (stageData.monsterDropReward.IsValid())
            {
                MonsterRewardData monsterReward = DataUtil.GetMonsterRewardData(stageData.type, stageData.monsterDropReward);
                if (monsterReward != null)
                {
                    if (monsterReward.exp > 0)
                    {
                        double amount = monsterReward.exp * stageData.monsterLimitCount;

                        ItemInfo itemInfo = new ItemInfo(ItemType.Exp).SetShowStr($"0 ~ {amount.ToAlphaString()}");

                        CreateItemSlot().Init(itemInfo);
                    }

                    if (monsterReward.gold > 0)
                    {
                        double amount = monsterReward.gold * stageData.monsterLimitCount;

                        ItemInfo itemInfo = new ItemInfo(ItemType.Gold).SetShowStr($"0 ~ {amount.ToAlphaString()}");

                        CreateItemSlot().Init(itemInfo);
                    }

                    if (monsterReward.stones > 0)
                    {
                        double amount = monsterReward.stones * stageData.monsterLimitCount;

                        ItemInfo itemInfo = new ItemInfo(ItemType.UpgradeStone).SetShowStr($"0 ~ {amount.ToAlphaString()}");

                        CreateItemSlot().Init(itemInfo);
                    }

                    if (monsterReward.reforgeStone > 0)
                    {
                        double amount = monsterReward.reforgeStone * stageData.monsterLimitCount;

                        ItemInfo itemInfo = new ItemInfo(ItemType.ReforgeStone).SetShowStr($"0 ~ {amount.ToAlphaString()}");

                        CreateItemSlot().Init(itemInfo);
                    }

                    if (monsterReward.petFood > 0)
                    {
                        double amount = monsterReward.petFood * stageData.monsterLimitCount;

                        ItemInfo itemInfo = new ItemInfo(ItemType.PetFood).SetShowStr($"0 ~ {amount.ToAlphaString()}");

                        CreateItemSlot().Init(itemInfo);
                    }

                    if (monsterReward.randomEquipment.IsValid())
                    {
                        var data = Lance.GameData.RandomEquipmentRewardData.TryGet(monsterReward.randomEquipment);

                        ItemInfo itemInfo = new ItemInfo(ItemType.Random_Equipment, 1)
                            .SetId(data.id)
                            .SetGrade(data.grade)
                            .SetSubGrade(data.subGrade)
                            .SetShowStr($"{monsterReward.equipmentProb * 100f:f2}%");

                        CreateItemSlot().Init(itemInfo);
                    }

                    if (monsterReward.randomAccessory.IsValid())
                    {
                        var data = Lance.GameData.RandomAccessoryRewardData.TryGet(monsterReward.randomAccessory);

                        ItemInfo itemInfo = new ItemInfo(ItemType.Random_Accessory, 1)
                            .SetId(data.id)
                            .SetGrade(data.grade)
                            .SetSubGrade(data.subGrade)
                            .SetShowStr($"{monsterReward.accessoryProb * 100f:f2}%");

                        CreateItemSlot().Init(itemInfo);
                    }
                }
            }

            ItemSlotUI CreateItemSlot()
            {
                var itemSlotUIObj = Lance.ObjectPool.AcquireUI("DungeonRewardSlotUI", rewardsObj.GetComponent<RectTransform>());

                var itemSlotUI = itemSlotUIObj.GetOrAddComponent<ItemSlotUI>();

                mRewardSlotUIs.Add(itemSlotUI);

                return itemSlotUIObj.GetOrAddComponent<ItemSlotUI>();
            }
        }

        void RefreshButtons()
        {
            int myBestStep = Lance.Account.GetBestDemonicRealmStep(mData.type);

            bool canEntrance = myBestStep >= mSelectedStep;
            bool canSweep = mData.type.HaveNextStepDemonicRealm() ? myBestStep > mSelectedStep : myBestStep != 1;
            bool isEnoughSweepTicket = Lance.Account.IsEnoughDemonicRealmStone(mData.type, SweepRequireTicket);
            bool isEnoughTicket = Lance.Account.IsEnoughDemonicRealmStone(mData.type, EntranceRequireTicket);
            bool isEnoughWatchAdCount = Lance.Account.IsEnoughDemonicRealmWatchAdCount(mData.type);

            mButtonSweep.SetActiveFrame(canSweep && isEnoughSweepTicket);
            mButtonEntranceByAd.gameObject.SetActive(isEnoughTicket == false && isEnoughWatchAdCount);
            mButtonEntranceByTicket.gameObject.SetActive(isEnoughTicket || isEnoughWatchAdCount == false);
            mButtonEntranceByTicket.SetActiveFrame(isEnoughTicket && canEntrance);
            //mRedDotObj.SetActive(isEnoughTicket && canEntrance);
        }

        void InitButtons()
        {
            // 자동 도전 버튼 초기화
            var buttonAutoChallenge = gameObject.FindComponent<Button>("Button_AutoChallenge");
            buttonAutoChallenge.SetButtonAction(OnAutoChallengeButton);
            mImageAutoChallengeCheck = gameObject.FindComponent<Image>("Image_Check");
            mImageAutoChallengeCheck.gameObject.SetActive(SaveBitFlags.DemonicRealmAutoChallenge.IsOn());

            // ========================================
            // 입장 버튼 초기화
            // ========================================
            mButtonSweep = gameObject.FindComponent<Button>("Button_Sweep");
            mButtonSweep.SetButtonAction(() => OnEntrance(EntranceType.Sweep));

            mButtonEntranceByAd = gameObject.FindComponent<Button>("Button_EntranceByAd");
            mButtonEntranceByAd.SetButtonAction(() => OnEntrance(EntranceType.AD));

            mButtonEntranceByTicket = gameObject.FindComponent<Button>("Button_EntranceByTicket");
            mButtonEntranceByTicket.SetButtonAction(() => OnEntrance(EntranceType.Ticket));

            mRedDotObj = mButtonEntranceByTicket.gameObject.FindGameObject("RedDot");
            mRedDotObj.SetActive(false);

            void OnEntrance(EntranceType entranceType)
            {
                if (entranceType == EntranceType.AD)
                {
                    StartDemonicRealm();
                }
                else
                {
                    // 내가 도달한 최고 스테이지보다 상위인지 확인한번하자
                    int myBestStep = Lance.Account.GetBestDemonicRealmStep(mData.type);
                    if (myBestStep < mSelectedStep)
                    {
                        UIUtil.ShowSystemErrorMessage("HaveNotReachedDemonicRealmStep");

                        return;
                    }

                    if (entranceType == EntranceType.Ticket)
                    {
                        StartDemonicRealm();
                    }
                    else if (entranceType == EntranceType.Sweep)
                    {
                        bool canSweep = mData.type.HaveNextStepDemonicRealm() ? (myBestStep > mSelectedStep) : myBestStep != 1;
                        if (canSweep == false)
                        {
                            UIUtil.ShowSystemErrorMessage("HaveNotReachedDemonicRealmSelectedStep");

                            return;
                        }

                        if (Lance.Account.IsEnoughDemonicRealmStone(mData.type, SweepRequireTicket) == false)
                        {
                            UIUtil.ShowSystemErrorMessage("IsNotEnoughDemonicRealmSweepStone");

                            return;
                        }

                        int sweepStep = mData.type.HaveNextStepDemonicRealm() ? mSelectedStep : Lance.Account.DemonicRealm.GetBestStep(mData.type);

                        var popup = Lance.PopupManager.CreatePopup<Popup_DemonicRealmSweepUI>();
                        popup.Init(this, mData.type, sweepStep);
                    }
                }

                void StartDemonicRealm()
                {
                    if (GetStageData() != null)
                    {
                        Lance.GameManager.StartDemonicRealm(GetStageData(), entranceType);

                        mParent.Refresh();

                        if (entranceType == EntranceType.AD)
                        {
                            Refresh();
                        }
                        else
                        {
                            Close();
                        }
                    }
                }
            }
        }

        void OnAutoChallengeButton()
        {
            SaveBitFlags.DemonicRealmAutoChallenge.Toggle();

            mImageAutoChallengeCheck.gameObject.SetActive(SaveBitFlags.DemonicRealmAutoChallenge.IsOn());
        }

        StageData GetStageData()
        {
            if (mData.type.HaveNextStepDemonicRealm())
            {
                return DataUtil.GetDemonicRealmStageData(mData.type, mSelectedStep);
            }
            else
            {
                return DataUtil.GetDemonicRealmStageData(mData.type, 1);
            }
        }
    }
}