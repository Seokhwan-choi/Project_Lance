using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public partial class Account
    {
        public (bool canTrain, double totalRequireGold) CanTrain(StatType type, int trainCount)
        {
            // 훈련이 가능한 상태인지 확인한다.
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
            // 훈련이 가능한 상태인지 확인한다.
            (bool canTrain, double totalRequireGold) = CanTrain(type, trainCount);
            if (canTrain == false)
                return false;

            // 골드를 먼저 소모하고
            if (UseGold(totalRequireGold) == false)
                return false;

            // 훈련을 적용
            GoldTrain.Train(type, trainCount);

            return true;
        }
    }
}


