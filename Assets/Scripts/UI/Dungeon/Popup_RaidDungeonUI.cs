using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace Lance
{
    class Popup_RaidDungeonUI : PopupBase
    {
        DungeonData mData;
        DungeonItemUI mParent;
        ElementalType mRaidBossType;

        Button mButtonSweep;
        Button mButtonEntranceByTicket;
        TextMeshProUGUI mTextTicketCount;
        Button mButtonEntranceByAd;
        TextMeshProUGUI mTextAdRemainCount;

        GameObject mTicketRedDotObj;
        public DungeonItemUI Parent => mParent;
        int SweepRequireTicket => Lance.GameData.DungeonCommonData.sweepRequireTicket;
        int EntranceRequireTicket => Lance.GameData.DungeonCommonData.entranceRequireTicket;
        public void Init(DungeonItemUI parent, DungeonData data)
        {
            mParent = parent;
            mData = data;
            mRaidBossType = (ElementalType)DataUtil.GetNowRaidBossElementalTypeIndex();

            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get($"Title_{data.type}Dungeon"));

            // 최고 점수
            var textBestScore = gameObject.FindComponent<TextMeshProUGUI>("Text_BestRecordValue");
            textBestScore.text = Lance.Account.Dungeon.GetRaidBossBestDamage((ElementalType)DataUtil.GetNowRaidBossElementalTypeIndex()).ToAlphaString();

            // 보스 정보
            var imageRaidBoss = gameObject.FindComponent<Image>("Image_RaidBoss");
            imageRaidBoss.sprite = Lance.Atlas.GetDungeonThumbnail(StageType.Raid);

            var buttonRank = gameObject.FindComponent<Button>("Button_Rank");
            buttonRank.SetButtonAction(OnRankButton);

            InitElementalTypes();

            InitRewardInfos();

            mButtonSweep = gameObject.FindComponent<Button>("Button_Sweep");
            mButtonSweep.SetButtonAction(() => OnEntrance(EntranceType.Sweep));
            mButtonEntranceByAd = gameObject.FindComponent<Button>("Button_EntranceByAd");
            mButtonEntranceByAd.SetButtonAction(() => OnEntrance(EntranceType.AD));
            mButtonEntranceByTicket = gameObject.FindComponent<Button>("Button_EntranceByTicket");
            mButtonEntranceByTicket.SetButtonAction(() => OnEntrance(EntranceType.Ticket));
            mTicketRedDotObj = mButtonEntranceByTicket.gameObject.FindGameObject("RedDot");
            mTextTicketCount = gameObject.FindComponent<TextMeshProUGUI>("Text_TicketCount");
            mTextAdRemainCount = gameObject.FindComponent<TextMeshProUGUI>("Text_AdRemainCount");

            void OnEntrance(EntranceType entranceType)
            {
                if (entranceType == EntranceType.Sweep)
                {
                    double raidBestDamage = Lance.Account.GetRaidBossBestDamage(mRaidBossType);

                    if (raidBestDamage <= 0)
                    {
                        string elementalName = StringTableUtil.GetName($"Elemental_{mRaidBossType}");

                        StringParam param = new StringParam("elemental", elementalName);

                        UIUtil.ShowSystemErrorMessage("NeedFirstCompleteRaidBoss", param: param);

                        return;
                    }

                    var popup = Lance.PopupManager.CreatePopup<Popup_RaidDungeonSweepUI>();
                    popup.Init(this, mData.type, mRaidBossType);
                }
                else
                {
                    var stageData = DataUtil.GetDungeonStageData(StageType.Raid, 1);
                    if (stageData != null)
                    {
                        Lance.GameManager.StartDungeon(stageData, entranceType, () => 
                        {
                            if (entranceType == EntranceType.AD)
                            {
                                mParent.Refresh();

                                Refresh();
                            }
                            else
                            {
                                mParent.Refresh();

                                Close();
                            }
                        });
                    }
                }
            }

            Refresh();
        }

        void InitElementalTypes()
        {
            // 보스 속성 표시
            var elementalSystemObj1 = gameObject.FindGameObject("Image_ElementalSystem_1");
            var elementalSystemObj2 = gameObject.FindGameObject("Image_ElementalSystem_2");

            InitElementals(elementalSystemObj1, Lance.GameData.PetCommonData.strongTypeAtkValue);
            InitElementals(elementalSystemObj2, Lance.GameData.PetCommonData.weakTypeAtkValue);

            void InitElementals(GameObject elementalSystemObj, float bonusValue)
            {
                var textBonusValue = elementalSystemObj.FindComponent<TextMeshProUGUI>("Text_BonusValue");

                StringParam param = new StringParam("valueAmount", $"{bonusValue:F1}");
                textBonusValue.text = StringTableUtil.Get("UIString_ValueAmount", param);

                for (int i = 0; i < (int)ElementalType.Count; ++i)
                {
                    ElementalType elementalType = (ElementalType)i;

                    var elementalFX = elementalSystemObj.FindGameObject($"ItemSparkle_{elementalType}");

                    elementalFX.SetActive(elementalType == mRaidBossType);
                }
            }
        }

        void InitRewardInfos()
        {
            // 보스 보상
            var rewardsObj = gameObject.FindGameObject("Rewards");

            rewardsObj.AllChildObjectOff();

            var rewardDatas = DataUtil.GetRaidRewardDatas(mRaidBossType);

            var maxRaidReward = rewardDatas.Last();
            var rewardData = Lance.GameData.RewardData.TryGet(maxRaidReward.reward);
            if (rewardData != null)
            {
                var itemInfos = ItemInfoUtil.CreateItemInfos(rewardData);
                if (itemInfos != null)
                {
                    for(int i = 0; i < itemInfos.Count; ++i)
                    {
                        var itemInfo = itemInfos[i];

                        itemInfo = itemInfo.SetShowStr($"0 ~ {itemInfo.Amount}");

                        var itemSlotUIObj = Lance.ObjectPool.AcquireUI("DungeonRewardSlotUI", rewardsObj.GetComponent<RectTransform>());

                        var itemSlotUI = itemSlotUIObj.GetOrAddComponent<ItemSlotUI>();

                        itemSlotUI.Init(itemInfo);
                    }
                    
                }
            }

            // 보스 보상 정보 버튼
            var buttonRewardInfo = gameObject.FindComponent<Button>("Button_RewardInfo");
            buttonRewardInfo.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_RaidDungeonRewardInfoUI>();
                popup.Init(mRaidBossType);
            });
        }
        void OnRankButton()
        {
            var popup = Lance.PopupManager.CreatePopup<Popup_RankingUI>();

            // 나의 최고 스코어를 확인해서 어떤 랭킹을 바로 보여줄지
            popup.Init(Lance.Account.Dungeon.CanUpdateRankScore() ? RankingTab.AdvancedRaid : RankingTab.BeginnerRaid);

            popup.SetOnCloseAction(() => Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.ConfirmRaidRank));
        }

        public void Refresh()
        {
            bool canSweep = Lance.Account.GetRaidBossBestDamage(mRaidBossType) > 0;
            bool isEnoughSweepTicket = Lance.Account.IsEnoughDungeonTicket(mData.type, SweepRequireTicket);
            bool isEnoughTicket = Lance.Account.IsEnoughDungeonTicket(mData.type, EntranceRequireTicket);
            bool isEnoughWatchAdCount = Lance.Account.IsEnoughDungeonWatchAdCount(mData.type);

            mButtonSweep.SetActiveFrame(canSweep && isEnoughSweepTicket);
            mButtonEntranceByAd.gameObject.SetActive(isEnoughTicket == false && isEnoughWatchAdCount);
            mButtonEntranceByTicket.gameObject.SetActive(isEnoughTicket || isEnoughWatchAdCount == false);
            mButtonEntranceByTicket.SetActiveFrame(isEnoughTicket);
            mTicketRedDotObj.SetActive(isEnoughTicket);

            mTextTicketCount.text = $"{Lance.Account.GetDungeonTicket(mData.type)} / {EntranceRequireTicket}";
            mTextAdRemainCount.text = $"{Lance.Account.GetDungeonRemainWatchAdCount(mData.type)} / {mData.dailyWatchAdCount}";
        }
    }
}