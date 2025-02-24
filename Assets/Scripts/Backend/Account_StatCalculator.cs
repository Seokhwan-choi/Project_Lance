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

            // 1. ĳ���� ���̽� ����
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

            // 2. ��� �Ʒ� ����
            totalStatValue += GoldTrain.GetTrainStatValue(statType);

            // 3. ��� ����
            totalStatValue += GatherInventoryStatValues(statType);

            // 4. Ư�� ����
            totalStatValue += Ability.GatherStatValues(statType);

            // 5. �Ѱ� ���� ����
            if (ExpLevel.GetUltimateLimitBreak() > 0)
                totalStatValue += ExpLevel.GetUltimateLimitBreakStatValue(statType);
            else
                totalStatValue += ExpLevel.GetLimitBreakStatValue(statType);

            // 7-1. ���� ����
            totalStatValue += Artifact.GatherStatValues(statType);

            // 7-2. ��� ���� ����
            totalStatValue += AncientArtifact.GatherStatValues(statType);

            // 7-3. ����Į���� ����
            totalStatValue += Excalibur.GetStatValue(statType);

            // 8. ���� ����
            totalStatValue += Buff.GatherStatValues(statType);

            // 9. �ż� ����
            totalStatValue += Pet.GatherStatValues(statType);

            // 10. �ڽ�Ƭ
            totalStatValue += Costume.GatherStatValues(statType);

            // 11. ����
            totalStatValue += Essence.GetStatValue(statType);

            // 12. ����
            totalStatValue += Collection.GatherStatValues(statType);

            // 13. ����
            totalStatValue += Achievement.GatherStatValues(statType);

            // 14. �Ź�
            totalStatValue += GatherPetInventoryStatValues(statType);

            // 15. ������ ����
            totalStatValue += JoustGloryOrb.GetStatValue(statType);

            // 16. ��ű�
            totalStatValue += GatherAccessoryInventoryStatValues(statType);

            if (statType == StatType.SkillDmg)
            {
                totalStatValue += SkillInventory.GetSkillProficiencyValue();
            }

            // 17. ���� ��Ʈ
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
