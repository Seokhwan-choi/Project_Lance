using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public class AccessoryEarringInventory : AccessoryInventory
    {
        public override string GetTableName()
        {
            return "EarringInventory";
        }
    }
}