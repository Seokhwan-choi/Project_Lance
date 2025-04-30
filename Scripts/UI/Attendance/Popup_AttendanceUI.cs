using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Linq;

namespace Lance
{
    class Popup_AttendanceUI : PopupBase
    {
        int mCurIdIndex;
        
        List<string> mAttendanceIdList;
        Dictionary<string, AttendanceTabUI> mAttendanceTabs;
        public void Init()
        {
            Lance.Account.Attendance.Check();

            Button buttonModal = gameObject.FindComponent<Button>("Button_Modal");
            buttonModal.SetButtonAction(() => Close());

            var contents = gameObject.FindGameObject("Contents");
            mAttendanceIdList = new List<string>();
            mAttendanceTabs = new Dictionary<string, AttendanceTabUI>();

            // 우선순위를 기준으로 정렬한다.
            foreach(var data in Lance.Account.Attendance.GetAttendanceDatasOrderByPriority())
            {
                if (data.type == AttendanceType.Onlyone)
                {
                    if (Lance.Account.Attendance.IsFinishedAttendance(data.id))
                        continue;
                }

                GameObject attendanceObj = gameObject.Find($"{data.prefab}", true);

                AttendanceTabUI tabUI = attendanceObj.GetOrAddComponent<AttendanceTabUI>();

                tabUI.Init(data.id);

                tabUI.SetVisible(false);

                mAttendanceIdList.Add(data.id);
                mAttendanceTabs.Add(data.id, tabUI);
            }

            ShowTab(GetCurId());

            // 출석부 생성
            Lance.GameManager.CheckQuest(QuestType.ConfirmAttendance, 1);

            Lance.Lobby.RefreshAttendanceRedDot();
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            ShowNextAttendance();

            string curId = GetCurId();

            if (curId.IsValid() == false)
                base.Close(immediate, hideMotion);
        }


        public void Refresh()
        {
            string curId = GetCurId();
            if (curId.IsValid())
            {
                var curTab = mAttendanceTabs.TryGet(curId);

                curTab?.Refresh();

                RefreshRedDots();
            }
        }

        void ShowNextAttendance()
        {
            string curId = GetCurId();
            if (curId.IsValid())
            {
                HideTab(curId);

                mCurIdIndex++;

                string nextId = GetCurId();

                if (nextId.IsValid())
                {
                    ShowTab(nextId);
                }
            }
        }

        string GetCurId()
        {
            if (mAttendanceIdList.Count <= mCurIdIndex)
                return string.Empty;

            return mAttendanceIdList[mCurIdIndex];
        }

        void ShowTab(string id)
        {
            AttendanceTabUI showTab = mAttendanceTabs.TryGet(id);

            showTab?.SetVisible(true);
        }

        void HideTab(string id)
        {
            AttendanceTabUI hideTab = mAttendanceTabs.TryGet(id);

            hideTab?.SetVisible(false);
        }

        //void ResizeFrame(string id, Action onFinish)
        //{
        //    var data = Lance.GameData.AttendanceData
        //    if (data != null)
        //    {
        //        int frameSizeIndex = (int)data.dayType;

        //        Vector2 sizeDelta = mImageFrame.rectTransform.sizeDelta;
        //        Vector2 endValue = new Vector2(sizeDelta.x, FrameSizeHeight[frameSizeIndex]);

        //        mImageFrame.rectTransform
        //            .DOSizeDelta(endValue, 0.25f)
        //            .SetAutoKill(false)
        //            .OnComplete(() => onFinish?.Invoke());
        //    }
        //}
    }
}