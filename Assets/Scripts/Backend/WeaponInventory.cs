using BackEnd;
using LitJson;

namespace Lance
{
    // ���� �κ��丮, DB ����ȭ�� ���ؼ� �ɰ� ����
    public class WeaponInventory : Inventory 
    {
        public override string GetTableName()
        {
            return "WeaponInventory";
        }
    }
}

