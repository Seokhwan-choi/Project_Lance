using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using System.Linq;

namespace Lance
{
    static class RewardResultUtil
    {
        public static RewardResult CreatePostRewardResult(JsonData data)
        {
            RewardResult reward = new RewardResult();

            double itemCountTemp = 0;

            double.TryParse(data["itemCount"].ToString(), out itemCountTemp);

            if (Enum.TryParse(data["item"]["itemType"].ToString(), out ItemType itemType))
            {
                if (itemType == ItemType.Gold)
                {
                    reward.gold = itemCountTemp;
                }
                else if (itemType == ItemType.Gem)
                {
                    reward.gem = itemCountTemp;
                }
                else if (itemType == ItemType.Exp)
                {
                    reward.exp = itemCountTemp;
                }
                else if (itemType == ItemType.UpgradeStone)
                {
                    reward.stones = itemCountTemp;
                }
                else if (itemType == ItemType.ReforgeStone)
                {
                    reward.reforgeStones = itemCountTemp;
                }
                else if (itemType == ItemType.GoldTicket)
                {
                    reward.tickets = reward.tickets.AddTicket(DungeonType.Gold, (int)itemCountTemp);
                }
                else if (itemType == ItemType.StoneTicket)
                {
                    reward.tickets = reward.tickets.AddTicket(DungeonType.Stone, (int)itemCountTemp);
                }
                else if (itemType == ItemType.PetTicket)
                {
                    reward.tickets = reward.tickets.AddTicket(DungeonType.Pet, (int)itemCountTemp);
                }
                else if (itemType == ItemType.GrowthTicket)
                {
                    reward.tickets = reward.tickets.AddTicket(DungeonType.Growth, (int)itemCountTemp);
                }
                else if (itemType == ItemType.RaidTicket)
                {
                    reward.tickets = reward.tickets.AddTicket(DungeonType.Raid, (int)itemCountTemp);
                }
                else if (itemType == ItemType.ReforgeTicket)
                {
                    reward.tickets = reward.tickets.AddTicket(DungeonType.Reforge, (int)itemCountTemp);
                }
                else if (itemType == ItemType.AncientTicket)
                {
                    reward.tickets = reward.tickets.AddTicket(DungeonType.Ancient, (int)itemCountTemp);
                }
                else if (itemType == ItemType.TicketBundle)
                {
                    reward.ticketBundle = (int)itemCountTemp;
                }
                else if (itemType == ItemType.DemonicRealmStone_Accessory)
                {
                    reward.demonicRealmStones = reward.demonicRealmStones.AddStone(DemonicRealmType.Accessory, (int)itemCountTemp);
                }
                else if (itemType == ItemType.AncientEssence)
                {
                    reward.ancientEssence = (int)itemCountTemp;
                }
                else if (itemType == ItemType.Essence_Chapter1)
                {
                    reward.essences = reward.essences.AddEssence(EssenceType.Chapter1, (int)itemCountTemp);
                }
                else if (itemType == ItemType.Essence_Chapter2)
                {
                    reward.essences = reward.essences.AddEssence(EssenceType.Chapter2, (int)itemCountTemp);
                }
                else if (itemType == ItemType.Essence_Chapter3)
                {
                    reward.essences = reward.essences.AddEssence(EssenceType.Chapter3, (int)itemCountTemp);
                }
                else if (itemType == ItemType.Essence_Chapter4)
                {
                    reward.essences = reward.essences.AddEssence(EssenceType.Chapter4, (int)itemCountTemp);
                }
                else if (itemType == ItemType.Essence_Chapter5)
                {
                    reward.essences = reward.essences.AddEssence(EssenceType.Chapter5, (int)itemCountTemp);
                }
                else if (itemType == ItemType.Mileage)
                {
                    reward.mileage = (int)itemCountTemp;
                }
                else if (itemType == ItemType.PetFood)
                {
                    reward.petFood = (int)itemCountTemp;
                }
                else if (itemType == ItemType.ElementalStone)
                {
                    reward.elementalStone = (int)itemCountTemp;
                }
                else if (itemType == ItemType.JoustingCoin)
                {
                    reward.joustCoin = (int)itemCountTemp;
                }
                else if (itemType == ItemType.GloryToken)
                {
                    reward.gloryToken = (int)itemCountTemp;
                }
                else if (itemType == ItemType.JoustingTicket)
                {
                    reward.joustTicket = (int)itemCountTemp;
                }
                else if (itemType == ItemType.SoulStone)
                {
                    reward.soulStone = (int)itemCountTemp;
                }
                else if (itemType == ItemType.ManaEssence)
                {
                    reward.manaEssence = (int)itemCountTemp;
                }
                else if (itemType == ItemType.CostumeUpgrade)
                {
                    reward.costumeUpgrade = itemCountTemp;
                }
                else 
                {
                    if (data["item"].ContainsKey("itemId"))
                    {
                        string itemId = data["item"]["itemId"].ToString();

                        var multiReward = new MultiReward[] { new MultiReward(itemType, itemId, (int)itemCountTemp) };

                        if (itemType.IsEquipment())
                        {
                            reward.equipments = multiReward;
                        }
                        else if (itemType.IsAccessory())
                        {
                            reward.accessorys = multiReward;
                        }
                        else if (itemType.IsSkill())
                        {
                            reward.skills = multiReward;
                        }
                        else if (itemType == ItemType.Costume)
                        {
                            reward.costumes = multiReward;
                        }
                        else if (itemType == ItemType.Achievement)
                        {
                            reward.achievements = multiReward;
                        }
                        else if (itemType == ItemType.PetEquipment)
                        {
                            reward.petEquipments = multiReward;
                        }
                        else if (itemType == ItemType.Artifact || itemType == ItemType.AncientArtifact)
                        {
                            reward.artifacts = multiReward;
                        }
                        else if (itemType == ItemType.Reward)
                        {
                            var rewardData = Lance.GameData.RewardData.TryGet(itemId);

                            reward = reward.AddReward(rewardData);
                        }
                        else
                        {
                            reward.eventCurrencys = multiReward;
                        }
                    }
                }
            }

            return reward;
        }
    }

