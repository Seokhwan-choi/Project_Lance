using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System;


namespace Lance
{
    class DamageInst
    {
        public Character Attacker;
        public Character Defender;

        public ObscuredDouble Damage;
        public ObscuredBool IsCritical;
        public ObscuredBool IsSuperCritical;

        public void OnRelease()
        {
            Attacker = null;
            Defender = null;
        }
    }

    static class StatCalculator
    {
        public static double GetPlayerStatValue(StatType statType)
        {
            Player player = Lance.GameManager.StageManager.Player;

            switch (statType)
            {
                case StatType.Atk:
                    return DamageCalculator.CalcPlayerAtk(player);
                case StatType.Hp:
                    return player.Stat.MaxHp;
                case StatType.CriProb:
                    return DamageCalculator.CalcPlayerCriProb(player);
                case StatType.CriDmg:
                    return player.Stat.CriDmg;
                case StatType.SuperCriProb:
                    return DamageCalculator.CalcPlayerSuperCriProb(player);
                case StatType.SuperCriDmg:
                    return player.Stat.SuperCriDmg;
                case StatType.AtkSpeed:
                    return player.Stat.AtkSpeed;
                case StatType.MoveSpeed:
                    return player.Stat.MoveSpeed;
                case StatType.AddDmg:
                    return DamageCalculator.CalcPlayerAddDmg(player);
                case StatType.BossDmg:
                    return DamageCalculator.CalcPlayerBossDmg(player);
                case StatType.MonsterDmg:
                    return player.Stat.MonsterDmg;
                case StatType.ExpAmount:
                    return player.Stat.IncreaseExpAmount;
                case StatType.GoldAmount:
                    return player.Stat.IncreaseGoldAmount;
                case StatType.SkillDmg:
                    return DamageCalculator.CalcPlayerSkillDmg(player);
                case StatType.FireAddDmg:
                    return player.Stat.FireAddDmg;
                case StatType.WaterAddDmg:
                    return player.Stat.WaterAddDmg;
                case StatType.GrassAddDmg:
                    return player.Stat.GrassAddDmg;
                case StatType.AmplifyAtk:
                    return player.Stat.AmplifyAtk;
                case StatType.AmplifyHp:
                    return player.Stat.AmplifyHp;
                case StatType.ManaSensitivity:
                    return player.Stat.ManaSensitivity;
                case StatType.AtkRange:
                case StatType.HpRatio:
                case StatType.ReduceSkillCoolTime:
                case StatType.AtkRatio:
                default:
                    return 0;
            }
        }
    }

    static class DamageCalculator
    {
       public static DamageInst Create(Character attacker, Character defender, double activeSkillValue = 1f)
       {
            DamageInst damageInst = new DamageInst();
            damageInst.Attacker = attacker;
            damageInst.Defender = defender;

            if (attacker is Player player)
            {
                bool isBoss = defender is Boss;
                bool isCritical = Util.Dice(CalcPlayerCriProb(player, damageInst));
                bool isSuperCritical = isCritical ? Util.Dice(CalcPlayerSuperCriProb(player, damageInst)) : false;

                damageInst.Damage = CalcPlayerAtk(player)
                    * (isSuperCritical ? (attacker.Stat.CriDmg * attacker.Stat.SuperCriDmg) : (isCritical ? attacker.Stat.CriDmg : 1))
                    * (activeSkillValue * (1 + CalcPlayerSkillDmg(player, damageInst)))
                    * (1 + CalcPlayerAddDmg(player, damageInst))
                    * (isBoss ? (1 + CalcPlayerBossDmg(player, damageInst)) : (1 + CalcPlayerMonsterDmg(player)))
                    * CalcElementalDmg(player, defender);

                // 마나 감응도가 있다면 계산해주자
                if (attacker.Stat.ManaSensitivity > 0 || defender.Stat.ManaSensitivity > 0)
                {
                    double manaSensitivity = (1.05 + (attacker.Stat.ManaSensitivity - defender.Stat.ManaSensitivity)) / 222;

                    damageInst.Damage = Math.Max(0, damageInst.Damage * (1 + manaSensitivity));
                }

                damageInst.IsCritical = isCritical;
                damageInst.IsSuperCritical = isSuperCritical;
            }
            else
            {
                // 몬스터의 경우, 추가 효과를 가지는 경우는 없다.

                // 캐릭터의 공격력
                ObscuredDouble baseAtk = attacker.Stat.Atk;
                // 캐릭터의 공격력 비율
                ObscuredDouble baseAtkRatio = 1;
                baseAtkRatio += attacker.Stat.AtkRatio;

                damageInst.Damage = ((baseAtk * baseAtkRatio) * (1 + attacker.Stat.AmplifyAtk)) * activeSkillValue * CalcElementalDmg(attacker, defender);
            }

            return damageInst;
       }

