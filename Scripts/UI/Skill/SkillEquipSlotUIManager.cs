using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace Lance
{
    class SkillEquipSlotUIManager : MonoBehaviour
    {
        int mSelectedPreset;
        List<SkillEquipSlotUI> mEquipSlotUIList;
        List<ButtonChangePresetUI> mButtonChangePresetUIList;
        Popup_SkillInfoUI mParent;
        public void Init(Popup_SkillInfoUI parent, SkillType skillType)
        {
            mParent = parent;
            mSelectedPreset = Lance.Account.SkillInventory.GetCurrentPreset();

            mEquipSlotUIList = new List<SkillEquipSlotUI>();
            mButtonChangePresetUIList = new List<ButtonChangePresetUI>();

            var equipSlotList = gameObject.FindGameObject("SkillEquipSlotList");
            equipSlotList.AllChildObjectOff();

            for (int i = 0; i < Lance.GameData.SkillCommonData.skillMaxSlot; ++i)
            {
                int slot = i;

                var equipSlotObj = Util.InstantiateUI("SkillEquipSlotUI", equipSlotList.transform);

                var equipSlotUI = equipSlotObj.GetOrAddComponent<SkillEquipSlotUI>();

                equipSlotUI.Init(skillType, slot, OnSelectEquipSlot, OnUnEquipButton);

                mEquipSlotUIList.Add(equipSlotUI);
            }

            var presetButtons = gameObject.FindGameObject("PresetButtons");
            presetButtons.AllChildObjectOff();

            for (int i = 0; i < Lance.GameData.SkillCommonData.presetMaxCount; ++i)
            {
                int preset = i;

                var buttonPresetObj = Util.InstantiateUI("Button_ChangePreset", presetButtons.transform);

                var buttonPresetUI = buttonPresetObj.GetOrAddComponent<ButtonChangePresetUI>();

                buttonPresetUI.Init(preset, OnChangePreset);

                mButtonChangePresetUIList.Add(buttonPresetUI);
            }

            void OnChangePreset(int preset)
            {
                if (Lance.GameManager.ChangeSkillPreset(preset))
                {
                    mSelectedPreset = Lance.Account.SkillInventory.GetCurrentPreset();

                    Refresh();
                }
            }
        }

        public void Refresh()
        {
            foreach (var equipSlotUI in mEquipSlotUIList)
            {
                equipSlotUI.Refresh();
            }

            foreach (var buttonChangePreset in mButtonChangePresetUIList)
            {
                buttonChangePreset.SetActiveSprite(buttonChangePreset.Preset == mSelectedPreset);
            }
        }

        void OnSelectEquipSlot(int slot)
        {
            //if (mEquipSlotUIList.Count <= slot)
            //    return;

            //var equipSlotUI = mEquipSlotUIList[slot];
            //if (equipSlotUI.Id.IsValid())
            //{
            //    mParent.SetId(equipSlotUI.Id);
            //}
        }

        void OnUnEquipButton()
        {
            Refresh();

            mParent.Refresh();

            mParent.Parent.Refresh();
        }
    }

    class SkillEquipSlotUI : MonoBehaviour
    {
        SkillType mType;
        int mSlot;
        SkillSlotUI mSkillSlotUI;
        Button mButtonUnEquip;
        public string Id => mSkillSlotUI.Id;
        public void Init(SkillType skillType, int slot, Action<int> onButton, Action onUnEquipButton)
        {
            mType = skillType;
            mSlot = slot;

            var skillSlotObj = gameObject.FindGameObject("SkillSlotUI");
            mSkillSlotUI = skillSlotObj.GetOrAddComponent<SkillSlotUI>();
            mSkillSlotUI.Init(skillType, slot, onButton);

            mButtonUnEquip = gameObject.FindComponent<Button>("Button_UnEquip");
            mButtonUnEquip.SetButtonAction(() =>
            {
                OnUnEquipButton();

                onUnEquipButton?.Invoke();
            });

            mButtonUnEquip.gameObject.SetActive(mSkillSlotUI.Id.IsValid());
        }

        public void Refresh()
        {
            mSkillSlotUI.Refresh();

            mButtonUnEquip.gameObject.SetActive(mSkillSlotUI.Id.IsValid());
        }

        void OnUnEquipButton()
        {
            if (mSkillSlotUI.Id.IsValid())
            {
                Lance.GameManager.UnEquipSkill(mType, mSlot);

                SoundPlayer.PlaySkillUnEquip();

                Refresh();
            }
        }
    }

    class ButtonChangePresetUI : MonoBehaviour
    {
        int mPreset;
        Image mImageBackground;
        public int Preset => mPreset;
        public void Init(int preset, Action<int> onChangePreset)
        {
            mPreset = preset;

            mImageBackground = GetComponent<Image>();

            var button = GetComponent<Button>();
            button.SetButtonAction(() => onChangePreset.Invoke(preset));

            var textPreset = gameObject.FindComponent<TextMeshProUGUI>("Text_PresetNum");
            textPreset.text = $"{preset + 1}";
        }

        public void SetActiveSprite(bool isActive)
        {
            mImageBackground.sprite = Lance.Atlas.GetUISprite(isActive ? "Button_Skill_Slot_Active" : "Button_Skill_Slot_Inactive");
        }
    }
}