using UnityEngine;
using UnityEngine.UI;

namespace Lance
{
    class ManaHeartExpUI : MonoBehaviour
    {
        RectTransform mExpUI;

        public void Init()
        {
            mExpUI = GetComponent<Image>().rectTransform;
        }

        // 130 ~ 390 좌표 움직여주면댐
        public void SetRatio(float ratio)
        {
            ratio = Mathf.Min(ratio, 1);

            float localPos = 260 * ratio;

            localPos += 130;    // 이미지 크기의 절반 만큼 보정

            mExpUI.localPosition = Vector2.up * localPos;
        }
    }
}