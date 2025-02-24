using BackEnd;
using LitJson;

namespace Lance
{
    public class GlovesInventory : Inventory 
    {
        public override string GetTableName()
        {
            return "GlovesInventory";
        }
    }
}