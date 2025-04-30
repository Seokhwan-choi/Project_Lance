using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    class Skill_Pet_Water : PetSkill
    {
        bool mAtkBubble;
        float mDuration;
        List<Skill_ChasingBubble> mChasingBubbleList;
        int mHitCount;
        public override void Init(PlayerPet playerPet, SkillData skillData)
        {
            base.Init(playerPet, skillData);

            mDelay = skillData.atkDelay;
            mDuration = skillData.duration;
            mAtkBubble = false;
        }

        public override void OnRelease()
        {
            base.OnRelease();

            if (mChasingBubbleList != null)
            {
                foreach (Skill_ChasingBubble chasingBubble in mChasingBubbleList)
                {
                    chasingBubble.OnRelease();
                }

                mChasingBubbleList = null;
            }
        }

        protected override void OnUpdate(float dt)
        {
            if (mAtkBubble == false)
            {
                mDelay -= dt;
                if (mDelay <= 0)
                {
                    mAtkBubble = true;

                    CreateChasingBubble();
                }
            }

            if (mChasingBubbleList != null)
            {
                if (mHitCount == mChasingBubbleList.Count)
                {
                    mHitCount = 0;

                    OnFinish();
                }
            }

            mDuration -= dt;
            if (mDuration <= 0f)
            {
                OnFinish();
            }
        }

        protected override void OnFixedUpdate()
        {
            if (mChasingBubbleList != null)
            {
                foreach (var chasingBubble in mChasingBubbleList)
                {
                    chasingBubble.OnFixedUpdate();
                }
            }
        }

        void CreateChasingBubble()
        {
            mChasingBubbleList = new List<Skill_ChasingBubble>();

            foreach (var bubble in gameObject.GetComponentsInChildren<Skill_ChasingBubble>())
            {
                bubble.Init(this);

                mChasingBubbleList.Add(bubble);
            }

            mHitCount = 0;
        }

        public void OnHit()
        {
            mHitCount++;
        }
    }
}