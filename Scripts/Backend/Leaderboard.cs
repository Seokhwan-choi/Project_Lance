using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using BackEnd;
using BackEnd.Leaderboard;
using LitJson;

namespace Lance
{
    public class Leaderboard : AccountBase
    {
        List<LeaderboardInfo> mLeaderboardInfos = new();

        public List<LeaderboardInfo> GetLeaderboardInfos()
        {
            return mLeaderboardInfos;
        }

        public LeaderboardInfo GetLeaderboardInfo(string rankName)
        {
            foreach (var leaderboardInfo in mLeaderboardInfos)
            {
                if (leaderboardInfo.Table == rankName)
                    return leaderboardInfo;
            }

            return null;
        }

        public override void BackendLoad(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            bool isSuccess = false;
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;
            string errorInfo = string.Empty;

            try
            {
                // SendQueue로 처리 불가능
                Backend.Leaderboard.User.GetLeaderboards(bro =>
                {
                    isSuccess = bro.IsSuccess();

                    if (bro.IsSuccess())
                    {
                        foreach (LeaderboardTableItem item in bro.GetLeaderboardTableList())
                        {
                            var leaderboardInfo = new LeaderboardInfo(item);

                            mLeaderboardInfos.Add(leaderboardInfo);
                        }

                        afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                    }
                    else
                    {
                        afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                    }
                });
            }
            catch (Exception e)
            {
                errorInfo = e.ToString();

                afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
            }
        }
    }
}