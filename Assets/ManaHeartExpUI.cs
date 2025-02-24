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

        // 130 ~ 390 ��ǥ �������ָ��
        public void SetRatio(float ratio)
        {
            ratio = Mathf.Min(ratio, 1);

            float localPos = 260 * ratio;

            localPos += 130;    // �̹��� ũ���� ���� ��ŭ ����

            mExpUI.localPosition = Vector2.up * localPos;
        }
    }
}