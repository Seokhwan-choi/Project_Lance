using BackEnd;
using LitJson;

namespace Lance
{
    public class ShoesInventory : Inventory 
    {
        public override string GetTableName()
        {
            return "ShoesInventory";
        }
    }
}