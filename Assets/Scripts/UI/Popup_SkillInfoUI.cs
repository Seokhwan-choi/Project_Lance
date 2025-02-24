using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Popup_SkillInfoUI : PopupBase
    {
        SkillData mSkillData;
        SkillInventoryTabUI mParent;

        // 스킬 장착 슬롯
        SkillEquipSlotUIManager mEquipSlotManager;

        // 스킬 정보
        SkillInventory_SkillItemUI mSkillItemUI;
        TextMeshProUGUI mTextDesc;                  // 설명
        TextMeshProUGUI mTextCoolTime;              // 쿨타임

        // 스킬 조각
        TextMeshProUGUI mTextSkillPiece;

        // 강화 버튼 ( 스킬 사용 )
        GameObject mUpgradeBySkill;
        Button mButtonUpgrade;
        GameObject mUpgradeRedDot;
        TextMeshProUGUI mTextUpgradeCount;

        // 강화 버튼 ( 조각 사용 )
        GameObject mUpgradeBySkillPiece;
        Button mButtonUpgrade2;
        GameObject mUpgradeRedDot2;
        TextMeshProUGUI mTextUpgradeCount2;

        // 강화 버튼 ( 조각 사용 )
        GameObject mUpgradeBySkillPieceX10;
        Button mButtonUpgrade2X10;
        GameObject mUpgradeRedDot2X10;
        TextMeshProUGUI mTextUpgradeCount2X10;

        // 분해 버튼
        Button mButtonDismantle;
        TextMeshProUGUI mTextDismantleAmount;

        Button mButtonAllDismantle;
        TextMeshProUGUI mTextAllDismantleAmount;

        // 장착 & 해제
        Button mButtonEquip;
        Button mButtonUnEquip;

        // 효과
        StatValueInfoUI mSkillStatValueInfoUI;

        public SkillInventoryTabUI Parent => mParent;
        public void Init(SkillInventoryTabUI parent, string id)
        {
            mParent = parent;
            mSkillData = DataUtil.GetSkillData(id);

            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get($"Name_{mSkillData.id}"));

            var presetObj = gameObject.FindGameObject("Preset");

            mEquipSlotManager = presetObj.GetOrAddComponent<SkillEquipSlotUIManager>();
            mEquipSlotManager.Init(this, parent.Tab.ChangeToSkillType());

            var skillItemObj = gameObject.FindGameObject("Skill");
            mSkillItemUI = skillItemObj.GetOrAddComponent<SkillInventory_SkillItemUI>();
            mSkillItemUI.Init(mSkillData.id);
            mSkillItemUI.SetActiveRedDot(false);
            mSkillItemUI.SetSelected(false);
            mSkillItemUI.SetActiveIsLocked(false);

            mTextDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_Desc");
            mTextCoolTime = gameObject.FindComponent<TextMeshProUGUI>("Text_CoolTimeValue");
            mTextSkillPiece = gameObject.FindComponent<TextMeshProUGUI>("Text_SkillPieceAmount");

            // 효과
            GameObject valueInfoObj = gameObject.FindGameObject("ValueInfo");
            mSkillStatValueInfoUI = valueInfoObj.GetOrAddComponent<StatValueInfoUI>();
            mSkillStatValueInfoUI.InitSkill(mSkillData.id);

            var buttonsObj = gameObject.FindGameObject("Buttons");

            // 강화 ( 스킬 사용 )
            mUpgradeBySkill = buttonsObj.FindGameObject("UpgradeBySkill");
            mButtonUpgrade = buttonsObj.FindComponent<Button>("Button_UpgradeBySkill");
            mButtonUpgrade.SetButtonAction(() => OnUpgradeButton(1));
            mUpgradeRedDot = mButtonUpgrade.gameObject.FindGameObject("RedDot");
            mTextUpgradeCount = mButtonUpgrade.FindComponent<TextMeshProUGUI>("Text_UpgradeCount");

            // 강화 ( 조각 사용 )
            mUpgradeBySkillPiece = buttonsObj.FindGameObject("UpgradeBySkillPiece");
            mButtonUpgrade2 = buttonsObj.FindComponent<Button>("Button_UpgradeBySkillPiece");
            mButtonUpgrade2.SetButtonAction(() => OnUpgradeButton(1));
            mUpgradeRedDot2 = mButtonUpgrade2.gameObject.FindGameObject("RedDot");
            mTextUpgradeCount2 = mButtonUpgrade2.FindComponent<TextMeshProUGUI>("Text_Amount");

            // 강화 ( 조각 사용 )
            mUpgradeBySkillPieceX10 = buttonsObj.FindGameObject("UpgradeBySkillPieceX10");
            mButtonUpgrade2X10 = buttonsObj.FindComponent<Button>("Button_UpgradeBySkillPieceX10");
            mButtonUpgrade2X10.SetButtonAction(() => OnUpgradeButton(10));
            mUpgradeRedDot2X10 = mButtonUpgrade2X10.gameObject.FindGameObject("RedDot");
            mTextUpgradeCount2X10 = mButtonUpgrade2X10.FindComponent<TextMeshProUGUI>("Text_Amount");

            // 분해 &일괄 분해
            mButtonDismantle = buttonsObj.FindComponent<Button>("Button_Dismantle");
            mButtonDismantle.SetButtonAction(OnDismantleButton);
            mTextDismantleAmount = mButtonDismantle.FindComponent<TextMeshProUGUI>("Text_Amount");

            mButtonAllDismantle = buttonsObj.FindComponent<Button>("Button_AllDismantle");
            mButtonAllDismantle.SetButtonAction(OnDismantleAllButton);
            mTextAllDismantleAmount = mButtonAllDismantle.FindComponent<TextMeshProUGUI>("Text_Amount");

            // 장착 & 해제
            mButtonEquip = buttonsObj.FindComponent<Button>("Button_Equip");
            mButtonEquip.SetButtonAction(OnEquipButton);
            mButtonUnEquip = buttonsObj.FindComponent<Button>("Button_UnEquip");
            mButtonUnEquip.SetButtonAction(OnUnEquipButton);

            // 스킬 초기화
            var buttonResetSkill = buttonsObj.FindComponent<Button>("Button_ResetSkill");
            buttonResetSkill.SetButtonAction(() =>
            {
                var skillInst = Lance.Account.GetSkill(mSkillData.type, id);
                if (skillInst == null)
                {
                    UIUtil.ShowSystemErrorMessage("HaveNotSkill");

                    return;
                }

                int level = skillInst.GetLevel();
                if (level <= 1)
                {
                    UIUtil.ShowSystemErrorMessage("CanNotResetSkill");

                    return;
                }

                string title = StringTableUtil.Get("Title_Confirm");

                StringParam descParam = new StringParam("skillName", StringTableUtil.GetName(id));

                int totalPiece = DataUtil.CalcTotalUsedSkillPiece(id, level);

                descParam.AddParam("amount", totalPiece);

                string desc = StringTableUtil.GetDesc("ResetSkill", descParam);

                UIUtil.ShowConfirmPopup(title, desc, () =>
                {
                    Lance.GameManager.ResetSkill(mSkillData.type, id);

                    mParent.Refresh();

                    Refresh();

                    Lance.Lobby.RefreshTabRedDot(LobbyTab.Skill);
                }, null);
            });

            if (Lance.LocalSave.IsNewSkill(mSkillData.id))
            {
                Lance.LocalSave.AddGetSkill(mSkillData.id);

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Skill);
            }

            Refresh();
        }

        public override void OnClose()
        {
            base.OnClose();

            Lance.GameManager.UpdatePlayerStat();

            Lance.Lobby.RefreshCollectionRedDot();
        }

        //public void SetId(string id)
        //{
        //    mSkillData.id = id;

        //    mSkillItemUI.ChangeId(id);

        //    SetTitleText(StringTableUtil.Get($"Name_{mSkillData.id}"));

        //    if (Lance.LocalSave.IsNewSkill(mSkillData.id))
        //    {
        //        Lance.LocalSave.AddGetSkill(mSkillData.id);

        //        Lance.Lobby.RefreshTabRedDot(LobbyTab.Skill);
        //    }

        //    Refresh();
        //}

        public void Refresh()
        {
            if (mSkillData == null)
                return;

            // 스킬 아이콘 설정
            mSkillItemUI.Refresh();
            mSkillItemUI.SetActiveRedDot(false);
            bool isEquipped = Lance.Account.IsEquippedSkill(mSkillData.type, mSkillData.id);
            
            if (isEquipped)
            {
                mSkillItemUI.SetActiveModal(false);
                mSkillItemUI.SetActiveIsEquipped(false);
            }
            
            // 스킬 효과
            mSkillStatValueInfoUI.Refresh();
            // 스킬 설명
            int level = Lance.Account.GetSkillLevel(mSkillData.type, mSkillData.id);
            StringParam param = new StringParam("value", $"{DataUtil.GetSkillValue(mSkillData.id, level) * 100:F2}");
            param.AddParam("targetCount", $"{mSkillData.targetCount}");
            param.AddParam("duration", $"{mSkillData.duration}");
            param.AddParam("atkDelay", $"{mSkillData.atkDelay}");
            param.AddParam("conditionValue", $"{mSkillData.conditionValue * 100:F2}");
            param.AddParam("stackCount", $"{mSkillData.stackCount}");

            mTextDesc.text = StringTableUtil.Get($"SkillDesc_{mSkillData.id}", param);
            mTextCoolTime.text = $"{mSkillData.coolTime:F2}s";
            mTextSkillPiece.text = $"{Lance.Account.Currency.GetSkillPiece()}";

            // 강화
            bool canUpgrade = Lance.Account.CanUpgradeSkill(mSkillData.type, mSkillData.id, 1);
            bool isEnoughUpgradeRequire = Lance.Account.IsEnoughSkillUpgradeRequireCount(mSkillData.type, mSkillData.id, 1);

            // 강화 x10
            bool canUpgradeX10 = Lance.Account.CanUpgradeSkill(mSkillData.type, mSkillData.id, 10);
            bool isEnoughUpgradeRequireX10 = Lance.Account.IsEnoughSkillUpgradeRequireCount(mSkillData.type, mSkillData.id, 10);

            level = Math.Max(level, 1);

            int skillCount = Lance.Account.GetSkillCount(mSkillData.type, mSkillData.id);
            int upgradeRequireCount = DataUtil.GetSkillLevelUpRequireCount(mSkillData.id, level, 1);
            int upgradeRequireCountX10 = DataUtil.GetSkillLevelUpRequireCount(mSkillData.id, level, 10);

            mUpgradeBySkill.SetActive(mSkillData.levelUpMaterial == SkillLevelUpMaterial.Skill);
            mUpgradeBySkillPiece.SetActive(mSkillData.levelUpMaterial != SkillLevelUpMaterial.Skill);
            mUpgradeBySkillPieceX10.SetActive(mSkillData.levelUpMaterial != SkillLevelUpMaterial.Skill);

            if (mSkillData.levelUpMaterial == SkillLevelUpMaterial.Skill)
            {
                mTextUpgradeCount.text = $"({skillCount}/{upgradeRequireCount})";
                mButtonUpgrade.SetActiveFrame(canUpgrade);
                mUpgradeRedDot.SetActive(canUpgrade);
            }
            else
            {
                mButtonUpgrade2.SetActiveFrame(canUpgrade);
                mTextUpgradeCount2.text = $"{upgradeRequireCount}";
                mTextUpgradeCount2.SetColor(isEnoughUpgradeRequire ? Const.EnoughTextColor : Const.NotEnoughTextColor);
                mUpgradeRedDot2.SetActive(canUpgrade);

                mButtonUpgrade2X10.SetActiveFrame(canUpgradeX10);
                mTextUpgradeCount2X10.text = $"{upgradeRequireCountX10}";
                mTextUpgradeCount2X10.SetColor(isEnoughUpgradeRequireX10 ? Const.EnoughTextColor : Const.NotEnoughTextColor);
                mUpgradeRedDot2X10.SetActive(canUpgradeX10);
            }

            // 분해
            //bool haveLimitBreakSkill = Lance.Account.HaveLimitBreakSkill();
            bool canDismantle = Lance.Account.CanDismantleSkill(mSkillData.type, mSkillData.id);

            mButtonDismantle.SetActiveFrame(canDismantle, btnActive:"Button_Green");
            mTextDismantleAmount.text = $"{DataUtil.GetSkillPieceAmount(mSkillData.grade, 1)}";
            mTextDismantleAmount.SetColor(canDismantle ? Const.EnoughTextColor : Const.NotEnoughTextColor);

            mButtonAllDismantle.SetActiveFrame(canDismantle, btnActive:"Button_Green");
            mTextAllDismantleAmount.text = $"{DataUtil.GetSkillPieceAmount(mSkillData.grade, skillCount)}";
            mTextAllDismantleAmount.SetColor(canDismantle ? Const.EnoughTextColor : Const.NotEnoughTextColor);

            // 장착
            mButtonEquip.gameObject.SetActive(!isEquipped);
            mButtonUnEquip.gameObject.SetActive(isEquipped);

            if (isEquipped == false)
            {
                var result = Lance.Account.GetEmptySkillSlot(mSkillData.type);

                bool haveSkill = Lance.Account.HaveSkill(mSkillData.type, mSkillData.id);

                mButtonEquip.SetActiveFrame(result.haveEmpty && haveSkill);
            }

            mEquipSlotManager.Refresh();
        }

        void OnUpgradeButton(int count)
        {
            if (Lance.GameManager.LevelUpSkill(mSkillData.id, count))
            {
                SoundPlayer.PlaySkillLevelUp();

                Lance.ParticleManager.AquireUI("SkillLevelUp", mSkillItemUI.RectTm, Vector3.up * 24f);

                mParent.Parent.Refresh();

                Refresh();
            }
        }

        void OnDismantleButton()
        {
            if (Lance.GameManager.DismantleSkill(mSkillData.id, 1))
            {
                SoundPlayer.PlaySkillLevelUp();

                Lance.ParticleManager.AquireUI("SkillLevelUp", mSkillItemUI.RectTm, Vector3.up * 24f);

                mParent.Parent.Refresh();

                Refresh();
            }
        }

        void OnDismantleAllButton()
        {
            int skillCount = Lance.Account.SkillInventory.GetSkillCount(mSkillData.type, mSkillData.id);

            if (Lance.GameManager.DismantleSkill(mSkillData.id, skillCount))
            {
                SoundPlayer.PlaySkillLevelUp();

                Lance.ParticleManager.AquireUI("SkillLevelUp", mSkillItemUI.RectTm, Vector3.up * 24f);

                mParent.Parent.Refresh();

                Refresh();
            }
        }

        void OnUnEquipButton()
        {
            if (mSkillData == null)
                return;

            if (Lance.Account.IsEquippedSkill(mSkillData.type, mSkillData.id) == false)
                return;

            bool haveSkill = Lance.Account.HaveSkill(mSkillData.type, mSkillData.id);
            if (haveSkill == false)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotSkill");

                return;
            }

            Lance.GameManager.UnEquipSkill(mSkillData.type, mSkillData.id);

            SoundPlayer.PlaySkillUnEquip();

            mParent.Refresh();

            Refresh();
        }

        void OnEquipButton()
        {
            if (mSkillData == null)
                return;

            bool haveSkill = Lance.Account.HaveSkill(mSkillData.type, mSkillData.id);
            if (haveSkill == false)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotSkill");

                return;
            }

            var result = Lance.Account.GetEmptySkillSlot(mSkillData.type);
            if (result.haveEmpty == false)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotEmptySkillSlot");

                return;
            }

            Lance.GameManager.EquipSkill(mSkillData.type, result.slot, mSkillData.id);

            SoundPlayer.PlaySkillEquip();

            mParent.Refresh();

            Refresh();
        }
    }
}