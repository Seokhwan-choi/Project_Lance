using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Mosframe;
using BackEnd;

namespace Lance
{
    class SnakeNewYearEventPassItemUI : MonoBehaviour, IDynamicScrollViewItem
    {
        SnakeNewYearEvent_PassTabUI mParent;
        TextMeshProUGUI mTextRequireValue;
        EventPassRewardItemSlotUI mFreeSlotUI;
        EventPassRewardItemSlotUI mPaySlotUI;
        string mEventId;
        bool mInit;
        string mId;
        int mStep;
        public string Id => mId;
        public int Step => mStep;
        public void Init()
        {
            if (mInit)
                return;

            mInit = true;

            mParent = gameObject.GetComponentInParent<SnakeNewYearEvent_PassTabUI>(true);
            mTextRequireValue = gameObject.FindComponent<TextMeshProUGUI>("Text_RequireValue");

            var freeRewardSlotObj = gameObject.FindGameObject("FreeRewardSlot");
            mFreeSlotUI = freeRewardSlotObj.GetOrAddComponent<EventPassRewardItemSlotUI>();

            var payRewardSlotObj = gameObject.FindGameObject("PayRewardSlot");
            mPaySlotUI = payRewardSlotObj.GetOrAddComponent<EventPassRewardItemSlotUI>();
        }

        public void OnUpdateItem(int index)
        {
            PassStepData stepData = mParent?.GetStepData(index);
            if (stepData != null)
            {
                mId = stepData.id;
                mEventId = mParent?.EventId ?? string.Empty;
                mStep = stepData.step;
                mTextRequireValue.text = $"{stepData.requireValue}";
                mFreeSlotUI.Init(stepData, mEventId, isFree: true);
                mPaySlotUI.Init(stepData, mEventId, isFree: false);
            }
        }

        public void PlayRewardMotion(bool isFree)
        {
            if (isFree)
                mFreeSlotUI.PlayReceiveRewardMotion();
            else
                mPaySlotUI.PlayReceiveRewardMotion();
        }
    }
}