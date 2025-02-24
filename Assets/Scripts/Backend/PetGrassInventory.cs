using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public class PetGrassInventory : PetInventory
    {
        public override string GetTableName()
        {
            return "PetGrassInventory";
        }
    }
}