    static class RewardResultExt
    {
        public static RewardResult BonusReward(this RewardResult reward, float bonusValue)
        {
            reward.exp += (reward.exp * bonusValue);
            reward.gold += (reward.gold * bonusValue);
            reward.gem += (reward.gem * bonusValue);
            reward.stones += (reward.stones * bonusValue);
            reward.reforgeStones += (reward.reforgeStones * bonusValue);
            reward.skillPiece += (int)((float)reward.skillPiece * bonusValue);
            reward.petFood += (int)((float)reward.petFood * bonusValue);
            reward.elementalStone += (int)((float)reward.elementalStone * bonusValue);
            reward.ticketBundle += (int)((float)reward.ticketBundle * bonusValue);
            reward.ancientEssence += (int)((float)reward.ancientEssence * bonusValue);
            reward.joustCoin += (int)((float)reward.joustCoin * bonusValue);
            reward.joustTicket += (int)((float)reward.joustTicket * bonusValue);
            reward.gloryToken += (int)((float)reward.gloryToken * bonusValue);
            reward.soulStone += (int)((float)reward.soulStone * bonusValue);
            reward.manaEssence += (int)((float)reward.manaEssence * bonusValue);
            reward.costumeUpgrade += reward.costumeUpgrade * bonusValue;

            if (reward.essences != null)
                reward.essences = reward.essences.BonusEssence(bonusValue);
            if (reward.tickets != null)
                reward.tickets = reward.tickets.BonusTicket(bonusValue);
            if (reward.demonicRealmStones != null)
                reward.demonicRealmStones = reward.demonicRealmStones.BonusStone(bonusValue);
            if (reward.equipments != null)
                reward.equipments = reward.equipments.BonusArray(bonusValue);
            if (reward.accessorys != null)
                reward.accessorys = reward.accessorys.BonusArray(bonusValue);
            if (reward.skills != null)
                reward.skills = reward.skills.BonusArray(bonusValue);
            if (reward.artifacts != null)
                reward.artifacts = reward.artifacts.BonusArray(bonusValue);
            if (reward.petEquipments != null)
                reward.petEquipments = reward.petEquipments.BonusArray(bonusValue);
            if (reward.eventCurrencys != null)
                reward.eventCurrencys = reward.eventCurrencys.BonusArray(bonusValue);

            return reward;
        }

