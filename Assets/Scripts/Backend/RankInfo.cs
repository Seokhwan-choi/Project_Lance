using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using BackEnd;
using LitJson;
using System.Reflection;

namespace Lance
{
    public class RankInfo
    {
        public string Date { get; private set; }
        public string Uuid { get; private set; }
        public string Order { get; private set; }
        public bool IsReset { get; private set; }
        public string Title { get; private set; }
        public string Table { get; private set; }
        public string Column { get; private set; }

        //일회성 랭킹에만 존재
        public DateTime RankStartDateAndTime { get; private set; }
        public DateTime RankEndDateAndTime { get; private set; }

        //추가 항목이 있을 경우에만 존재
        public string ExtraDataColumn { get; private set; }
        public string ExtraDataType { get; private set; }

        public int TotalUserCount { get; private set; }

        public DateTime UpdateTime { get; private set; }
        public DateTime MyRankUpdateTime { get; private set; }
        public RankItem MyRankItem { get; private set; }

        private List<RankItem> mRankItemList = new();

        // 랭킹을 불러온 후에 바뀐 List값을 리턴하는 대리자 함수
        public delegate void GetListFunc(bool isSuccess, IReadOnlyList<RankItem> rankList);

        // 랭킹 리스트를 전달하는 함수
        public void GetRankList(GetListFunc getListFunc)
        {
            // 갱신한지 5분이 지나지 않았으면 캐싱된 값을 리턴
            if ((TimeUtil.UtcNow - UpdateTime).Minutes < 15 && mRankItemList.Count > 0)
            {
                getListFunc(true, mRankItemList);
                return;
            }
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;
            // 5분이 지났을 경우에는 랭킹 함수 호출
            // [뒤끝] 랭킹 리스트 불러오기 함수
            SendQueue.Enqueue(Backend.URank.User.GetRankList, Uuid, 100, callback => {
                try
                {
                    Debug.Log($"Backend.URank.User.GetRankList({Uuid}) : {callback}");
                    if (callback.IsSuccess())
                    {
                        UpdateTime = TimeUtil.UtcNow;

                        JsonData rankJson = callback.GetFlattenJSON();

                        TotalUserCount = int.Parse(rankJson["totalCount"].ToString());
                        mRankItemList.Clear();
                        foreach (JsonData tempJson in rankJson["rows"])
                        {
                            mRankItemList.Add(new RankItem(tempJson, Table, ExtraDataColumn));
                        }

                        getListFunc(true, mRankItemList);
                    }
                    else
                    {
                        getListFunc(false, mRankItemList);
                    }
                }
                catch (Exception e)
                {
                    // To Do : 오류 팝업 보여주자

                    getListFunc(false, mRankItemList);
                }
            });
        }

        public delegate void GetMyRankFunc(bool isSuccess, RankItem rankItem);

        // 내 랭킹을 불러오기 위해 갱신을 한번 했는지 여부
        bool mIsTwiceRepeat = false;

        // 내 랭킹이 갱신되지 않았을 경우에는 Update를 한번 호출
        public void GetMyRank(GetMyRankFunc getMyRankFunc)
        {
            // 5분이 지나지 않았을 경우에는 캐싱된 값 리턴
            if ((TimeUtil.UtcNow - MyRankUpdateTime).Minutes < 15 && MyRankItem != null)
            {
                getMyRankFunc(true, MyRankItem);
                return;
            }

            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            SendQueue.Enqueue(Backend.URank.User.GetMyRank, Uuid, callback => {
                try
                {
                    Debug.Log($"Backend.URank.User.GetMyRank({Uuid}) : {callback}");
                    if (callback.IsSuccess())
                    {
                        // 갱신 주기 갱신
                        MyRankUpdateTime = TimeUtil.UtcNow;

                        // 내 데이터 생성
                        MyRankItem = new RankItem(callback.FlattenRows()[0], Table, ExtraDataColumn);

                        // 생성된 데이터로 리턴
                        getMyRankFunc(true, MyRankItem);
                    }
                    else
                    {
                        // 에러가 발생하였을 경우

                        // 만약 내 랭킹이 갱신되어있지 않을 경우
                        if (callback.GetMessage().Contains("userRank not found"))
                        {
                            // 갱신을 하였는대도 다시 여기를 호출할 경우
                            if (mIsTwiceRepeat)
                            {
                                // 무한 루프의 위험이 있기 때문에 bool값으로 한번만 호출하게 제어한다
                                mIsTwiceRepeat = false;
                                getMyRankFunc(false, MyRankItem);
                                return;
                            }

                            mIsTwiceRepeat = false;

                            Lance.BackEnd.UpdateUserRankScores(Uuid, (afterUpdateCallback) => {
                                if (afterUpdateCallback == null)
                                {
                                    getMyRankFunc(false, MyRankItem);

                                    return;
                                }
                                else
                                {
                                    if (afterUpdateCallback.IsSuccess())
                                    {
                                        // 성공하였을 경우 다시한번 내 랭킹 불러오기 호출
                                        mIsTwiceRepeat = true;

                                        GetMyRank(getMyRankFunc);
                                    }
                                    else
                                    {
                                        getMyRankFunc(false, MyRankItem);
                                    }
                                }
                            });
                        }
                        else
                        {
                            // "userRank not found" 에러 외 다른 에러일 경우, 그냥 리턴
                            // 에러는 Lance.BackEnd.UpdateUserScores에서 기록한다.
                            getMyRankFunc(false, MyRankItem);
                        }
                    }
                }
                catch (Exception e)
                {
                    Lance.BackEnd.SendBugReport(className, funcName, e.ToString());

                    getMyRankFunc(false, MyRankItem);
                }
            });
        }

        // Backend.URank.User.GetRankTableList()의 리턴데이터 파싱
        public RankInfo(JsonData gameDataJson)
        {
            Date = gameDataJson["date"].ToString();
            Uuid = gameDataJson["uuid"].ToString();
            Order = gameDataJson["order"].ToString();
            IsReset = gameDataJson["isReset"].ToString() == "true" ? true : false;
            Title = gameDataJson["title"].ToString();
            Table = gameDataJson["table"].ToString();
            Column = gameDataJson["column"].ToString();

            if (gameDataJson.ContainsKey("rankStartDateAndTime"))
            {
                RankStartDateAndTime = DateTime.Parse(gameDataJson["rankStartDateAndTime"].ToString());
                RankEndDateAndTime = DateTime.Parse(gameDataJson["rankEndDateAndTime"].ToString());
            }

            if (gameDataJson.ContainsKey("extraDataColumn"))
            {
                ExtraDataColumn = gameDataJson["extraDataColumn"].ToString();
                ExtraDataType = gameDataJson["extraDataType"].ToString();
            }

            //string type = gameDataJson["rankType"].ToString();

            //if (!Enum.TryParse<RankType>(gameDataJson["rankType"].ToString(), out var tempRankType))
            //{
            //    throw new Exception($"[{Uuid}] - 지정되지 않은 RankType 입니다.");
            //}

            //RankType = tempRankType;
            MyRankUpdateTime = TimeUtil.UtcNow;
            UpdateTime = TimeUtil.UtcNow;
        }
    }
}
