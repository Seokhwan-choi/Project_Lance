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

            //[�ڳ�] ��� ���� ��ŷ ������ ���� ��ȸ �Լ� ȣ��
            SendQueue.Enqueue(Backend.URank.User.GetRankTableList, callback => {
                try
                {
                    Debug.Log($"Backend.URank.User.GetRankTableList : {callback}");

                    if (callback.IsSuccess())
                    {
                        JsonData rankInfoJson = callback.FlattenRows();

                        // ���� �� ���ϵ� ���� �̿��Ͽ� �Ľ�.
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
                            // To Do : ��ŷ�� ����..?
                            //StaticManager.UI.AlertUI.OpenWarningUI("��ŷ�� �������� �ʽ��ϴ�.", "��ŷ�� ã��  �� �����ϴ�.\n���� �����Ͱ� ������ ���� �ش� �����͸� �̿��Ͽ� ��ŷ�� �������ֽñ� �ٶ��ϴ�.");
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