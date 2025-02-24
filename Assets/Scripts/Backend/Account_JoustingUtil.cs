using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public partial class Account
    {
        public void UpdateJoustingRankExtraInfo()
        {
            string bodyCostume = Costume.GetEquippedCostumeId(CostumeType.Body);
            string achievement = Achievement.GetEquippedAchievement();

            string extraInfo = $"{bodyCostume}\\split{achievement}";

            JoustRankInfo.SetExtraInfo(extraInfo);
        }

        public bool CanUpgradeJoustGloryOrb()
        {
            int step = JoustGloryOrb.GetStep();

            var stepData = Lance.GameData.JoustingGloryOrbStepData.TryGet(step);
            if (stepData == null)
                return false;

            int require = JoustGloryOrb.GetUpgradeRequire(stepData.type);
            if (require <= 0)
                return false;

            return IsEnoughJoustGloryToken(require);
        }

        public bool UpgradeJoustGloryOrb()
        {
            int step = JoustGloryOrb.GetStep();

            var stepData = Lance.GameData.JoustingGloryOrbStepData.TryGet(step);
            if (stepData == null)
                return false;

            int require = JoustGloryOrb.GetUpgradeRequire(stepData.type);
            if (require <= 0)
                return false;

            if (IsEnoughJoustGloryToken(require))
            {
                if (UseJoustGloryToken(require))
                {
                    return JoustGloryOrb.UpgradeJoustGloryOrb();
                }
            }

            return false;
        }
    }
}