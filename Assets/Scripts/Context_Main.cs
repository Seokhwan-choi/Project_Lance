using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    //�׻� ����� Ŭ����
    public class Context_Main : MonoBehaviour
    {
        private void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Lance.InitInGame(gameObject);
        }
    }
}