        public static RewardResult AddReward(this RewardResult reward, ItemInfo itemInfo)
        {
            switch (itemInfo.Type)
            {
                case ItemType.Gold:
                    reward.gold += itemInfo.Amount;
                    break;
                case ItemType.Gem:
                    reward.gem += itemInfo.Amount;
                    break;
                case ItemType.Exp:
                    reward.exp += itemInfo.Amount;
                    break;
                case ItemType.UpgradeStone:
                    reward.stones += itemInfo.Amount;
                    break;
                case ItemType.ReforgeStone:
                    reward.reforgeStones += itemInfo.Amount;
                    break;
                case ItemType.SkillPiece:
                    reward.skillPiece += (int)itemInfo.Amount;
                    break;
                case ItemType.PetFood:
                    reward.petFood += (int)itemInfo.Amount;
                    break;
                case ItemType.ElementalStone:
                    reward.elementalStone += (int)itemInfo.Amount;
                    break;
                case ItemType.Mileage:
                    reward.mileage += (int)itemInfo.Amount;
                    break;
                case ItemType.TicketBundle:
                    reward.ticketBundle += (int)itemInfo.Amount;
                    break;
                case ItemType.AncientEssence:
                    reward.ancientEssence += (int)itemInfo.Amount;
                    break;
                case ItemType.JoustingCoin:
                    reward.joustCoin += (int)itemInfo.Amount;
                    break;
                case ItemType.JoustingTicket:
                    reward.joustTicket += (int)itemInfo.Amount;
                    break;
                case ItemType.GloryToken:
                    reward.gloryToken += (int)itemInfo.Amount;
                    break;
                case ItemType.SoulStone:
                    reward.soulStone += (int)itemInfo.Amount;
                    break;
                case ItemType.ManaEssence:
                    reward.manaEssence += (int)itemInfo.Amount;
                    break;
                case ItemType.CostumeUpgrade:
                    reward.costumeUpgrade += itemInfo.Amount;
                    break;
                case ItemType.GoldTicket:
                case ItemType.StoneTicket:
                case ItemType.PetTicket:
                case ItemType.ReforgeTicket:
                case ItemType.GrowthTicket:
                case ItemType.RaidTicket:
                case ItemType.AncientTicket:
                    {
                        if (reward.tickets == null)
                            reward.tickets = Enumerable.Repeat(0, (int)DungeonType.Count).Select(x => x).ToArray();

                        DungeonType dungeonType = itemInfo.Type.ChangeToDungeonType();

                        reward.tickets[(int)dungeonType] += (int)itemInfo.Amount;

                        break;
                    }
                case ItemType.DemonicRealmStone_Accessory:
                    {
                        if (reward.demonicRealmStones == null)
                            reward.demonicRealmStones = Enumerable.Repeat(0, (int)DemonicRealmType.Count).Select(x => x).ToArray();

                        DemonicRealmType demonicRealmType = itemInfo.Type.ChangeToDemonicRealmType();

                        reward.demonicRealmStones[(int)demonicRealmType] += (int)itemInfo.Amount;

                        break;
                    }
                case ItemType.Essence_Chapter1:
                case ItemType.Essence_Chapter2:
                case ItemType.Essence_Chapter3:
                case ItemType.Essence_Chapter4:
                case ItemType.Essence_Chapter5:
                    {
                        if (reward.essences == null)
                            reward.essences = Enumerable.Repeat(0, (int)EssenceType.Count).Select(x => x).ToArray();

                        EssenceType essenceType = itemInfo.Type.ChangeToEssenceType();

                        reward.essences[(int)essenceType] += (int)itemInfo.Amount;

                        break;
                    }
                case ItemType.Weapon:
                case ItemType.Armor:
                case ItemType.Gloves:
                case ItemType.Shoes:
                    List<MultiReward> temp = new List<MultiReward>();

                    if (reward.equipments != null)
                        temp.AddRange(reward.equipments);

                    if (itemInfo.Id.IsValid())
                    {
                        temp.Add(new MultiReward(itemInfo.Type, itemInfo.Id, (int)itemInfo.Amount));
                    }

                    reward.equipments = temp.ToArray();

                    temp = null;

                    break;
                case ItemType.Necklace:
                case ItemType.Earring:
                case ItemType.Ring:
                    List<MultiReward> accessoryTemp = new List<MultiReward>();

                    if (reward.accessorys != null)
                        accessoryTemp.AddRange(reward.accessorys);

                    if (itemInfo.Id.IsValid())
                    {
                        accessoryTemp.Add(new MultiReward(itemInfo.Type, itemInfo.Id, (int)itemInfo.Amount));
                    }

                    reward.accessorys = accessoryTemp.ToArray();

                    temp = null;

                    break;
                case ItemType.Skill:
                    List<MultiReward> temp2 = new List<MultiReward>();

                    if (reward.skills != null)
                        temp2.AddRange(reward.skills);

                    if (itemInfo.Id.IsValid())
                    {
                        temp2.Add(new MultiReward(itemInfo.Type, itemInfo.Id, (int)itemInfo.Amount));
                    }

                    reward.skills = temp2.ToArray();

                    temp2 = null;

                    break;
                case ItemType.Artifact:
                case ItemType.AncientArtifact:

                    List<MultiReward> temp3 = new List<MultiReward>();

                    if (reward.artifacts != null)
                        temp3.AddRange(reward.artifacts);

                    if (itemInfo.Id.IsValid())
                    {
                        temp3.Add(new MultiReward(itemInfo.Type, itemInfo.Id, (int)itemInfo.Amount));
                    }

                    reward.artifacts = temp3.ToArray();

                    temp3 = null;

                    break;
                case ItemType.Costume:
                    List<MultiReward> temp4 = new List<MultiReward>();

                    if (reward.costumes != null)
                        temp4.AddRange(reward.costumes);

                    if (itemInfo.Id.IsValid())
                    {
                        temp4.Add(new MultiReward(itemInfo.Type, itemInfo.Id, (int)itemInfo.Amount));
                    }

                    reward.costumes = temp4.ToArray();

                    temp4 = null;
                    
                    break;

                case ItemType.Achievement:
                    List<MultiReward> temp6 = new List<MultiReward>();

                    if (reward.achievements != null)
                        temp6.AddRange(reward.achievements);

                    if (itemInfo.Id.IsValid())
                    {
                        temp6.Add(new MultiReward(itemInfo.Type, itemInfo.Id, (int)itemInfo.Amount));
                    }

                    reward.achievements = temp6.ToArray();

                    temp6 = null;

                    break;

                case ItemType.PetEquipment:
                    List<MultiReward> temp7 = new List<MultiReward>();

                    if (reward.petEquipments != null)
                        temp7.AddRange(reward.petEquipments);

                    if (itemInfo.Id.IsValid())
                    {
                        temp7.Add(new MultiReward(itemInfo.Type, itemInfo.Id, (int)itemInfo.Amount));
                    }

                    reward.petEquipments = temp7.ToArray();

                    temp7 = null;

                    break;

                case ItemType.EventCurrency:
                    List<MultiReward> temp5 = new List<MultiReward>();

                    if (reward.eventCurrencys != null)
                        temp5.AddRange(reward.eventCurrencys);

                    if (itemInfo.Id.IsValid())
                    {
                        temp5.Add(new MultiReward(itemInfo.Type, itemInfo.Id, (int)itemInfo.Amount));
                    }

                    reward.eventCurrencys = temp5.ToArray();

                    temp5 = null;
                    break;

                default:
                    break;
            }

            return reward;
        }

