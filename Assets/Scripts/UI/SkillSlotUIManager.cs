using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    class SkillSlotUIManager
    {
        List<SkillSlotUI> mSkillSlotList;
        public void Init(GameObject parent, string skillSlotName)
        {
            mSkillSlotList = new List<SkillSlotUI>();

            parent.AllChildObjectOff();

            ObscuredString[] ids = Lance.Account.SkillInventory.GetCurSkillsetIds(SkillType.Active);

            for (int i = 0; i < ids.Length; ++i)
            {
                string id = ids[i];
                int slot = i;

                var skillSlotObj = Util.InstantiateUI(skillSlotName, parent.transform);

                var skillSlotUI = skillSlotObj.GetOrAddComponent<SkillSlotUI>();
                skillSlotUI.Init(SkillType.Active, slot, OnSkillSlotSelect);

                if (i == 0)
                {
                    var guideActionTag = skillSlotObj.GetOrAddComponent<GuideActionTag>();

                    guideActionTag.Tag = GuideActionType.Highlight_FirstSkillCastSlot;
                }

                mSkillSlotList.Add(skillSlotUI);
            }
        }

        public void OnUpdate(Player player)
        {
            if (player == null)
                return;

            ObscuredString[] ids = Lance.Account.SkillInventory.GetCurSkillsetIds(SkillType.Active);
            if (ids != null)
            {
                for (int i = 0; i < ids.Length; ++i)
                {
                    string id = ids[i];
                    int slot = i;

                    if (mSkillSlotList.Count <= slot)
                        continue;

                    if (id.IsValid())
                    {
                        var data = DataUtil.GetSkillData(id);
                        if (data == null)
                            continue;

                        float remainCoolTime = player.SkillManager?.GetSkillRemainCoolTime(id) ?? float.MaxValue;
                        float coolTime = data.coolTime;

                        float coolTimeValue = coolTime == 0 ? 0 : remainCoolTime / coolTime;

                        mSkillSlotList[slot].UpdateCoolTime(remainCoolTime, coolTimeValue);
                    }
                }
            }
        }

        public void Refresh()
        {
            foreach (var slot in mSkillSlotList)
            {
                slot.Refresh();
            }
        }

        void OnSkillSlotSelect(int slot)
        {
            ObscuredString[] ids = Lance.Account.SkillInventory.GetCurSkillsetIds(SkillType.Active);
            if (ids.Length <= slot)
                return;

            if (Lance.Account.IsUnlockSkillSlot(slot) == false)
            {
                int slotUnlockLevel = Lance.GameData.SkillSlotUnlockData.unlockLevel[slot];

                StringParam param = new StringParam("unlockLevel", slotUnlockLevel);

                UIUtil.ShowSystemMessage(StringTableUtil.GetSystemMessage("UnlockLevel", param));

                return;
            }
                

            if (ids[slot].IsValid())
            {
                // 스킬 시전 예약
                Lance.GameManager.StageManager.Player.SkillManager.ReserveActiveSkill(ids[slot]);
            }
            else
            {
                if (Lance.GameManager.StageManager.InContents)
                    return;

                Lance.Lobby.ChangeTab(LobbyTab.Skill);

                var tab = Lance.Lobby.GetLobbyTabUI<Lobby_SkillInventoryUI>();

                tab.OnSelectEmptySkillSlot();
            }
        }
    }
}

