//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Linq;
//using System;


//namespace Lance
//{
//    class SkillInventory_SkillInfoUI : MonoBehaviour
//    {
//        string mId;
//        SkillInventoryTabUI mParent;

//        // ��ų ����
//        SkillInventory_SkillItemUI mSkillItemUI;
//        TextMeshProUGUI mTextName;                  // �̸�
//        TextMeshProUGUI mTextDesc;                  // ����
//        TextMeshProUGUI mTextCoolTime;              // ��Ÿ��
//        TextMeshProUGUI mTextUpgradeCount;          // ��ȭ

//        // ��ȭ ��ư
//        Button mButtonUpgrade;
//        GameObject mUpgradeRedDot;

//        // ���� & ����
//        Button mButtonEquip;
//        Button mButtonUnEquip;

//        // ȿ��
//        StatValueInfoUI mSkillStatValueInfoUI;
//        public string Id => mId;
//        public void Init(SkillInventoryTabUI parent)
//        {
//            SkillType skillType = parent.Tab.ChangeToSkillType();

//            mParent = parent;
//            mId = DataUtil.GetFirstSkillData(skillType)?.id ?? string.Empty;

//            var skillItemObj = gameObject.FindGameObject("Skill");
//            mSkillItemUI = skillItemObj.GetOrAddComponent<SkillInventory_SkillItemUI>();
//            mSkillItemUI.Init(mId);
//            mSkillItemUI.SetSelected(false);

//            mTextName = gameObject.FindComponent<TextMeshProUGUI>("Text_SkillName");
//            mTextDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_Info");
//            mTextCoolTime = gameObject.FindComponent<TextMeshProUGUI>("Text_CoolTimeValue");

//            // ȿ��
//            GameObject valueInfoObj = gameObject.FindGameObject("ValueInfo");
//            mSkillStatValueInfoUI = valueInfoObj.GetOrAddComponent<StatValueInfoUI>();
//            mSkillStatValueInfoUI.InitSkill(mId);

//            // ��ȭ
//            mButtonUpgrade = gameObject.FindComponent<Button>("Button_Upgrade");
//            mButtonUpgrade.SetButtonAction(OnUpgradeButton);

//            var guideTag = mButtonUpgrade.GetOrAddComponent<GuideActionTag>();
//            guideTag.Tag = skillType.ChangeToGuideActionUpgradeSkillButtonType();

//            mUpgradeRedDot = mButtonUpgrade.gameObject.FindGameObject("RedDot");
//            mTextUpgradeCount = gameObject.FindComponent<TextMeshProUGUI>("Text_UpgradeCount");

//            // ���� & ����
//            mButtonEquip = gameObject.FindComponent<Button>("Button_Equip");
//            mButtonEquip.SetButtonAction(OnEquipButton);
//            mButtonUnEquip = gameObject.FindComponent<Button>("Button_UnEquip");
//            mButtonUnEquip.SetButtonAction(OnUnEquipButton);

//            var guideTag2 = mButtonEquip.GetOrAddComponent<GuideActionTag>();
//            guideTag2.Tag = skillType.ChangeToGuideActionEquipSkillButtonType();

//            Refresh();
//        }

//        //public void SetId(string id)
//        //{
//        //    mId = id;

//        //    mSkillItemUI.ChangeId(id);
//        //    mSkillStatValueInfoUI.ChangeId(id);

//        //    Refresh();
//        //}

//        public void Refresh()
//        {
//            if (mId.IsValid() == false)
//                return;

//            SkillData data = DataUtil.GetSkillData(mId);
//            if (data == null)
//                return;

//            // ��ų ������ ����
//            mSkillItemUI.Refresh();
//            mSkillItemUI.SetActiveModal(false);
//            mSkillItemUI.SetActiveIsEquipped(false);
//            // ��ų ȿ��
//            mSkillStatValueInfoUI.Refresh();
//            // ��ų �̸�
//            mTextName.text = StringTableUtil.Get($"Name_{mId}");
//            // ��ų ����
//            int level = Lance.Account.GetSkillLevel(data.type, mId);
//            StringParam param = new StringParam("value", $"{DataUtil.GetSkillValue(mId, level) * 100:F2}");
//            param.AddParam("targetCount", $"{data.targetCount}");
//            param.AddParam("duration", $"{data.duration}");
//            param.AddParam("atkDelay", $"{data.atkDelay}");
//            param.AddParam("conditionValue", $"{data.conditionValue * 100:F2}");
//            param.AddParam("stackCount", $"{data.stackCount}");

//            mTextDesc.text = StringTableUtil.Get($"SkillDesc_{mId}", param);
//            mTextCoolTime.text = $"{data.coolTime:F2}s";

//            level = Math.Max(level, 1);
//            mTextUpgradeCount.text = $"({Lance.Account.GetSkillCount(data.type, mId)}/{DataUtil.GetSkillLevelUpRequireCount(mId, level)})";

//            bool canUpgrade = Lance.Account.CanUpgradeSkill(data.type, mId);

//            mButtonUpgrade.SetActiveFrame(canUpgrade);
//            mUpgradeRedDot.SetActive(canUpgrade);

//            bool isEquipped = Lance.Account.IsEquippedSkill(data.type, mId);

//            mButtonEquip.gameObject.SetActive(!isEquipped);
//            mButtonUnEquip.gameObject.SetActive(isEquipped);

//            if (isEquipped == false)
//            {
//                var result = Lance.Account.GetEmptySkillSlot(data.type);

//                mButtonEquip.SetActiveFrame(result.haveEmpty);
//            }
//        }

//        void OnUpgradeButton()
//        {
//            if (Lance.GameManager.LevelUpSkill(mId))
//            {
//                SoundPlayer.PlaySkillLevelUp();

//                Lance.ParticleManager.AquireUI("SkillLevelUp", mSkillItemUI.RectTm, Vector3.up * 24f);

//                mParent.Parent.Refresh();
//            }
//        }

//        void OnUnEquipButton()
//        {
//            if (mId.IsValid() == false)
//                return;

//            var data = DataUtil.GetSkillData(mId);
//            if (data == null)
//                return;

//            if (Lance.Account.IsEquippedSkill(data.type, mId) == false)
//                return;

//            bool haveSkill = Lance.Account.HaveSkill(data.type, mId);
//            if (haveSkill == false)
//            {
//                UIUtil.ShowSystemErrorMessage("HaveNotSkill");

//                return;
//            }

//            Lance.GameManager.UnEquipSkill(data.type, mId);

//            SoundPlayer.PlaySkillUnEquip();

//            mParent.Refresh();
//        }

//        void OnEquipButton()
//        {
//            if (mId.IsValid() == false)
//                return;

//            var data = DataUtil.GetSkillData(mId);
//            if (data == null)
//                return;

//            bool haveSkill = Lance.Account.HaveSkill(data.type, mId);
//            if (haveSkill == false)
//            {
//                UIUtil.ShowSystemErrorMessage("HaveNotSkill");

//                return;
//            }

//            var result = Lance.Account.GetEmptySkillSlot(data.type);
//            if (result.haveEmpty == false)
//            {
//                UIUtil.ShowSystemErrorMessage("HaveNotEmptySkillSlot");

//                return;
//            }

//            Lance.GameManager.EquipSkill(data.type, result.slot, mId);

//            SoundPlayer.PlaySkillEquip();

//            mParent.Refresh();
//        }
//    }
//}