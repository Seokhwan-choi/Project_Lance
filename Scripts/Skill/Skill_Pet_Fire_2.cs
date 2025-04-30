using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    class Skill_Pet_Fire_2 : PetSkill
    {
        public override void Init(PlayerPet playerPet, SkillData skillData)
        {
            base.Init(playerPet, skillData);

            foreach (var skillFireball in gameObject.GetComponentsInChildren<Skill_Fireball>(true))
            {
                skillFireball.Init(this);
            }
        }
    }
}