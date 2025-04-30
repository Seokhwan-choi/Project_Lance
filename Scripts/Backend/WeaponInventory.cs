using BackEnd;
using LitJson;

namespace Lance
{
    // 무기 인벤토리, DB 최적화를 위해서 쪼개 놓음
    public class WeaponInventory : Inventory 
    {
        public override string GetTableName()
        {
            return "WeaponInventory";
        }
    }
}

