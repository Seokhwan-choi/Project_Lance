using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using BackEnd;
using System;

namespace Lance
{
    public class Achievement : AccountBase
    {
        Dictionary<ObscuredString, AchievementInst> mAchievements = new();

        protected override void InitializeData()
        {
            foreach(var data in Lance.GameData.AchievementData.Values)
            {
                bool isDefault = data.type == AchievementType.Default;
                var achievementInst = new AchievementInst();
                achievementInst.Init(data.id, 0, 
                    complete: isDefault,
                    received: isDefault,
                    savedIsEquipped:isDefault);

                mAchievements.Add(data.id, achievementInst);
            }
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            var achievementsData = gameDataJson["Achievements"];

            for (int i = 0; i < achievementsData.Count; i++)
            {
                var achievementJsonData = achievementsData[i];

                string id = achievementJsonData["Id"].ToString();

                var data = Lance.GameData.AchievementData.TryGet(id);
                if (data != null)
                {
                    int stackedQuestCountTemp = 0;
                    bool completeTemp = false;
                    bool receivedTemp = false;
                    bool isEquippedTemp = false;

                    int.TryParse(achievementJsonData["StackedQuestCount"].ToString(), out stackedQuestCountTemp);
                    bool.TryParse(achievementJsonData["Complete"].ToString(), out completeTemp);
                    bool.TryParse(achievementJsonData["Received"].ToString(), out receivedTemp);
                    bool.TryParse(achievementJsonData["SavedIsEquipped"].ToString(), out isEquippedTemp);

                    var achievementInst = new AchievementInst();
                    achievementInst.Init(id, stackedQuestCountTemp, completeTemp, receivedTemp, isEquippedTemp);

                    mAchievements.Add(id, achievementInst);
                }
            }

            foreach(var data in Lance.GameData.AchievementData.Values)
            {
                if (mAchievements.ContainsKey(data.id) == false)
                {
                    bool isDefault = data.type == AchievementType.Default;
                    var achievementInst = new AchievementInst();
                    achievementInst.Init(data.id, 0,
                        complete: isDefault,
                        received: isDefault,
                        savedIsEquipped: isDefault);

                    mAchievements.Add(data.id, achievementInst);
                }
            }
        }

        public override string GetTableName()
        {
            return "Achievement";
        }

        public override Param GetParam()
        {
            Dictionary<string, AchievementInst> saveAchievemets = new Dictionary<string, AchievementInst>();

            foreach (var item in mAchievements)
            {
                item.Value.ReadyToSave();

                saveAchievemets.Add(item.Key, item.Value);
            }

            Param param = new Param();

            param.Add("Achievements", saveAchievemets);

            return param;
        }

        public double GatherStatValues(StatType statType)
        {
            double totalValue = 0;

            foreach (var achievement in mAchievements.Values)
            {
                if (achievement.IsComplete() && achievement.IsReceived())
                {
                    var data = Lance.GameData.AchievementData.TryGet(achievement.GetId());
                    if (data != null && data.rewardStat.IsValid())
                    {
                        var statData = Lance.GameData.AchievementStatData.TryGet(data.rewardStat);
                        if (statData != null && statData.valueType == statType)
                            totalValue += statData.statValue;
                    }
                }
            }

            return totalValue;
        }

        public bool EquipAchievement(string id)
        {
            var achievementInst = mAchievements.TryGet(id);
            if (achievementInst == null)
                return false;

            if (achievementInst.GetIsEquipped())
                return false;

            // 이전에 착용중이던 업적은 해제 후
            AchievementInst prevEquippedAchievement = GetEquippedAchievementInst();
            prevEquippedAchievement?.SetIsEquipped(false);

            // 새롭게 장착
            achievementInst.SetIsEquipped(true);

            SetIsChangedData(true);

            return true;
        }

        public bool CanEquip(string id)
        {
            var achievementInst = mAchievements.TryGet(id);
            if (achievementInst == null)
                return false;

            return 
                achievementInst.GetIsEquipped() == false &&
                achievementInst.IsComplete() &&
                achievementInst.IsReceived();
        }

        public bool AnyCanReceive()
        {
            foreach(var achievement in mAchievements.Values)
            {
                if (achievement.IsComplete() && achievement.IsReceived() == false)
                    return true;
            }

            return false;
        }

        public bool CanReceive(string id)
        {
            var achievementInst = mAchievements.TryGet(id);
            if (achievementInst == null)
                return false;

            return 
                achievementInst.IsComplete() && 
                achievementInst.IsReceived() == false;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            if (mAchievements != null && mAchievements.Count > 0)
            {
                foreach (var item in mAchievements)
                {
                    item.Key.RandomizeCryptoKey();
                    item.Value.RandomizeKey();
                }
            }
        }

        public int GetStackedQuestCount(string id)
        {
            var achievementInst = mAchievements.TryGet(id);

            return achievementInst?.GetStackedQuestCount() ?? 0;
        }

