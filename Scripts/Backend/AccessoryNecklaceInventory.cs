using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public class AccessoryNecklaceInventory : AccessoryInventory
    {
        public override string GetTableName()
        {
            return "NecklaceInventory";
        }
    }
}