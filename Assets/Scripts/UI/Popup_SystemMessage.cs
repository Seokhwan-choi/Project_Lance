using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG;
using DG.Tweening;

namespace Lance
{
    class Popup_SystemMessageUI : PopupBase
    {
        const float MotionPower = 10f;
        public void Init(string systemMessage, float duration = 1f, float startPosY = 524f)
        {
            TextMeshProUGUI textSystemMessage = gameObject.FindComponent<TextMeshProUGUI>("Text_SystemMessage");

            textSystemMessage.text = systemMessage;

            var rectTm = GetComponent<RectTransform>();

            rectTm.anchoredPosition = new Vector2(rectTm.anchoredPosition.x, startPosY);

            StartMotion(duration);
        }

        void StartMotion(float duration)
        {
            Tweener tweener = transform.DOPunchPosition(Vector3.up * MotionPower, duration);

            tweener.OnComplete(() => Close(immediate: false, hideMotion: false));
        }
    }
}