        public static double CalcPlayerAtk(Player player)
        {   
            // 캐릭터의 공격력
            ObscuredDouble baseAtk = player.Stat.Atk;
            // 캐릭터의 공격력 비율
            ObscuredDouble baseAtkRatio = 1 + player.Stat.AtkRatio;

            return ((baseAtk * baseAtkRatio) * ( 1 + player.Stat.AmplifyAtk ) + player.Stat.BonusAtk);
        }

        public static double CalcPlayerAddDmg(Player player, DamageInst inst = null)
        {
            return player.Stat.AddDmg + player.SkillManager.GatherPassiveSkillValue(StatType.AddDmg, inst);
        }

        public static double CalcPlayerSkillDmg(Player player, DamageInst inst = null)
        {
            // 스킬 숙련도로 얻는 스킬 추가 데미지
            return player.Stat.SkillDmg + player.SkillManager.GatherPassiveSkillValue(StatType.SkillDmg, inst);
        }

        public static double CalcPlayerBossDmg(Player player, DamageInst inst = null)
        {
            return player.Stat.BossDmg + player.SkillManager.GatherPassiveSkillValue(StatType.BossDmg, inst);
        }

        public static double CalcPlayerMonsterDmg(Player player)
        {
            return player.Stat.MonsterDmg;
        }

        public static float CalcPlayerCriProb(Player player, DamageInst inst = null)
        {
            return Mathf.Min(1f, player.Stat.CriProb);
        }

        public static float CalcPlayerSuperCriProb(Player player, DamageInst inst = null)
        {
            return Mathf.Min(1f, player.Stat.SuperCriProb + (float)player.SkillManager.GatherPassiveSkillValue(StatType.SuperCriProb, inst));
        }

        public static float CalcElementalDmg(Character attacker, Character defender)
        {
            if (attacker != null && defender != null)
            {
                // 속성의 상성에 따라서 추가데미지가 들어갈 수 있다.
                ElementalType atkElementalType = attacker.Stat.ElementalType;
                ElementalType defElementalType = defender.Stat.ElementalType;

                var compatibilityData = Lance.GameData.CompatibilityData.TryGet(atkElementalType);
                if (compatibilityData == null)
                    return 1f;

                if (compatibilityData.strongType == defElementalType)
                {
                    if (atkElementalType == ElementalType.Fire)
                        return Lance.GameData.PetCommonData.strongTypeAtkValue + (float)attacker.Stat.FireAddDmg;
                    else if (atkElementalType == ElementalType.Water)
                        return Lance.GameData.PetCommonData.strongTypeAtkValue + (float)attacker.Stat.WaterAddDmg;
                    else if (atkElementalType == ElementalType.Grass)
                        return Lance.GameData.PetCommonData.strongTypeAtkValue + (float)attacker.Stat.GrassAddDmg;
                    else
                        return Lance.GameData.PetCommonData.strongTypeAtkValue;
                }
                else if (compatibilityData.weakType == defElementalType)
                {
                    return Lance.GameData.PetCommonData.weakTypeAtkValue;
                }
                else
                {
                    if (atkElementalType == ElementalType.Fire)
                        return 1f + (float)attacker.Stat.FireAddDmg;
                    else if (atkElementalType == ElementalType.Water)
                        return 1f + (float)attacker.Stat.WaterAddDmg;
                    else if (atkElementalType == ElementalType.Grass)
                        return 1f + (float)attacker.Stat.GrassAddDmg;
                    else
                        return 1f;
                }
            }
            else
            {
                return 1f;
            }
        }
    }


