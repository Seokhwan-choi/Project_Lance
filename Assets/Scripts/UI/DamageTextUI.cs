using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Lance
{
    class DamageTextUI : MonoBehaviour
    {
        const float MotionPower = 10f;
        const float CriticalMotionPower = 30f;

        Character mTarget;
        Transform mFollowBase;
        Transform mMotionBase;

        Sequence mTweenSequence;
        Tween mMoveTween;
        Tween mPunchTween;
        TextMeshProUGUI mTextDamage;
        public void Init(Character target, double damageText, bool isCritical, bool isSuperCritical)
        {
            mTarget = target;

            if (mFollowBase == null)
                mFollowBase = gameObject.FindGameObject("Follower").transform;

            if (mMotionBase == null)
                mMotionBase = gameObject.FindGameObject("Motion").transform;

            if (mTextDamage == null)
                mTextDamage = gameObject.FindComponent<TextMeshProUGUI>("Text_Damage");

            mTextDamage.color = Color.white;
            mTextDamage.text = damageText.ToAlphaString().ChangeToDamageFont(isCritical, isSuperCritical);
            mTextDamage.fontSize = isCritical ? 72 : 50;

            SetPos();

            mMoveTween?.Rewind();
            mMoveTween?.Kill();
            mMoveTween = null;

            mPunchTween?.Rewind();
            mPunchTween?.Kill();
            mPunchTween = null;

            mTweenSequence?.Kill();
            mTweenSequence = null;

            StartMotion(isCritical);
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
            else
            {
                bounds = mTarget.GetComponent<PolygonCollider2D>().bounds;

                Vector3 uiPos = Util.WorldToScreenPoint(new Vector3(bounds.center.x, bounds.min.y + 0.5f));

                mFollowBase.position = uiPos;
            }
        }

        void StartMotion(bool isCritical)
        {
            mTweenSequence = DOTween.Sequence()
                .Join(mMoveTween = mFollowBase.DOMoveY(mFollowBase.position.y + 50f, 0.5f))
                .Join(mPunchTween = mMotionBase.DOPunchPosition(Vector3.one * (isCritical ? CriticalMotionPower : MotionPower), 0.25f, vibrato: 10))
                .Append(mTextDamage.DOFade(0f, 0.5f)).OnComplete(() => Lance.ObjectPool.ReleaseUI(gameObject));
        }
    }
}