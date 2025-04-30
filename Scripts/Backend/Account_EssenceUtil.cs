using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    public partial class Account
    {
        public bool AnyCanUpgradeEssence()
        {
            foreach(EssenceInst essence in Essence.GetEssences())
            {
                if (CanUpgradeEssence(essence.GetEssenceType()))
                    return true;
            }

            return false;
        }

        public bool CanUpgradeEssence(EssenceType essenceType)
        {
            int step = Essence.GetStep(essenceType);

            if (essenceType == EssenceType.Central)
            {
                bool isMaxStep = Essence.IsMaxStep(essenceType);
                if (isMaxStep)
                    return false;

                CentralEssenceStepData essenceStepData = Lance.GameData.CentralEssenceStepData.TryGet(step);
                if (essenceStepData == null)
                    return false;

                if (essenceStepData.requireAllEssenceLevel > 0)
                {
                    for(int i = 0; i < (int)EssenceType.Count; ++i)
                    {
                        var type = (EssenceType)i;

                        var essence = Essence.GetEssence(type);

                        int level = essence?.GetLevel() ?? 0;

                        if (level < essenceStepData.requireAllEssenceLevel)
                            return false;
                    }
                }

                return true;
            }
            else
            {
                bool isMaxLevel = Essence.IsMaxLevel(essenceType);
                if (isMaxLevel)
                    return false;

                // 재화가 충분한가?
                int myEssenceElement = Lance.Account.Currency.GetEssence(essenceType);
                int requireEssenceElement = Lance.Account.Essence.GetUpgradeRequireElements(essenceType);
                bool isEnough = myEssenceElement >= requireEssenceElement;
                if (isEnough == false)
                {
                    return false;
                }

                EssenceStepData essenceStepData = DataUtil.GetEssenceStepData(essenceType, step);
                if (essenceStepData == null)
                    return false;

                if (essenceStepData.requireCentralStep > 0)
                {
                    var centralEssence = Essence.GetEssence(EssenceType.Central);
                    if (centralEssence == null)
                        return false;

                    return essenceStepData.requireCentralStep <= centralEssence.GetStep();
                }

                return true;
            }
        }

        public bool UpgradeEssence(EssenceType essenceType)
        {
            int step = Essence.GetStep(essenceType);

            if (essenceType == EssenceType.Central)
            {
                bool isMaxStep = Essence.IsMaxStep(essenceType);
                if (isMaxStep)
                    return false;

                CentralEssenceStepData essenceStepData = Lance.GameData.CentralEssenceStepData.TryGet(step);
                if (essenceStepData == null)
                    return false;

                if (essenceStepData.requireAllEssenceLevel > 0)
                {
                    for (int i = 0; i < (int)EssenceType.Count; ++i)
                    {
                        var type = (EssenceType)i;

                        var essence = Essence.GetEssence(type);

                        int level = essence?.GetLevel() ?? 0;

                        if (level < essenceStepData.requireAllEssenceLevel)
                            return false;
                    }
                }

                return Essence.UpgradeEssence(essenceType);
            }
            else
            {
                bool isMaxLevel = Essence.IsMaxLevel(essenceType);
                if (isMaxLevel)
                    return false;

                // 재화가 충분한가?
                int myEssenceElement = Currency.GetEssence(essenceType);
                int requireEssenceElement = Essence.GetUpgradeRequireElements(essenceType);
                bool isEnough = myEssenceElement >= requireEssenceElement;
                if (isEnough == false)
                    return false;

                EssenceStepData essenceStepData = DataUtil.GetEssenceStepData(essenceType, step);
                if (essenceStepData == null)
                    return false;

                if (essenceStepData.requireCentralStep > 0)
                {
                    var centralEssence = Essence.GetEssence(EssenceType.Central);
                    if (centralEssence == null)
                        return false;

                    if (essenceStepData.requireCentralStep > centralEssence.GetStep())
                        return false;
                }

                if (Currency.UseEssence(essenceType, requireEssenceElement))
                {
                    return Essence.UpgradeEssence(essenceType);
                }

                return false;
            }
        }

        public bool CanActiveCentralEssence()
        {
            var essence = Lance.Account.Essence.GetEssence(EssenceType.Central);
            if (essence == null)
                return false;

            // 이미 활성화 중인가?
            if (Lance.Account.Essence.IsActiveCentralEssence())
                return false;

            // 태초의 정수 단계가 충분한가?
            int step = essence.GetStep();
            var data = Lance.GameData.CentralEssenceActiveRequireData.TryGet(step);
            if (data == null || data.requireAllEssenceAmount <= 0)
                return false;

            // 정수 조각이 충분한가?
            for (int i = 0; i < (int)EssenceType.Count; ++i)
            {
                EssenceType type = (EssenceType)i;

                if (Lance.Account.Currency.IsEnoughEssence(type, data.requireAllEssenceAmount) == false)
                    return false;
            }

            // 활성화 횟수가 충분한가?
            int remainCount = Lance.Account.Essence.GetRemainActiveCount();
            if (remainCount <= 0)
                return false;

            return true;
        }
    }
}