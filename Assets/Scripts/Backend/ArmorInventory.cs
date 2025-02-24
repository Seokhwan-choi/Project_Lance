using BackEnd;
using LitJson;

namespace Lance
{
    public class ArmorInventory : Inventory
    {
        public override string GetTableName()
        {
            return "ArmorInventory";
        }
    }
}