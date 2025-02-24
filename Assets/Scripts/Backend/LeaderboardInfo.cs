using System.Collections.Generic;
using UnityEngine;
using System;
using BackEnd;
using BackEnd.Leaderboard;
using LitJson;
using System.Reflection;

namespace Lance
{
    public class LeaderboardInfo
    {
        LeaderboardTableItem mLeaderboardItem;
        public bool IsDivision => mLeaderboardItem.isDivision;
        public string Uuid => mLeaderboardItem.uuid;
        public string Table => mLeaderboardItem.table;
        //추가 항목이 있을 경우에만 존재
        public string ExtraDataType => mLeaderboardItem.extraDataType;
        public string ExtraDataColumn => mLeaderboardItem.extraDataColumn;
        public DateTime UpdateTime { get; private set; }
        public DateTime MyRankUpdateTime { get; private set; }
        public UserLeaderboardItem MyUserLeaderboardItem { get; private set; }

        private List<UserLeaderboardItem> mUserLeaderboardItemList = new();

        // 랭킹을 불러온 후에 바뀐 List값을 리턴하는 대리자 함수
        public delegate void GetListFunc(bool isSuccess, IReadOnlyList<UserLeaderboardItem> rankList);

        // 랭킹 리스트를 전달하는 함수
        public void GetRankList(GetListFunc getListFunc)
        {
            // 갱신한지 15분이 지나지 않았으면 캐싱된 값을 리턴
            if ((TimeUtil.UtcNow - UpdateTime).Minutes < 15 && mUserLeaderboardItemList.Count > 0)
            {
                getListFunc(true, mUserLeaderboardItemList);
                return;
            }

            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            try
            {
                // leaderboardUuid 랭킹에서 1 ~ 50등 랭커 조회
                Backend.Leaderboard.User.GetLeaderboard(Uuid, 50, callback =>
                {
                    if (callback.IsSuccess())
                    {
                        UpdateTime = TimeUtil.UtcNow;

                        Debug.Log("리더보드 총 유저 등록 수 : " + callback.GetTotalCount());

                        mUserLeaderboardItemList.Clear();

                        mUserLeaderboardItemList = callback.GetUserLeaderboardList();

                        getListFunc(true, mUserLeaderboardItemList);
                    }
                    else
                    {
                        getListFunc(false, mUserLeaderboardItemList);
                    }
                });
            }
            catch(Exception e)
            {
                Lance.BackEnd.SendBugReport(className, funcName, e.ToString());

                getListFunc(false, mUserLeaderboardItemList);
            }
            
        }

        public delegate void GetMyRankFunc(bool isSuccess, UserLeaderboardItem UserLeaderboardItem);

        // 내 랭킹을 불러오기 위해 갱신을 한번 했는지 여부
        bool mIsTwiceRepeat = false;

        // 내 랭킹이 갱신되지 않았을 경우에는 Update를 한번 호출
        public void GetMyRank(GetMyRankFunc getMyRankFunc)
        {
            // 15분이 지나지 않았을 경우에는 캐싱된 값 리턴
            if ((TimeUtil.UtcNow - MyRankUpdateTime).Minutes < 15 && MyUserLeaderboardItem != null)
            {
                getMyRankFunc(true, MyUserLeaderboardItem);
                return;
            }

            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            try
            {
                // 자기 자신의 랭킹만 조회
                Backend.Leaderboard.User.GetMyLeaderboard(Uuid, bro => {
                    if (bro.IsSuccess())
                    {
                        MyRankUpdateTime = TimeUtil.UtcNow;

                        Debug.Log("리더보드 총 유저 등록 수 : " + bro.GetTotalCount());

                        MyUserLeaderboardItem = bro.GetUserLeaderboardList()[0];

                        getMyRankFunc(true, MyUserLeaderboardItem);
                    }
                    else
                    {
                        getMyRankFunc(false, MyUserLeaderboardItem);
                    }
                });
            }
            catch (Exception e)
            {
                Lance.BackEnd.SendBugReport(className, funcName, e.ToString());

                getMyRankFunc(false, MyUserLeaderboardItem);
            }
        }

        public LeaderboardInfo(LeaderboardTableItem item)
        {
            mLeaderboardItem = item;

            MyRankUpdateTime = TimeUtil.UtcNow;
            UpdateTime = TimeUtil.UtcNow;
        }
    }
}