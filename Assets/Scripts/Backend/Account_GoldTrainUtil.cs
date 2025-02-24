using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public partial class Account
    {
        public (bool canTrain, double totalRequireGold) CanTrain(StatType type, int trainCount)
        {
            // �Ʒ��� ������ �������� Ȯ���Ѵ�.
            var result = GoldTrain.CanTrain(type, trainCount);

            if (DataUtil.HaveGoldTrainRequireType(type))
            {
                if (GoldTrain.IsSatisfiedRequireType(type) == false)
                    return (false, result.totalRequireGold);
            }
            
            return result;
        }

        public bool Train(StatType type, int trainCount)
        {
            // �Ʒ��� ������ �������� Ȯ���Ѵ�.
            (bool canTrain, double totalRequireGold) = CanTrain(type, trainCount);
            if (canTrain == false)
                return false;

            // ��带 ���� �Ҹ��ϰ�
            if (UseGold(totalRequireGold) == false)
                return false;

            // �Ʒ��� ����
            GoldTrain.Train(type, trainCount);

            return true;
        }
    }
}


