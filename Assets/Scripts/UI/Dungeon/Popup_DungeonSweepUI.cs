using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;

namespace Lance
{
    class Popup_DungeonSweepUI : PopupBase
    {
        Popup_DungeonUI mParent;
        StageType mType;
        int mSweepStep;
        int mSweepCount;
        TextMeshProUGUI mTextTicketCount;
        TextMeshProUGUI mTextSweepCount;
        public void Init(Popup_DungeonUI parent, StageType type, int sweepStep)
        {
            mParent = parent;

            SetUpCloseAction();

            // ������ �ܰ�
            StringParam param = new StringParam("step", sweepStep);

            SetTitleText($"{StringTableUtil.Get("UIString_Step", param)} {StringTableUtil.Get($"Title_{type}DungeonSweep")}");

            mType = type;
            mSweepStep = sweepStep;
            
            // ���� Ƚ�� ��ư
            var buttonMinus = gameObject.FindComponent<Button>("Button_Minus");
            buttonMinus.SetButtonAction(() => OnChangeSweepCount(minus:true));
            var buttonPlus = gameObject.FindComponent<Button>("Button_Plus");
            buttonPlus.SetButtonAction(() => OnChangeSweepCount(minus: false));

            var buttonMin = gameObject.FindComponent<Button>("Button_Min");
            buttonMin.SetButtonAction(() => SetSweepCount(1));

            var buttonMax = gameObject.FindComponent<Button>("Button_Max");
            buttonMax.SetButtonAction(() => SetSweepCount(Lance.Account.GetDungeonTicket(mType)));
            
            // ���� ���� ��ư
            var buttonSweep = gameObject.FindComponent<Button>("Button_Sweep");
            buttonSweep.SetButtonAction(OnSweepButton);

            // ���� Ƚ��
            mSweepCount = 1;
            mTextSweepCount = gameObject.FindComponent<TextMeshProUGUI>("Text_SweepCount");
            mTextSweepCount.text = $"{mSweepCount}";

            // Ƽ�� �̹���
            var imageTicket = gameObject.FindComponent<Image>("Image_Ticket");
            imageTicket.sprite = Lance.Atlas.GetTicket(mType);

            // ���� ���� Ƽ��
            mTextTicketCount = gameObject.FindComponent<TextMeshProUGUI>("Text_TicketCount");

            Refresh();
        }

        void OnChangeSweepCount(bool minus)
        {
            mSweepCount = mSweepCount + (minus ? -1 : 1);

            SetSweepCount(mSweepCount);
        }

        void SetSweepCount(int sweepCount)
        {
            mSweepCount = Mathf.Clamp(sweepCount, 1, Lance.Account.GetDungeonTicket(mType));

            mTextSweepCount.text = $"{mSweepCount}";
        }

        void Refresh()
        {
            int myTicket = Lance.Account.GetDungeonTicket(mType);
            int sweepRequireTicket = Lance.GameData.DungeonCommonData.sweepRequireTicket;
            mTextTicketCount.text = $"{myTicket}/{sweepRequireTicket}";
        }

        void OnSweepButton()
        {
            int bestStep = Lance.Account.GetBestDungeonStep(mType);
            if (bestStep == 1)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotReachedDungeonSelectedStep");

                return;
            }

            if (mType.HaveNextStepDungeon())
            {
                if (mSweepStep >= bestStep)
                {
                    UIUtil.ShowSystemErrorMessage("HaveNotReachedDungeonSelectedStep");

                    return;
                }
            }
            else
            {
                if (mSweepStep > bestStep)
                {
                    UIUtil.ShowSystemErrorMessage("HaveNotReachedDungeonSelectedStep");

                    return;
                }
            }

            if (Lance.Account.IsEnoughDungeonTicket(mType, mSweepCount) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughDungeonSweepTicket");

                return;
            }

