using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using System.Linq;
using BackEnd;

namespace Lance
{
    public class Attendance : AccountBase
    {
        Dictionary<string, AttendanceInfo> mAttendanceInfos;
        public Attendance()
        {
            mAttendanceInfos = new Dictionary<string, AttendanceInfo>();
        }
        public override string GetTableName()
        {
            return "Attendance";
        }
        public override string GetColumnName()
        {
            return "Attendance";
        }

        protected override void InitializeData()
        {
            foreach(var data in Lance.GameData.AttendanceData.Values)
            {
                if (data.eventId.IsValid())
                {
                    var eventData = Lance.GameData.EventData.TryGet(data.eventId);
                    if (eventData.active == false ||
                        TimeUtil.IsActiveDateNum(eventData.startDate, eventData.endDate) == false)
                        continue;
                }

                if ( mAttendanceInfos.ContainsKey(data.id) == false )
                {
                    var info = new AttendanceInfo();

                    var receviedReward = new List<ObscuredInt>();

                    info.Init(data.id, 0, 0, receviedReward);

                    mAttendanceInfos.Add(data.id, info);
                }
            }
        }
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            for (int i = 0; i < gameDataJson.Count; i++)
            {
                var jsonData = gameDataJson[i];

                string id = jsonData["Id"].ToString();

                var attendanceData = Lance.GameData.AttendanceData.TryGet(id);
                if (attendanceData != null)
                {
                    int attendanceDayTemp = 0;

                    int.TryParse(jsonData["AttendanceDay"].ToString(), out attendanceDayTemp);

                    int lastUpdateDateNumTemp = 0;

                    int.TryParse(jsonData["LastUpdateDateNum"].ToString(), out lastUpdateDateNumTemp);

                    var receivedReward = new List<ObscuredInt>();

                    var receivedRewardData = jsonData["ReceivedReward"];

                    if (receivedRewardData.Count > 0)
                    {
                        for (int j = 0; j < receivedRewardData.Count; ++j)
                        {
                            int receiveTemp = 0;

                            int.TryParse(receivedRewardData[j].ToString(), out receiveTemp);

                            receivedReward.Add(receiveTemp);
                        }
                    }

                    var info = new AttendanceInfo();

                    if (lastUpdateDateNumTemp >= 20680101)
                        lastUpdateDateNumTemp = TimeUtil.NowDateNum() - 1;

                    info.Init(id, attendanceDayTemp, lastUpdateDateNumTemp, receivedReward);

                    mAttendanceInfos.Add(id, info);
                }
            }

            InitializeData();
        }

        public IEnumerable<AttendanceData> GetAttendanceDatasOrderByPriority()
        {
            List<AttendanceData> dataList = new List<AttendanceData>();

            foreach(var id in mAttendanceInfos.Keys)
            {
                var data = Lance.GameData.AttendanceData.TryGet(id);

                dataList.Add(data);
            }

            return dataList.OrderBy(x => x.priority);
        }

        public bool Check()
        {
            bool attendance = false;

            foreach(var info in mAttendanceInfos.Values)
            {
                if (info.Attendance())
                    attendance = true;
            }

            if (attendance)
                SetIsChangedData(true);

            return attendance;
        }

        public int[] ReceiveAllReward(string id)
        {
            var info = mAttendanceInfos.TryGet(id);
            if (info == null)
                return null;

            var reward = info.ReceiveAllReward();

            SetIsChangedData(true);

            return reward;
        }

        public bool ReceiveReward(string id, int day)
        {
            var info = mAttendanceInfos.TryGet(id);
            if (info == null)
                return false;

            if (info.ReceiveReward(day))
            {
                SetIsChangedData(true);

                return true;
            }

            return false;
        }

        public bool IsReceivedReward(string id, int day)
        {
            var info = mAttendanceInfos.TryGet(id);
            if (info == null)
                return true;

            return info.IsReceivedReward(day);
        }

        public bool IsFinishedAttendance(string id)
        { 
            AttendanceInfo info = mAttendanceInfos.TryGet(id);
            if (info == null)
                return false;

            int maxDay = DataUtil.GetAttendanceMaxDay(id);
            if (maxDay > info.GetAttendanceDay())
                return false;

            return info.AnyCanReceiveReward() == false;
        }

