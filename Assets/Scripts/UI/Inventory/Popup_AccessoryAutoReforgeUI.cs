using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Popup_AccessoryAutoReforgeUI : PopupBase
    {
        int mReforgeStep;
        AccessoryData mData;
        AutoReforge_ItemUI mAfterEquipmentItemUI;
        TextMeshProUGUI mTextReforgeStep;
        public void Init(string accessoryId, Action<int> onConfirmAutoReforge)
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_AutoReforge"));

            mData = DataUtil.GetAccessoryData(accessoryId);

            var beforeEquipmentItemObj = gameObject.FindGameObject("BeforeEquipment");
            var beforeEquipmenItemUI = beforeEquipmentItemObj.GetOrAddComponent<AutoReforge_ItemUI>();
            beforeEquipmenItemUI.InitAccessory(accessoryId);

            var afterEquipmentItemObj = gameObject.FindGameObject("AfterEquipment");
            mAfterEquipmentItemUI = afterEquipmentItemObj.GetOrAddComponent<AutoReforge_ItemUI>();
            mAfterEquipmentItemUI.InitAccessory(accessoryId);

            mTextReforgeStep = gameObject.FindComponent<TextMeshProUGUI>("Text_TargetReforgeStep");

            SetReforgeStep(Lance.Account.GetAccessoryReforgeStep(accessoryId) + 1);

            // ¼ÒÅÁ È½¼ö ¹öÆ°
            var buttonMinus = gameObject.FindComponent<Button>("Button_Minus");
            buttonMinus.SetButtonAction(() => OnChangeReforgeStep(minus: true));
            var buttonPlus = gameObject.FindComponent<Button>("Button_Plus");
            buttonPlus.SetButtonAction(() => OnChangeReforgeStep(minus: false));

            var buttonMin = gameObject.FindComponent<Button>("Button_Min");
            buttonMin.SetButtonAction(() => SetReforgeStep(Lance.Account.GetAccessoryReforgeStep(accessoryId)));

            var buttonMax = gameObject.FindComponent<Button>("Button_Max");
            buttonMax.SetButtonAction(() => SetReforgeStep(DataUtil.GetAccessoryReforgeMaxStep(mData.grade)));

            var buttonCancel = gameObject.FindComponent<Button>("Button_Cancel");
            buttonCancel.SetButtonAction(() => Close());

            var buttonConfirm = gameObject.FindComponent<Button>("Button_Confirm");
            buttonConfirm.SetButtonAction(() =>
            {
                Close();

                onConfirmAutoReforge?.Invoke(mReforgeStep);
            });
        }

        void OnChangeReforgeStep(bool minus)
        {
            mReforgeStep = mReforgeStep + (minus ? -1 : 1);

            SetReforgeStep(mReforgeStep);
        }

        void SetReforgeStep(int reforgeStep)
        {
            mReforgeStep = Mathf.Clamp(reforgeStep, Mathf.Max(1, Lance.Account.GetAccessoryReforgeStep(mData.id)), DataUtil.GetAccessoryReforgeMaxStep(mData.grade));

            mTextReforgeStep.text = $"{mReforgeStep}";

            mAfterEquipmentItemUI.OnChangeAccessoryReforgeStep(mReforgeStep);
        }
    }
}