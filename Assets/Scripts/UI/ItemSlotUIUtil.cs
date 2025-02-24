using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    static class ItemSlotUIUtil
    {
        public static List<ItemSlotUI> CreateItemSlotUIList(RectTransform parent, MultiReward[] rewards, string objName = "")
        {
            List<ItemSlotUI> list = new List<ItemSlotUI>();

            foreach (MultiReward reward in rewards)
            {
                ItemSlotUI itemSlotUI = Create(parent, objName);

                itemSlotUI.Init(reward);

                list.Add(itemSlotUI);
            }

            return list;
        }

        public static List<ItemSlotUI> CreateItemSlotUIList(RectTransform parent, RewardResult reward, string objName = "")
        {
            List<ItemSlotUI> list = new List<ItemSlotUI>();

            if (reward.exp > 0)
            {
                ItemSlotUI expSlot = Create(parent, objName);

                expSlot.Init(new ItemInfo(ItemType.Exp, reward.exp));

                list.Add(expSlot);
            }

            if (reward.gold > 0)
            {
                ItemSlotUI goldSlot = Create(parent, objName);

                goldSlot.Init(new ItemInfo(ItemType.Gold, reward.gold));

                list.Add(goldSlot);
            }

            if (reward.gem > 0)
            {
                ItemSlotUI gemSlot = Create(parent, objName);

                gemSlot.Init(new ItemInfo(ItemType.Gem, reward.gem));

                list.Add(gemSlot);
            }

            if (reward.stones > 0)
            {
                ItemSlotUI stoneSlot = Create(parent, objName);

                stoneSlot.Init(new ItemInfo(ItemType.UpgradeStone, reward.stones));

                list.Add(stoneSlot);
            }

            if (reward.reforgeStones > 0)
            {
                ItemSlotUI reforgeStoneSlot = Create(parent, objName);

                reforgeStoneSlot.Init(new ItemInfo(ItemType.ReforgeStone, reward.reforgeStones));

                list.Add(reforgeStoneSlot);
            }

            if (reward.skillPiece > 0)
            {
                ItemSlotUI skillPieceSlot = Create(parent, objName);

                skillPieceSlot.Init(new ItemInfo(ItemType.SkillPiece, reward.skillPiece));

                list.Add(skillPieceSlot);
            }

            if (reward.petFood > 0)
            {
                ItemSlotUI petFoodSlot = Create(parent, objName);

                petFoodSlot.Init(new ItemInfo(ItemType.PetFood, reward.petFood));

                list.Add(petFoodSlot);
            }

            if (reward.elementalStone > 0)
            {
                ItemSlotUI elementalStoneSlot = Create(parent, objName);

                elementalStoneSlot.Init(new ItemInfo(ItemType.ElementalStone, reward.elementalStone));

                list.Add(elementalStoneSlot);
            }

            if (reward.ticketBundle > 0)
            {
                ItemSlotUI bundleSlot = Create(parent, objName);

                bundleSlot.Init(new ItemInfo(ItemType.TicketBundle, reward.ticketBundle));

                list.Add(bundleSlot);
            }

            if (reward.ancientEssence > 0)
            {
                ItemSlotUI ancientEssenceSlot = Create(parent, objName);

                ancientEssenceSlot.Init(new ItemInfo(ItemType.AncientEssence, reward.ancientEssence));

                list.Add(ancientEssenceSlot);
            }

            if (reward.joustCoin > 0)
            {
                ItemSlotUI joustCoinSlot = Create(parent, objName);

                joustCoinSlot.Init(new ItemInfo(ItemType.JoustingCoin, reward.joustCoin));

                list.Add(joustCoinSlot);
            }

            if (reward.joustTicket > 0)
            {
                ItemSlotUI joustTicketSlot = Create(parent, objName);

                joustTicketSlot.Init(new ItemInfo(ItemType.JoustingTicket, reward.joustTicket));

                list.Add(joustTicketSlot);
            }

            if (reward.gloryToken > 0)
            {
                ItemSlotUI gloryTokenSlot = Create(parent, objName);

                gloryTokenSlot.Init(new ItemInfo(ItemType.GloryToken, reward.gloryToken));

                list.Add(gloryTokenSlot);
            }

            if (reward.soulStone > 0)
            {
                ItemSlotUI soulStoneSlot = Create(parent, objName);

                soulStoneSlot.Init(new ItemInfo(ItemType.SoulStone, reward.soulStone));

                list.Add(soulStoneSlot);
            }

            if (reward.manaEssence > 0)
            {
                ItemSlotUI manaEssenceSlot = Create(parent, objName);

                manaEssenceSlot.Init(new ItemInfo(ItemType.ManaEssence, reward.manaEssence));

                list.Add(manaEssenceSlot);
            }

            if (reward.costumeUpgrade > 0)
            {
                ItemSlotUI costumeUpgradeSlot = Create(parent, objName);

                costumeUpgradeSlot.Init(new ItemInfo(ItemType.CostumeUpgrade, reward.costumeUpgrade));

                list.Add(costumeUpgradeSlot);
            }

            if (reward.tickets != null)
            {
                for(int i = 0; i < reward.tickets.Length; ++i)
                {
                    if (reward.tickets[i] > 0)
                    {
                        DungeonType type = (DungeonType)i;

                        ItemSlotUI ticketSlot = Create(parent, objName);

                        ticketSlot.Init(new ItemInfo(type.ChangeToItemType(), reward.tickets[i]));

                        list.Add(ticketSlot);
                    }
                }
            }

            if (reward.demonicRealmStones != null)
            {
                for (int i = 0; i < reward.demonicRealmStones.Length; ++i)
                {
                    if (reward.demonicRealmStones[i] > 0)
                    {
                        DemonicRealmType type = (DemonicRealmType)i;

                        ItemSlotUI stoneSlot = Create(parent, objName);

                        stoneSlot.Init(new ItemInfo(type.ChangeToItemType(), reward.demonicRealmStones[i]));

                        list.Add(stoneSlot);
                    }
                }
            }

            if (reward.essences != null)
            {
                for (int i = 0; i < reward.essences.Length; ++i)
                {
                    if (reward.essences[i] > 0)
                    {
                        EssenceType type = (EssenceType)i;

                        ItemSlotUI essenceSlot = Create(parent, objName);

                        essenceSlot.Init(new ItemInfo(type.ChangeToItemType(), reward.essences[i]));

                        list.Add(essenceSlot);
                    }
                }
            }

            if (reward.mileage > 0)
            {
                ItemSlotUI mileageSlot = Create(parent, objName);

                mileageSlot.Init(new ItemInfo(ItemType.Mileage, reward.mileage));

                list.Add(mileageSlot);
            }

            if (reward.equipments != null)
            {
                var equipmentSlots = CreateItemSlotUIList(parent, reward.equipments.GatherReward(), objName);

                list.AddRange(equipmentSlots);
            }

            if (reward.accessorys != null)
            {
                var accessorySlots = CreateItemSlotUIList(parent, reward.accessorys.GatherReward(), objName);

                list.AddRange(accessorySlots);
            }

            if (reward.skills != null)
            {
                var skillSlots = CreateItemSlotUIList(parent, reward.skills.GatherReward(), objName);

                list.AddRange(skillSlots);
            }

            if (reward.artifacts != null)
            {
                var artifactSlots = CreateItemSlotUIList(parent, reward.artifacts.GatherReward(), objName);

                list.AddRange(artifactSlots);
            }

            if (reward.costumes != null)
            {
                var costumeSlots = CreateItemSlotUIList(parent, reward.costumes.GatherReward(), objName);

                list.AddRange(costumeSlots);
            }

            if (reward.achievements != null)
            {
                var achievementSlots = CreateItemSlotUIList(parent, reward.achievements.GatherReward(), objName);

                list.AddRange(achievementSlots);
            }

            if (reward.petEquipments != null)
            {
                var petEquipmentSlots = CreateItemSlotUIList(parent, reward.petEquipments.GatherReward(), objName);

                list.AddRange(petEquipmentSlots);
            }

            if (reward.eventCurrencys != null)
            {
                var eventCurrencySlots = CreateItemSlotUIList(parent, reward.eventCurrencys.GatherReward(), objName);

                list.AddRange(eventCurrencySlots);
            }

            return list;
        }

        static ItemSlotUI Create(RectTransform parent, string objName = "")
        {
            var itemSlotObj = Lance.ObjectPool.AcquireUI(objName.IsValid() ? objName : "ItemSlotUI", parent);

            return itemSlotObj.GetOrAddComponent<ItemSlotUI>();
        }
    }
}


