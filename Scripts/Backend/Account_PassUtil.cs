using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    public partial class Account
    {
        public void UpdatePassRewardValues()
        {
            for (int i = 0; i < (int)PassType.Count; ++i)
            {
                PassType passType = (PassType)i;

                UpdatePassRewardValue(passType);
            }
        }

        public void UpdatePassRewardValue(PassType passType)
        {
            Lance.Account.Pass.UpdateRewardValues(passType, GetRewardValue(passType));
        }

        double GetRewardValue(PassType passType)
        {
            switch(passType)
            {
                case PassType.Level:
                    return ExpLevel.GetLevel();
                case PassType.PlayTime:
                    return UserInfo.GetPlayTime();
                case PassType.Spawn:
                    return Spawn.GetTotalStackedSpawnCount();
                case PassType.Stage:
                default:
                    return StageRecords.GetBestTotalStage();
            }
        }
    }
}