        public static RewardResult AddReward(this RewardResult reward, MultiReward multiReward)
        {
            switch (multiReward.ItemType)
            {
                case ItemType.Weapon:
                case ItemType.Armor:
                case ItemType.Gloves:
                case ItemType.Shoes:
                    List<MultiReward> temp = new List<MultiReward>();

                    if (reward.equipments != null)
                        temp.AddRange(reward.equipments);

                    if (multiReward.Id.IsValid())
                    {
                        temp.Add(new MultiReward(multiReward.ItemType, multiReward.Id, multiReward.Count));
                    }

                    reward.equipments = temp.ToArray();

                    break;
                case ItemType.Necklace:
                case ItemType.Earring:
                case ItemType.Ring:
                    List<MultiReward> accessoryTemp = new List<MultiReward>();

                    if (reward.accessorys != null)
                        accessoryTemp.AddRange(reward.accessorys);

                    if (multiReward.Id.IsValid())
                    {
                        accessoryTemp.Add(new MultiReward(multiReward.ItemType, multiReward.Id, multiReward.Count));
                    }

                    reward.accessorys = accessoryTemp.ToArray();

                    break;
                case ItemType.Skill:
                    List<MultiReward> temp2 = new List<MultiReward>();

                    if (reward.skills != null)
                        temp2.AddRange(reward.skills);

                    if (multiReward.Id.IsValid())
                    {
                        temp2.Add(new MultiReward(multiReward.ItemType, multiReward.Id, multiReward.Count));
                    }

                    reward.skills = temp2.ToArray();
                    break;
                case ItemType.Artifact:
                case ItemType.AncientArtifact:

                    List<MultiReward> temp3 = new List<MultiReward>();

                    if (reward.artifacts != null)
                        temp3.AddRange(reward.artifacts);

                    if (multiReward.Id.IsValid())
                    {
                        temp3.Add(new MultiReward(multiReward.ItemType, multiReward.Id, multiReward.Count));
                    }

                    reward.artifacts = temp3.ToArray();
                    break;
                case ItemType.Costume:
                    List<MultiReward> temp4 = new List<MultiReward>();

                    if (reward.costumes != null)
                        temp4.AddRange(reward.costumes);

                    if (multiReward.Id.IsValid())
                    {
                        temp4.Add(new MultiReward(multiReward.ItemType, multiReward.Id, (int)multiReward.Count));
                    }

                    reward.costumes = temp4.ToArray();

                    temp4 = null;

                    break;
                case ItemType.Achievement:
                    List<MultiReward> temp5 = new List<MultiReward>();

                    if (reward.achievements != null)
                        temp5.AddRange(reward.achievements);

                    if (multiReward.Id.IsValid())
                    {
                        temp5.Add(new MultiReward(multiReward.ItemType, multiReward.Id, (int)multiReward.Count));
                    }

                    reward.achievements = temp5.ToArray();

                    temp5 = null;

                    break;

                case ItemType.PetEquipment:
                    List<MultiReward> temp7 = new List<MultiReward>();

                    if (reward.petEquipments != null)
                        temp7.AddRange(reward.petEquipments);

                    if (multiReward.Id.IsValid())
                    {
                        temp7.Add(new MultiReward(multiReward.ItemType, multiReward.Id, multiReward.Count));
                    }

                    reward.petEquipments = temp7.ToArray();

                    temp7 = null;

                    break;

                case ItemType.EventCurrency:
                    List<MultiReward> temp6 = new List<MultiReward>();

                    if (reward.eventCurrencys != null)
                        temp6.AddRange(reward.eventCurrencys);

                    if (multiReward.Id.IsValid())
                    {
                        temp6.Add(new MultiReward(multiReward.ItemType, multiReward.Id, (int)multiReward.Count));
                    }

                    reward.eventCurrencys = temp6.ToArray();

                    temp6 = null;

                    break;
                default:
                    break;
            }

            return reward;
        }

        public static RewardResult AddReward(this RewardResult reward, RewardResult reward2)
        {
            reward.gold += reward2.gold;
            reward.gem += reward2.gem;
            reward.exp += reward2.exp;
            reward.stones += reward2.stones;
            reward.reforgeStones += reward2.reforgeStones;
            reward.mileage += reward2.mileage;
            reward.skillPiece += reward2.skillPiece;
            reward.petFood += reward2.petFood;
            reward.elementalStone += reward2.elementalStone;
            reward.ticketBundle += reward2.ticketBundle;
            reward.ancientEssence += reward2.ancientEssence;
            reward.joustCoin += reward2.joustCoin;
            reward.joustTicket += reward2.joustTicket;
            reward.gloryToken += reward2.gloryToken;
            reward.soulStone += reward2.soulStone;
            reward.manaEssence += reward2.manaEssence;
            reward.costumeUpgrade += reward2.costumeUpgrade;

            reward.essences = reward.essences.AddEssence(reward2.essences);
            reward.tickets = reward.tickets.AddTicket(reward2.tickets);
            reward.demonicRealmStones = reward.demonicRealmStones.AddStone(reward2.demonicRealmStones);
            reward.equipments = reward.equipments.AddArray(reward2.equipments);
            reward.accessorys = reward.accessorys.AddArray(reward2.accessorys);
            reward.skills = reward.skills.AddArray(reward2.skills);
            reward.artifacts = reward.artifacts.AddArray(reward2.artifacts);
            reward.costumes = reward.costumes.AddArray(reward2.costumes);
            reward.achievements = reward.achievements.AddArray(reward2.achievements);
            reward.petEquipments = reward.petEquipments.AddArray(reward2.petEquipments);
            reward.eventCurrencys = reward.eventCurrencys.AddArray(reward2.eventCurrencys);

            return reward;
        }

