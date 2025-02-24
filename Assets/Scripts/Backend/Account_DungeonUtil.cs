using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public partial class Account
    {
        public int GetBestDungeonStep(StageType type)
        {
            return Dungeon.GetBestStep(type);
        }

        public int GetBestDemonicRealmStep(StageType type)
        {
            return DemonicRealm.GetBestStep(type);
        }

        public bool CanChangeStep(StageType type, int step)
        {
            return Dungeon.CanChangeStep(type, step);
        }

        public int GetDungeonRemainWatchAdCount(StageType type)
        {
            return Dungeon.GetRemainWatchAdCount(type);
        }

        public int GetDemonicRealmRemainWatchAdCount(StageType type)
        {
            return DemonicRealm.GetRemainWatchAdCount(type);
        }

        public bool IsEnoughDungeonWatchAdCount(StageType type)
        {
            return Dungeon.IsEnoughWatchAdCount(type);
        }

        public bool IsEnoughDemonicRealmWatchAdCount(StageType type)
        {
            return DemonicRealm.IsEnoughWatchAdCount(type);
        }

        public void UpdateRaidBossDamage(ElementalType type, double damage)
        {
            Dungeon.UpdateRaidBossDamage(type, damage);

            if (Dungeon.CanUpdateRankScore() == false)
            {
                NewBeginnerRaidScore.UpdateRaidBossDamage(type, damage);
            }
        }

        public double GetRaidBossBestDamage(ElementalType type)
        {
            if (Dungeon.CanUpdateRankScore())
            {
                return Dungeon.GetRaidBossBestDamage(type);
            }
            else
            {
                return NewBeginnerRaidScore.GetRaidBossBestDamage(type);
            }
        }

        public bool CanEntranceDungeon(StageType type)
        {
            // 티켓이나 광고 횟수가 충분한지 확인
            return Currency.IsEnoughDungeonTicket(type, 1) ||
                Dungeon.IsEnoughWatchAdCount(type);
        }
    }
}