    static class PowerLevelCalculator
    {
        public static double Calc()
        {
            double atk = Lance.Account.GatherStat(StatType.Atk);
            atk += Lance.Account.GatherStat(StatType.PowerAtk);
            double atkRatio = Lance.Account.GatherStat(StatType.AtkRatio);
            double amplifyAtk = Lance.Account.GatherStat(StatType.AmplifyAtk);
            float atkSpeed = (float)Lance.Account.GatherStat(StatType.AtkSpeed) * (1 + (float)Lance.Account.GatherStat(StatType.AtkSpeedRatio));

            ObscuredDouble newHp = Lance.Account.GatherStat(StatType.Hp);
            newHp += Lance.Account.GatherStat(StatType.PowerHp);
            double hpRatio = Lance.Account.GatherStat(StatType.HpRatio);
            double amplifyHp = Lance.Account.GatherStat(StatType.AmplifyHp);

            newHp *= (1 + hpRatio);
            newHp *= (1 + amplifyHp);

            double maxHp = newHp;

            float criProb = (float)Lance.Account.GatherStat(StatType.CriProb);
            double criDmg = Lance.Account.GatherStat(StatType.CriDmg);
            float superCriProb = (float)Lance.Account.GatherStat(StatType.SuperCriProb);
            double superCriDmg = Lance.Account.GatherStat(StatType.SuperCriDmg);
            double skillDmg = Lance.Account.GatherStat(StatType.SkillDmg);
            double equippedSkillValues = Lance.Account.SkillInventory.GetTotalEquippedSkillValues();
            float bossDmg = (float)Lance.Account.GatherStat(StatType.BossDmg);
            float monsterDmg = (float)Lance.Account.GatherStat(StatType.MonsterDmg);
            float addDmg = (float)Lance.Account.GatherStat(StatType.AddDmg);
            float elementalAddDmg = (float)Lance.Account.GatherStat(StatType.FireAddDmg) 
                + (float)Lance.Account.GatherStat(StatType.WaterAddDmg)
                + (float)Lance.Account.GatherStat(StatType.GrassAddDmg);

            double powerLevel = ((CalcAtk() + (maxHp * 0.5f)) * (atkSpeed) 
                * (1 + Mathf.Min(1f, criProb)) * criDmg) 
                * (1 + Mathf.Min(1f, superCriProb) * superCriDmg)
                * (1 + skillDmg + equippedSkillValues) 
                * (1 + bossDmg + monsterDmg)
                * (1 + addDmg) * (1 + elementalAddDmg);

            double manaSensitivity = Lance.Account.GatherStat(StatType.ManaSensitivity);
            // 마나 감응도가 있다면 계산해주자
            if (manaSensitivity > 0)
            {
                double manaSensitivityValue = (1.05 + manaSensitivity) / 222;

                powerLevel = Math.Max(0, powerLevel * (1 + manaSensitivityValue));
            }

            double CalcAtk()
            {
                return (atk * (1 + atkRatio) * ( 1 + amplifyAtk));
            }

            return powerLevel;
        }

        public static double CalcPetEquipmentPowerLevel(string id)
        {
            double atk = Math.Max(1, Lance.Account.GetPetEquipmentStatValue(id, StatType.Atk, ignoreEquipped: true));
            double atkRatio = Math.Max(1, Lance.Account.GetPetEquipmentStatValue(id, StatType.AtkRatio, ignoreEquipped: true));
            double amplifyAtk = Math.Max(1, Lance.Account.GetPetEquipmentStatValue(id, StatType.AmplifyAtk, ignoreEquipped: true));
            float atkSpeed = Math.Max(1, (1 + (float)Lance.Account.GetPetEquipmentStatValue(id, StatType.AtkSpeedRatio, ignoreEquipped: true)));

            ObscuredDouble newHp = Math.Max(1, Lance.Account.GetPetEquipmentStatValue(id, StatType.Hp, ignoreEquipped: true));
            double hpRatio = Math.Max(1, Lance.Account.GetPetEquipmentStatValue(id, StatType.HpRatio, ignoreEquipped: true));
            double amplifyHp = Math.Max(1, Lance.Account.GetPetEquipmentStatValue(id, StatType.AmplifyHp, ignoreEquipped: true));

            newHp *= (1 + hpRatio);
            newHp *= (1 + amplifyHp);

            double maxHp = newHp;

            float criProb = (float)Lance.Account.GetPetEquipmentStatValue(id, StatType.CriProb, ignoreEquipped: true);
            double criDmg = Math.Max(1, Lance.Account.GetPetEquipmentStatValue(id, StatType.CriDmg, ignoreEquipped: true));
            float superCriProb = (float)Lance.Account.GetPetEquipmentStatValue(id, StatType.SuperCriProb, ignoreEquipped: true);
            double superCriDmg = Math.Max(1, Lance.Account.GetPetEquipmentStatValue(id, StatType.SuperCriDmg, ignoreEquipped: true));
            double skillDmg = Lance.Account.GetPetEquipmentStatValue(id, StatType.SkillDmg, ignoreEquipped: true);
            float bossDmg = (float)Lance.Account.GetPetEquipmentStatValue(id, StatType.BossDmg, ignoreEquipped: true);
            float monsterDmg = (float)Lance.Account.GetPetEquipmentStatValue(id, StatType.MonsterDmg, ignoreEquipped: true);
            float addDmg = (float)Lance.Account.GetPetEquipmentStatValue(id, StatType.AddDmg, ignoreEquipped: true);
            float elementalAddDmg = (float)Lance.Account.GetPetEquipmentStatValue(id, StatType.FireAddDmg, ignoreEquipped: true)
                + (float)Lance.Account.GetPetEquipmentStatValue(id, StatType.WaterAddDmg, ignoreEquipped: true)
                + (float)Lance.Account.GetPetEquipmentStatValue(id, StatType.GrassAddDmg, ignoreEquipped: true);

            double powerLevel = ((CalcAtk() + (maxHp * 0.5f)) * (atkSpeed)
                * (1 + Mathf.Min(1f, criProb)) * criDmg)
                * (1 + Mathf.Min(1f, superCriProb) * superCriDmg)
                * (1 + skillDmg) * (1 + bossDmg + monsterDmg)
                * (1 + addDmg) * (1 + elementalAddDmg);

            double manaSensitivity = Lance.Account.GetPetEquipmentStatValue(id, StatType.ManaSensitivity, ignoreEquipped: true);
            // 마나 감응도가 있다면 계산해주자
            if (manaSensitivity > 0)
            {
                double manaSensitivityValue = (1.05 + manaSensitivity) / 222;

                powerLevel = Math.Max(0, powerLevel * (1 + manaSensitivityValue));
            }

            double CalcAtk()
            {
                return (atk * (1 + atkRatio) * (1 + amplifyAtk));
            }

            return powerLevel;
        }

