using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace Lance
{
    class Popup_UpdatePowerLevelUI : PopupBase
    {
        float mCloseTime;
        public void Init(double powerLevel, bool isUp)
        {
            var textPowerLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_PowerLevel");
            textPowerLevel.SetColor(isUp ? "B4F3B4" : "F22042");
            textPowerLevel.text = powerLevel.ToAlphaString();

            var imageArrowUp = gameObject.FindGameObject("Image_ArrowUp");
            var imageArrowDown = gameObject.FindGameObject("Image_ArrowDown");

            imageArrowUp.SetActive(isUp);
            imageArrowDown.SetActive(!isUp);

            mCloseTime = 2f;
        }

        private void Update()
        {
            if (mCloseTime > 0)
            {
                mCloseTime -= Time.unscaledDeltaTime;

                if (mCloseTime <= 0 && mClosing == false)
                {
                    Close(hideMotion:false);
                }
            }
        }
    }
}