        public static RewardResult AddReward(this RewardResult reward, RewardData rewardData)
        {
            if (rewardData.gold > 0)
                reward.gold += rewardData.gold;
            if (rewardData.gem > 0)
                reward.gem += rewardData.gem;
            if (rewardData.upgradeStone > 0)
                reward.stones += rewardData.upgradeStone;
            if (rewardData.reforgeStone > 0)
                reward.reforgeStones += rewardData.reforgeStone;
            if (rewardData.skillPiece > 0)
                reward.skillPiece += rewardData.skillPiece;
            if (rewardData.petFood > 0)
                reward.petFood += rewardData.petFood;
            if (rewardData.elementalStone > 0)
                reward.elementalStone += rewardData.elementalStone;
            if (rewardData.mileage > 0)
                reward.mileage += rewardData.mileage;
            if (rewardData.ticketBundle > 0)
                reward.ticketBundle += rewardData.ticketBundle;
            if (rewardData.ancientEssence > 0)
                reward.ancientEssence += rewardData.ancientEssence;
            if (rewardData.joustingCoin > 0)
                reward.joustCoin += rewardData.joustingCoin;
            if (rewardData.joustingTicket > 0)
                reward.joustTicket += rewardData.joustingTicket;
            if (rewardData.gloryToken > 0)
                reward.gloryToken += rewardData.gloryToken;
            if (rewardData.soulStone > 0)
                reward.soulStone += rewardData.soulStone;
            if (rewardData.manaEssence > 0)
                reward.manaEssence += rewardData.manaEssence;
            if (rewardData.costumeUpgrade > 0)
                reward.costumeUpgrade += rewardData.costumeUpgrade;
            if (rewardData.dungeonTicket.Any(x => x > 0))
            {
                if (reward.tickets != null)
                {
                    if (reward.tickets.Length == rewardData.dungeonTicket.Length)
                    {
                        for (int i = 0; i < reward.tickets.Length; ++i)
                        {
                            reward.tickets[i] = reward.tickets[i] + rewardData.dungeonTicket[i];
                        }
                    }
                }
                else
                {
                    reward.tickets = rewardData.dungeonTicket.Select(x => x).ToArray();
                }
            }
            if (rewardData.demonicRealmStone.Any(x => x > 0))
            {
                if (reward.demonicRealmStones != null)
                {
                    if (reward.demonicRealmStones.Length == rewardData.demonicRealmStone.Length)
                    {
                        for (int i = 0; i < reward.demonicRealmStones.Length; ++i)
                        {
                            reward.demonicRealmStones[i] = reward.demonicRealmStones[i] + rewardData.demonicRealmStone[i];
                        }
                    }
                }
                else
                {
                    reward.demonicRealmStones = rewardData.demonicRealmStone.Select(x => x).ToArray();
                }
            }
            if (rewardData.essence.Any(x => x > 0))
            {
                if (reward.essences != null)
                {
                    if (reward.essences.Length == rewardData.essence.Length)
                    {
                        for (int i = 0; i < reward.essences.Length; ++i)
                        {
                            reward.essences[i] = reward.essences[i] + rewardData.essence[i];
                        }
                    }
                }
                else
                {
                    reward.essences = rewardData.essence.Select(x => x).ToArray();
                }
            }

            if (rewardData.equipment.IsValid())
            {
                List<MultiReward> temp = new List<MultiReward>();

                if (reward.equipments != null)
                    temp.AddRange(reward.equipments);

                string[] equipments = rewardData.equipment.SplitByDelim();
                if (equipments != null)
                {
                    for (int i = 0; i < equipments.Length; ++i)
                    {
                        var equipmentData = DataUtil.GetEquipmentData(equipments[i]);
                        if (equipmentData == null)
                        {
                            if (equipments[i].SplitWord('*', out string id, out string count))
                            {
                                equipmentData = DataUtil.GetEquipmentData(id);
                                if (equipmentData != null)
                                {
                                    AddEquipment(equipmentData, count.ToIntSafe(1));
                                }
                            }
                        }
                        else
                        {
                            AddEquipment(equipmentData, 1);
                        }
                        
                    }

                    void AddEquipment(EquipmentData equipmentData, int count)
                    {
                        temp.Add(new MultiReward(equipmentData.type, equipmentData.id, count));
                    }

                    reward.equipments = temp.ToArray();

                    temp = null;
                }
            }

            if (rewardData.accessory.IsValid())
            {
                List<MultiReward> temp = new List<MultiReward>();

                if (reward.accessorys != null)
                    temp.AddRange(reward.accessorys);

                string[] accessorys = rewardData.accessory.SplitByDelim();
                if (accessorys != null)
                {
                    for (int i = 0; i < accessorys.Length; ++i)
                    {
                        var accessoryData = DataUtil.GetAccessoryData(accessorys[i]);
                        if (accessoryData == null)
                        {
                            if (accessorys[i].SplitWord('*', out string id, out string count))
                            {
                                accessoryData = DataUtil.GetAccessoryData(id);
                                if (accessoryData != null)
                                {
                                    AddAccessory(accessoryData, count.ToIntSafe(1));
                                }
                            }
                        }
                        else
                        {
                            AddAccessory(accessoryData, 1);
                        }

                    }

                    void AddAccessory(AccessoryData accessoryData, int count)
                    {
                        temp.Add(new MultiReward(accessoryData.type, accessoryData.id, count));
                    }

                    reward.accessorys = temp.ToArray();

                    temp = null;
                }
            }

            if (rewardData.skill.IsValid())
            {
                List<MultiReward> temp = new List<MultiReward>();

                if (reward.skills != null)
                    temp.AddRange(reward.skills);

                string[] skills = rewardData.skill.SplitByDelim();
                if (skills != null)
                {
                    for (int i = 0; i < skills.Length; ++i)
                    {
                        var skillData = DataUtil.GetSkillData(skills[i]);
                        if (skillData == null)
                        {
                            if (skills[i].SplitWord('*', out string id, out string count))
                            {
                                skillData = DataUtil.GetSkillData(id);
                                if (skillData != null)
                                {
                                    AddSkill(skillData, count.ToIntSafe(1));
                                }
                            }
                        }
                        else
                        {
                            AddSkill(skillData, 1);
                        }
                    }
                }

                void AddSkill(SkillData skillData, int count)
                {
                    temp.Add(new MultiReward(ItemType.Skill, skillData.id, 1));
                }

                reward.skills = temp.ToArray();

                temp = null;
            }

            if (rewardData.artifact.IsValid()) 
            {
                List<MultiReward> temp = new List<MultiReward>();

                if (reward.artifacts != null)
                    temp.AddRange(reward.artifacts);

                string[] artifacts = rewardData.artifact.SplitByDelim();
                for (int i = 0; i < artifacts.Length; ++i)
                {
                    ArtifactData data = DataUtil.GetArtifactData(artifacts[i]);
                    if (data == null)
                    {
                        if (artifacts[i].SplitWord('*', out string id, out string count))
                        {
                            data = DataUtil.GetArtifactData(id);
                            if (data != null)
                            {
                                AddArtifact(data, count.ToIntSafe(1));
                            }
                        }
                    }
                    else
                    {
                        AddArtifact(data, 1);
                    }
                }

                void AddArtifact(ArtifactData artifactData, int count)
                {
                    bool isAncientArtifact = DataUtil.IsAncientArtifact(artifactData.id);

                    temp.Add(new MultiReward(isAncientArtifact ? ItemType.AncientArtifact : ItemType.Artifact, artifactData.id, count));
                }

                reward.artifacts = temp.ToArray();

                temp = null;
            }

            // 랜덤 장비
            if (rewardData.randomEquipment != null)
            {
                var randomEquipment = DataUtil.GetRandomEquipmentData(rewardData.randomEquipment);
                if (randomEquipment != null)
                {
                    List<MultiReward> temp = new List<MultiReward>();

                    if (reward.equipments != null)
                        temp.AddRange(reward.equipments);

                    temp.Add(new MultiReward(randomEquipment.type, randomEquipment.id, 1));

                    reward.equipments = temp.ToArray();

                    temp = null;
                }
            }

            // 랜덤 장신구
            if (rewardData.randomAccessory != null)
            {
                var randomAccessory = DataUtil.GetRandomAccessoryData(rewardData.randomAccessory);
                if (randomAccessory != null)
                {
                    List<MultiReward> temp = new List<MultiReward>();

                    if (reward.accessorys != null)
                        temp.AddRange(reward.accessorys);

                    temp.Add(new MultiReward(randomAccessory.type, randomAccessory.id, 1));

                    reward.accessorys = temp.ToArray();

                    temp = null;
                }
            }

            // 랜덤 스킬
            if (rewardData.randomSkill != null)
            {
                var randomSkill = DataUtil.GetRandomSkillData(rewardData.randomSkill);
                if (randomSkill != null)
                {
                    List<MultiReward> temp = new List<MultiReward>();

                    if (reward.skills != null)
                        temp.AddRange(reward.skills);

                    temp.Add(new MultiReward(ItemType.Skill, randomSkill.id, 1));

                    reward.skills = temp.ToArray();

                    temp = null;
                }
            }

            // 코스튬
            if (rewardData.costume.IsValid())
            {
                List<MultiReward> temp = new List<MultiReward>();

                if (reward.costumes != null)
                    temp.AddRange(reward.costumes);

                string[] costumes = rewardData.costume.SplitByDelim();
                for (int i = 0; i < costumes.Length; ++i)
                {
                    CostumeData data = DataUtil.GetCostumeData(costumes[i]);
                    if (data != null)
                    {
                        AddCostume(data, 1);
                    }
                }

                void AddCostume(CostumeData costumeData, int count)
                {
                    temp.Add(new MultiReward(ItemType.Costume, costumeData.id, count));
                }

                reward.costumes = temp.ToArray();

                temp = null;
            }

            // 업적
            if (rewardData.achievement.IsValid())
            {
                List<MultiReward> temp = new List<MultiReward>();

                if (reward.achievements != null)
                    temp.AddRange(reward.achievements);

                string[] achievements = rewardData.achievement.SplitByDelim();
                for (int i = 0; i < achievements.Length; ++i)
                {
                    AchievementData data = Lance.GameData.AchievementData.TryGet(achievements[i]);
                    if (data != null)
                    {
                        AddAchievement(data, 1);
                    }
                }

                void AddAchievement(AchievementData costumeData, int count)
                {
                    temp.Add(new MultiReward(ItemType.Achievement, costumeData.id, count));
                }

                reward.achievements = temp.ToArray();

                temp = null;
            }

            // 신물
            if (rewardData.petEquipment.IsValid())
            {
                List<MultiReward> temp = new List<MultiReward>();

                if (reward.petEquipments != null)
                    temp.AddRange(reward.petEquipments);

                string[] petEquipments = rewardData.petEquipment.SplitByDelim();
                if (petEquipments != null)
                {
                    for (int i = 0; i < petEquipments.Length; ++i)
                    {
                        var equipmentData = Lance.GameData.PetEquipmentData.TryGet(petEquipments[i]);
                        if (equipmentData == null)
                        {
                            if (petEquipments[i].SplitWord('*', out string id, out string count))
                            {
                                equipmentData = Lance.GameData.PetEquipmentData.TryGet(id);
                                if (equipmentData != null)
                                {
                                    AddPetEquipment(equipmentData, count.ToIntSafe(1));
                                }
                            }
                        }
                        else
                        {
                            AddPetEquipment(equipmentData, 1);
                        }

                    }

                    void AddPetEquipment(PetEquipmentData equipmentData, int count)
                    {
                        temp.Add(new MultiReward(ItemType.PetEquipment, equipmentData.id, count));
                    }

                    reward.petEquipments = temp.ToArray();

                    temp = null;
                }
            }

            if (rewardData.eventCurrency.IsValid())
            {
                List<MultiReward> temp = new List<MultiReward>();

                if (reward.eventCurrencys != null)
                    temp.AddRange(reward.eventCurrencys);

                string[] eventCurrencys = rewardData.eventCurrency.SplitByDelim();
                for (int i = 0; i < eventCurrencys.Length; ++i)
                {
                    EventData data = Lance.GameData.EventData.TryGet(eventCurrencys[i]);
                    if (data == null)
                    {
                        if (eventCurrencys[i].SplitWord('*', out string id, out string count))
                        {
                            data = Lance.GameData.EventData.TryGet(id);
                            if (data != null)
                            {
                                AddEventCurrency(data.id, count.ToIntSafe(1));
                            }
                        }
                    }
                    else
                    {
                        AddEventCurrency(data.id, 1);
                    }
                }

                void AddEventCurrency(string id, int count)
                {
                    temp.Add(new MultiReward(ItemType.EventCurrency, id, count));
                }

                reward.eventCurrencys = temp.ToArray();

                temp = null;
            }

            return reward;
        }

