using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using BackEnd;
using LitJson;

namespace Lance
{
    public class Rank : AccountBase
    {
        List<RankInfo> mRankInfos = new();

        public List<RankInfo> GetRankInfos()
        {
            return mRankInfos;
        }

        public RankInfo GetRankInfo(string rankName)
        {
            foreach(var rankInfo in mRankInfos)
            {
                if (rankInfo.Table == rankName)
                    return rankInfo;
            }

            return null;
        }

        public override void BackendLoad(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            bool isSuccess = false;
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;
            string errorInfo = string.Empty;

            //[뒤끝] 모든 유저 랭킹 설정값 정보 조회 함수 호출
            SendQueue.Enqueue(Backend.URank.User.GetRankTableList, callback => {
                try
                {
                    Debug.Log($"Backend.URank.User.GetRankTableList : {callback}");

                    if (callback.IsSuccess())
                    {
                        JsonData rankInfoJson = callback.FlattenRows();

                        // 성공 시 리턴된 값을 이용하여 파싱.
                        for (int i = 0; i < rankInfoJson.Count; i++)
                        {
                            RankInfo rankInfo = new RankInfo(rankInfoJson[i]);

                            mRankInfos.Add(rankInfo);
                        }
                    }
                    else
                    {
                        if (callback.GetMessage().Contains("rank not found"))
                        {
                            // To Do : 랭킹이 없네..?
                            //StaticManager.UI.AlertUI.OpenWarningUI("랭킹이 존재하지 않습니다.", "랭킹을 찾을  수 없습니다.\n게임 데이터가 생성된 이후 해당 데이터를 이용하여 랭킹을 생성해주시기 바랍니다.");
                        }
                        else
                        {
                            throw new Exception(callback.ToString());
                        }
                    }
                    isSuccess = true;
                }
                catch (Exception e)
                {
                    errorInfo = e.ToString();
                }
                finally
                {
                    afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                }
            });
        }
    }
}