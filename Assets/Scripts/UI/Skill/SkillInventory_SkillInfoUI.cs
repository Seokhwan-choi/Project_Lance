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

//        // 스킬 정보
//        SkillInventory_SkillItemUI mSkillItemUI;
//        TextMeshProUGUI mTextName;                  // 이름
//        TextMeshProUGUI mTextDesc;                  // 설명
//        TextMeshProUGUI mTextCoolTime;              // 쿨타임
//        TextMeshProUGUI mTextUpgradeCount;          // 강화

//        // 강화 버튼
//        Button mButtonUpgrade;
//        GameObject mUpgradeRedDot;

//        // 장착 & 해제
//        Button mButtonEquip;
//        Button mButtonUnEquip;

//        // 효과
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

//            // 효과
//            GameObject valueInfoObj = gameObject.FindGameObject("ValueInfo");
//            mSkillStatValueInfoUI = valueInfoObj.GetOrAddComponent<StatValueInfoUI>();
//            mSkillStatValueInfoUI.InitSkill(mId);

//            // 강화
//            mButtonUpgrade = gameObject.FindComponent<Button>("Button_Upgrade");
//            mButtonUpgrade.SetButtonAction(OnUpgradeButton);

//            var guideTag = mButtonUpgrade.GetOrAddComponent<GuideActionTag>();
//            guideTag.Tag = skillType.ChangeToGuideActionUpgradeSkillButtonType();

//            mUpgradeRedDot = mButtonUpgrade.gameObject.FindGameObject("RedDot");
//            mTextUpgradeCount = gameObject.FindComponent<TextMeshProUGUI>("Text_UpgradeCount");

//            // 장착 & 해제
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

//            // 스킬 아이콘 설정
//            mSkillItemUI.Refresh();
//            mSkillItemUI.SetActiveModal(false);
//            mSkillItemUI.SetActiveIsEquipped(false);
//            // 스킬 효과
//            mSkillStatValueInfoUI.Refresh();
//            // 스킬 이름
//            mTextName.text = StringTableUtil.Get($"Name_{mId}");
//            // 스킬 설명
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