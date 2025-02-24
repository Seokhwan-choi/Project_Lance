using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    class GuideAction_MoveLobbyTab : GuideAction
    {
        public override void OnStart()
        {
            var type = mData.type;

            mParent.SetActivHighlightObj(false);
            mParent.SetActiveTouchBlock(true);
            //mParent.SetTouchBlockAlpha(0f);

            Lance.Lobby.ChangeTab(type.ChangeToLobbyTab());

            if (type == GuideActionType.Move_Stature_AbilityTab ||
                type == GuideActionType.Move_Stature_ArtifactTab ||
                type == GuideActionType.Move_Stature_TrainTab ||
                type == GuideActionType.Move_Stature_LimitBreakTab)
            {
                var statureTabUI = Lance.Lobby.GetLobbyTabUI<Lobby_StatureUI>();
                statureTabUI.ChangeTab(type.ChangeToStatureTab());
            }
            else if (type == GuideActionType.Move_Inventory_WeaponTab ||
                    type == GuideActionType.Move_Inventory_ArmorTab ||
                    type == GuideActionType.Move_Inventory_GlovesTab ||
                    type == GuideActionType.Move_Inventory_ShoesTab)
            {
                var inventoryTabUI = Lance.Lobby.GetLobbyTabUI<Lobby_InventoryUI>();
                inventoryTabUI.ChangeTab(type.ChangeToInventoryTab());
            }
            else if (type == GuideActionType.Move_Skill_ActiveTab ||
                    type == GuideActionType.Move_Skill_PassiveTab)
            {
                var skillTabUI = Lance.Lobby.GetLobbyTabUI<Lobby_SkillInventoryUI>();
                skillTabUI.ChangeTab(type.ChangeToSkillTab());
            }
            else if (type == GuideActionType.Move_Spawn_SkillTab ||
                    type == GuideActionType.Move_Spawn_EquipmentTab ||
                    type == GuideActionType.Move_Spawn_ArtifactTab )
            {
                var spawnTabUI = Lance.Lobby.GetLobbyTabUI<Lobby_SpawnUI>();
                spawnTabUI.ChangeTab(type.ChangeToSpawnTab());
            }

            mIsFinish = true;
        }

        public override void OnFinish()
        {
            base.OnFinish();

            mParent.SetActiveTouchBlock(false);
        }
    }
}