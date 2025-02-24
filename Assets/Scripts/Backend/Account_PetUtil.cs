using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    public partial class Account
    {
        public ElementalType GetElementalType()
        {
            return Pet.GetCurrentElementalType();
        }
        // 펫 레벨업
        public bool CanLevelUpPet(string id)
        {
            // 최고 레벨인지 확인
            if (Pet.IsMaxLevel(id))
                return false;

            // 펫 먹이가 충분한지 확인
            int requirePetFood = Pet.GetRequirePetFood(id);
            if (Currency.GetPetFood() < requirePetFood)
                return false;

            return true;
        }

        public (bool, int) FeedPet(string id, int petFood)
        {
            // 최고 레벨인지 확인
            if (Pet.IsMaxLevel(id))
                return (false, 0);

            // 펫 먹이가 충분한지 확인
            if (Currency.GetPetFood() < petFood)
                return (false, 0);

            int curExp = Pet.GetCurrentExp(id);
            int totalPetFood = curExp + petFood;

            int needFood = petFood;
            int maxLevelRequireFood = Pet.GetMaxLevelRequirePetFood(id);

            if (totalPetFood > maxLevelRequireFood)
            {
                needFood = maxLevelRequireFood;
            }

            // 펫 먹이를 소모하고
            if (Currency.UsePetFood(needFood))
            {
                // 펫 경험치 누적
                return Pet.StackExp(id, needFood);
            }

            return (false, 0);
        }

        public bool AnyCanEvolutionPet()
        {
            foreach(var key in Pet.GetKeys())
            {
                if (CanEvolutionPet(key))
                {
                    return true;
                }
            }

            return false;
        }

        // 펫 진화
        public bool CanEvolutionPet(string id)
        {
            // 최고 단계인지 확인
            if (Pet.IsMaxStep(id))
                return false;

            // 최고 레벨인지 확인
            if (Pet.IsMaxLevel(id) == false)
                return false;

            // 속성석이 충분한지 확인
            var require = Pet.GetRequireElementalStone(id);

            return Currency.IsEnoughElementalStone(require);
        }

        public bool EvolutionPet(string id)
        {
            // 최고 단계인지 확인
            if (Pet.IsMaxStep(id))
                return false;

            // 최고 레벨인지 확인
            if (Pet.IsMaxLevel(id) == false)
                return false;

            // 속성석이 충분한지 확인
            var require = Pet.GetRequireElementalStone(id);
            if (Currency.IsEnoughElementalStone(require) == false)
                return false;

            // 속성석 소모하고
            if (Currency.UseElementalStone(require))
            {
                // 펫 진화
                Pet.EvolutionPet(id);

                return true;
            }

            return false;
        }

        public int GetChangePetEvolutionStatRequireStone(string id)
        {
            var pet = Pet.GetPet(id);
            if (pet == null)
                return int.MaxValue;

            return pet.GetRequireEvolutionStatStone();
        }

        public bool AnyCanChangePetEvolutionStat(string id)
        {
            var pet = Pet.GetPet(id);
            if (pet == null)
                return false;

            return pet.AnyCanChangeEvolutionStat();
        }

        public bool ChangePetEvolutionStat(string id)
        {
            int petEvolutionStep = Pet.GetStep(id);
            if (petEvolutionStep == 0)
                return false;

            int statLockCount = Pet.GetEvolutionStatLockCount(id);
            if (Lance.GameData.PetCommonData.evolutionStatMaxSlot == statLockCount)
                return false;

            int changePrice = DataUtil.GetPetEvolutionChangePrice(statLockCount);
            if (Currency.IsEnoughElementalStone(changePrice) == false)
                return false;

            if (Currency.UseElementalStone(changePrice))
            {
                if (Pet.ChangeEvolutionStats(id))
                {
                    Pet.SetIsChangedData(true);

                    return true;
                }
            }

            return false;
        }
    }
}