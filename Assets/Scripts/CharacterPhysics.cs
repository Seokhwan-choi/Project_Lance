using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Lance
{
    class CharacterPhysics
    {
        float mBodySize;
        bool mInForce;
        Character mParent;
        Transform mTm;
        Rigidbody2D mRigidbody2D;
        public float BodySize => mBodySize;
        public void Init(Character parent)
        {
            mParent = parent;
            mBodySize = parent.GetBodySize();
            mRigidbody2D = parent.GetComponent<Rigidbody2D>();
            mTm = parent.transform;
            mTm.rotation = Quaternion.Euler(Vector3.zero);

            ResetKnockback();
        }

        public void OnUpdate(float dt)
        {
            if (mInForce == false)
                return;

            mForceTime -= dt;
            if (mForceTime <= 0)
            {
                mInForce = false;

                ResetKnockback();
            }
        }

        public bool IsInRange(CharacterPhysics physics, float range)
        {
            float distance = Math.Abs(GetPosition().x - physics.GetPosition().x);

            bool isInRange = distance <= (range + (physics.BodySize * 0.5f));

            return isInRange;
        }

        public bool IsOverLap(CharacterPhysics physics)
        {
            return IsInRange(physics, mBodySize * 0.5f);
        }

        public void SetPosition(Vector2 pos)
        {
            mTm.position = pos;
        }

        public Vector2 GetPosition()
        {
            return mTm?.position ?? Vector2.zero;
        }

        public void MoveTo(float dt, float moveSpeed)
        {
            mTm.Translate(Vector2.left * dt * moveSpeed);
        }

        public void ResetKnockback()
        {
            mRigidbody2D.linearVelocity = Vector2.zero;
            mRigidbody2D.angularVelocity = 0f;
        }

        float mForceTime;
        public void PlayKnockback(Vector2 force)
        {
            mInForce = true;
            mForceTime = Const.DefaultKnockbackTime;
            mRigidbody2D.AddForce(force, ForceMode2D.Impulse);
        }

        public void OnDeath()
        {
            mInForce = true;
            mForceTime = 0.25f;

            mRigidbody2D.gravityScale = 1;
            mRigidbody2D.AddForce(
                (Vector2.right * UnityEngine.Random.Range(-3, 3f) + 
                 Vector2.up * UnityEngine.Random.Range(3f, 5f)) , ForceMode2D.Impulse);
            mRigidbody2D.AddTorque(180f, ForceMode2D.Force);
        }

        public void OnRelease()
        {
            mInForce = false;
            mRigidbody2D.gravityScale = 0;

            if (mParent.IsPlayer)
            {
                ResetKnockback();

                mTm.position = new Vector2(mTm.position.x, 0);
            }

            mParent = null;
            mRigidbody2D = null;
            mTm = null;
        }
    }
}