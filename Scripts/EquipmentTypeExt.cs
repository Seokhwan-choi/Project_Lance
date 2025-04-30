using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    static class EquipmentTypeExt
    {
        public static ItemType ChangeToRandomItemType(this EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.Weapon:
                    return ItemType.Random_Weapon;
                case EquipmentType.Armor:
                    return ItemType.Random_Armor;
                case EquipmentType.Gloves:
                    return ItemType.Random_Gloves;
                case EquipmentType.Shoes:
                default:
                    return ItemType.Random_Shoes;
            }
        }

        public static ItemType ChangeToItemType(this EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.Weapon:
                    return ItemType.Weapon;
                case EquipmentType.Armor:
                    return ItemType.Armor;
                case EquipmentType.Gloves:
                    return ItemType.Gloves;
                case EquipmentType.Shoes:
                default:
                    return ItemType.Shoes;
            }
        }

        
    }
}


