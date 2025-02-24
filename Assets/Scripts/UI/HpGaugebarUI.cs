using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


namespace Lance
{
    class HpGaugebarUI : MonoBehaviour
    {
        Character mParent;
        Image mImageFront;
        CanvasGroup mCanvasGroup;
        Tweener mRendererHideTwenner;
        public void Init(Character parent)
        {
            mParent = parent;
            mImageFront = gameObject.FindComponent<Image>("Image_Front");
            mCanvasGroup = GetComponent<CanvasGroup>();
            mCanvasGroup.alpha = 1f;

            UpdatePos();
            UpdateGaugebar();
        }
        public void UpdatePos()
        {
            if (mParent is Player)
            {
                Bounds bounds = mParent.FindComponent<SpriteRenderer>("Body").bounds;

                Vector3 uiPos = Util.WorldToScreenPoint(new Vector3(bounds.center.x - 0.1f, bounds.min.y + 0.1f));

                transform.position = uiPos;
            }
            else
            {
                Bounds bounds = mParent.GetBounds();
                Bounds bounds2 = mParent.GetComponent<SpriteRenderer>().bounds;

                Vector3 uiPos = Util.WorldToScreenPoint(new Vector3(bounds.center.x, bounds2.min.y - 0.05f));

                transform.position = uiPos;
            }
        }

        public void UpdateGaugebar()
        {
            mImageFront.fillAmount = (float)(mParent.Stat.CurHp / mParent.Stat.MaxHp);
        }

        public void OnDeath()
        {
            if (mRendererHideTwenner != null)
            {
                mRendererHideTwenner.Rewind();
                mRendererHideTwenner.Play();
            }
            else
            {
                mRendererHideTwenner = mCanvasGroup.DOFade(0f, 0.5f)
                    .SetAutoKill(false);
            }
        }

        public void OnRelease()
        {
            mParent = null;
            mImageFront = null;
            if (mRendererHideTwenner != null)
            {
                mRendererHideTwenner.Kill();
                mRendererHideTwenner = null;
            }
            Lance.ObjectPool.ReleaseUI(gameObject);
        }
    }
}


