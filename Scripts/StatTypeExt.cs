using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    static class StatTypeExt
    {
        public static bool IsGoldTrainType(this StatType type)
        {
            return type == StatType.Atk ||
                type == StatType.Hp ||
                type == StatType.CriProb ||
                type == StatType.CriDmg ||
                type == StatType.GoldAmount ||
                type == StatType.ExpAmount ||
                type == StatType.SuperCriProb ||
                type == StatType.SuperCriDmg ||
                type == StatType.PowerAtk ||
                type == StatType.PowerHp ||
                type == StatType.AmplifyAtk ||
                type == StatType.AmplifyHp ||
                type == StatType.FireAddDmg ||
                type == StatType.WaterAddDmg ||
                type == StatType.GrassAddDmg;
        }

        public static bool IsPercentType(this StatType type)
        {
            return type != StatType.Atk &&
                type != StatType.AtkRange &&
                type != StatType.Hp &&
                type != StatType.PowerAtk &&
                type != StatType.PowerHp &&
                type != StatType.MoveSpeed &&
                type != StatType.AtkSpeed &&
                type != StatType.Level &&
                type != StatType.ManaSensitivity;
        }

        public static bool IsNoneAlphaType(this StatType type)
        {
            return type == StatType.MoveSpeed || type == StatType.AtkSpeed || type == StatType.Level || type == StatType.ManaSensitivity;
        }

        public static QuestType ChangeToQuestType(this StatType type)
        {
            switch (type)
            {
                case StatType.Atk:
                    return QuestType.TrainAtk;
                case StatType.PowerAtk:
                    return QuestType.TrainPowerAtk;
                case StatType.Hp:
                    return QuestType.TrainHp;
                case StatType.PowerHp:
                    return QuestType.TrainPowerHp;
                case StatType.CriProb:
                    return QuestType.TrainCriProb;
                case StatType.CriDmg:
                    return QuestType.TrainCriDmg;
                case StatType.SuperCriProb:
                    return QuestType.TrainSuperCriProb;
                case StatType.SuperCriDmg:
                    return QuestType.TrainSuperCriDmg;
                default:
                    return QuestType.TrainAtk;

            }
        }

        public static GuideActionType TrainTypeChangeToGuideActionType(this StatType type)
        {
            switch (type)
            {
                case StatType.Atk:
                    return GuideActionType.Highlight_TrainAtkButton;
                case StatType.Hp:
                    return GuideActionType.Highlight_TrainHpButton;
                case StatType.CriProb:
                    return GuideActionType.Highlight_TrainCriProbButton;
                case StatType.CriDmg:
                    return GuideActionType.Highlight_TrainCriDmgButton;
                default:
                    return GuideActionType.Highlight_TrainAtkButton;
            }
        }

        public static GuideActionType BuffTypeChangeToGuideActionType(this StatType type)
        {
            switch (type)
            {
                case StatType.Atk:
                    return GuideActionType.Highlight_AtkBuffButton;
                case StatType.GoldAmount:
                    return GuideActionType.Highlight_GoldBuffButton;
                case StatType.ExpAmount:
                    return GuideActionType.Highlight_ExpBuffButton;
                default:
                    return GuideActionType.Highlight_AtkBuffButton;
            }
        }

        public static bool IsShowingUserInfo(this StatType type)
        {
            return type == StatType.Atk ||
                type == StatType.AmplifyAtk ||
                type == StatType.Hp ||
                type == StatType.AmplifyHp ||
                type == StatType.CriProb ||
                type == StatType.CriDmg ||
                type == StatType.SuperCriProb ||
                type == StatType.SuperCriDmg ||
                type == StatType.AtkSpeed ||
                type == StatType.MoveSpeed ||
                type == StatType.AddDmg ||
                type == StatType.FireAddDmg ||
                type == StatType.WaterAddDmg ||
                type == StatType.GrassAddDmg ||
                type == StatType.MonsterDmg ||
                type == StatType.BossDmg ||
                type == StatType.SkillDmg ||
                type == StatType.ExpAmount ||
                type == StatType.GoldAmount ||
                type == StatType.ManaSensitivity;
        }

        public static AdType ChangeToAdType(this StatType statType)
        {
            switch (statType)
            {
                case StatType.Atk:
                    return AdType.Active_Buff_Atk;
                case StatType.GoldAmount:
                    return AdType.Active_Buff_Gold;
                case StatType.ExpAmount:
                    return AdType.Active_Buff_Exp;
                default:
                    return AdType.Active_Buff_Atk;
            }
        }
    }
}