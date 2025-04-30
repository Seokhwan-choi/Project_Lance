using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace Lance
{
    enum NewbieDay
    {
        Day1,
        Day2,
        Day3,
        Day4,
        Day5,
        Day6,
        Day7,

        Count
    }

    class NewEvent_NewbieDayUIManager
    {
        readonly float[] FrameMovePos = new float[] { 52, 152, 252, 352, 452, 552, 652 };
        Image mImageButtonFrame;

        string mEventId;
        NewbieDay mSelectedDay;
        Action mOnChangeAction;
        List<NewbieDayItemUI> mNewbieDayItemList;
        public NewbieDay SelectedDay => mSelectedDay;
        public void Init(string eventId, GameObject go, Action onChangeAction)
        {
            mEventId = eventId;
            mOnChangeAction = onChangeAction;
            mNewbieDayItemList = new List<NewbieDayItemUI>();

            bool isKorean = Lance.LocalSave.LangCode == LangCode.KR;
            var imageBanner = go.FindComponent<Image>("Banner");
            imageBanner.sprite = Lance.Atlas.GetUISprite(isKorean ? "Image_Event_NewbieKnight" : "Image_Event_NewbieKnight_Eng");

            var list = go.FindGameObject("DayList");

            mImageButtonFrame = list.FindComponent<Image>("Image_Frame");

            for (int i = 0; i < (int)NewbieDay.Count; ++i)
            {
                NewbieDay newbieDay = (NewbieDay)i;

                var itemObj = list.FindGameObject($"{newbieDay}");

                var itemUI = itemObj.GetOrAddComponent<NewbieDayItemUI>();
                itemUI.Init(newbieDay, OnChangeNewbieDay);

                mNewbieDayItemList.Add(itemUI);
            }

            RefreshNewbieDayItems();
        }

        void MoveToButtonFrame()
        {
            float endValue = FrameMovePos[(int)mSelectedDay];

            mImageButtonFrame.rectTransform.DOAnchorPosX(endValue, 0.25f).SetAutoKill(false);
        }

        void OnChangeNewbieDay(NewbieDay newbieDay)
        {
            if (Lance.Account.Event.CalcEventQuestPassDay(mEventId) < (int)newbieDay + 1)
            {
                UIUtil.ShowSystemErrorMessage("IsNotSatisfiedPassDay");

                return;
            }

            if (mSelectedDay != newbieDay)
            {
                mSelectedDay = newbieDay;

                MoveToButtonFrame();
            }

            mOnChangeAction?.Invoke();

            RefreshNewbieDayItems();
        }

        public void RefreshNewbieDayItems()
        {
            int passDay = Lance.Account.Event.CalcEventQuestPassDay(mEventId);

            foreach (var item in mNewbieDayItemList)
            {
                item.SetActiveRedDot(passDay > (int)item.NewbieDay && Lance.Account.Event.AnyCanReceiveQuestReward(mEventId, (int)item.NewbieDay + 1));
                item.SetActiveLock(passDay <= (int)item.NewbieDay);
                item.SetActiveTextDay(mSelectedDay == item.NewbieDay);
            }
        }
    }

    class NewbieDayItemUI : MonoBehaviour
    {
        NewbieDay mNewbieDay;
        TextMeshProUGUI mTextDay;
        Image mImageLock;
        GameObject mRedDotObj;
        public NewbieDay NewbieDay => mNewbieDay;
        public void Init(NewbieDay newbieDay, Action<NewbieDay> onButton)
        {
            mNewbieDay = newbieDay;

            mImageLock = gameObject.FindComponent<Image>("Image_Lock");

            mTextDay = gameObject.FindComponent<TextMeshProUGUI>("Text_Day");

            StringParam dayParam = new StringParam("day", (int)newbieDay + 1);

            mTextDay.text = StringTableUtil.Get("UIString_DayValue", dayParam);

            mRedDotObj = gameObject.FindGameObject("RedDot");

            var button = gameObject.GetComponent<Button>();
            button.SetButtonAction(() => onButton.Invoke(newbieDay));
        }

        public void SetActiveLock(bool isActive)
        {
            mImageLock.gameObject.SetActive(isActive);
        }

        public void SetActiveTextDay(bool isActive)
        {
            mTextDay.color = isActive ? Color.white : Color.gray;
        }

        public void SetActiveRedDot(bool isActive)
        {
            mRedDotObj.SetActive(isActive);
        }
    }
}