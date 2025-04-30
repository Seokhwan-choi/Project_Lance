using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using DG.Tweening;
using System;

namespace Lance
{
    public enum FollowType
    {
        None,
    }

    struct FollowInfo
    {
        public Transform FollowTm;
        public FollowType FollowType;
        public void Reset()
        {
            FollowTm = null;
            FollowType = FollowType.None;
        }
    }

    class CameraManager : MonoBehaviour
    {
        const float OffsetX = 0.5f;
        const float FixedPosY = 0;
        const float FixedPosZ = -10f;

        float mMovePower;
        Vector3 mMovePos;
        FollowInfo mFollowInfo;

        Camera mMainCamera;
        Tweener mShakeTweener;
        public Camera MainCamera => mMainCamera;
        public void Init()
        {
            mMainCamera = Camera.main;
            mMovePower = Const.CameraDefaultMovePower;
        }
        void LateUpdate()
        {
            if (mFollowInfo.FollowTm != null)
            {
                mMovePos = GetFollowPos();
            }
            else
            {
                mMovePos = new Vector3(-2f, FixedPosY, FixedPosZ);
            }
            
            MoveToTarget(mMovePos, Time.deltaTime);
        }

        public void SetFollowInfo(FollowInfo info)
        {
            mFollowInfo = info;
        }

        Vector3 GetFollowPos()
        {
            Vector3 followPos = mFollowInfo.FollowTm.position;

            return new Vector3(followPos.x, FixedPosY, FixedPosZ);
        }

        public void MoveToTarget(Vector3 target, float dt)
        {
            Vector3 endValue = new Vector3(target.x + OffsetX, FixedPosY, FixedPosZ);

            SetMainCameraPos(Vector3.Lerp(mMainCamera.transform.position, endValue, dt * GetMovePower()));
        }

        public void SetMainCameraPos(Vector3 pos)
        {
            mMainCamera.transform.position = pos;
        }

        public void Shake(float duration = 0.25f, float strength = 0.125f)
        {
            if (SaveBitFlags.CameraEffect.IsOn())
            {
                if (mShakeTweener == null)
                {
                    mShakeTweener = mMainCamera.DOShakePosition(duration, strength: strength, vibrato: 30);
                }
                else
                {
                    mShakeTweener.Rewind();
                    mShakeTweener.Kill();
                    mShakeTweener = null;

                    mShakeTweener = mMainCamera.DOShakePosition(duration, strength: strength, vibrato: 30);
                }
            }
        }

        float GetMovePower()
        {
            return mMovePower;
        }

        public void SetMovePower(float movePower)
        {
            mMovePower = movePower;
        }
    }
}

