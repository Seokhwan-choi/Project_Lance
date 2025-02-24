using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;


namespace Lance
{
    class Lobby_SkillInventoryUI : LobbyTabUI
    {
        SkillInventoryTabUIManager mTabUIManager;
        public override void Init(LobbyTab tab)
        {
            base.Init(tab);

            mTabUIManager = new SkillInventoryTabUIManager();
            mTabUIManager.Init(gameObject);
        }

        public override void Localize()
        {
            mTabUIManager.Localize();
        }

        public override void OnEnter()
        {
            mTabUIManager.Refresh();
        }

        public override void OnUpdate()
        {
            mTabUIManager.OnUpdate();
        }

        public override void RefreshRedDots()
        {
            mTabUIManager.RefreshRedDots();
        }

        public void ChangeTab(SkillInventoryTab tab)
        {
            mTabUIManager.ChangeTab(tab);
            mTabUIManager.RefreshActiveTab();
        }

        public SkillInventory_SkillItemUI GetBestSkillItemUI()
        {
            return mTabUIManager.GetBestSkillItemUI();
        }

        public SkillInventory_SkillItemUI GetCanEquipBestSkillItemUI()
        {
            return mTabUIManager.GetCanEquipBestSkillItemUI();
        }

        public SkillInventory_SkillItemUI GetCanUpgradeSkillItemUI()
        {
            return mTabUIManager.GetCanUpgradeSkillItemUI();
        }

        public void OnSelectEmptySkillSlot()
        {
            SkillData bestData = null;
            Grade bestGrade = Grade.D;

            foreach (var data in Lance.GameData.ActiveSkillData.Values)
            {
                // ���� ������ �ְ�
                if (Lance.Account.SkillInventory.HaveSkill(data.type, data.id) == false)
                    continue;

                // ���� ���Կ� �����Ǿ� ���� �����鼭
                if (Lance.Account.SkillInventory.IsEquipped(data.type, data.id))
                    continue;

                // ����� ���� ���� ��ų�� ã�´�.
                if (data.grade > bestGrade)
                {
                    bestData = data;
                    bestGrade = data.grade;
                }
            }

            if (bestData == null)
                bestData = Lance.GameData.ActiveSkillData.Values.First();

            mTabUIManager.OnSelectedSkillItemUI(bestData.id);
        }
    }
}