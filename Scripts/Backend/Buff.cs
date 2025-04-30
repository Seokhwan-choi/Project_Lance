using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using System.Linq;
using BackEnd;
using System;

namespace Lance
{
    public class Buff : AccountBase
    {
        Dictionary<string, BuffInfo> mBuffInfos;
        public Buff()
        {
            mBuffInfos = new Dictionary<string, BuffInfo>();
        }

        public void OnUpdate(float time, bool purchasedRemovedAD)
        {
            if (purchasedRemovedAD)
                return;

            foreach(var info in mBuffInfos.Values)
            {
                if (info.OnUpdate(time))
                {
                    SetIsChangedData(true);
                }
            }
        }

        public double GatherStatValues(StatType type)
        {
            double totalValue = 0;

            foreach(var info in mBuffInfos.Values)
            {
                if (info.GetBuffType() == type && info.IsAlreadyActive())
                    totalValue += info.GetBuffValue();
            }

            return totalValue;
        }

        public double GetBuffValue(string id)
        {
            BuffInfo buffInfo = mBuffInfos.TryGet(id);
            if (buffInfo == null)
                return 0;

            return buffInfo.GetBuffValue();
        }

        public bool IsMaxLevel(string id)
        {
            BuffInfo buffInfo = mBuffInfos.TryGet(id);
            if (buffInfo == null)
                return false;

            return buffInfo.IsMaxLevel();
        }

        public int GetRequireMonsterKillCount(string id)
        {
            BuffInfo buffInfo = mBuffInfos.TryGet(id);
            if (buffInfo == null)
                return 0;

            return buffInfo.GetRequireMonsterKillCount();
        }

        public int GetMonsterKillCount(string id)
        {
            BuffInfo buffInfo = mBuffInfos.TryGet(id);
            if (buffInfo == null)
                return 0;

            return buffInfo.GetMonsterKillCount();
        }

        public int GetStackedActiveCount(string id)
        {
            BuffInfo buffInfo = mBuffInfos.TryGet(id);
            if (buffInfo == null)
                return int.MaxValue;

            return buffInfo.GetStackedActiveCount();
        }

        public bool CanActiveBuff(string id)
        {
            BuffInfo buffInfo = mBuffInfos.TryGet(id);
            if (buffInfo == null)
                return false;

            if (IsAlreadyActiveBuff(id))
                return false;

            return buffInfo.IsEnoughCount();
        }

        public bool CanLevelUpBuff(string id)
        {
            BuffInfo buffInfo = mBuffInfos.TryGet(id);
            if (buffInfo == null)
                return false;

            return buffInfo.CanLevelUp();
        }

        public void OnPurchasedRemoveAd()
        {
            foreach (var info in mBuffInfos.Values)
            {
                if (info.ActiveBuff())
                {
                    SetIsChangedData(true);
                }
            }
        }

        public bool ActiveBuff(string id)
        {
            BuffInfo buffInfo = mBuffInfos.TryGet(id);
            if (buffInfo == null)
                return false;

            if (buffInfo.CanActive() == false)
                return false;

            if (buffInfo.ActiveBuff())
            {
                SetIsChangedData(true);

                return true;
            }

            return false;
        }

        public bool LevelUpBuff(string id)
        {
            BuffInfo buffInfo = mBuffInfos.TryGet(id);
            if (buffInfo == null)
                return false;

            if (buffInfo.CanLevelUp() == false)
                return false;

            if (buffInfo.LevelUp())
            {
                SetIsChangedData(true);

                return true;
            }

            return false;
        }

        public void OnMonsterDeath(int count = 1)
        {
            foreach (var info in mBuffInfos.Values)
            {
                if (info.IsAlreadyActive())
                {
                    info.StackMonsterKillCount(count);

                    SetIsChangedData(true);
                }
            }
        }

        public int GetBuffRemainCount(string id)
        {
            var info = mBuffInfos.TryGet(id);

            return info?.GetRemainCount() ?? 0;
        }

        public bool IsAlreadyActiveBuff(string id)
        {
            var buffInfo = mBuffInfos.TryGet(id);
            if (buffInfo == null)
                return false;

            return buffInfo.IsAlreadyActive();
        }

        public bool IsEnoughActiveCount(string id)
        {
            var buffInfo = mBuffInfos.TryGet(id);
            if (buffInfo == null)
                return false;

            return buffInfo.IsEnoughCount();
        }

