using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Lance
{
    class Lobby_BossAppearUI : MonoBehaviour
    {
        StageType mStageType;
        RectTransform mRectTm;
        float mCloseTime;

        public void Init()
        {
            gameObject.SetActive(false);

            mRectTm = gameObject.FindComponent<RectTransform>("Frame_Rectangle");
        }

        public void OnUpdate()
        {
            var dt = Time.deltaTime;

            if (mCloseTime > 0)
            {
                mCloseTime -= dt;
                if (mCloseTime <= 0f)
                {
                    Hide();
                }
            }
        }

        public void Show(StageType stageType, float closeTime = 2f)
        {
            gameObject.SetActive(true);
            mStageType = stageType;
            mCloseTime = closeTime;

            StartCoroutine(PlayBossAppearSound());

            Vector2 endValue = new Vector2(mRectTm.sizeDelta.x, 300f);

            mRectTm.sizeDelta = new Vector2(mRectTm.sizeDelta.x, 0f);
            mRectTm.DORewind();
            mRectTm.DOSizeDelta(endValue, 0.25f)
                .SetAutoKill(false)
                .SetEase(Ease.OutBack);
        }
        

        IEnumerator PlayBossAppearSound()
        {
            SoundPlayer.PlayBossAppear();

            yield return new WaitForSeconds(0.5f);

            //SoundPlayer.PlayBossAppear();
        }
        void Hide()
        {
            mRectTm.sizeDelta = new Vector2(mRectTm.sizeDelta.x, 300f);

            Vector2 endValue = new Vector2(mRectTm.sizeDelta.x, 0f);

            mRectTm.DOSizeDelta(endValue, 0.25f)
                .SetAutoKill(false)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
        }
    }
}