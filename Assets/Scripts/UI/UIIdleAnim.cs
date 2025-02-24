using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lance
{
    class UIIdleAnim : MonoBehaviour
    {
        public List<Sprite> ImageList;
        public float PlaySpeedSeconds = 1f;

        Coroutine mRoutine;
        private void OnEnable()
        {
            this.GetComponent<Image>().sprite = ImageList[0];

            mRoutine = StartCoroutine(ChangeImageCoroutine());
        }

        private void OnDisable()
        {
            StopCoroutine(mRoutine);
        }

        int mCurrentImgIdx; // 현재 선택된 이미지의 인덱스
        IEnumerator ChangeImageCoroutine()
        {
            while (true)
            {
                this.GetComponent<Image>().sprite = ImageList[mCurrentImgIdx];
                mCurrentImgIdx = (mCurrentImgIdx + 1) % ImageList.Count;
                yield return new WaitForSeconds(PlaySpeedSeconds);
            }
        }
    }
}