using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    static class WeekendDoubleUtil
    {
        public static bool IsDoubleEvent()
        {
            bool isWeekend = TimeUtil.IsWeekend();
            bool isBeginnerSupport = Lance.Account.ExpLevel.GetLevel() < Lance.GameData.WeekendDoubleData.beginnerSupportLevel;
            bool isDoubleEvent = false;

            foreach (DoubleEventData data in Lance.GameData.DoubleEventData.Values)
            {
                if (data.startDate > 0 && data.endDate > 0)
                {
                    if (TimeUtil.IsActiveDateNum(data.startDate, data.endDate))
                    {
                        isDoubleEvent = true;
                        break;
                    }
                }
            }

            return isWeekend || isBeginnerSupport || isDoubleEvent;
        }
        public static float GetValue(ItemType itemType)
        {
            if (IsDoubleEvent() == false)
                return 1f;

            if (Lance.GameData.WeekendDoubleData.active == false)
                return 1f;

            if (itemType == ItemType.Gold)
                return Lance.GameData.WeekendDoubleData.goldBonusValue;
            else if (itemType == ItemType.Exp)
                return Lance.GameData.WeekendDoubleData.expBonusValue;
            else if (itemType == ItemType.PetFood)
                return Lance.GameData.WeekendDoubleData.petFoodBonusValue;
            else if (itemType == ItemType.UpgradeStone)
                return Lance.GameData.WeekendDoubleData.stoneBonusValue;
            else if (itemType == ItemType.ReforgeStone)
                return Lance.GameData.WeekendDoubleData.reforgeStoneBonusValue;
            else if (itemType == ItemType.CostumeUpgrade)
                return Lance.GameData.WeekendDoubleData.threadBonusValue;
            else
                return 1f;
        }
    }
}


