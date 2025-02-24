using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    //항상 실행될 클래스
    public class Context_Login : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;

            Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.Normal;
            QualitySettings.vSyncCount = 0; //수직동기화 Off...비디오카드 불라불라

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Input.multiTouchEnabled = false;
        }
    }
}