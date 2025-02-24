using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using DG.Tweening;
using TMPro;

namespace Lance
{
    class AttendanceTabUI : MonoBehaviour
    {
        string mId;
        Transform mDayTm;
        List<AttendanceSlotUI> mSlotUIList;
        Button mButtonAllReceive;

        Canvas mCanvas;
        GraphicRaycaster mGraphicRaycaster;
        public virtual void Init(string id)
        {
            mId = id;
            mSlotUIList = new List<AttendanceSlotUI>();

            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();

            mButtonAllReceive = gameObject.FindComponent<Button>("Button_ReceiveAllReward");
            mButtonAllReceive.SetButtonAction(OnReceiveAllRewardButton);

            var data = Lance.GameData.AttendanceData.TryGet(id);
            if (data != null)
            {
                bool isKorean = Lance.LocalSave.LangCode == LangCode.KR;

                var imageCharacter = gameObject.FindComponent<Image>("Image_Character");
                imageCharacter.sprite = Lance.Atlas.GetUISprite(isKorean ? data.character : data.characterEng);

                if (data.eventId.IsValid())
                {
                    var eventData = Lance.GameData.EventData.TryGet(data.eventId);
                    if (eventData != null)
                    {
                        var textDuration = gameObject.FindComponent<TextMeshProUGUI>("Text_DurationTime");

                        StringParam durationParam = new StringParam("startDate", TimeUtil.GetTimeStr(eventData.startDate));
                        durationParam.AddParam("endDate", TimeUtil.GetTimeStr(eventData.endDate));

                        textDuration.text = StringTableUtil.Get("UIString_DateRange", durationParam);
                    }
                }

                GameObject tabUIObj = gameObject.FindGameObject("AttendanceSlotList");

                mDayTm = tabUIObj.transform;

                if (data.dayType == AttendanceDayType.D7)
                {
                    var day6Tm = mDayTm.FindGameObject("D6");
                    var dayLastTm = mDayTm.FindGameObject("DLast");

                    int maxDay = DataUtil.GetAttendanceMaxDay(data.id);

                    foreach (AttendanceDayData dayData in Lance.GameData.AttendanceDayData.Where(x => x.id == data.id))
                    {
                        if (dayData.day != maxDay)
                        {
                            CreateSlot(dayData, day6Tm);
                        }
                        else
                        {
                            var lastSlot = dayLastTm.gameObject.FindGameObject("AttendanceSlotUI");
                            var lastSlotUI = lastSlot.GetOrAddComponent<AttendanceSlotUI>();

                            lastSlotUI.Init(dayData.id, dayData.day);

                            mSlotUIList.Add(lastSlotUI);
                        }
                    }
                }
                else
                {
                    foreach (AttendanceDayData dayData in Lance.GameData.AttendanceDayData.Where(x => x.id == mId))
                    {
                        CreateSlot(dayData, tabUIObj);
                    }
                }

                void CreateSlot(AttendanceDayData dayData, GameObject parent)
                {
                    var attendanceSlotParentObj = parent.FindGameObject($"AttendanceSlotUI_{dayData.day}");

                    var attendanceSlotObj = attendanceSlotParentObj.FindGameObject("AttendanceSlotUI");

                    var attendanceSlotUI = attendanceSlotObj.GetOrAddComponent<AttendanceSlotUI>();

                    attendanceSlotUI.Init(dayData.id, dayData.day);

                    mSlotUIList.Add(attendanceSlotUI);
                }
            }

            RefreshReceiveButton();
        }

        int[] ReceiveAllReward()
        {
            var receievdDays = Lance.Account.Attendance.ReceiveAllReward(mId);
            if (receievdDays != null)
            {
                foreach (var receivedDay in receievdDays)
                {
                    var slotUI = GetSlotUI(receivedDay);

                    slotUI?.PlayReceiveRewardMotion();
                }
            }

            AttendanceSlotUI GetSlotUI(int day)
            {
                return mSlotUIList.Where(x => x.Day == day).FirstOrDefault();
            }

            return receievdDays;
        }

        public void Refresh() 
        {
            RefreshReceiveButton();
        }

        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }

        void RefreshReceiveButton()
        {
            mButtonAllReceive.SetActiveFrame(Lance.Account.Attendance.AnyCanReceiveReward(mId));
        }

        void OnReceiveAllRewardButton()
        {
            int[] resultDays = ReceiveAllReward();
            if (resultDays == null)
            {
                UIUtil.ShowSystemErrorMessage("CanNotReceiveAnyAttendanceReward");

                return;
            }

            RewardResult totalReward = new RewardResult();
            foreach (var resultDay in resultDays)
            {
                var dayData = DataUtil.GetAttendanceDayData(mId, resultDay);
                if (dayData != null)
                {
                    RewardData reward = Lance.GameData.RewardData.TryGet(dayData.reward);
                    if (reward != null)
                    {
                        totalReward = totalReward.AddReward(reward);
                    }
                }
            }

            if (totalReward.IsEmpty() == false)
            {
                Lance.GameManager.GiveReward(totalReward, ShowRewardType.Popup);

                Lance.Lobby.RefreshAttendanceRedDot();

                RefreshReceiveButton();
            }
        }
    }
}