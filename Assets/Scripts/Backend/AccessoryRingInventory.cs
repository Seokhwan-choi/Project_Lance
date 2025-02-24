using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public class AccessoryRingInventory : AccessoryInventory
    {
        public override string GetTableName()
        {
            return "RingInventory";
        }
    }
}