using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using DG.Tweening;

namespace Lance
{
    class PlayerAnim : CharacterAnim
    {
        SpriteLibrary mBodyLibrary;
        SpriteLibrary mHandLibrary;
        SpriteLibrary mEtcLibrary;
        SpriteRenderer mBodyRenderer;
        SpriteRenderer mHandRenderer;
        SpriteRenderer mEtcRenderer;

        Sequence mHitSequence;
        public override void Init(Character parent)
        {
            base.Init(parent);

            mBodyRenderer = parent.FindComponent<SpriteRenderer>("Body");
            mBodyRenderer.color = Color.white;

            mHandRenderer = parent.FindComponent<SpriteRenderer>("Hand");
            mHandRenderer.color = Color.white;

            mEtcRenderer = parent.FindComponent<SpriteRenderer>("Etc");
            mEtcRenderer.color = Color.white;

            mBodyLibrary = mBodyRenderer.GetComponent<SpriteLibrary>();
            mHandLibrary = mHandRenderer.GetComponent<SpriteLibrary>();
            mEtcLibrary = mEtcRenderer.GetComponent<SpriteLibrary>();
        }

        public override void OnRelease()
        {
            base.OnRelease();

            if (mHitSequence != null)
            {
                mHitSequence.Kill();
                mHitSequence = null;
            }
        }

        public override void PlayHit()
        {
            mBodyRenderer.color = Color.red;
            mHandRenderer.color = Color.red;
            mEtcRenderer.color = Color.red;

            if (mHitSequence != null)
            {
                mHitSequence.Rewind();
                mHitSequence.Play();
            }
            else
            {
                mHitSequence = DOTween.Sequence()
                    .Append(mBodyRenderer.DOColor(Color.white, 0.25f).SetEase(Ease.OutBack))
                    .Join(mHandRenderer.DOColor(Color.white, 0.25f).SetEase(Ease.OutBack))
                    .Join(mEtcRenderer.DOColor(Color.white, 0.25f).SetEase(Ease.OutBack))
                    .SetAutoKill(false);
            }
        }

        public override void SetBodyLibraryAsset(string assetId)
        {
            var libraryAssets = Lance.SpriteLibrary.GetCostumeLibraryAsset(assetId);
            if (libraryAssets != null)
            {
                mBodyLibrary.spriteLibraryAsset = libraryAssets;
            }
        }

        public override void SetHandLibraryAsset(string assetId)
        {
            var libraryAsset = Lance.SpriteLibrary.GetCostumeLibraryAsset(assetId);
            if (libraryAsset != null)
            {
                mHandLibrary.spriteLibraryAsset = libraryAsset;
            }
        }

        public override void SetEtcLibraryAsset(string assetId, int orderInLayer)
        {
            var libraryAsset = Lance.SpriteLibrary.GetCostumeLibraryAsset(assetId);
            if (libraryAsset != null)
            {
                mEtcLibrary.spriteLibraryAsset = libraryAsset;
                mEtcRenderer.sortingOrder = orderInLayer;
            }
        }
    }
}