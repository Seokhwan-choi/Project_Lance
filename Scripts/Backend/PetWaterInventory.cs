using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public class PetWaterInventory : PetInventory
    {
        public override string GetTableName()
        {
            return "PetWaterInventory";
        }
    }
}