using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class AbilityInfoUI : MonoBehaviour
    {
        string mId;
        Stature_AbilityTabUI mParent;
        AbilityInfoSlotUI mSlotUI;
        Button mButtonLevelUp;
        GameObject mRedDotObj;
        GameObject mLevelUpObj;
        GameObject mMaxLevelObj;
        TextMeshProUGUI mTextCurrentValue;
        TextMeshProUGUI mTextNextValue;
        TextMeshProUGUI mTextRequireAP;
        TextMeshProUGUI mTextTotalAP;
        public void Init(Stature_AbilityTabUI parent)
        {
            mParent = parent;

            var abilitySlot = gameObject.FindGameObject("Ability");

            mSlotUI = abilitySlot.GetOrAddComponent<AbilityInfoSlotUI>();
            mSlotUI.Init();

            mButtonLevelUp = gameObject.FindComponent<Button>("Button_LevelUp");
            mButtonLevelUp.SetButtonAction(OnLevelUpButton);

            mLevelUpObj = mButtonLevelUp.gameObject.FindGameObject("LevelUp");
            mMaxLevelObj = mButtonLevelUp.gameObject.FindGameObject("MaxLevel");

            mRedDotObj = mButtonLevelUp.gameObject.FindGameObject("RedDot");

            mTextCurrentValue = gameObject.FindComponent<TextMeshProUGUI>("Text_CurrentValue");
            mTextNextValue = gameObject.FindComponent<TextMeshProUGUI>("Text_NextValue");
            mTextRequireAP = gameObject.FindComponent<TextMeshProUGUI>("Text_RequireAP");
            mTextTotalAP = gameObject.FindComponent<TextMeshProUGUI>("Text_TotalAP");
        }

        public void ChangeId(string id)
        {
            mId = id;

            mSlotUI.ChangeId(id);

            Refresh();
        }

        public void Localize()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (mId.IsValid())
            {
                var data = Lance.GameData.AbilityData.TryGet(mId);
                if (data == null)
                    return;

                int level = Lance.Account.Ability.GetAbilityLevel(mId);
                int maxLevel = data.maxLevel;
                int nextLevel = Math.Min(maxLevel, level + 1);
                int requireAP = AbilityUtil.CalcRequireAP(mId, level);
                bool canLevelUp = Lance.Account.CanLevelUpAbility(mId);
                bool isMaxLevel = level == maxLevel;

                mLevelUpObj.SetActive(!isMaxLevel);
                mMaxLevelObj.SetActive(isMaxLevel);

                bool isPercentValue = data.statType.IsPercentType();
                double currentValue = AbilityUtil.CalcStatValue(mId, level);
                double nextValue = AbilityUtil.CalcStatValue(mId, nextLevel);

                string statName = StringTableUtil.Get($"Name_{data.statType}");
                mTextTotalAP.text = $"AP : {Lance.Account.ExpLevel.GetAP()}";
                mTextRequireAP.text = $"AP : {requireAP}";

                mTextCurrentValue.text = isPercentValue ?
                    $"{statName} : {currentValue * 100}%" :
                    $"{statName} : {currentValue}";
                mTextNextValue.text = isPercentValue ?
                    $"{statName} : {nextValue * 100}%" :
                    $"{statName} : {nextValue}";

                mButtonLevelUp.SetActiveFrame(canLevelUp);
                mRedDotObj.SetActive(canLevelUp);
            }
        }

        void OnLevelUpButton()
        {
            if ( Lance.GameManager.LevelUpAbility(mId) )
            {
                SoundPlayer.PlayLevelUp();

                Lance.ParticleManager.AquireUI("AbilityLevelUp", mSlotUI.ImageIconTm);

                mSlotUI.Refresh();

                Refresh();

                mParent.RefreshItems();

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
            }
        }
    }

    class AbilityInfoSlotUI : MonoBehaviour
    {
        string mId;
        Image mImageIcon;
        TextMeshProUGUI mTextLevel;
        Image mImageModal;
        public RectTransform ImageIconTm => mImageIcon.rectTransform;
        public void Init()
        {
            mImageIcon = gameObject.FindComponent<Image>("Image_Icon");
            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            mImageModal = gameObject.FindComponent<Image>("Image_Modal");
        }

        public void ChangeId(string id)
        {
            mId = id;

            Refresh();
        }

        public void Refresh()
        {
            if (mId.IsValid())
            {
                var data = Lance.GameData.AbilityData.TryGet(mId);
                if (data == null)
                    return;

                int level = Lance.Account.Ability.GetAbilityLevel(mId);
                int maxLevel = data.maxLevel;

                mImageIcon.sprite = Lance.Atlas.GetUISprite(data.sprite);
                mTextLevel.text = $"{level} / {maxLevel}";
                mImageModal.gameObject.SetActive(level <= 0);
            }
        }
    }
}