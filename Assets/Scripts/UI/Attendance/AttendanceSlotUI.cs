using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace Lance
{
    class AttendanceSlotUI : MonoBehaviour
    {
        string mId;
        int mDay;

        GameObject mRedDotObj;
        ItemSlotUI mItemSlotUI;
        Image mImageModal;
        Image mImageCheck;
        public int Day => mDay;
        public void Init(string id, int day)
        {
            mId = id;
            mDay = day;

            var data = DataUtil.GetAttendanceDayData(id, day);
            Debug.Assert(data != null, $"출석부 {id}, Day : {day} 데이터가 없다");

            var rewardData = Lance.GameData.RewardData.TryGet(data.reward);
            Debug.Assert(rewardData != null, $"출석부 {id}, Day : {day} 보상 데이터가 없다");

            GameObject receivedObj = gameObject.FindGameObject("Received");
            receivedObj.SetActive(true);

            mRedDotObj = gameObject.FindGameObject("RedDot");

            mImageModal = receivedObj.FindComponent<Image>("Image_Modal");
            mImageCheck = receivedObj.FindComponent<Image>("Image_Check");

            var textDay = gameObject.FindComponent<TextMeshProUGUI>("Text_Day");
            var param = new StringParam("day", $"{day}");
            textDay.text = StringTableUtil.Get("UIString_AttendanceDay", param);

            var itemSlotUIObj = gameObject.FindGameObject("ItemSlotUI");
            mItemSlotUI = itemSlotUIObj.GetOrAddComponent<ItemSlotUI>();

            var itemInfo = new ItemInfo(rewardData);

            mItemSlotUI.Init(itemInfo);

            if (itemInfo.Type == ItemType.Gem)
            {
                if (itemInfo.Amount >= 200000)
                {
                    mItemSlotUI.SetItemSprite(Lance.Atlas.GetUISprite("GemItem_7"));
                }
            }

            var buttonReceive = GetComponent<Button>();
            buttonReceive.SetButtonAction(() =>
            {
                if (Lance.GameManager.ReceiveAttendanceReward(id, day))
                {
                    PlayReceiveRewardMotion();

                    var popup = Lance.PopupManager.GetPopup<Popup_AttendanceUI>();

                    popup?.Refresh();
                }
            });

            RefreshReceived();
        }

        void RefreshReceived()
        {
            bool isReceived = Lance.Account.Attendance.IsReceivedReward(mId, mDay);
            bool canReceive = Lance.Account.Attendance.CanReceiveReward(mId, mDay);

            mRedDotObj.SetActive(canReceive);
            mItemSlotUI.SetActiveItemSparkle(canReceive && ( mItemSlotUI.Type.IsEquipment() || mItemSlotUI.Type.IsSkill() ));
            mImageModal.gameObject.SetActive(isReceived || canReceive == false);
            mImageCheck.gameObject.SetActive(isReceived);
        }

        public void PlayReceiveRewardMotion()
        {
            RefreshReceived();

            UIUtil.PlayStampMotion(mImageModal, mImageCheck);
        }
    }
}