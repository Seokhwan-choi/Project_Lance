using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    static class AbilityUtil
    {
        public static double CalcStatValue(string id, int level)
        {
            var data = Lance.GameData.AbilityData.TryGet(id);
            if (data == null)
                return 0;

            if (data.increaseStartLevel > 0 && data.increaseLevelUpValue > 0)
            {
                if (level > data.increaseStartLevel)
                {
                    double defaultValue = data.levelUpValue * data.increaseStartLevel;
                    double totalIncreaseValue = 0;
                    double totalValue = 0;

                    int increaseLevel = level - data.increaseStartLevel;

                    for (int i = 0; i < increaseLevel; ++i)
                    {
                        int count = i + 1;

                        totalIncreaseValue += (data.levelUpValue + (data.increaseLevelUpValue * count));
                    }

                    totalValue = defaultValue + totalIncreaseValue;

                    return totalValue;
                }
                else
                {
                    return data.levelUpValue * level;
                }
            }
            else
            {
                return data.levelUpValue * level;
            }
        }

        public static int CalcRequireAP(string id, int level)
        {
            var data = Lance.GameData.AbilityData.TryGet(id);
            if (data == null)
                return int.MaxValue;

            return (level + 1) * data.requireAp;
        }
    }
}