        public static double CalcEquipmentPowerLevel(string id)
        {
            double atk = Math.Max(1, Lance.Account.GetEquipmentStatValue(id ,StatType.Atk, ignoreEquipped:true));
            double atkRatio = Math.Max(0, Lance.Account.GetEquipmentStatValue(id, StatType.AtkRatio, ignoreEquipped: true));
            double amplifyAtk = Math.Max(0, Lance.Account.GetEquipmentStatValue(id, StatType.AmplifyAtk, ignoreEquipped: true));
            float atkSpeed = Math.Max(1, (1 + (float)Lance.Account.GetEquipmentStatValue(id, StatType.AtkSpeedRatio, ignoreEquipped: true)));

            ObscuredDouble newHp = Math.Max(1, Lance.Account.GetEquipmentStatValue(id, StatType.Hp, ignoreEquipped: true));
            double hpRatio = Math.Max(0, Lance.Account.GetEquipmentStatValue(id, StatType.HpRatio, ignoreEquipped: true));
            double amplifyHp = Math.Max(0, Lance.Account.GetEquipmentStatValue(id, StatType.AmplifyHp, ignoreEquipped: true));

            newHp *= (1 + hpRatio);
            newHp *= (1 + amplifyHp);

            double maxHp = newHp;

            float criProb = (float)Lance.Account.GetEquipmentStatValue(id, StatType.CriProb, ignoreEquipped: true);
            double criDmg = Math.Max(1, Lance.Account.GetEquipmentStatValue(id, StatType.CriDmg, ignoreEquipped: true));
            float superCriProb = (float)Lance.Account.GetEquipmentStatValue(id, StatType.SuperCriProb, ignoreEquipped: true);
            double superCriDmg = Math.Max(1, Lance.Account.GetEquipmentStatValue(id, StatType.SuperCriDmg, ignoreEquipped: true));
            double skillDmg = Lance.Account.GetEquipmentStatValue(id, StatType.SkillDmg, ignoreEquipped: true);
            float bossDmg = (float)Lance.Account.GetEquipmentStatValue(id, StatType.BossDmg, ignoreEquipped: true);
            float monsterDmg = (float)Lance.Account.GetEquipmentStatValue(id, StatType.MonsterDmg, ignoreEquipped: true);
            float addDmg = (float)Lance.Account.GetEquipmentStatValue(id, StatType.AddDmg, ignoreEquipped: true);
            float elementalAddDmg = (float)Lance.Account.GetEquipmentStatValue(id, StatType.FireAddDmg, ignoreEquipped:true)
                + (float)Lance.Account.GetEquipmentStatValue(id, StatType.WaterAddDmg, ignoreEquipped:true)
                + (float)Lance.Account.GetEquipmentStatValue(id, StatType.GrassAddDmg, ignoreEquipped: true);

            double powerLevel = ((CalcAtk() + (maxHp * 0.5f)) * (atkSpeed) 
                * (1 + Mathf.Min(1f, criProb)) * criDmg) 
                * (1 + Mathf.Min(1f, superCriProb) * superCriDmg)
                * (1 + skillDmg) * (1 + bossDmg + monsterDmg)
                * (1 + addDmg) * (1 + elementalAddDmg);

            double manaSensitivity = Lance.Account.GetEquipmentStatValue(id, StatType.ManaSensitivity, ignoreEquipped: true);
            // 마나 감응도가 있다면 계산해주자
            if (manaSensitivity > 0)
            {
                double manaSensitivityValue = (1.05 + manaSensitivity) / 222;

                powerLevel = Math.Max(0, powerLevel * (1 + manaSensitivityValue));
            }

            double CalcAtk()
            {
                return (atk * (1 + atkRatio) * (1 + amplifyAtk));
            }

            return powerLevel;
        }

