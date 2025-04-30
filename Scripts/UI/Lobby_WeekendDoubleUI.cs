using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace Lance
{
    class Lobby_WeekendDoubleUI : MonoBehaviour
    {
        public void Init()
        {
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (WeekendDoubleUtil.IsDoubleEvent())
            {
                if (gameObject.activeSelf == false)
                    gameObject.SetActive(true);
            }
            else
            {
                if (gameObject.activeSelf)
                    gameObject.SetActive(false);
            }
        }
    }
}