        public float GetBuffDurationTime(string id)
        {
            var buffInfo = mBuffInfos.TryGet(id);

            return buffInfo?.GetDurationTime() ?? 0;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach(var buffInfo in mBuffInfos.Values)
            {
                buffInfo.RandomizeKey();
            }
        }

        public override string GetColumnName()
        {
            return "Buff";
        }

        public override string GetTableName()
        {
            return "Buff";
        }

        public override Param GetParam()
        {
            Param param = new Param();

            foreach(var info in mBuffInfos.Values)
            {
                info.ReadyToSave();
            }

            param.Add(GetColumnName(), mBuffInfos);

            return param;
        }

        protected override void InitializeData()
        {
            foreach(var data in Lance.GameData.BuffData.Values)
            {
                if (mBuffInfos.ContainsKey(data.id) == false)
                {
                    var info = new BuffInfo();

                    info.Init(data.id, 0, 0, 0, 0, 1);

                    mBuffInfos.Add(data.id, info);
                }
            }
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            for(int i = 0; i < gameDataJson.Count; ++i)
            {
                JsonData buffJsonData = gameDataJson[i];

                string id = buffJsonData["Id"].ToString();

                var buffData = Lance.GameData.BuffData.TryGet(id);
                if (buffData == null)
                    continue;

                int stackedActiveCountTemp = 0;

                int.TryParse(buffJsonData["StackedActiveCount"].ToString(), out stackedActiveCountTemp);

                float durationTimeTemp = 0;

                float.TryParse(buffJsonData["DurationTime"].ToString(), out durationTimeTemp);

                int monsterKillCountTemp = 0;

                if (buffJsonData.ContainsKey("MonsterKillCount"))
                {
                    int.TryParse(buffJsonData["MonsterKillCount"].ToString(), out monsterKillCountTemp);
                }

                int stackedMonsterKillCountTemp = 0;

                if (buffJsonData.ContainsKey("StackedMonsterKillCount"))
                {
                    int.TryParse(buffJsonData["StackedMonsterKillCount"].ToString(), out stackedMonsterKillCountTemp);
                }

                int levelTemp = 1;

                if (buffJsonData.ContainsKey("Level"))
                {
                    int.TryParse(buffJsonData["Level"].ToString(), out levelTemp);
                }

                var buffInfo = new BuffInfo();

                buffInfo.Init(id, stackedActiveCountTemp, durationTimeTemp, monsterKillCountTemp, stackedMonsterKillCountTemp, levelTemp);

                mBuffInfos.Add(id, buffInfo);
            }
        }

        public bool AnyCanActive()
        {
            foreach (var buff in mBuffInfos.Values)
            {
                if (buff.CanActive())
                    return true;
            }

            return false;
        }

        public bool AnyCanLevelUp()
        {
            foreach (var buff in mBuffInfos.Values)
            {
                if (buff.CanLevelUp())
                    return true;
            }

            return false;
        }

        public bool AnyIsAlreadyActive()
        {
            foreach(BuffInfo buff in mBuffInfos.Values)
            {
                if (buff.IsAlreadyActive())
                    return true;
            }

            return false;
        }

        public int GetLevel(string id)
        {
            BuffInfo buffInfo = mBuffInfos.TryGet(id);
            if (buffInfo == null)
                return 0;

            return buffInfo.GetLevel();
        }

#if UNITY_EDITOR
        public void DeBuff()
        {
            foreach (BuffInfo buff in mBuffInfos.Values)
            {
                buff.InActiveBuff();
            }

            SetIsChangedData(true);
        }
#endif
    }

    public class BuffInfo
    {
        public string Id;
        public int StackedActiveCount;
        public float DurationTime;
        public int MonsterKillCount;
        public int StackedMonsterKillCount;
        public int Level;
        //public DailyCounter WatchAdDailyCounter;
        
        ObscuredString mId;
        ObscuredInt mStackedActiveCount;
        ObscuredFloat mDurationTime;   // 버프가 지속시간 종료시간
        ObscuredInt mMonsterKillCount;
        ObscuredInt mStackedMonsterKillCount;
        ObscuredInt mLevel;
        //DailyCounter mWatchAdDailyCounter;
        BuffData mData;
        public void Init(string id, int stackedActiveCount, float duration, int monsterKillCount, int stackedMonsterKillCount, int level)
        {
            mId = id;
            mStackedActiveCount = stackedActiveCount;
            mDurationTime = duration;
            //mWatchAdDailyCounter = dailyCounter;

            mMonsterKillCount = monsterKillCount;
            mStackedMonsterKillCount = stackedMonsterKillCount;
            mLevel = level;
            mData = Lance.GameData.BuffData.TryGet(mId);
        }