        public static List<ItemInfo> Split(this RewardResult reward)
        {
            List<ItemInfo> result = new List<ItemInfo>();

            if (reward.gold > 0)
            {
                result.Add(new ItemInfo(ItemType.Gold, reward.gold));
            }
            
            if (reward.gem > 0)
            {
                result.Add(new ItemInfo(ItemType.Gem, reward.gem));
            }
            
            if (reward.exp > 0)
            {
                result.Add(new ItemInfo(ItemType.Exp, reward.exp));
            }

            if (reward.petFood > 0)
            {
                result.Add(new ItemInfo(ItemType.PetFood, reward.petFood));
            }

            if (reward.elementalStone > 0)
            {
                result.Add(new ItemInfo(ItemType.ElementalStone, reward.elementalStone));
            }
            
            if (reward.stones > 0)
            {
                result.Add(new ItemInfo(ItemType.UpgradeStone, reward.stones));
            }

            if (reward.reforgeStones > 0)
            {
                result.Add(new ItemInfo(ItemType.ReforgeStone, reward.reforgeStones));
            }

            if (reward.skillPiece > 0)
            {
                result.Add(new ItemInfo(ItemType.SkillPiece, reward.skillPiece));
            }
            
            if (reward.mileage > 0)
            {
                result.Add(new ItemInfo(ItemType.UpgradeStone, reward.mileage));
            }

            if (reward.ticketBundle > 0)
            {
                result.Add(new ItemInfo(ItemType.TicketBundle, reward.ticketBundle));
            }

            if (reward.ancientEssence > 0)
            {
                result.Add(new ItemInfo(ItemType.AncientEssence, reward.ancientEssence));
            }

            if (reward.joustCoin > 0)
            {
                result.Add(new ItemInfo(ItemType.JoustingCoin, reward.joustCoin));
            }

            if (reward.joustTicket > 0)
            {
                result.Add(new ItemInfo(ItemType.JoustingTicket, reward.joustTicket));
            }

            if (reward.gloryToken > 0)
            {
                result.Add(new ItemInfo(ItemType.GloryToken, reward.gloryToken));
            }

            if (reward.soulStone > 0)
            {
                result.Add(new ItemInfo(ItemType.SoulStone, reward.soulStone));
            }

            if (reward.manaEssence > 0)
            {
                result.Add(new ItemInfo(ItemType.ManaEssence, reward.manaEssence));
            }

            if (reward.costumeUpgrade > 0)
            {
                result.Add(new ItemInfo(ItemType.CostumeUpgrade, reward.costumeUpgrade));
            }

            if (reward.tickets != null)
            {
                for (int i = 0; i < reward.tickets.Length; ++i)
                {
                    if (reward.tickets[i] > 0)
                    {
                        DungeonType type = (DungeonType)i;

                        result.Add(new ItemInfo(type.ChangeToItemType(), reward.tickets[i]));
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

                        result.Add(new ItemInfo(type.ChangeToItemType(), reward.tickets[i]));
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

                        result.Add(new ItemInfo(type.ChangeToItemType(), reward.essences[i]));
                    }
                }
            }

            if (reward.equipments != null)
            {
                reward.equipments = reward.equipments.GatherReward();

                for (int i = 0; i < reward.equipments.Length; ++i)
                {
                    var equipment = reward.equipments[i];
                    if (equipment.Id.IsValid())
                    {
                        var data = DataUtil.GetEquipmentData(equipment.Id);
                        if (data != null)
                        {
                            result.Add(new ItemInfo(data.type, equipment.Count)
                                .SetId(data.id).SetGrade(data.grade).SetSubGrade(data.subGrade));
                        }
                    }
                }
            }

            if (reward.accessorys != null)
            {
                reward.accessorys = reward.accessorys.GatherReward();

                for (int i = 0; i < reward.accessorys.Length; ++i)
                {
                    var accessory = reward.accessorys[i];
                    if (accessory.Id.IsValid())
                    {
                        var data = DataUtil.GetAccessoryData(accessory.Id);
                        if (data != null)
                        {
                            result.Add(new ItemInfo(data.type, accessory.Count)
                                .SetId(data.id).SetGrade(data.grade).SetSubGrade(data.subGrade));
                        }
                    }
                }
            }

            if (reward.skills != null)
            {
                reward.skills = reward.skills.GatherReward();

                for (int i = 0; i < reward.skills.Length; ++i)
                {
                    var skill = reward.skills[i];
                    if (skill.Id.IsValid())
                    {
                        var data = DataUtil.GetSkillData(skill.Id);
                        if (data != null)
                        {
                            result.Add(new ItemInfo(ItemType.Skill, skill.Count)
                            .SetId(data.id).SetGrade(data.grade));
                        }
                    }
                }
            }
            
            if (reward.artifacts != null)
            {
                reward.artifacts = reward.artifacts.GatherReward();

                for (int i = 0; i < reward.artifacts.Length; ++i)
                {
                    var artifact = reward.artifacts[i];
                    if (artifact.Id.IsValid())
                    {
                        var data = DataUtil.GetArtifactData(artifact.Id);
                        if (data != null)
                        {
                            bool isAncientArtifact = DataUtil.IsAncientArtifact(data.id);

                            result.Add(new ItemInfo(isAncientArtifact ? ItemType.AncientArtifact : ItemType.Artifact, 1)
                                .SetId(data.id));
                        }
                    }
                }
            }

            if (reward.costumes != null)
            {
                reward.costumes = reward.costumes.GatherReward();

                for (int i = 0; i < reward.costumes.Length; ++i)
                {
                    var costume = reward.costumes[i];
                    if (costume.Id.IsValid())
                    {
                        var data = DataUtil.GetCostumeData(costume.Id);
                        if (data != null)
                        {
                            result.Add(new ItemInfo(ItemType.Costume, 1)
                                .SetId(data.id));
                        }
                    }
                }
            }

            if (reward.achievements != null)
            {
                reward.achievements = reward.achievements.GatherReward();

                for (int i = 0; i < reward.achievements.Length; ++i)
                {
                    var achievement = reward.achievements[i];
                    if (achievement.Id.IsValid())
                    {
                        var data = Lance.GameData.AchievementData.TryGet(achievement.Id);
                        if (data != null)
                        {
                            result.Add(new ItemInfo(ItemType.Achievement, 1)
                                .SetId(data.id));
                        }
                    }
                }
            }

            if (reward.petEquipments != null)
            {
                reward.petEquipments = reward.petEquipments.GatherReward();

                for (int i = 0; i < reward.petEquipments.Length; ++i)
                {
                    var petEquipment = reward.petEquipments[i];
                    if (petEquipment.Id.IsValid())
                    {
                        var data = Lance.GameData.PetEquipmentData.TryGet(petEquipment.Id);
                        if (data != null)
                        {
                            result.Add(new ItemInfo(ItemType.PetEquipment, petEquipment.Count)
                                .SetId(data.id).SetGrade(data.grade).SetSubGrade(data.subGrade));
                        }
                    }
                }
            }

            if (reward.eventCurrencys != null)
            {
                reward.eventCurrencys = reward.eventCurrencys.GatherReward();

                for (int i = 0; i < reward.eventCurrencys.Length; ++i)
                {
                    var eventCurrency = reward.eventCurrencys[i];
                    if (eventCurrency.Id.IsValid())
                    {
                        var data = Lance.GameData.EventData.TryGet(eventCurrency.Id);
                        if (data != null)
                        {
                            result.Add(new ItemInfo(ItemType.EventCurrency, eventCurrency.Count)
                                .SetId(data.id));
                        }
                    }
                }
            }

            return result;
        }
    }

