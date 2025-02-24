using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    class Skill_Pet_Fire : PetSkill
    {
        public override void Init(PlayerPet playerPet, SkillData skillData)
        {
            base.Init(playerPet, skillData);

            var skillFireball = gameObject.GetComponentInChildren<Skill_Fireball>(true);

            skillFireball.Init(this);
        }
    }
}