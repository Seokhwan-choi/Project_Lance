using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System;


namespace Lance
{
    class Lobby_SkillCastUI : MonoBehaviour
    {
        bool mInDungeon;
        bool mInDemonicRealm;
        bool mInJousting;

        TextMeshProUGUI mTextPresetNum;
        SkillSlotUIManager mSkillSlotUIManager;
        public void Init()
        {
            var buttonChangePreset = gameObject.FindComponent<Button>("Button_ChangePreset");
            buttonChangePreset.SetButtonAction(OnChangePresetButton);

            mTextPresetNum = buttonChangePreset.gameObject.FindComponent<TextMeshProUGUI>("Text_PresetNum");

            var skillSlotListObj = gameObject.FindGameObject("SkillSlotUIList");

            mSkillSlotUIManager = new SkillSlotUIManager();
            mSkillSlotUIManager.Init(skillSlotListObj, "SkillSlotUI");

            Refresh();
        }
        
        public void OnUpdate(Player player)
        {
            if (mInDungeon || mInJousting)
                return;

            mSkillSlotUIManager.OnUpdate(player);
        }

        public void Refresh()
        {
            if (mInDungeon || mInDemonicRealm || mInJousting)
                return;

            mSkillSlotUIManager.Refresh();

            RefreshPresetUI();
        }

        public void OnStartStage(StageData stageData)
        {
            mInDungeon = stageData.type.IsDungeon();
            mInDemonicRealm = stageData.type.IsDemonicRealm();
            mInJousting = stageData.type.IsJousting();

            gameObject.SetActive(!mInDungeon && !mInDemonicRealm && !mInJousting);

            Refresh();
        }

        void OnChangePresetButton()
        {
            var popup = Lance.PopupManager.CreatePopup<Popup_PresetUI>();

            popup.Init();

            //int currPreset = Lance.Account.SkillInventory.GetCurrentPreset();
            //int nextPreset = currPreset + 1;

            //nextPreset = nextPreset >= Lance.GameData.SkillCommonData.presetMaxCount ? 0 : nextPreset;

            //Lance.GameManager.ChangeSkillPreset(nextPreset);
        }

        void RefreshPresetUI()
        {
            mTextPresetNum.text = $"{Lance.Account.SkillInventory.GetCurrentPreset() + 1}";
        }
    }
}