using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    //항상 실행될 클래스
    public class Context_Main : MonoBehaviour
    {
        private void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Lance.InitInGame(gameObject);
        }
    }
}