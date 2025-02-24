using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Lance
{
    public partial class Account
    {
        public bool CompleteCollect(string id)
        {
            if (CanCompleteCollect(id) == false)
                return false;

            Collection.CompleteCollect(id);

            return true;
        }

        public List<string> AllCompleteCollect(ItemType itemType)
        {
            List<string> result = new List<string>();
            foreach (var data in Lance.GameData.CollectionData.Values.Where(x => x.itemType == itemType))
            {
                if (CompleteCollect(data.id))
                    result.Add(data.id);
            }

            return result;
        }

        public bool AnyCompleteCollect()
        {
            foreach (var data in Lance.GameData.CollectionData.Values)
            {
                if (CanCompleteCollect(data.id))
                    return true;
            }

            return false;
        }

        public bool AnyCompleteCollect(ItemType itemType)
        {
            foreach(var data in Lance.GameData.CollectionData.Values.Where(x => x.itemType == itemType))
            {
                if (CanCompleteCollect(data.id))
                    return true;
            }

            return false;
        }

        public bool CanCompleteCollect(string id)
        {
            return Collection.IsAlreadyCollect(id) == false && IsSatisfiedCollection(id);
        }

        public bool IsSatisfiedCollection(string id)
        {
            var data = Lance.GameData.CollectionData.TryGet(id);
            if (data == null)
                return false;

            // 해당 아이템의 조건이 충분한지 확인한다.
            for (int i = 0; i < data.requireId.Length; ++i)
            {
                string requireId = data.requireId[i];
                int requireLevel = data.requireLevel[i];

                if (requireId.IsValid())
                {
                    if (data.itemType.IsEquipment())
                    {
                        var result = IsSatisfiedCollectionEquipment(requireId, requireLevel);
                        if (result == false)
                            return false;
                    }
                    else if (data.itemType.IsSkill())
                    {
                        var result = IsSatisfiedCollectionSkill(requireId, requireLevel);
                        if (result == false)
                            return false;
                    }
                    else if (data.itemType.IsAccessory())
                    {
                        var result = IsSatisfiedCollectionAccessory(requireId, requireLevel);
                        if (result == false)
                            return false;
                    }
                }
            }

            return true;
        }

        public bool IsSatisfiedCollectionEquipment(string requireId, int requireLevel)
        {
            var equipmentInst = GetEquipment(requireId);
            if (equipmentInst == null)
                return false;

            return equipmentInst.GetLevel() >= requireLevel;
        }

        public bool IsSatisfiedCollectionAccessory(string requireId, int requireLevel)
        {
            var accessoryInst = GetAccessory(requireId);
            if (accessoryInst == null)
                return false;

            return accessoryInst.GetLevel() >= requireLevel;
        }

        public bool IsSatisfiedCollectionSkill(string requireId, int requireLevel)
        {
            var skillData = DataUtil.GetSkillData(requireId);

            return GetSkillLevel(skillData.type, requireId) >= requireLevel;
        }

        public bool DeleteDissatisfyingCollection(ItemType itemType)
        {
            var ids = Lance.Account.Collection.GatherCollectionsByItemType(itemType);
            var removeList = new List<string>();

            foreach(var id in ids)
            {
                if (IsSatisfiedCollection(id) == false)
                    removeList.Add(id);
            }

            if (removeList.Count > 0)
            {
                foreach (var remove in removeList)
                {
                    Lance.Account.Collection.RemoveCollect(remove);
                }

                Lance.Account.Collection.SetIsChangedData(true);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
