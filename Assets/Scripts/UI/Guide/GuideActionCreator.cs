
namespace Lance
{
    static class GuideActionCreater
    {
        public static GuideAction Create(GuideActionManager manager, GuideActionData data)
        {
            GuideAction action = null;
            switch (data.type)
            {
                case GuideActionType.JustWait:
                    action = new GuideAction_JustWaitTime();
                    break;
                case GuideActionType.OpenMenu:
                    action = new GuideAction_OpenMenu();
                    break;

                case GuideActionType.Move_StatureTab:
                case GuideActionType.Move_Stature_TrainTab:
                case GuideActionType.Move_Stature_AbilityTab:
                case GuideActionType.Move_Stature_ArtifactTab:
                case GuideActionType.Move_Stature_LimitBreakTab:
                case GuideActionType.Move_SkillTab:
                case GuideActionType.Move_InventoryTab:
                case GuideActionType.Move_Inventory_WeaponTab:
                case GuideActionType.Move_Inventory_ArmorTab:
                case GuideActionType.Move_Inventory_GlovesTab:
                case GuideActionType.Move_Inventory_ShoesTab:
                case GuideActionType.Move_SpawnTab:
                case GuideActionType.Move_Spawn_SkillTab:
                case GuideActionType.Move_Spawn_EquipmentTab:
                case GuideActionType.Move_Spawn_ArtifactTab:
                case GuideActionType.Move_DungeonTab:
                case GuideActionType.Move_ShopTab:
                case GuideActionType.Move_Skill_ActiveTab:
                case GuideActionType.Move_Skill_PassiveTab:
                case GuideActionType.Move_PetTab:
                    action = new GuideAction_MoveLobbyTab();
                    break;
                //case GuideActionType.EnsureVisible_TrainAtkButton:
                //case GuideActionType.EnsureVisible_TrainHpButton:
                //case GuideActionType.EnsureVisible_TrainCriProbButton:
                //case GuideActionType.EnsureVisible_TrainCriDmgButton:
                //case GuideActionType.EnsureVisible_BestEquipment:
                //case GuideActionType.EnsureVisible_BestSkill:
                //case GuideActionType.EnsureVisible_CanUpgradeSkill:
                //case GuideActionType.EnsureVisible_SpawnWeaponButton:
                //case GuideActionType.EnsureVisible_SpawnArmorButton:
                //case GuideActionType.EnsureVisible_SpawnGlovesButton:
                //case GuideActionType.EnsureVisible_SpawnShoesButton:
                //case GuideActionType.Count:
                //    break;
                case GuideActionType.Highlight_TrainAtkButton:
                case GuideActionType.Highlight_TrainHpButton:
                case GuideActionType.Highlight_TrainCriProbButton:
                case GuideActionType.Highlight_TrainCriDmgButton:

                case GuideActionType.Highlight_FirstAbility:
                case GuideActionType.Highlight_LevelUpAbilityButton:

                case GuideActionType.Highlight_BestSkill:
                case GuideActionType.Highlight_CanEquipBestSkill:
                case GuideActionType.Highlight_CanUpgradeSkill:
                case GuideActionType.Highlight_EquipActiveSkillButton:
                case GuideActionType.Highlight_UpgradeActiveSkillButton:
                case GuideActionType.Highlight_EquipPassiveSkillButton:
                case GuideActionType.Highlight_UpgradePassiveSkillButton:
                case GuideActionType.Highlight_AutoCastSkillButton:

                case GuideActionType.Highlight_ChallengeBoss:
                case GuideActionType.Highlight_AutoChallengeBoss:

                case GuideActionType.Highlight_SpawnWeaponButton:
                case GuideActionType.Highlight_SpawnArmorButton:
                case GuideActionType.Highlight_SpawnGlovesButton:
                case GuideActionType.Highlight_SpawnShoesButton:
                case GuideActionType.Highlight_SpawnSkillButton:
                case GuideActionType.Highlight_SpawnArtifactButton:

                case GuideActionType.Highlight_GoldDungeon:
                case GuideActionType.Highlight_StoneDungeon:
                case GuideActionType.Highlight_ReforgeDungeon:
                case GuideActionType.Highlight_GrowthDungeon:
                case GuideActionType.Highlight_PetDungeon:
                case GuideActionType.Highlight_RaidDungeon:

                case GuideActionType.Highlight_AttendanceButton:
                case GuideActionType.Highlight_RankButton:
                case GuideActionType.Highlight_SettingButton:

                case GuideActionType.Highlight_BestEquipment:
                case GuideActionType.Highlight_CombineEquipmentButton:
                case GuideActionType.Highlight_EquipEquipmentButton:
                case GuideActionType.Highlight_UpgradeEquipmentButton:

                case GuideActionType.Highlight_BuffButton:
                case GuideActionType.Highlight_AtkBuffButton:
                case GuideActionType.Highlight_GoldBuffButton:
                case GuideActionType.Highlight_ExpBuffButton:

                case GuideActionType.Highlight_QuestButton:
                case GuideActionType.Highlight_RepeatQuestButton:

                case GuideActionType.Highlight_LimitBreakButton:
                case GuideActionType.Highlight_AllCombineWeaponButton:
                case GuideActionType.Highlight_AllCombineArmorButton:
                case GuideActionType.Highlight_AllCombineGlovesButton:
                case GuideActionType.Highlight_AllCombineShoesButton:
                case GuideActionType.Highlight_FirstSkillCastSlot:
                case GuideActionType.Highlight_RaidRankButton:
                case GuideActionType.Highlight_GuideQuest:
                case GuideActionType.Highlight_SpeedModeButton:
                case GuideActionType.Highlight_BestPet:
                case GuideActionType.Highlight_EquipPetButton:
                case GuideActionType.Highlight_FeedPetButton:
                case GuideActionType.Highlight_BountyButton:
                case GuideActionType.Highlight_EssenceButton:
                default:
                    action = new GuideAction_Highlight();
                    break;
            }

            action?.Init(manager, data);

            return action;
        }
    }
}
