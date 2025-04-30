using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;


namespace Lance
{
    class Popup_AutoChangeOptionStatSelectUI : PopupBase
    {
        List<StatTypeItemUI> mStatTypeItemUIList;
        List<StatGradeItemUI> mStatGradeItemUIList;
        public void Init(string title, StatType[] statTypeList, Grade[] statGradeList, Action<StatType[], Grade> onConfirmSelect)
        {
            SetUpCloseAction();

            SetTitleText(title);

            mStatTypeItemUIList = new List<StatTypeItemUI>();

            var statTypeItemUIList = gameObject.FindGameObject("StatTypeItemUIs");

            statTypeItemUIList.AllChildObjectOff();

            for(int i = 0; i < statTypeList.Length; ++i)
            {
                StatType statType = statTypeList[i];

                var statTypeItemObj = Lance.ObjectPool.AcquireUI("StatTypeItemUI", statTypeItemUIList.GetComponent<RectTransform>());

                var statTypeItemUI = statTypeItemObj.GetOrAddComponent<StatTypeItemUI>();

                statTypeItemUI.Init(statType);

                mStatTypeItemUIList.Add(statTypeItemUI);
            }

            mStatGradeItemUIList = new List<StatGradeItemUI>();

            var statGradeItemUIList = gameObject.FindGameObject("StatGradeItemUIs");

            statGradeItemUIList.AllChildObjectOff();

            for(int i = 0; i < statGradeList.Length; ++i)
            {
                Grade statGrade = statGradeList[i];

                var statGradeItemObj = Lance.ObjectPool.AcquireUI("StatGradeItemUI", statGradeItemUIList.GetComponent<RectTransform>());

                var statGradeItemUI = statGradeItemObj.GetOrAddComponent<StatGradeItemUI>();

                statGradeItemUI.Init(statGrade, OnSelectGrade);

                mStatGradeItemUIList.Add(statGradeItemUI);
            }

            var buttonCancel = gameObject.FindComponent<Button>("Button_Cancel");
            buttonCancel.SetButtonAction(() => Close());

            var buttonConfirm = gameObject.FindComponent<Button>("Button_Confirm");
            buttonConfirm.SetButtonAction(() =>
            {
                StatType[] selectedStatTypes = GetSelectedStatTypes();
                Grade selectedGrade = GetSelectedGrade();

                if (selectedStatTypes.Length <= 0)
                {
                    UIUtil.ShowSystemErrorMessage("NeedSelectedOptionStatType");

                    return;
                }

                if (selectedGrade == Grade.D)
                {
                    UIUtil.ShowSystemErrorMessage("NeedSelectedOptionGrade");

                    return;
                }

                onConfirmSelect?.Invoke(selectedStatTypes, selectedGrade);

                Close();
            });
        }

        StatType[] GetSelectedStatTypes()
        {
            List<StatType> selectedStatTypes = new List<StatType>();

            foreach(var statItemUI in mStatTypeItemUIList)
            {
                if (statItemUI.IsSelected)
                {
                    selectedStatTypes.Add(statItemUI.StatType);
                }
            }

            return selectedStatTypes.ToArray();
        }


        // Grade.D는 선택을 안했다는 것임
        Grade GetSelectedGrade()
        {
            Grade selectedGrade = Grade.D;

            foreach(var gradeItemUI in mStatGradeItemUIList)
            {
                if (gradeItemUI.IsSelected)
                {
                    selectedGrade = gradeItemUI.Grade;
                }
            }

            return selectedGrade;
        }

        void OnSelectGrade(Grade selectGrade)
        {
            foreach(var gradeItemUI in mStatGradeItemUIList)
            {
                if (gradeItemUI.Grade != selectGrade)
                    gradeItemUI.SetSelected(false);
            }
        }
    }

    class StatTypeItemUI : MonoBehaviour
    {
        StatType mStatType;
        bool mIsSelected;
        Image mImageCheck;
        public bool IsSelected => mIsSelected;
        public StatType StatType => mStatType;
        public void Init(StatType statType)
        {
            mStatType = statType;
            mIsSelected = false;

            var textStatTypeName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatTypeName");
            textStatTypeName.text = StringTableUtil.GetName($"{mStatType}");

            mImageCheck = gameObject.FindComponent<Image>("Image_Check");
            mImageCheck.gameObject.SetActive(false);

            var button = GetComponent<Button>();
            button.SetButtonAction(OnButton);
        }

        void OnButton()
        {
            mIsSelected = !mIsSelected;

            mImageCheck.gameObject.SetActive(mIsSelected);
        }
    }

    class StatGradeItemUI : MonoBehaviour
    {
        Grade mGrade;
        bool mIsSelected;
        Image mImageCheck;
        public bool IsSelected => mIsSelected;
        public Grade Grade => mGrade;
        public void Init(Grade grade, Action<Grade> onButton)
        {
            mGrade = grade;
            mIsSelected = false;

            var imageGrade = gameObject.FindComponent<Image>("Image_StatGrade");
            imageGrade.sprite = Lance.Atlas.GetIconGrade(mGrade);

            mImageCheck = gameObject.FindComponent<Image>("Image_Check");
            mImageCheck.gameObject.SetActive(false);

            var button = GetComponent<Button>();
            button.SetButtonAction(() =>
            {
                OnButton();

                onButton?.Invoke(mGrade);
            });
        }

        public void Refresh()
        {
            mImageCheck.gameObject.SetActive(mIsSelected);
        }

        public void SetSelected(bool selected)
        {
            mIsSelected = selected;

            Refresh();
        }

        void OnButton()
        {
            mIsSelected = !mIsSelected;

            Refresh();
        }
    }
}