            StageData stageData = mType.HaveNextStepDungeon() ? 
                DataUtil.GetDungeonStageData(mType, mSweepStep) :
                DataUtil.GetDungeonStageData(mType, 1);
            if (stageData == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (mType == StageType.Ancient)
            {
                // ���� �ѷ��� ���ϰ�
                RewardResult reward = new RewardResult();

                for (int i = 0; i < mSweepCount; ++i)
                {
                    for (int j = 1; j <= mSweepStep; ++j)
                    {
                        stageData = DataUtil.GetDungeonStageData(StageType.Ancient, j);
                        if (stageData != null)
                        {
                            MonsterRewardData monsterReward = DataUtil.GetMonsterRewardData(mType, stageData.bossDropReward);
                            if (monsterReward != null)
                            {
                                reward.ancientEssence += monsterReward.ancientEssence;
                            }
                        }
                    }
                }

                if (reward.ancientEssence > 0)
                {
                    if (Lance.Account.UseDungeonTicket(mType, mSweepCount))
                    {
                        // �˾� �ݱ�
                        Close();

                        // �����Ѵ�.
                        Lance.GameManager.GiveReward(reward, ShowRewardType.Popup);

                        Lance.Lobby.RefreshTabRedDot(LobbyTab.Adventure);

                        mParent.Refresh();

                        mParent.Parent.Refresh();

                        Param param = new Param();
                        param.Add("type", $"{stageData.type}");
                        param.Add("useTicket", mSweepCount);
                        param.Add("remainTicket", Lance.Account.Currency.GetDungeonTicket(stageData.type));
                        param.Add("reward", reward);
                        param.Add("nowDateNum", TimeUtil.NowDateNum());

                        Lance.BackEnd.InsertLog("SweepDungeon", param, 7);
                    }
                }
            }
            else if (mType == StageType.Pet)
            {
                // ���� �ѷ��� ���ϰ�
                RewardResult reward = new RewardResult();

                for (int i = 0; i < mSweepCount; ++i)
                {
                    for (int j = 1; j <= mSweepStep; ++j)
                    {
                        stageData = DataUtil.GetDungeonStageData(StageType.Pet, j);
                        if (stageData != null)
                        {
                            MonsterRewardData monsterRewardData = DataUtil.GetMonsterRewardData(stageData.type, stageData.bossDropReward);

                            var monsterReward = DataUtil.GetMonsterReward(stageData.diff, stageData.type, stageData.chapter, stageData.stage, monsterRewardData);

                            reward = reward.AddReward(monsterReward);
                        }
                    }
                }

                if (reward.petFood > 0)
                {
                    if (Lance.Account.UseDungeonTicket(mType, mSweepCount))
                    {
                        // �˾� �ݱ�
                        Close();

                        // �����Ѵ�.
                        Lance.GameManager.GiveReward(reward, ShowRewardType.Popup);

                        Lance.Lobby.RefreshTabRedDot(LobbyTab.Adventure);

                        mParent.Refresh();

                        mParent.Parent.Refresh();

                        Param param = new Param();
                        param.Add("type", $"{stageData.type}");
                        param.Add("useTicket", mSweepCount);
                        param.Add("remainTicket", Lance.Account.Currency.GetDungeonTicket(stageData.type));
                        param.Add("reward", reward);
                        param.Add("nowDateNum", TimeUtil.NowDateNum());

                        Lance.BackEnd.InsertLog("SweepDungeon", param, 7);
                    }
                }
            }
            else
            {
                if (stageData.monsterDropReward.IsValid())
                {
                    MonsterRewardData monsterRewardData = DataUtil.GetMonsterRewardData(stageData.type, stageData.monsterDropReward);
                    if (monsterRewardData != null)
                    {
                        if (Lance.Account.UseDungeonTicket(mType, mSweepCount))
                        {
                            // �˾� �ݱ�
                            Close();

                            // ���� �ѷ��� ���ϰ�
                            RewardResult reward = new RewardResult();

                            for (int i = 0; i < mSweepCount; ++i)
                            {
                                if (stageData.atOnceSpawn)
                                {
                                    for (int j = 0; j < mSweepStep; ++j)
                                    {
                                        var monsterReward = DataUtil.GetMonsterReward(stageData.diff, stageData.type, stageData.chapter, stageData.stage, monsterRewardData);

                                        reward = reward.AddReward(monsterReward);
                                    }
                                }
                                else
                                {
                                    for (int j = 0; j < stageData.monsterLimitCount; ++j)
                                    {
                                        var monsterReward = DataUtil.GetMonsterReward(stageData.diff, stageData.type, stageData.chapter, stageData.stage, monsterRewardData);

                                        reward = reward.AddReward(monsterReward);
                                    }
                                }
                            }

                            reward.exp = reward.exp * (1 + Lance.Account.GatherStat(StatType.ExpAmount));
                            reward.gold = reward.gold * (1 + Lance.Account.GatherStat(StatType.GoldAmount));

                            // �����Ѵ�.
                            Lance.GameManager.GiveReward(reward, ShowRewardType.Popup);

                            Lance.Lobby.RefreshTabRedDot(LobbyTab.Adventure);

                            mParent.Refresh();

                            mParent.Parent.Refresh();

                            Param param = new Param();
                            param.Add("type", $"{stageData.type}");
                            param.Add("useTicket", mSweepCount);
                            param.Add("remainTicket", Lance.Account.Currency.GetDungeonTicket(stageData.type));
                            param.Add("reward", reward);
                            param.Add("nowDateNum", TimeUtil.NowDateNum());

                            Lance.BackEnd.InsertLog("SweepDungeon", param, 7);
                        }
                    }
                }
            }
        }
    }
}