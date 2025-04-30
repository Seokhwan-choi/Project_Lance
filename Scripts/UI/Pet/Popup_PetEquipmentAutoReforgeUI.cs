using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Popup_PetEquipmentAutoReforgeUI : PopupBase
    {
        int mReforgeStep;
        PetEquipmentData mData;
        AutoReforge_ItemUI mAfterEquipmentItemUI;
        TextMeshProUGUI mTextReforgeStep;
        public void Init(string equipmentId, Action<int> onConfirmAutoReforge)
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_AutoReforge"));

            mData = Lance.GameData.PetEquipmentData.TryGet(equipmentId);

            var beforeEquipmentItemObj = gameObject.FindGameObject("BeforeEquipment");
            var beforeEquipmenItemUI = beforeEquipmentItemObj.GetOrAddComponent<AutoReforge_ItemUI>();
            beforeEquipmenItemUI.InitPetEquipment(equipmentId);

            var afterEquipmentItemObj = gameObject.FindGameObject("AfterEquipment");
            mAfterEquipmentItemUI = afterEquipmentItemObj.GetOrAddComponent<AutoReforge_ItemUI>();
            mAfterEquipmentItemUI.InitPetEquipment(equipmentId);

            mTextReforgeStep = gameObject.FindComponent<TextMeshProUGUI>("Text_TargetReforgeStep");

            SetReforgeStep(Lance.Account.GetPetEquipmentReforgeStep(equipmentId) + 1);

            // ¼ÒÅÁ È½¼ö ¹öÆ°
            var buttonMinus = gameObject.FindComponent<Button>("Button_Minus");
            buttonMinus.SetButtonAction(() => OnChangeReforgeStep(minus: true));
            var buttonPlus = gameObject.FindComponent<Button>("Button_Plus");
            buttonPlus.SetButtonAction(() => OnChangeReforgeStep(minus: false));

            var buttonMin = gameObject.FindComponent<Button>("Button_Min");
            buttonMin.SetButtonAction(() => SetReforgeStep(Lance.Account.GetPetEquipmentReforgeStep(equipmentId)));

            var buttonMax = gameObject.FindComponent<Button>("Button_Max");
            buttonMax.SetButtonAction(() => SetReforgeStep(DataUtil.GetPetEquipmentReforgeMaxStep(mData.grade)));

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
            mReforgeStep = Mathf.Clamp(reforgeStep, Mathf.Max(1, Lance.Account.GetPetEquipmentReforgeStep(mData.id)), DataUtil.GetPetEquipmentReforgeMaxStep(mData.grade));

            mTextReforgeStep.text = $"{mReforgeStep}";

            mAfterEquipmentItemUI.OnChangePetEquipmentReforgeStep(mReforgeStep);
        }
    }
}