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
        //�߰� �׸��� ���� ��쿡�� ����
        public string ExtraDataType => mLeaderboardItem.extraDataType;
        public string ExtraDataColumn => mLeaderboardItem.extraDataColumn;
        public DateTime UpdateTime { get; private set; }
        public DateTime MyRankUpdateTime { get; private set; }
        public UserLeaderboardItem MyUserLeaderboardItem { get; private set; }

        private List<UserLeaderboardItem> mUserLeaderboardItemList = new();

        // ��ŷ�� �ҷ��� �Ŀ� �ٲ� List���� �����ϴ� �븮�� �Լ�
        public delegate void GetListFunc(bool isSuccess, IReadOnlyList<UserLeaderboardItem> rankList);

        // ��ŷ ����Ʈ�� �����ϴ� �Լ�
        public void GetRankList(GetListFunc getListFunc)
        {
            // �������� 15���� ������ �ʾ����� ĳ�̵� ���� ����
            if ((TimeUtil.UtcNow - UpdateTime).Minutes < 15 && mUserLeaderboardItemList.Count > 0)
            {
                getListFunc(true, mUserLeaderboardItemList);
                return;
            }

            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            try
            {
                // leaderboardUuid ��ŷ���� 1 ~ 50�� ��Ŀ ��ȸ
                Backend.Leaderboard.User.GetLeaderboard(Uuid, 50, callback =>
                {
                    if (callback.IsSuccess())
                    {
                        UpdateTime = TimeUtil.UtcNow;

                        Debug.Log("�������� �� ���� ��� �� : " + callback.GetTotalCount());

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

        // �� ��ŷ�� �ҷ����� ���� ������ �ѹ� �ߴ��� ����
        bool mIsTwiceRepeat = false;

        // �� ��ŷ�� ���ŵ��� �ʾ��� ��쿡�� Update�� �ѹ� ȣ��
        public void GetMyRank(GetMyRankFunc getMyRankFunc)
        {
            // 15���� ������ �ʾ��� ��쿡�� ĳ�̵� �� ����
            if ((TimeUtil.UtcNow - MyRankUpdateTime).Minutes < 15 && MyUserLeaderboardItem != null)
            {
                getMyRankFunc(true, MyUserLeaderboardItem);
                return;
            }

            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            try
            {
                // �ڱ� �ڽ��� ��ŷ�� ��ȸ
                Backend.Leaderboard.User.GetMyLeaderboard(Uuid, bro => {
                    if (bro.IsSuccess())
                    {
                        MyRankUpdateTime = TimeUtil.UtcNow;

                        Debug.Log("�������� �� ���� ��� �� : " + bro.GetTotalCount());

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