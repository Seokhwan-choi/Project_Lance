using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Lance
{
    class LevelUpTextUI : MonoBehaviour
    {
        Character mTarget;
        Transform mFollowBase;
        Transform mMotionBase;
        Image mImageLevelUp;
        Tweener mVisibleTweener;

        public void Init(Character target)
        {
            mTarget = target;
            mFollowBase = gameObject.FindGameObject("Follower").transform;
            mMotionBase = gameObject.FindGameObject("Motion").transform;
            mImageLevelUp = gameObject.FindComponent<Image>("Image_LevelUp");

            SetPos();
            StartMotion();
        }

        void SetPos()
        {
            Bounds bounds;
            if (mTarget is Player)
            {
                bounds = mTarget.FindComponent<SpriteRenderer>("Body").bounds;

                Vector3 uiPos = Util.WorldToScreenPoint(new Vector3(bounds.center.x - 0.1f, bounds.min.y + 0.5f));

                mFollowBase.position = uiPos;
            }
        }

        void StartMotion()
        {
            mFollowBase.DOMoveY(mFollowBase.position.y + 50f, 0.5f);

            mMotionBase.transform.localScale = Vector3.one * 1.25f;
            mMotionBase.DOScale(1f, 1f)
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    mMotionBase.DOKill();

                    CloseMotion();
                });
        }

        void CloseMotion()
        {
            mVisibleTweener = mImageLevelUp.DOFade(0f, 0.5f)
                .OnComplete(() =>
                {
                    mVisibleTweener.Rewind();
                    mVisibleTweener.Kill();
                    mVisibleTweener = null;

                    mTarget = null;
                    mFollowBase = null;
                    mMotionBase = null;
                    mImageLevelUp = null;

                    Lance.ObjectPool.ReleaseUI(gameObject);
                });
        }
    }
}