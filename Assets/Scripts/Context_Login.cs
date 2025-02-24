using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    //�׻� ����� Ŭ����
    public class Context_Login : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;

            Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.Normal;
            QualitySettings.vSyncCount = 0; //��������ȭ Off...����ī�� �Ҷ�Ҷ�

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Input.multiTouchEnabled = false;
        }
    }
}