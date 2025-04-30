using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public partial class Account
    {
        public double GatherStat(StatType statType, PlayerStatUpdateBitFlags updateFlags = PlayerStatUpdateBitFlags.All)
        {
            PlayerBaseStatData baseStatData = Lance.GameData.PlayerBaseStatData;

            // 1. Ä³¸¯ÅÍ º£ÀÌ½º ½ºÅÈ
            double totalStatValue = 0;

            switch (statType)
            {
                case StatType.Atk:
                    totalStatValue += baseStatData.atk;
                    break;
                case StatType.AtkRange:
                    totalStatValue += baseStatData.atkRange;
                    break;
                case StatType.Hp:
                    totalStatValue += baseStatData.hp;
                    break;
                case StatType.CriProb:
                    totalStatValue += baseStatData.criProb;
                    break;
                case StatType.CriDmg:
                    totalStatValue += baseStatData.criDmg;
                    break;
                case StatType.SuperCriProb:
                    totalStatValue += baseStatData.superCriProb;
                    break;
                case StatType.SuperCriDmg:
                    totalStatValue += baseStatData.superCriDmg;
                    break;
                case StatType.AtkSpeed:
                    totalStatValue += baseStatData.atkSpeed;
                    break;
                case StatType.MoveSpeed:
                    totalStatValue += baseStatData.moveSpeed;
                    break;
                default:
                    break;
            }

            // 2. °ñµå ÈÆ·Ã ½ºÅÈ
            totalStatValue += GoldTrain.GetTrainStatValue(statType);

            // 3. Àåºñ ½ºÅÈ
            totalStatValue += GatherInventoryStatValues(statType);

            // 4. Æ¯¼º ½ºÅÈ
            totalStatValue += Ability.GatherStatValues(statType);

            // 5. ÇÑ°è µ¹ÆÄ ½ºÅÈ
            if (ExpLevel.GetUltimateLimitBreak() > 0)
                totalStatValue += ExpLevel.GetUltimateLimitBreakStatValue(statType);
            else
                totalStatValue += ExpLevel.GetLimitBreakStatValue(statType);

            // 7-1. À¯¹° ½ºÅÈ
            totalStatValue += Artifact.GatherStatValues(statType);

            // 7-2. °í´ë À¯¹° ½ºÅÈ
            totalStatValue += AncientArtifact.GatherStatValues(statType);

            // 7-3. ¿¢½ºÄ®¸®¹ö ½ºÅÈ
            totalStatValue += Excalibur.GetStatValue(statType);

            // 8. ¹öÇÁ ½ºÅÈ
            totalStatValue += Buff.GatherStatValues(statType);

            // 9. ½Å¼ö ½ºÅÈ
            totalStatValue += Pet.GatherStatValues(statType);

            // 10. ÄÚ½ºÆ¬
            totalStatValue += Costume.GatherStatValues(statType);

            // 11. Á¤¼ö
            totalStatValue += Essence.GetStatValue(statType);

            // 12. µµ°¨
            totalStatValue += Collection.GatherStatValues(statType);

            // 13. ¾÷Àû
            totalStatValue += Achievement.GatherStatValues(statType);

            // 14. ½Å¹°
            totalStatValue += GatherPetInventoryStatValues(statType);

            // 15. ¿µ±¤ÀÇ º¸ÁÖ
            totalStatValue += JoustGloryOrb.GetStatValue(statType);

            // 16. Àå½Å±¸
            totalStatValue += GatherAccessoryInventoryStatValues(statType);

            if (statType == StatType.SkillDmg)
            {
                totalStatValue += SkillInventory.GetSkillProficiencyValue();
            }

            // 17. ¸¶³ª ÇÏÆ®
            totalStatValue += ManaHeart.GetStatValue(statType);

            return totalStatValue;
        }
    }

    public enum PlayerStatUpdateBitFlags
    {
        GoldTrain = 1 << 0,
        Inventory = 1 << 1,     
        Ability = 1 << 2,
        LimitBreak = 1 << 3,
        Artifact = 1 << 4,
        AncientArtifact = 1 << 5,
        Excalibur = 1 << 6,
        Buff = 1 << 7,
        Pet = 1 << 8,
        Costume = 1 << 9,
        Collection = 1 << 10,
        Achievement = 1 << 11,
        PetInventory = 1 << 12,
        JoustGloryOrb = 1 << 13,
        AccessoryInventory = 1 << 14,
        SkillInventory = 1 << 15,
        ManaHeart = 1 << 16,

        All = GoldTrain | Inventory | Ability | LimitBreak | Artifact 
            | AncientArtifact | Excalibur | Buff | Pet | Costume 
            | Collection | Achievement | PetInventory | JoustGloryOrb | AccessoryInventory
            | SkillInventory | ManaHeart
    }
}
