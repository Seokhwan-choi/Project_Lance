using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public partial class Account
    {
        public bool CanUpgradeManaHeart()
        {
            int manaHeartStep = ManaHeart.GetStep();
            var manaHeartStepData = Lance.GameData.ManaHeartStepData.TryGet(manaHeartStep);
            if (manaHeartStepData == null)
                return false;

            int upgradeStep = ManaHeart.GetUpgradeStep();
            if (manaHeartStepData.maxUpgradeStep > upgradeStep)
                return false;

            return manaHeartStepData.requireLevel <= ExpLevel.GetLevel();
        }

        public bool CanUpgradeManaHeartInst()
        {
            int manaHeartStep = ManaHeart.GetStep();
            var manaHeartStepData = Lance.GameData.ManaHeartStepData.TryGet(manaHeartStep);
            if (manaHeartStepData == null)
                return false;

            int upgradeStep = ManaHeart.GetUpgradeStep();
            var stepData = Lance.GameData.ManaHeartUpgradeStepData.TryGet(upgradeStep);
            if (stepData == null)
                return false;

            if (manaHeartStepData.maxUpgradeStep < upgradeStep)
                return false;

            int require = ManaHeart.GetUpgradeRequire(stepData.type);
            if (require <= 0)
                return false;

            return IsEnoughManaEssence(require);
        }

        public bool UpgradeManaHeart()
        {
            int manaHeartStep = ManaHeart.GetStep();
            if (DataUtil.GetManaHeartMaxStep() == manaHeartStep)
                return false;

            var manaHeartStepData = Lance.GameData.ManaHeartStepData.TryGet(manaHeartStep);
            if (manaHeartStepData == null)
                return false;

            int upgradeStep = ManaHeart.GetUpgradeStep();
            if (manaHeartStepData.maxUpgradeStep > upgradeStep)
                return false;

            if (manaHeartStepData.requireLevel > ExpLevel.GetLevel())
                return false;

            return ManaHeart.UpgradeManaHeart();
        }

        public bool UpgradeManaHeartInst()
        {
            int manaHeartStep = ManaHeart.GetStep();
            var manaHeartStepData = Lance.GameData.ManaHeartStepData.TryGet(manaHeartStep);
            if (manaHeartStepData == null)
                return false;

            int upgradeStep = ManaHeart.GetUpgradeStep();
            var upgradeStepData = Lance.GameData.ManaHeartUpgradeStepData.TryGet(upgradeStep);
            if (upgradeStepData == null)
                return false;

            if (manaHeartStepData.maxUpgradeStep < upgradeStep)
                return false;

            int require = ManaHeart.GetUpgradeRequire(upgradeStepData.type);
            if (require <= 0)
                return false;

            if (IsEnoughManaEssence(require))
            {
                if (UseManaEssence(require))
                {
                    return ManaHeart.UpgradeManaHeartInst();
                }
            }

            return false;
        }
    }
}