using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    class Skill_Pet_Water_2 : PetSkill
    {
        float mDuration;
        public override void Init(PlayerPet playerPet, SkillData skillData)
        {
            mPlayerPet = playerPet;
            mSkillData = skillData;
            mIsFinish = false;
            mDelay = 0;
            mDuration = skillData.duration;
            mSounds = new List<AudioSource>();
            mAnim = gameObject.GetComponent<Animation>();

            foreach (var skill in gameObject.GetComponentsInChildren<Skill_Pet_Water>(true))
            {
                skill.Init(playerPet, skillData);
            }
        }

        protected override void OnUpdate(float dt)
        {
            mDuration -= dt;
            if (mDuration <= 0f)
            {
                OnFinish();
            }
        }
    }
}