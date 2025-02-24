using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using BackEnd;
using LitJson;
using Unity.VisualScripting;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    //===============================================================
    // ���� �������� �ҷ������ ���⿡ ���� �������� ������ ���� Ŭ����
    //===============================================================
    public class AccountBase : BackendBase
    {
        ObscuredString mInDate;
        protected bool mIsChangedData;
        public ObscuredString InDate => mInDate;
        public bool IsChangedData => mIsChangedData;
        public virtual void RandomizeKey()
        {
            mInDate?.RandomizeCryptoKey();
        }
        public void SetIsChangedData(bool isChanged)
        {
            mIsChangedData = isChanged;
        }
        public virtual bool CanUpdateRankScore() { return false; }
        public virtual string GetTableName() { return string.Empty; }  // �ڽ� ��ü�� ������ ���̺� �̸� �������� �Լ�
        public virtual string GetColumnName() { return string.Empty; } // �ڽ� ��ü�� ������ �÷� �̸� �������� �Լ�
        public virtual Param GetParam() { return new Param(); } // �ڽ� ��ü�� ������ ������Ʈ�� ���� Param�� �������� �Լ�
        protected virtual void InitializeData() { } // �ڽ� ��ü�� ������ ������ �ʱ�ȭ�� ���ִ� �Լ�
        protected virtual void SetServerDataToLocal(JsonData gameDataJson) { } // �ڽ� ��ü�� ������ �ҷ����� �Լ� ȣ�� ���� �� ���̺� �°� �Ľ��ϴ� �Լ�

        public void BackendGameDataLoad(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            string tableName = GetTableName();
            string columnName = GetColumnName();
            string className = tableName;

            bool isSuccess = false;
            string errorInfo = string.Empty;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            // [�ڳ�] �� �������� �ҷ����� �Լ�
            SendQueue.Enqueue(Backend.GameData.GetMyData, tableName, new Where(), callback => {
                try
                {
                    Debug.Log($"Backend.GameData.GetMyData({tableName}) : {callback}");

                    if (callback.IsSuccess())
                    {
                        // �ҷ��� �����Ͱ� �ϳ��� ������ ���
                        if (callback.FlattenRows().Count > 0)
                        {
                            // ���� ������Ʈ�� ���� �� �������� indate�� ����
                            mInDate = callback.FlattenRows()[0]["inDate"].ToString();

                            //Dictionary �� ������ ������ ���� �÷�����  �������� ���
                            if (columnName.IsValid() == false)
                            {
                                // FlattenRows�� ���, ���ϰ��� ["S"], ["N"]���� ������ Ÿ�԰��� ������ ��, Json�� ����
                                SetServerDataToLocal(callback.FlattenRows()[0]);
                            }
                            else
                            {
                                // �������� �ʾ��� ���(UserData)
                                // columnName���� ������ ��, Json�� ����
                                SetServerDataToLocal(callback.FlattenRows()[0][columnName]);
                            }

                            isSuccess = true;
                            // �ҷ����Ⱑ ���� ���Ŀ� ȣ��Ǵ� �Լ� ȣ��
                            afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                        }
                        else
                        {
                            // �ҷ��� �����Ͱ� ���� ���, ������ �������� �ʴ� ���
                            // �����͸� ���� ����
                            BackendInsert(afterBackendLoadFunc);
                        }
                    }
                    else
                    {
                        // ������ ���� ���� ������� ������ �߻����� ���(������ �����Ͱ� ������ ��������)
                        errorInfo = callback.ToString();

                        afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                    }
                }
                catch (Exception e)
                {
                    // ���ܰ� �߻����� ���
                    // �Ľ� ���е�
                    errorInfo = e.ToString();

                    afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                }
            });
        }


        // ������ �����Ͱ� �������� ���� ���, �����͸� ���� ����
        void BackendInsert(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            string tableName = GetTableName();
            bool isSuccess = false;
            string errorInfo = string.Empty;
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            // ������ �ʱ�ȭ(�� �ڽ� ��ü�� ����)
            InitializeData();

            var param = GetParam();

            // [�ڳ�] ���� ���� ���� �Լ�
            SendQueue.Enqueue(Backend.GameData.Insert, tableName, param, callback => {
                try
                {
                    Debug.Log($"Backend.GameData.Insert({tableName}) : {callback}");

                    isSuccess = true;

                    if (callback.IsSuccess())
                    {
                        mInDate = callback.GetInDate();
                    }
                    else
                    {
                        errorInfo = callback.ToString();
                    }
                }
                catch (Exception e)
                {
                    errorInfo = e.ToString();
                }
                finally
                {
                    afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);

                    param = null;
                }
            });
        }

        // ������Ʈ�� �Ϸ�� ���� ���ϰ��� �Բ� ȣ��Ǵ� �Լ�
        public delegate void AfterCallback(BackendReturnObject callback);

        // �ش� ���̺� ������ ������Ʈ
        public void Update(AfterCallback afterCallback)
        {
            SendQueue.Enqueue(Backend.GameData.UpdateV2, GetTableName(), mInDate.ToString(), Backend.UserInDate, GetParam(),
                callback => {
                    Debug.Log($"Backend.GameData.UpdateV2({GetTableName()}, {mInDate}, {Backend.UserInDate}) : {callback}");
                    afterCallback(callback);
                });
        }

        // �ش� ���̺� ������Ʈ�� ������ Ʈ�����(�ѹ��� ���� ���̺� ����)���� ����� ����
        public TransactionValue GetTransactionUpdateValue()
        {
            return TransactionValue.SetUpdateV2(GetTableName(), mInDate, Backend.UserInDate, GetParam());
        }

        // �ش� ���̺� ������Ʈ�� ������ Ʈ�����(�ѹ��� ���� ���̺� ����)���� ����� ����
        public TransactionValue GetTransactionGetValue()
        {
            Where where = new Where();
            where.Equal("owner_inDate", Backend.UserInDate);
            return TransactionValue.SetGet(GetTableName(), where);
        }

        public void BackendGameDataLoadByTransaction(JsonData gameDataJson, AfterBackendLoadFunc afterBackendLoadFunc)
        {
            string columnName = GetColumnName();
            string errorInfo = string.Empty;
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            try
            {
                // ���� ������Ʈ�� ���� �� �������� inDate�� ����
                mInDate = gameDataJson["inDate"].ToString();

                //Dictionary �� ������ ������ ���� �÷�����  �������� ���
                if (columnName.IsValid() == false)
                {
                    // FlattenRows�� ���, ���ϰ��� ["S"], ["N"]���� ������ Ÿ�԰��� ������ ��, Json�� ����
                    SetServerDataToLocal(gameDataJson);
                }
                else
                {
                    // �������� �ʾ��� ���(UserData)
                    // columnName���� ������ ��, Json�� ����
                    SetServerDataToLocal(gameDataJson[columnName]);
                }

                afterBackendLoadFunc(true, className, funcName, errorInfo);
            }
            catch (Exception e)
            {
                errorInfo = e.ToString();

                afterBackendLoadFunc(false, className, funcName, errorInfo);
            }
        }
    }
}