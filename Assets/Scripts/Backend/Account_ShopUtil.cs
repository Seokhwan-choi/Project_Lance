using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public partial class Account
    {
        public bool PurchaseNormalItem(string id, int count)
        {
            var data = Lance.GameData.NormalShopData.TryGet(id);
            if (data == null)
                return false;

            var inst = NormalShop.GetInst(id);
            if (inst == null)
                return false;

            if (inst.GetRemainPurchaseCount() < count)
                return false;

            int totalPrice = count * data.price;

            if (IsEnoughGem(totalPrice) == false)
                return false;

            if (UseGem(totalPrice))
            {
                inst.Purchase(count);

                NormalShop.SetIsChangedData(true);
            }

            return true;
        }

        public bool PurchaseJoustItem(string id, int count)
        {
            var data = Lance.GameData.JoustShopData.TryGet(id);
            if (data == null)
                return false;

            var inst = JoustShop.GetInst(id);
            if (inst == null)
                return false;

            if (inst.GetRemainPurchaseCount() < count)
                return false;

            int totalPrice = count * data.price;

            if (IsEnoughJoustCoin(totalPrice) == false)
                return false;

            if (UseJoustCoin(totalPrice))
            {
                inst.Purchase(count);

                JoustShop.SetIsChangedData(true);
            }

            return true;
        }

        public bool PurchaseDemonicRealmSpoilsItem(string id, int count)
        {
            var data = Lance.GameData.DemonicRealmSpoilsData.TryGet(id);
            if (data == null)
                return false;

            var inst = DemonicRealmSpoils.GetInst(id);
            if (inst == null)
                return false;

            if (DemonicRealmSpoils.IsEnoughPurchaseCount(id, count) == false)
                return false;

            if (inst.GetRequireFriendShipLevel() > data.requireFriendShipLevel)
                return false;

            int totalPrice = count * data.price;

            if (IsEnoughSoulStone(totalPrice) == false)
                return false;

            if (UseSoulStone(totalPrice))
            {
                return DemonicRealmSpoils.Purchase(id, count);
            }
            else
            {
                return false;
            }
        }
    }
}