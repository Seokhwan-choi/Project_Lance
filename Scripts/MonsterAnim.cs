using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using DG.Tweening;


namespace Lance
{
    class MonsterAnim : CharacterAnim
    {
        SpriteRenderer mRenderer;
        SpriteLibrary mSpriteLibrary;
        Tweener mRendererHitTweener;
        Tweener mRendererKillTwenner;
        public override void Init(Character parent)
        {
            base.Init(parent);

            mRenderer = mAnim.gameObject.GetComponent<SpriteRenderer>();
            mRenderer.color = Color.white;

            mSpriteLibrary = parent.GetComponentInChildren<SpriteLibrary>();
        }

        public override void OnRelease()
        {
            if (mRendererHitTweener != null)
            {
                mRendererHitTweener.Kill();
                mRendererHitTweener = null;
            }

            if (mRendererKillTwenner != null)
            {
                mRendererKillTwenner.Kill();
                mRendererKillTwenner = null;
            }
        }

        public override void SetBodyLibraryAsset(string assetId)
        {
            var asset = Lance.SpriteLibrary.GetAsset(assetId);

            if (mSpriteLibrary != null && asset != null)
                mSpriteLibrary.spriteLibraryAsset = asset;
        }

        public override void PlayDeath()
        {
            base.PlayDeath();

            if (mRendererKillTwenner != null)
            {
                mRendererKillTwenner.Rewind();
                mRendererKillTwenner.Play();
            }
            else
            {
                mRendererKillTwenner = mRenderer.DOFade(0f, 1.25f)
                .SetAutoKill(false);
            }
        }

        public override void PlayHit()
        {
            mRenderer.color = Color.red;

            if (mRendererHitTweener != null)
            {
                mRendererHitTweener.Rewind();
                mRendererHitTweener.Play();
            }
            else
            {
                mRendererHitTweener = mRenderer.DOColor(Color.white, 0.25f).SetEase(Ease.OutBack)
                .SetAutoKill(false);
            }
        }
    }
}