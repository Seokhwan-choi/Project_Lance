using System;
using System.Collections.Generic;
using System.Reflection;
using BackEnd;
using UnityEngine;

namespace Lance
{
    //===================================================================
    // ��Ʈ, ���������� �ϰ������� ������ ������ �����ϱ� ���� ����� ���̽� Ŭ����
    //==================================================================
    public class BackendBase
    {
        // LoadingScene���� ����ϴ� �ҷ����Ⱑ �Ϸ�� ���Ŀ� ȣ��Ǵ� �Լ�
        public delegate void AfterBackendLoadFunc(bool isSuccess, string className, string functionName, string errorInfo);

        // �⺻���� ����
        public virtual void BackendLoad(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;
            afterBackendLoadFunc(true, className, funcName, String.Empty);
        }
    }
}