    static class EssenceUtil
    {
        public static int[] AddEssence(this int[] essences, EssenceType essenceType, int essenceCount)
        {
            if (essences == null)
                essences = Enumerable.Repeat(0, (int)EssenceType.Count).Select(x => x).ToArray();

            int essenceIndex = (int)essenceType;

            if (essences.Length <= (int)essenceIndex || essenceIndex < 0)
                return essences;

            essences[essenceIndex] += essenceCount;

            return essences;
        }

        public static int[] BonusEssence(this int[] reward, float bonusValue)
        {
            var bonusReward = reward.Select(x => (x + Mathf.RoundToInt(x * bonusValue)));

            return bonusReward.ToArray();
        }

        public static int[] AddEssence(this int[] reward, int[] reward2)
        {
            if (reward == null)
                reward = Enumerable.Repeat(0, (int)EssenceType.Count).Select(x => x).ToArray();

            if (reward2 == null)
                reward2 = Enumerable.Repeat(0, (int)EssenceType.Count).Select(x => x).ToArray();

            if (reward.Length != reward2.Length)
                return reward;

            for (int i = 0; i < reward.Length; ++i)
            {
                reward[i] = reward[i] + reward2[i];
            }

            return reward;
        }
    }

    static class DemonicRealmStoneUtil
    {
        public static int[] AddStone(this int[] stones, DemonicRealmType stoneType, int stoneCount)
        {
            if (stones == null)
                stones = Enumerable.Repeat(0, (int)DemonicRealmType.Count).Select(x => x).ToArray();

            int stoneIndex = (int)stoneType;

            if (stones.Length <= (int)stoneIndex || stoneIndex < 0)
                return stones;

            stones[stoneIndex] += stoneCount;

            return stones;
        }

