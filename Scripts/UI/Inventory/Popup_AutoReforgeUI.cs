using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class Popup_AutoReforgeUI : PopupBase
    {
        int mReforgeStep;
        EquipmentData mData;
        AutoReforge_ItemUI mAfterEquipmentItemUI;
        TextMeshProUGUI mTextReforgeStep;
        public void Init(string equipmentId, Action<int> onConfirmAutoReforge)
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_AutoReforge"));

            mData = DataUtil.GetEquipmentData(equipmentId);

            var beforeEquipmentItemObj = gameObject.FindGameObject("BeforeEquipment");
            var beforeEquipmenItemUI = beforeEquipmentItemObj.GetOrAddComponent<AutoReforge_ItemUI>();
            beforeEquipmenItemUI.InitEquipment(equipmentId);

            var afterEquipmentItemObj = gameObject.FindGameObject("AfterEquipment");
            mAfterEquipmentItemUI = afterEquipmentItemObj.GetOrAddComponent<AutoReforge_ItemUI>();
            mAfterEquipmentItemUI.InitEquipment(equipmentId);

            mTextReforgeStep = gameObject.FindComponent<TextMeshProUGUI>("Text_TargetReforgeStep");

            SetReforgeStep(Lance.Account.GetEquipmentReforgeStep(equipmentId) + 1);
            
            // ¼ÒÅÁ È½¼ö ¹öÆ°
            var buttonMinus = gameObject.FindComponent<Button>("Button_Minus");
            buttonMinus.SetButtonAction(() => OnChangeReforgeStep(minus: true));
            var buttonPlus = gameObject.FindComponent<Button>("Button_Plus");
            buttonPlus.SetButtonAction(() => OnChangeReforgeStep(minus: false));

            var buttonMin = gameObject.FindComponent<Button>("Button_Min");
            buttonMin.SetButtonAction(() => SetReforgeStep(Lance.Account.GetEquipmentReforgeStep(equipmentId)));

            var buttonMax = gameObject.FindComponent<Button>("Button_Max");
            buttonMax.SetButtonAction(() => SetReforgeStep(DataUtil.GetEquipmentReforgeMaxStep(mData.grade)));

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
            mReforgeStep = Mathf.Clamp(reforgeStep, Mathf.Max(1,Lance.Account.GetEquipmentReforgeStep(mData.id)), DataUtil.GetEquipmentReforgeMaxStep(mData.grade));

            mTextReforgeStep.text = $"{mReforgeStep}";

            mAfterEquipmentItemUI.OnChangeEquipmentReforgeStep(mReforgeStep);
        }
    }

    class AutoReforge_ItemUI : MonoBehaviour
    {
        string mEquipmentId;
        string mPetEquipmentId;
        string mAccessoryId;
        TextMeshProUGUI mTextLevel;
        TextMeshProUGUI mTextReforgeStep;

        public void InitEquipment(string equipmentId)
        {
            mEquipmentId = equipmentId;

            var data = DataUtil.GetEquipmentData(equipmentId);
            if (data != null)
            {
                var imageItem = gameObject.FindComponent<Image>("Image_Item");
                imageItem.sprite = Lance.Atlas.GetItemSlotUISprite(data.sprite);

                var gradeBackground = gameObject.FindComponent<Image>("Image_GradeBackground");
                gradeBackground.sprite = Lance.Atlas.GetFrameGrade(data.grade);

                var gradeIcon = gameObject.FindComponent<Image>("Image_Grade");
                gradeIcon.sprite = Lance.Atlas.GetIconGrade(data.grade);

                var subGradeManager = new SubGradeManager();
                subGradeManager.Init(gameObject.FindGameObject("SubGrade"));

                subGradeManager.SetSubGrade(data.subGrade);

                mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
                SetLevel(Lance.Account.GetEquipmentLevel(equipmentId));

                mTextReforgeStep = gameObject.FindComponent<TextMeshProUGUI>("Text_ReforgeStep");
                SetReforgeStep(Lance.Account.GetEquipmentReforgeStep(equipmentId));
            }
        }

        public void InitPetEquipment(string petEquipmentId)
        {
            mPetEquipmentId = petEquipmentId;

            var data = Lance.GameData.PetEquipmentData.TryGet(petEquipmentId);
            if (data != null)
            {
                var imageItem = gameObject.FindComponent<Image>("Image_Item");
                imageItem.sprite = Lance.Atlas.GetItemSlotUISprite(data.sprite);

                var gradeBackground = gameObject.FindComponent<Image>("Image_GradeBackground");
                gradeBackground.sprite = Lance.Atlas.GetFrameGrade(data.grade);

                var gradeIcon = gameObject.FindComponent<Image>("Image_Grade");
                gradeIcon.sprite = Lance.Atlas.GetIconGrade(data.grade);

                var subGradeManager = new SubGradeManager();
                subGradeManager.Init(gameObject.FindGameObject("SubGrade"));

                subGradeManager.SetSubGrade(data.subGrade);

                mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
                SetLevel(Lance.Account.GetPetEquipmentLevel(petEquipmentId));

                mTextReforgeStep = gameObject.FindComponent<TextMeshProUGUI>("Text_ReforgeStep");
                SetReforgeStep(Lance.Account.GetPetEquipmentReforgeStep(petEquipmentId));
            }
        }

        public void InitAccessory(string accessoryId)
        {
            mAccessoryId = accessoryId;

            var data = DataUtil.GetAccessoryData(accessoryId);
            if (data != null)
            {
                var imageItem = gameObject.FindComponent<Image>("Image_Item");
                imageItem.sprite = Lance.Atlas.GetItemSlotUISprite(data.sprite);

                var gradeBackground = gameObject.FindComponent<Image>("Image_GradeBackground");
                gradeBackground.sprite = Lance.Atlas.GetFrameGrade(data.grade);

                var gradeIcon = gameObject.FindComponent<Image>("Image_Grade");
                gradeIcon.sprite = Lance.Atlas.GetIconGrade(data.grade);

                var subGradeManager = new SubGradeManager();
                subGradeManager.Init(gameObject.FindGameObject("SubGrade"));

                subGradeManager.SetSubGrade(data.subGrade);

                mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
                SetLevel(Lance.Account.GetAccessoryLevel(accessoryId));

                mTextReforgeStep = gameObject.FindComponent<TextMeshProUGUI>("Text_ReforgeStep");
                SetReforgeStep(Lance.Account.GetAccessoryReforgeStep(accessoryId));
            }
        }

        public void OnChangeEquipmentReforgeStep(int reforgeStep)
        {
            int defaultMaxLevel = DataUtil.GetEquipmentData(mEquipmentId).maxLevel;

            int reforgeBonusLevel = Lance.GameData.EquipmentCommonData.reforgeAddMaxLevel * (reforgeStep - 1);

            SetLevel(defaultMaxLevel + reforgeBonusLevel);
            SetReforgeStep(reforgeStep);
        }

        public void OnChangePetEquipmentReforgeStep(int reforgeStep)
        {
            int defaultMaxLevel = Lance.GameData.PetEquipmentData.TryGet(mPetEquipmentId).maxLevel;

            int reforgeBonusLevel = Lance.GameData.PetEquipmentCommonData.reforgeAddMaxLevel * (reforgeStep - 1);

            SetLevel(defaultMaxLevel + reforgeBonusLevel);
            SetReforgeStep(reforgeStep);
        }

        public void OnChangeAccessoryReforgeStep(int reforgeStep)
        {
            int defaultMaxLevel = DataUtil.GetAccessoryData(mAccessoryId).maxLevel;

            int reforgeBonusLevel = Lance.GameData.AccessoryCommonData.reforgeAddMaxLevel * (reforgeStep - 1);

            SetLevel(defaultMaxLevel + reforgeBonusLevel);
            SetReforgeStep(reforgeStep);
        }

        void SetLevel(int level)
        {
            mTextLevel.text = $"Lv.{level}";
        }

        void SetReforgeStep(int reforgeStep)
        {
            mTextReforgeStep.text = $"+{reforgeStep}";
        }
    }
}