        public bool AnyCanReceiveReward(string id)
        {
            var info = mAttendanceInfos.TryGet(id);
            if (info == null)
                return false;

            return info.AnyCanReceiveReward();
        }

        public bool AnyCanReceiveReward()
        {
            foreach (var info in mAttendanceInfos.Values)
            {
                if (info.AnyCanReceiveReward())
                    return true;
            }

            return false;
        }

        public bool CanReceiveReward(string id, int day)
        {
            var info = mAttendanceInfos.TryGet(id);
            if (info == null)
                return false;

            return info.CanReceiveReward(day);
        }

        public override Param GetParam()
        {
            Dictionary<string, AttendanceInfo> saveInfos = new Dictionary<string, AttendanceInfo>();

            foreach (var info in mAttendanceInfos)
            {
                info.Value.ReadyToSave();

                saveInfos.Add(info.Key, info.Value);
            }

            Param param = new Param();

            param.Add(GetColumnName(), saveInfos);

            return param;
        }
    }

    class AttendanceInfo
    {
        public string Id;
        public int AttendanceDay;
        public int LastUpdateDateNum;
        public List<int> ReceivedReward;
        
        ObscuredString mId;
        ObscuredInt mAttendanceDay;
        ObscuredInt mLastUpdateDateNum;
        List<ObscuredInt> mReceivedReward;
        public void Init(string id, int attendanceDay, int lastUpdateDateNum, List<ObscuredInt> receivedReward)
        {
            mId = id;
            mAttendanceDay = attendanceDay;
            mLastUpdateDateNum = lastUpdateDateNum;
            mReceivedReward = receivedReward;
        }

        public bool Attendance()
        {
            var nowDateNum = TimeUtil.NowDateNum();

            if (mLastUpdateDateNum > nowDateNum)
                return false;

            if (mLastUpdateDateNum < nowDateNum)
            {
                mLastUpdateDateNum = nowDateNum;

                int maxDay = DataUtil.GetAttendanceMaxDay(mId);

                if (maxDay < mAttendanceDay + 1)
                {
                    var data = Lance.GameData.AttendanceData.TryGet(mId);

                    if (data.type == AttendanceType.Repeat)
                    {
                        mAttendanceDay = 1;

                        mReceivedReward.Clear();
                    }
                }
                else
                {
                    mAttendanceDay += 1;
                }

                return true;
            }

            return false;
        }

        public bool IsReceivedReward(int day)
        {
            return mReceivedReward.Contains(day);
        }

        public bool AnyCanReceiveReward()
        {
            int maxDay = DataUtil.GetAttendanceMaxDay(mId);

            for(int i = 0; i < maxDay; ++i)
            {
                int day = i + 1;

                if (CanReceiveReward(day))
                    return true;
            }

            return false;
        }

        public bool CanReceiveReward(int day)
        {
            return IsReceivedReward(day) == false &&
                mAttendanceDay >= day;
        }

        public int[] ReceiveAllReward()
        {
            List<int> receivedRewardDays = new List<int>();

            for(int day = 1; day <= mAttendanceDay; ++day)
            {
                if (ReceiveReward(day))
                {
                    receivedRewardDays.Add(day);
                }
            }

            return receivedRewardDays.ToArray();
        }

        public bool ReceiveReward(int day)
        {
            if (CanReceiveReward(day)==false)
                return false;

            mReceivedReward.Add(day);

            return true;
        }

        public void ReadyToSave()
        {
            Id = mId;
            AttendanceDay = mAttendanceDay;
            LastUpdateDateNum = mLastUpdateDateNum;
            ReceivedReward = mReceivedReward.Select(x => (int)x).ToList();
        }

        public void RandomizeKey()
        {
            mId?.RandomizeCryptoKey();
            mAttendanceDay.RandomizeCryptoKey();
            mLastUpdateDateNum.RandomizeCryptoKey();

            foreach(var receive in mReceivedReward)
            {
                receive.RandomizeCryptoKey();
            }
        }

        public string GetId()
        {
            return mId;
        }

        public int GetAttendanceDay()
        {
            return mAttendanceDay;
        }
    }
}