        public bool IsEquippedAchievement(string id)
        {
            var achievementInst = mAchievements.TryGet(id);

            return achievementInst?.GetIsEquipped() ?? false;
        }

        public bool IsReceivedAchievement(string id)
        {
            var achievementInst = mAchievements.TryGet(id);

            return achievementInst?.IsReceived() ?? false;
        }

        public bool IsCompleteAchievement(string id)
        {
            var achievementInst = mAchievements.TryGet(id);

            return achievementInst?.IsComplete() ?? false;
        }

        public void CheckQuest(QuestType questType, int questTypeValue)
        {
            bool any = false;

            foreach(var inst in mAchievements.Values)
            {
                any = inst.StackQuestCount(questType, questTypeValue);
            }

            if (any)
                SetIsChangedData(true);
        }

        public void CompleteAchievement(string id)
        {
            var inst = mAchievements.TryGet(id);

            inst?.SetComplete(true);

            SetIsChangedData(true);
        }

        public bool ReceiveAchievement(string id)
        {
            var inst = mAchievements.TryGet(id);
            if (inst == null)
                return false;

            inst.Receive();

            SetIsChangedData(true);

            return true;
        }

        public AchievementInst GetEquippedAchievementInst()
        {
            foreach (var achievement in mAchievements.Values)
            {
                if (achievement.GetIsEquipped())
                    return achievement;
            }

            return null;
        }

        public string GetEquippedAchievement()
        {
            foreach(var achievement in mAchievements.Values)
            {
                if (achievement.GetIsEquipped())
                    return achievement.GetId();
            }

            return string.Empty;
        }
    }

    public class AchievementInst
    {
        public string Id;
        public int StackedQuestCount;
        public bool Complete;
        public bool Received;
        public bool SavedIsEquipped;

        ObscuredString mId;
        ObscuredInt mStackedQuestCount;
        ObscuredBool mComplete;
        ObscuredBool mReceived;
        ObscuredBool mSavedIsEquipped;

        AchievementData mData;
        QuestData mQuestData;
        public void Init(string id, int stackedQuestCount, bool complete, bool received, bool savedIsEquipped)
        {
            mId = id;
            mStackedQuestCount = stackedQuestCount;
            mComplete = complete;
            mReceived = received;
            mSavedIsEquipped = savedIsEquipped;

            mData = Lance.GameData.AchievementData.TryGet(id);
            mQuestData = mData.quest.IsValid() ? DataUtil.GetQuestData(mData.quest) : null;
        }

        public bool StackQuestCount(QuestType questType, int questTypeValue)
        { 
            if (mQuestData != null)
            {
                if (mQuestData.type == questType)
                {
                    if (mQuestData.checkType == QuestCheckType.Stack)
                    {
                        StackQuestCount(questTypeValue);

                        return true;
                    }
                    else
                    {
                        AttainRequireCount(questTypeValue);

                        return true;
                    }
                }   
            }

            return false;
        }

        public void StackQuestCount(int count = 1)
        {
            mStackedQuestCount += count;
            mStackedQuestCount = Math.Min(mStackedQuestCount, GetMaxRequireCount());

            if (IsSatisfiedQuest())
            {
                mComplete = true;
            }
        }

        public void AttainRequireCount(int count)
        {
            mStackedQuestCount = count;
            mStackedQuestCount = Math.Min(mStackedQuestCount, GetMaxRequireCount());

            if (IsSatisfiedQuest())
            {
                mComplete = true;
            }
        }

        public bool IsSatisfiedQuest()
        {
            return mStackedQuestCount >= GetMaxRequireCount();
        }

        public int GetMaxRequireCount()
        {
            return mQuestData?.requireCount ?? int.MaxValue;
        }

        public void SetComplete(bool complete)
        {
            mComplete = complete;
        }

        public void Receive()
        {
            mReceived = true;
        }

        public string GetId()
        {
            return mId;
        }

        public bool IsComplete()
        {
            return mComplete;
        }

        public bool IsReceived()
        {
            return mReceived;
        }

        public void SetIsEquipped(bool isEquipped)
        {
            mSavedIsEquipped = isEquipped;
        }

        public bool GetIsEquipped()
        {
            return mSavedIsEquipped;
        }

        public int GetStackedQuestCount()
        {
            return mStackedQuestCount;
        }

        public void ReadyToSave()
        {
            Id = mId;
            StackedQuestCount = mStackedQuestCount;
            Complete = mComplete;
            Received = mReceived;
            SavedIsEquipped = mSavedIsEquipped;
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mStackedQuestCount.RandomizeCryptoKey();
            mComplete.RandomizeCryptoKey();
            mReceived.RandomizeCryptoKey();
            mSavedIsEquipped.RandomizeCryptoKey();
        }
    }
}