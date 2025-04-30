using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class BackgroundScroller : MonoBehaviour  
    {
        bool mIsPause;
        float mBackgroundSize;
        float mAdjSpeedValue;
        float mCurSpeed;

        public float Speed;
        public int Index;
        public SpriteRenderer[] Backgrounds;
        float CalcSpeed()
        {
            if (mAdjSpeedValue > 0)
            {
                return Speed * (1 + mAdjSpeedValue);
            }
            else
            {
                return Speed;
            }
        }

        public void Init()
        {
            if (Backgrounds.Length > 0)
                mBackgroundSize = Backgrounds[0].bounds.size.x;
        }

        public void OnUpdate(float dt)
        {
            if (mIsPause || Backgrounds.Length <= 0)
                return;

            if (mForceTime > 0)
            {
                if (mCurSpeed > 0)
                    mCurSpeed -= dt;

                mForceTime -= dt;

                for (int i = 0; i < Backgrounds.Length; ++i)
                {
                    Backgrounds[i].transform.Translate(Vector2.right * dt * mCurSpeed);

                    float nowBackgroundPosX = Backgrounds[i].transform.localPosition.x;

                    if (mBackgroundSize * 2f <= nowBackgroundPosX)
                    {
                        float newPosX = nowBackgroundPosX - (mBackgroundSize * Backgrounds.Length);

                        Backgrounds[i].transform.localPosition = new Vector3(newPosX, 0, 0);
                    }
                }

            }
            else
            {
                if (mCurSpeed < CalcSpeed())
                {
                    mCurSpeed += (dt * Const.Acceleration);
                }

                for (int i = 0; i < Backgrounds.Length; ++i)
                {
                    Backgrounds[i].transform.Translate(Vector2.left * dt * mCurSpeed);

                    float nowBackgroundPosX = Backgrounds[i].transform.localPosition.x;

                    if (-mBackgroundSize * 2f >= nowBackgroundPosX)
                    {
                        float newPosX = nowBackgroundPosX + (mBackgroundSize * Backgrounds.Length);

                        Backgrounds[i].transform.localPosition = new Vector3(newPosX, 0, 0);
                    }
                }
            }
        }

        public void ChangeBackground(StageType stageType, int chapter)
        {
            for (int i = 0; i < Backgrounds.Length; ++i)
            {
                Backgrounds[i].sprite = Lance.Atlas.GetBackground(stageType, chapter, Index);
            }
        }

        float mForceTime;
        public void PlayKnockback()
        {
            mForceTime = Const.DefaultKnockbackTime;

            mCurSpeed = CalcSpeed() * 2f;
        }

        public void SetPause(bool pause)
        {
            mIsPause = pause;

            if (mIsPause)
            {
                mCurSpeed = 0f;
            }
        }

        public void SetAdjSpeedValue(float value)
        {
            mAdjSpeedValue = value;
        }
    }
}
