using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Lance
{
    class Popup_PresetUI : PopupBase
    {
        List<PresetInfoItemUI> mPresetInfoItemUIList;
        public void Init()
        {
            mPresetInfoItemUIList = new List<PresetInfoItemUI>();

            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_SettingPreset"));

            int presetMaxSlot = Lance.GameData.PresetData.Values.Count;

            for(int i = 0; i < presetMaxSlot; ++i)
            {
                int presetSlot = i + 1;

                var presetInfoItemUIObj = gameObject.FindGameObject($"PresetInfoItemUI_{presetSlot}");

                var presetInfoItemUI = presetInfoItemUIObj.GetOrAddComponent<PresetInfoItemUI>();
                presetInfoItemUI.Init(this, presetSlot);

                mPresetInfoItemUIList.Add(presetInfoItemUI);
            }
        }

        public void Refresh()
        {
            foreach(var itemUI in mPresetInfoItemUIList)
            {
                itemUI.Refresh();
            }
        }
    }

    class PresetInfoItemUI : MonoBehaviour
    {
        Popup_PresetUI mParent;
        int mPreset;
        PresetData mData;

        GameObject mUnlockObj;
        GameObject mLockObj;
        TextMeshProUGUI mTextPresetName;
        Image mImagePreview;
        Image mImageEmpty;
        Button mButtonApplyPreset;
        Button mSavePreset;

        Button mUnlockPreset;
        TextMeshProUGUI mTextUnlockPrice;
        public void Init(Popup_PresetUI parent, int preset)
        {
            mParent = parent;
            mPreset = preset;
            mData = Lance.GameData.PresetData.TryGet(mPreset);

            mUnlockObj = gameObject.FindGameObject("Unlock");
            mLockObj = gameObject.FindGameObject("Lock");
            mTextPresetName = mUnlockObj.FindComponent<TextMeshProUGUI>("Text_PresetName");
            var buttonChangePresetName = mUnlockObj.FindComponent<Button>("Button_ChangePresetName");
            buttonChangePresetName.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_ChangePresetNameUI>();

                popup.Init(preset, Refresh);
            });
            var buttonShowInfo = mUnlockObj.FindComponent<Button>("Button_ShowInfo");
            buttonShowInfo.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_PresetInfoUI>();

                popup.Init(preset);
            });
            mImageEmpty = mUnlockObj.FindComponent<Image>("Image_Empty");
            mImagePreview = mUnlockObj.FindComponent<Image>("Image_Preview");

            mButtonApplyPreset = mUnlockObj.FindComponent<Button>("Button_ApplyPreset");
            mButtonApplyPreset.SetButtonAction(OnApplyButton);
            mSavePreset = mUnlockObj.FindComponent<Button>("Button_SavePreset");
            mSavePreset.SetButtonAction(OnSaveButton);

            mUnlockPreset = mLockObj.FindComponent<Button>("Button_UnlockPreset");
            mUnlockPreset.SetButtonAction(OnUnlockButton);
            mTextUnlockPrice = mLockObj.FindComponent<TextMeshProUGUI>("Text_UnlockPrice");

            Refresh();
        }

        public void Refresh()
        {
            var info = Lance.Account.Preset.GetPresetInfo(mPreset);

            mTextPresetName.text = info.GetPresetName();

            bool isUnlocked = info.IsUnlocked();
            if (isUnlocked)
            {
                mUnlockObj.SetActive(true);
                mLockObj.SetActive(false);

                bool isEmpty = info.IsEmpty();
                if (isEmpty)
                {
                    mButtonApplyPreset.gameObject.SetActive(false);

                    mImageEmpty.gameObject.SetActive(true);
                    mImagePreview.gameObject.SetActive(false);
                }
                else
                {
                    mButtonApplyPreset.gameObject.SetActive(true);

                    mImageEmpty.gameObject.SetActive(false);
                    mImagePreview.gameObject.SetActive(true);

                    var costumes = info.GetEquippedCostumes();

                    var bodyCostume = costumes[(int)CostumeType.Body];
                    var costumeData = Lance.GameData.BodyCostumeData.TryGet(bodyCostume);
                    if (costumeData != null)
                    {
                        mImagePreview.sprite = Lance.Atlas.GetPlayerSprite(costumeData.uiSprite);
                    }
                }
            }
            else
            {
                mUnlockObj.SetActive(false);
                mLockObj.SetActive(true);

                int unlockPrice = mData.unlockPrice;
                bool isSatisfied = Lance.Account.Preset.IsUnlockedPreset(mData.requireUnlockPreset);
                bool isEnoughGem = Lance.Account.IsEnoughGem(unlockPrice);

                mUnlockPreset.SetActiveFrame(isEnoughGem && isSatisfied);
                mTextUnlockPrice.text = $"{mData.unlockPrice}";
                mTextUnlockPrice.SetColor(isEnoughGem ? Const.EnoughTextColor : Const.NotEnoughTextColor);
            }
        }

        void OnApplyButton()
        {
            string title = StringTableUtil.Get("Title_Confirm");

            var presetInfo = Lance.Account.Preset.GetPresetInfo(mPreset);
            var presetName = presetInfo.GetPresetName();

            StringParam presetNameParam = new StringParam("presetName", presetName);

            string desc = StringTableUtil.Get("Desc_ConfirmApplyPreset", presetNameParam);

            UIUtil.ShowConfirmPopup(title, desc, () =>
             {
                 if (presetInfo.IsUnlocked() == false)
                 {
                     UIUtil.ShowSystemErrorMessage("IsLockedPreset");

                     return;
                 }

                 Lance.Account.ApplyPreset(mPreset);

                 Lance.GameManager.OnChangePreset();          

                 var message = StringTableUtil.GetSystemMessage("ApplyPreset", presetNameParam);

                 UIUtil.ShowSystemMessage(message);
             }, null);
        }

        void OnSaveButton()
        {
            string title = StringTableUtil.Get("Title_Confirm");

            var presetInfo = Lance.Account.Preset.GetPresetInfo(mPreset);
            var presetName = presetInfo.GetPresetName();

            StringParam presetNameParam = new StringParam("presetName", presetName);

            string desc = StringTableUtil.Get("Desc_ConfirmSavePreset", presetNameParam);

            UIUtil.ShowConfirmPopup(title, desc, () =>
            {
                if (presetInfo.IsUnlocked() == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsLockedPreset");

                    return;
                }

                Lance.Account.SavePreset(mPreset);

                Refresh();

            }, null);
        }

        void OnUnlockButton()
        {
            var presetInfo = Lance.Account.Preset.GetPresetInfo(mPreset);

            if (presetInfo.IsUnlocked())
            {
                UIUtil.ShowSystemErrorMessage("AlreadyUnlockPreset");

                return;
            }

            bool isEnoughGem = Lance.Account.IsEnoughGem(mData.requireUnlockPreset);
            if (isEnoughGem == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughGem");

                return;
            }

            bool isSatisfied = Lance.Account.Preset.IsUnlockedPreset(mData.requireUnlockPreset);
            if (isSatisfied == false)
            {
                UIUtil.ShowSystemErrorMessage("RequirePrevPreset");

                return;
            }

            string title = StringTableUtil.Get("Title_Confirm");

            StringParam unlockPriceParam = new StringParam("unlockPrice", mData.unlockPrice);

            string desc = StringTableUtil.Get("Desc_ConfirmUnlockPreset", unlockPriceParam);

            UIUtil.ShowConfirmPopup(title, desc, () =>
            {
                Lance.Account.UnlockPreset(mPreset);

                Lance.Lobby.UpdateGemUI();

                mParent.Refresh();

            }, null);
        }
    }
}