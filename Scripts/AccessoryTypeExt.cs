using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    static class AccessoryTypeExt
    {
        public static ItemType ChangeToItemType(this AccessoryType type)
        {
            switch (type)
            {
                case AccessoryType.Necklace:
                    return ItemType.Necklace;
                case AccessoryType.Earring:
                    return ItemType.Earring;
                case AccessoryType.Ring:
                default:
                    return ItemType.Ring;
            }
        }
    }
}