        public bool IsAlreadyActive()
        {
            return mDurationTime > 0f;
        }

        public StatType GetBuffType()
        {
            return mData?.type ?? StatType.Atk;
        }

        public double GetBuffValue()
        {
            double level = mLevel - 1;

            double baseValue = mData?.buffValue ?? 0;
            double levelValue = mData.buffLevelUpValue * level;

            return baseValue + levelValue;
        }

        public void InActiveBuff()
        {
            mDurationTime = 0;
        }

        public float GetDurationTime()
        {
            return mDurationTime;
        }

        public bool OnUpdate(float time)
        {
            if (mDurationTime > 0)
            {
                mDurationTime -= time;

                // 버프가 종료되었다는 것
                if (mDurationTime <= 0f)
                {
                    mDurationTime = 0f;

                    Lance.GameManager.UpdatePlayerStat(mData?.type == StatType.AddDmg);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        public void RandomizeKey()
        {
            mId?.RandomizeCryptoKey();
            mStackedActiveCount.RandomizeCryptoKey();
            mDurationTime.RandomizeCryptoKey();

            mMonsterKillCount.RandomizeCryptoKey();
            mStackedMonsterKillCount.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
            //mWatchAdDailyCounter?.RandomizeKey();
        }

        public void ReadyToSave()
        {
            Id = mId;
            StackedActiveCount = mStackedActiveCount;
            DurationTime = mDurationTime;
            MonsterKillCount = mMonsterKillCount;
            StackedMonsterKillCount = mStackedMonsterKillCount;
            Level = mLevel;
            //mWatchAdDailyCounter.ReadyToSave();
            //WatchAdDailyCounter = mWatchAdDailyCounter;
        }

        public bool CanActive()
        {
            if (IsAlreadyActive())
                return false;

            if (IsEnoughCount() == false)
                return false;

            return true;
        }

        public bool IsEnoughCount()
        {
            if (mData == null)
                return false;

            if (mStackedActiveCount == 0)
                return true;

            return true;
        }

        public int GetRemainCount()
        {
            if (mData == null)
                return 0;

            return 0;//mWatchAdDailyCounter.GetRemainCount(Data.dailyCount);
        }

        public bool ActiveBuff()
        {
            if (mData == null)
                return false;

            if (CanActive() == false)
                return false;

            mDurationTime = mData.duration;

            // 최초는 바로 적용할 수 있음
            // 활성화 횟수가 0이면 최초 활성화임
            //if (mStackedActiveCount != 0)
               // mWatchAdDailyCounter.StackCount(Data.dailyCount);

            mStackedActiveCount++;

            return true;
        }

        public int GetStackedActiveCount()
        {
            return mStackedActiveCount;
        }

        public bool IsMaxLevel()
        {
            return mData.maxLevel <= mLevel;
        }

        public int GetRequireMonsterKillCount()
        {
            int nextLevel = mLevel + 1;

            var levelUpData = Lance.GameData.BuffLevelUpData.TryGet(nextLevel);

            return levelUpData?.requireMonsterKillCount ?? int.MaxValue;
        }

        public int GetMonsterKillCount()
        {
            return mMonsterKillCount;
        }

        public int GetLevel()
        {
            return mLevel;
        }

        public bool CanLevelUp()
        {
            return GetRequireMonsterKillCount() <= mMonsterKillCount;
        }

        public bool LevelUp()
        {
            int nextLevel = mLevel + 1;

            var levelUpData = Lance.GameData.BuffLevelUpData.TryGet(nextLevel);
            if (levelUpData == null)
                return false;

            if (levelUpData.requireMonsterKillCount > mMonsterKillCount)
                return false;

            mMonsterKillCount -= levelUpData.requireMonsterKillCount;
            mLevel += 1;

            return true;
        }

        public void StackMonsterKillCount(int count)
        {
            mMonsterKillCount += count;
            mStackedMonsterKillCount += count;
        }
    }
}