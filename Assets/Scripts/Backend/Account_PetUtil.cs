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
        // �� ������
        public bool CanLevelUpPet(string id)
        {
            // �ְ� �������� Ȯ��
            if (Pet.IsMaxLevel(id))
                return false;

            // �� ���̰� ������� Ȯ��
            int requirePetFood = Pet.GetRequirePetFood(id);
            if (Currency.GetPetFood() < requirePetFood)
                return false;

            return true;
        }

        public (bool, int) FeedPet(string id, int petFood)
        {
            // �ְ� �������� Ȯ��
            if (Pet.IsMaxLevel(id))
                return (false, 0);

            // �� ���̰� ������� Ȯ��
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

            // �� ���̸� �Ҹ��ϰ�
            if (Currency.UsePetFood(needFood))
            {
                // �� ����ġ ����
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

        // �� ��ȭ
        public bool CanEvolutionPet(string id)
        {
            // �ְ� �ܰ����� Ȯ��
            if (Pet.IsMaxStep(id))
                return false;

            // �ְ� �������� Ȯ��
            if (Pet.IsMaxLevel(id) == false)
                return false;

            // �Ӽ����� ������� Ȯ��
            var require = Pet.GetRequireElementalStone(id);

            return Currency.IsEnoughElementalStone(require);
        }

        public bool EvolutionPet(string id)
        {
            // �ְ� �ܰ����� Ȯ��
            if (Pet.IsMaxStep(id))
                return false;

            // �ְ� �������� Ȯ��
            if (Pet.IsMaxLevel(id) == false)
                return false;

            // �Ӽ����� ������� Ȯ��
            var require = Pet.GetRequireElementalStone(id);
            if (Currency.IsEnoughElementalStone(require) == false)
                return false;

            // �Ӽ��� �Ҹ��ϰ�
            if (Currency.UseElementalStone(require))
            {
                // �� ��ȭ
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