        public static double CalcAccessoryPowerLevel(string id)
        {
            double atk = Math.Max(1, Lance.Account.GetAccessoryStatValue(id, StatType.Atk, ignoreEquipped: true));
            double atkRatio = Math.Max(0, Lance.Account.GetAccessoryStatValue(id, StatType.AtkRatio, ignoreEquipped: true));
            double amplifyAtk = Math.Max(0, Lance.Account.GetAccessoryStatValue(id, StatType.AmplifyAtk, ignoreEquipped: true));
            float atkSpeed = Math.Max(1, (1 + (float)Lance.Account.GetAccessoryStatValue(id, StatType.AtkSpeedRatio, ignoreEquipped: true)));

            ObscuredDouble newHp = Math.Max(1, Lance.Account.GetAccessoryStatValue(id, StatType.Hp, ignoreEquipped: true));
            double hpRatio = Math.Max(0, Lance.Account.GetAccessoryStatValue(id, StatType.HpRatio, ignoreEquipped: true));
            double amplifyHp = Math.Max(0, Lance.Account.GetAccessoryStatValue(id, StatType.AmplifyHp, ignoreEquipped: true));

            newHp *= (1 + hpRatio);
            newHp *= (1 + amplifyHp);

            double maxHp = newHp;

            float criProb = (float)Lance.Account.GetAccessoryStatValue(id, StatType.CriProb, ignoreEquipped: true);
            double criDmg = Math.Max(0, Lance.Account.GetAccessoryStatValue(id, StatType.CriDmg, ignoreEquipped: true));
            float superCriProb = (float)Lance.Account.GetAccessoryStatValue(id, StatType.SuperCriProb, ignoreEquipped: true);
            double superCriDmg = Math.Max(0, Lance.Account.GetAccessoryStatValue(id, StatType.SuperCriDmg, ignoreEquipped: true));
            double skillDmg = Lance.Account.GetAccessoryStatValue(id, StatType.SkillDmg, ignoreEquipped: true);
            float bossDmg = (float)Lance.Account.GetAccessoryStatValue(id, StatType.BossDmg, ignoreEquipped: true);
            float monsterDmg = (float)Lance.Account.GetAccessoryStatValue(id, StatType.MonsterDmg, ignoreEquipped: true);
            float addDmg = (float)Lance.Account.GetAccessoryStatValue(id, StatType.AddDmg, ignoreEquipped: true);
            float elementalAddDmg = (float)Lance.Account.GetAccessoryStatValue(id, StatType.FireAddDmg, ignoreEquipped: true)
                + (float)Lance.Account.GetAccessoryStatValue(id, StatType.WaterAddDmg, ignoreEquipped: true)
                + (float)Lance.Account.GetAccessoryStatValue(id, StatType.GrassAddDmg, ignoreEquipped: true);

            double powerLevel = ((CalcAtk() + (maxHp * 0.5f)) * (atkSpeed)
                * (1 + Mathf.Min(1f, criProb)) * (1 + criDmg))
                * (1 + Mathf.Min(1f, superCriProb) * (1 + superCriDmg))
                * (1 + skillDmg) * (1 + bossDmg + monsterDmg)
                * (1 + addDmg) * (1 + elementalAddDmg);

            double manaSensitivity = Lance.Account.GetAccessoryStatValue(id, StatType.ManaSensitivity, ignoreEquipped: true);
            // 마나 감응도가 있다면 계산해주자
            if (manaSensitivity > 0)
            {
                double manaSensitivityValue = (1.05 + manaSensitivity) / 222;

                powerLevel = Math.Max(0, powerLevel * (1 + manaSensitivityValue));
            }

            double CalcAtk()
            {
                return (atk * (1 + atkRatio) * (1 + amplifyAtk));
            }

            return powerLevel;
        }
    }
}