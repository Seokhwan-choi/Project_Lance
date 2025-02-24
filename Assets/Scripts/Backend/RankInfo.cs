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

        //��ȸ�� ��ŷ���� ����
        public DateTime RankStartDateAndTime { get; private set; }
        public DateTime RankEndDateAndTime { get; private set; }

        //�߰� �׸��� ���� ��쿡�� ����
        public string ExtraDataColumn { get; private set; }
        public string ExtraDataType { get; private set; }

        public int TotalUserCount { get; private set; }

        public DateTime UpdateTime { get; private set; }
        public DateTime MyRankUpdateTime { get; private set; }
        public RankItem MyRankItem { get; private set; }

        private List<RankItem> mRankItemList = new();

        // ��ŷ�� �ҷ��� �Ŀ� �ٲ� List���� �����ϴ� �븮�� �Լ�
        public delegate void GetListFunc(bool isSuccess, IReadOnlyList<RankItem> rankList);

        // ��ŷ ����Ʈ�� �����ϴ� �Լ�
        public void GetRankList(GetListFunc getListFunc)
        {
            // �������� 5���� ������ �ʾ����� ĳ�̵� ���� ����
            if ((TimeUtil.UtcNow - UpdateTime).Minutes < 15 && mRankItemList.Count > 0)
            {
                getListFunc(true, mRankItemList);
                return;
            }
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;
            // 5���� ������ ��쿡�� ��ŷ �Լ� ȣ��
            // [�ڳ�] ��ŷ ����Ʈ �ҷ����� �Լ�
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
                    // To Do : ���� �˾� ��������

                    getListFunc(false, mRankItemList);
                }
            });
        }

        public delegate void GetMyRankFunc(bool isSuccess, RankItem rankItem);

        // �� ��ŷ�� �ҷ����� ���� ������ �ѹ� �ߴ��� ����
        bool mIsTwiceRepeat = false;

        // �� ��ŷ�� ���ŵ��� �ʾ��� ��쿡�� Update�� �ѹ� ȣ��
        public void GetMyRank(GetMyRankFunc getMyRankFunc)
        {
            // 5���� ������ �ʾ��� ��쿡�� ĳ�̵� �� ����
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
                        // ���� �ֱ� ����
                        MyRankUpdateTime = TimeUtil.UtcNow;

                        // �� ������ ����
                        MyRankItem = new RankItem(callback.FlattenRows()[0], Table, ExtraDataColumn);

                        // ������ �����ͷ� ����
                        getMyRankFunc(true, MyRankItem);
                    }
                    else
                    {
                        // ������ �߻��Ͽ��� ���

                        // ���� �� ��ŷ�� ���ŵǾ����� ���� ���
                        if (callback.GetMessage().Contains("userRank not found"))
                        {
                            // ������ �Ͽ��´뵵 �ٽ� ���⸦ ȣ���� ���
                            if (mIsTwiceRepeat)
                            {
                                // ���� ������ ������ �ֱ� ������ bool������ �ѹ��� ȣ���ϰ� �����Ѵ�
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
                                        // �����Ͽ��� ��� �ٽ��ѹ� �� ��ŷ �ҷ����� ȣ��
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
                            // "userRank not found" ���� �� �ٸ� ������ ���, �׳� ����
                            // ������ Lance.BackEnd.UpdateUserScores���� ����Ѵ�.
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

        // Backend.URank.User.GetRankTableList()�� ���ϵ����� �Ľ�
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
            //    throw new Exception($"[{Uuid}] - �������� ���� RankType �Դϴ�.");
            //}

            //RankType = tempRankType;
            MyRankUpdateTime = TimeUtil.UtcNow;
            UpdateTime = TimeUtil.UtcNow;
        }
    }
}