        public static int[] BonusStone(this int[] reward, float bonusValue)
        {
            var bonusReward = reward.Select(x => (x + Mathf.RoundToInt(x * bonusValue)));

            return bonusReward.ToArray();
        }

        public static int[] AddStone(this int[] reward, int[] reward2)
        {
            if (reward == null)
                reward = Enumerable.Repeat(0, (int)DemonicRealmType.Count).Select(x => x).ToArray();

            if (reward2 == null)
                reward2 = Enumerable.Repeat(0, (int)DemonicRealmType.Count).Select(x => x).ToArray();

            if (reward.Length != reward2.Length)
                return reward;

            for (int i = 0; i < reward.Length; ++i)
            {
                reward[i] = reward[i] + reward2[i];
            }

            return reward;
        }
    }

    static class TicketUtil
    {
        public static int[] AddTicket(this int[] tickets, DungeonType ticketType, int ticketCount)
        {
            if (tickets == null)
                tickets = Enumerable.Repeat(0, (int)DungeonType.Count).Select(x => x).ToArray();

            int ticketIndex = (int)ticketType;

            if (tickets.Length <= (int)ticketIndex || ticketIndex < 0)
                return tickets;

            tickets[ticketIndex] += ticketCount;

            return tickets;
        }

        public static int[] BonusTicket(this int[] reward, float bonusValue)
        {
            var bonusReward = reward.Select(x => (x + Mathf.RoundToInt(x * bonusValue)));

            return bonusReward.ToArray();
        }

        public static int[] AddTicket(this int[] reward, int[] reward2)
        {
            if (reward == null)
                reward = Enumerable.Repeat(0, (int)DungeonType.Count).Select(x => x).ToArray();

            if (reward2 == null)
                reward2 = Enumerable.Repeat(0, (int)DungeonType.Count).Select(x => x).ToArray();

            if (reward.Length != reward2.Length)
                return reward;

            for(int i = 0; i < reward.Length; ++i)
            {
                reward[i] = reward[i] + reward2[i];
            }

            return reward;
        }
    }

    static class MultiRewardExt
    {
        public static MultiReward[] BonusArray(this MultiReward[] reward, float bonusValue)
        {
            var bonusReward = reward.Select(x => new MultiReward(x.ItemType, x.Id, x.Count + Mathf.RoundToInt(x.Count * bonusValue)));

            return bonusReward.ToArray();
        }

        public static MultiReward[] AddArray(this MultiReward[] reward, MultiReward[] reward2)
        {
            if (reward != null && reward2 != null)
            {
                var equipmentsTemp = reward.ToList();
                equipmentsTemp.AddRange(reward2);
                reward = equipmentsTemp.ToArray();
            }
            else if (reward == null && reward2 != null)
            {
                reward = reward2;
            }

            return reward;
        }

        public static MultiReward[] GatherReward(this MultiReward[] reward)
        {
            List<MultiReward> gatherRewards = new List<MultiReward>();
            // 아이디만 중복을 제거해서 저장
            var ids = reward.Select(x => x.Id).Distinct().ToArray();

            foreach (var id in ids)
            {
                var temps = reward.Where(x => x.Id == id);

                gatherRewards.Add(new MultiReward()
                {
                    ItemType = temps.First().ItemType,
                    Id = id,
                    Count = temps.Sum(x => x.Count),
                });
            }

            return gatherRewards.ToArray();
        }
    }
}