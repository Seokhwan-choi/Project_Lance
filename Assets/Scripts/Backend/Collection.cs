using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using BackEnd;
using LitJson;
using System.Linq;

namespace Lance
{
    public class Collection : AccountBase
    {
        Dictionary<ObscuredString, CollectionInst> mCompleteDics = new();
        public double GatherStatValues(StatType type)
        {
            double totalValue = 0;

            var completeInsts = mCompleteDics.Values.Where(x => x.GetRewardStatType() == type);

            if (completeInsts != null && completeInsts.Count() > 0)
            {
                foreach (var completeInst in completeInsts)
                {
                    totalValue += completeInst.GetRewardStatValue();
                }
            }

            return totalValue;
        }

        public void CompleteCollect(string id)
        {
            var completeInst = new CollectionInst();

            completeInst.Init(id);

            mCompleteDics.Add(id, completeInst);

            SetIsChangedData(true);
        }

        public IEnumerable<ObscuredString> GatherCollectionsByItemType(ItemType itemType)
        {
            foreach (var completeInst in mCompleteDics.Values)
            {
                if (completeInst.GetItemType() == itemType)
                    yield return completeInst.Id;
            }
        }

        public bool RemoveCollect(string id)
        {
            if (IsAlreadyCollect(id) == false)
                return false;

            mCompleteDics.Remove(id);

            SetIsChangedData(true);

            return true;
        }

        public bool IsAlreadyCollect(string id)
        {
            return mCompleteDics.ContainsKey(id);
        }

        public override string GetTableName()
        {
            return "Collection";
        }

        public override Param GetParam()
        {
            Param param = new Param();

            param.Add("CompleteList", mCompleteDics.Keys.Select(x => x.ToString()).ToArray());

            return param;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            if (gameDataJson.ContainsKey("CompleteList"))
            {
                foreach(var complete in gameDataJson["CompleteList"])
                {
                    string id = complete.ToString();

                    var data = Lance.GameData.CollectionData.TryGet(id);
                    if (data == null)
                        continue;

                    var completeInst = new CollectionInst();

                    completeInst.Init(id);

                    mCompleteDics.Add(id, completeInst);
                }
            }
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach(var complete in mCompleteDics)
            {
                complete.Key.RandomizeCryptoKey();
                complete.Value.RandomizeKey();
            }
        }
    }

    public class CollectionInst
    {
        ObscuredString mId;
        CollectionData mData;
        CollectionStatData mStatData;
        public string Id => mId;
        public void Init(string id)
        {
            mId = id;

            mData = Lance.GameData.CollectionData.TryGet(id);
            mStatData = Lance.GameData.CollectionStatData.TryGet(mData.rewardStat);
        }

        public void RandomizeKey()
        {
            mId?.RandomizeCryptoKey();
        }

        public ItemType GetItemType()
        {
            return mData.itemType;
        }

        public StatType GetRewardStatType()
        {
            return mStatData.valueType;
        }

        public double GetRewardStatValue()
        {
            return mStatData.statValue;
        }
    }
}