using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Lance
{
    public class ItemInfo
    {
        ItemType mType;
        string mId;
        double mAmount;
        string mShowStr;
        Grade mGrade;
        SubGrade mSubGrade;
        public string Id => mId;
        public string ShowStr => mShowStr;
        public ItemType Type => mType;
        public double Amount => mAmount;
        public SubGrade SubGrade => mSubGrade;
        public ItemInfo(ItemType type)
        {
            mType = type;
        }

        public ItemInfo(ItemType type, double amount)
        {
            mType = type;
            mAmount = amount;
        }

        public ItemInfo(RewardResult reward)
        {
            if (reward.gold > 0)
            {
                mType = ItemType.Gold;
                mAmount = reward.gold;
            }
            else if (reward.gem > 0)
            {
                mType = ItemType.Gem;
                mAmount = reward.gem;
            }
            else if (reward.exp > 0)
            {
                mType = ItemType.Exp;
                mAmount = reward.exp;
            }
            else if (reward.stones > 0)
            {
                mType = ItemType.UpgradeStone;
                mAmount = reward.stones;
            }
            else if (reward.reforgeStones >0)
            {
                mType = ItemType.ReforgeStone;
                mAmount = reward.reforgeStones;
            }
            else if (reward.skillPiece > 0)
            {
                mType = ItemType.SkillPiece;
                mAmount = reward.skillPiece;
            }
            else if (reward.mileage > 0)
            {
                mType = ItemType.Mileage;
                mAmount = reward.mileage;
            }
            else if (reward.ticketBundle > 0)
            {
                mType = ItemType.TicketBundle;
                mAmount = reward.ticketBundle;
            }
            else if (reward.soulStone > 0)
            {
                mType = ItemType.SoulStone;
                mAmount = reward.soulStone;
            }
            else if (reward.manaEssence > 0)
            {
                mType = ItemType.ManaEssence;
                mAmount = reward.manaEssence;
            }
            else if (reward.costumeUpgrade > 0)
            {
                mType = ItemType.CostumeUpgrade;
                mAmount = reward.costumeUpgrade;
            }
            else if (reward.eventCurrencys != null)
            {
                var firstEventCurrency = reward.eventCurrencys.FirstOrDefault();
                if (firstEventCurrency.Id.IsValid())
                {
                    var data = Lance.GameData.EventData.TryGet(firstEventCurrency.Id);
                    if (data != null)
                    {
                        mId = data.id;
                        mType = ItemType.EventCurrency;
                        mAmount = firstEventCurrency.Count;
                    }
                }
            }
            else if (reward.tickets != null)
            {
                for (int i = 0; i < reward.tickets.Length; ++i)
                {
                    if (reward.tickets[i] > 0)
                    {
                        DungeonType type = (DungeonType)i;

                        mType = type.ChangeToItemType();
                        mAmount = reward.tickets[i];
                    }
                }
            }
            else if (reward.demonicRealmStones != null)
            {
                for (int i = 0; i < reward.demonicRealmStones.Length; ++i)
                {
                    if (reward.demonicRealmStones[i] > 0)
                    {
                        DemonicRealmType type = (DemonicRealmType)i;

                        mType = type.ChangeToItemType();
                        mAmount = reward.demonicRealmStones[i];
                    }
                }
            }
            else if (reward.essences != null)
            {
                for (int i = 0; i < reward.essences.Length; ++i)
                {
                    if (reward.essences[i] > 0)
                    {
                        EssenceType type = (EssenceType)i;

                        mType = type.ChangeToItemType();
                        mAmount = reward.essences[i];
                    }
                }
            }
            else if (reward.equipments != null)
            {
                var firstEquipment = reward.equipments.FirstOrDefault();
                if (firstEquipment.Id.IsValid())
                {
                    var data = DataUtil.GetEquipmentData(firstEquipment.Id);
                    if (data != null)
                    {
                        mId = data.id;
                        mGrade = data.grade;
                        mSubGrade = data.subGrade;
                        mType = data.type;
                        mAmount = 1;
                    }
                }
            }
            else if (reward.accessorys != null)
            {
                var firstAccessory = reward.accessorys.FirstOrDefault();
                if (firstAccessory.Id.IsValid())
                {
                    var data = DataUtil.GetAccessoryData(firstAccessory.Id);
                    if (data != null)
                    {
                        mId = data.id;
                        mGrade = data.grade;
                        mSubGrade = data.subGrade;
                        mType = data.type;
                        mAmount = 1;
                    }
                }
            }
            else if (reward.skills != null)
            {
                var firstSkill = reward.skills.FirstOrDefault();
                if (firstSkill.Id.IsValid())
                {
                    var data = DataUtil.GetSkillData(firstSkill.Id);
                    if (data != null)
                    {
                        mId = data.id;
                        mGrade = data.grade;
                        mType = ItemType.Skill;
                        mAmount = 1;
                    }
                }
            }
            else if (reward.artifacts != null)
            {
                var firstArtifact = reward.artifacts.FirstOrDefault();
                if (firstArtifact.Id.IsValid())
                {
                    var data = DataUtil.GetArtifactData(firstArtifact.Id);
                    if (data != null)
                    {
                        mId = data.id;
                        mType = DataUtil.IsAncientArtifact(data.id) ? ItemType.AncientArtifact : ItemType.Artifact;
                        mAmount = 1;
                    }
                }
            }
            else if (reward.costumes != null)
            {
                var firstCostume = reward.costumes.FirstOrDefault();
                if (firstCostume.Id.IsValid())
                {
                    var data = DataUtil.GetCostumeData(firstCostume.Id);
                    if (data != null)
                    {
                        mId = data.id;
                        mType = ItemType.Costume;
                        mAmount = 1;
                    }
                }
            }
            else if (reward.achievements != null)
            {
                var firstAchievement = reward.achievements.FirstOrDefault();
                if (firstAchievement.Id.IsValid())
                {
                    var data = Lance.GameData.AchievementData.TryGet(firstAchievement.Id);
                    if (data != null)
                    {
                        mId = data.id;
                        mType = ItemType.Achievement;
                        mAmount = 1;
                    }
                }
            }
            else if (reward.petEquipments != null)
            {
                var firstPetEquipment = reward.petEquipments.FirstOrDefault();
                if (firstPetEquipment.Id.IsValid())
                {
                    var data = Lance.GameData.PetEquipmentData.TryGet(firstPetEquipment.Id);
                    if (data != null)
                    {
                        mId = data.id;
                        mType = ItemType.PetEquipment;
                        mAmount = 1;
                    }
                }
            }
        }

        public ItemInfo(RewardData reward)
        {
            if (reward.gold > 0)
            {
                mType = ItemType.Gold;
                mAmount = reward.gold;
            }
            else if (reward.gem > 0)
            {
                mType = ItemType.Gem;
                mAmount = reward.gem;
            }
            else if (reward.upgradeStone > 0)
            {
                mType = ItemType.UpgradeStone;
                mAmount = reward.upgradeStone;
            }
            else if (reward.reforgeStone > 0)
            {
                mType = ItemType.ReforgeStone;
                mAmount = reward.reforgeStone;
            }
            else if (reward.petFood > 0)
            {
                mType = ItemType.PetFood;
                mAmount = reward.petFood;
            }
            else if (reward.elementalStone > 0)
            {
                mType = ItemType.ElementalStone;
                mAmount = reward.elementalStone;
            }
            else if (reward.skillPiece > 0)
            {
                mType = ItemType.SkillPiece;
                mAmount = reward.skillPiece;
            }
            else if (reward.mileage > 0)
            {
                mType = ItemType.Mileage;
                mAmount = reward.mileage;
            }
            else if (reward.ticketBundle > 0)
            {
                mType = ItemType.TicketBundle;
                mAmount = reward.ticketBundle;
            }
            else if (reward.joustingTicket > 0)
            {
                mType = ItemType.JoustingTicket;
                mAmount = reward.joustingTicket;
            }
            else if (reward.joustingCoin > 0)
            {
                mType = ItemType.JoustingCoin;
                mAmount = reward.joustingCoin;
            }
            else if (reward.gloryToken > 0)
            {
                mType = ItemType.GloryToken;
                mAmount = reward.gloryToken;
            }
            else if (reward.soulStone > 0)
            {
                mType = ItemType.SoulStone;
                mAmount = reward.soulStone;
            }
            else if (reward.manaEssence > 0)
            {
                mType = ItemType.ManaEssence;
                mAmount = reward.manaEssence;
            }
            else if (reward.costumeUpgrade > 0)
            {
                mType = ItemType.CostumeUpgrade;
                mAmount = reward.costumeUpgrade;
            }
            else if (reward.ancientEssence > 0)
            {
                mType = ItemType.AncientEssence;
                mAmount = reward.ancientEssence;
            }
            else if (reward.dungeonTicket.Any(x => x > 0))
            {
                for(int i = 0; i < reward.dungeonTicket.Length; ++i)
                {
                    DungeonType type = (DungeonType)i;

                    if (reward.dungeonTicket[i] > 0)
                    {
                        mType = type.ChangeToItemType();
                        mAmount = reward.dungeonTicket[i];
                    }
                }
            }
            else if (reward.demonicRealmStone.Any(x => x > 0))
            {
                for (int i = 0; i < reward.demonicRealmStone.Length; ++i)
                {
                    DemonicRealmType type = (DemonicRealmType)i;

                    if (reward.demonicRealmStone[i] > 0)
                    {
                        mType = type.ChangeToItemType();
                        mAmount = reward.demonicRealmStone[i];
                    }
                }
            }
            else if (reward.essence.Any(x => x > 0))
            {
                for (int i = 0; i < reward.essence.Length; ++i)
                {
                    EssenceType type = (EssenceType)i;

                    if (reward.essence[i] > 0)
                    {
                        mType = type.ChangeToItemType();
                        mAmount = reward.essence[i];
                    }
                }
            }
            else if (reward.eventCurrency.IsValid())
            {
                var data = Lance.GameData.EventData.TryGet(reward.eventCurrency);
                if (data == null)
                {
                    if (reward.eventCurrency.SplitWord('*', out string id, out string count))
                    {
                        data = Lance.GameData.EventData.TryGet(id);
                        if (data != null)
                        {
                            mId = data.id;
                            mType = ItemType.EventCurrency;
                            mAmount = count.ToIntSafe(1);
                        }
                    }
                }
                else
                {
                    mId = data.id;
                    mType = ItemType.EventCurrency;
                    mAmount = 1;
                }
            }
            else if (reward.equipment.IsValid())
            {
                var data = DataUtil.GetEquipmentData(reward.equipment);
                if (data == null)
                {
                    if (reward.equipment.SplitWord('*', out string id, out string count))
                    {
                        data = DataUtil.GetEquipmentData(id);
                        if (data != null)
                        {
                            mId = data.id;
                            mGrade = data.grade;
                            mSubGrade = data.subGrade;
                            mType = data.type;
                            mAmount = count.ToIntSafe(1);
                        }
                    }
                }
                else
                {
                    mId = data.id;
                    mGrade = data.grade;
                    mSubGrade = data.subGrade;
                    mType = data.type;
                    mAmount = 1;
                }
            }
            else if (reward.accessory.IsValid())
            {
                var data = DataUtil.GetAccessoryData(reward.accessory);
                if (data == null)
                {
                    if (reward.accessory.SplitWord('*', out string id, out string count))
                    {
                        data = DataUtil.GetAccessoryData(id);
                        if (data != null)
                        {
                            mId = data.id;
                            mGrade = data.grade;
                            mSubGrade = data.subGrade;
                            mType = data.type;
                            mAmount = count.ToIntSafe(1);
                        }
                    }
                }
                else
                {
                    mId = data.id;
                    mGrade = data.grade;
                    mSubGrade = data.subGrade;
                    mType = data.type;
                    mAmount = 1;
                }
            }
            else if (reward.skill.IsValid())
            {
                var data = DataUtil.GetSkillData(reward.skill);
                if (data == null)
                {
                    if (reward.skill.SplitWord('*', out string id, out string count))
                    {
                        data = DataUtil.GetSkillData(id);
                        if (data != null)
                        {
                            mId = data.id;
                            mGrade = data.grade;
                            mType = ItemType.Skill;
                            mAmount = count.ToIntSafe(1);
                        }
                    }
                }
                else
                {
                    mId = data.id;
                    mGrade = data.grade;
                    mType = ItemType.Skill;
                    mAmount = 1;
                }
            }
            else if (reward.artifact.IsValid())
            {
                var data = DataUtil.GetArtifactData(reward.artifact);
                if (data == null)
                {
                    if (reward.artifact.SplitWord('*', out string id, out string count))
                    {
                        data = DataUtil.GetArtifactData(id);
                        if (data != null)
                        {
                            mId = data.id;
                            mType = DataUtil.IsAncientArtifact(id) ? ItemType.AncientArtifact : ItemType.Artifact;
                            mAmount = count.ToIntSafe(1);
                        }
                    }
                }
                else
                {
                    mId = data.id;
                    mType = DataUtil.IsAncientArtifact(data.id) ? ItemType.AncientArtifact : ItemType.Artifact;
                    mAmount = 1;
                }
            }
            else if (reward.randomEquipment.IsValid())
            {
                var data = Lance.GameData.RandomEquipmentRewardData.TryGet(reward.randomEquipment);
                if (data != null)
                {
                    mId = data.id;
                    mGrade = data.grade;
                    mSubGrade = data.subGrade;
                    mType = ItemType.Random_Equipment;
                    mAmount = 1;
                }
            }
            else if (reward.randomAccessory.IsValid())
            {
                var data = Lance.GameData.RandomAccessoryRewardData.TryGet(reward.randomAccessory);
                if (data != null)
                {
                    mId = data.id;
                    mGrade = data.grade;
                    mSubGrade = data.subGrade;
                    mType = ItemType.Random_Accessory;
                    mAmount = 1;
                }
            }
            else if (reward.randomSkill.IsValid())
            {
                var data = Lance.GameData.RandomSkillRewardData.TryGet(reward.randomSkill);
                if (data != null)
                {
                    mId = data.id;
                    mGrade = data.grade;
                    mType = ItemType.Random_Skill;
                    mAmount = 1;
                }
            }
            else if (reward.costume.IsValid())
            {
                var data = DataUtil.GetCostumeData(reward.costume);
                if (data != null)
                {
                    mId = data.id;
                    mType = ItemType.Costume;
                    mAmount = 1;
                }
            }
            else if (reward.achievement.IsValid())
            {
                var data = Lance.GameData.AchievementData.TryGet(reward.achievement);
                if (data != null)
                {
                    mId = data.id;
                    mType = ItemType.Achievement;
                    mAmount = 1;
                }
            }
            else if (reward.petEquipment.IsValid())
            {
                var data = Lance.GameData.PetEquipmentData.TryGet(reward.petEquipment);
                if (data == null)
                {
                    if (reward.petEquipment.SplitWord('*', out string id, out string count))
                    {
                        data = Lance.GameData.PetEquipmentData.TryGet(id);
                        if (data != null)
                        {
                            mId = data.id;
                            mGrade = data.grade;
                            mSubGrade = data.subGrade;
                            mType = ItemType.PetEquipment;
                            mAmount = count.ToIntSafe(1);
                        }
                    }
                }
                else
                {
                    mId = data.id;
                    mGrade = data.grade;
                    mSubGrade = data.subGrade;
                    mType = ItemType.PetEquipment;
                    mAmount = 1;
                }
            }
        }

        public string GetName()
        {
            if (mType == ItemType.EventCurrency)
            {
                return StringTableUtil.GetName($"Currency_{mId}");
            }
            else if (mType == ItemType.Random_Equipment)
            {
                StringParam param = new StringParam("grade", $"{mGrade}");

                return StringTableUtil.GetName($"RandomEquipment", param);
            }
            else if (mType == ItemType.Random_Skill)
            {
                StringParam param = new StringParam("grade", $"{mGrade}");

                return StringTableUtil.GetName($"RandomSkill", param);
            }
            else if (mType == ItemType.Costume)
            {
                return StringTableUtil.GetName(mId);
            }
            else if (mType == ItemType.Achievement)
            {
                return StringTableUtil.GetName(mId);
            }
            else
            {
                return StringTableUtil.GetName($"{mType}");
            }
        }

        public ItemInfo SetId(string id)
        {
            mId = id;

            return this;
        }

        public ItemInfo SetShowStr(string showStr)
        {
            mShowStr = showStr;

            return this;
        }

        public ItemInfo SetGrade(Grade grade)
        {
            mGrade = grade;

            return this;
        }

        public ItemInfo SetSubGrade(SubGrade subGrade)
        {
            mSubGrade = subGrade;

            return this;
        }

        public string GetAmountString()
        {
            if (mShowStr.IsValid())
            {
                return mShowStr;
            }
            else if (ItemType.Gold == mType || 
                ItemType.Exp == mType ||
                ItemType.UpgradeStone == mType ||
                ItemType.ReforgeStone == mType ||
                ItemType.CostumeUpgrade == mType)
            {
                return mAmount.ToAlphaString();
            }
            else
            {
                return mAmount.ToString();
            }
        }

        public Grade GetGrade()
        {
            return mGrade;
        }

        public Sprite GetGradeSprite()
        {
            return Lance.Atlas.GetIconGrade(mGrade);
        }

        public string GetGradeColor()
        {
            if (mGrade == Grade.A)
            {
                return Const.ColorGrade_A;
            }
            else if (mGrade == Grade.S)
            {
                return Const.ColorGrade_S;
            }
            else if (mGrade == Grade.SS)
            {
                return Const.ColorGrade_SS;
            }
            else if (mGrade == Grade.SSS)
            {
                return Const.ColorGrade_SSS;
            }
            else
            {
                return "FFFFFF";
            }
        }

        public Sprite GetGradeBackground()
        {
            if (mType.IsEquipment() || mType.IsSkill() || mType.IsRandom())
            {
                return Lance.Atlas.GetFrameGrade(mGrade);
            }
            else
            {
                string artifactDeco = DataUtil.IsAncientArtifact(mId) ? "Frame_Deco_Artifact2_3" : "Frame_Deco_Artifact2_4";

                return Lance.Atlas.GetItemSlotUISprite(artifactDeco);
            }
        }

        public Sprite GetSprite()
        {
            if (mType.IsEquipment())
            {
                var data = DataUtil.GetEquipmentData(mId);

                return Lance.Atlas.GetItemSlotUISprite(data.sprite);
            }
            else if (mType.IsAccessory())
            {
                var data = DataUtil.GetAccessoryData(mId);

                return Lance.Atlas.GetItemSlotUISprite(data.sprite);
            }
            else if (mType.IsPetEquipment())
            {
                var data = Lance.GameData.PetEquipmentData.TryGet(mId);

                return Lance.Atlas.GetItemSlotUISprite(data.sprite);
            }
            else if (mType.IsDungeonTicket())
            {
                return Lance.Atlas.GetDungeonTicket(mType);
            }
            else if (mType.IsDemonicRealmStone())
            {
                return Lance.Atlas.GetDemonicRealmStone(mType);
            }
            else if (mType.IsRandom())
            {
                if (mType == ItemType.Random_Equipment)
                {
                    return Lance.Atlas.GetItemSlotUISprite("Icon_Random_Equipment");
                }
                else if (mType == ItemType.Random_Accessory)
                {
                    return Lance.Atlas.GetItemSlotUISprite("Icon_Random_Accessory");
                }
                else
                {
                    return Lance.Atlas.GetItemSlotUISprite($"Icon_Shop_Random_{mType.RandomChangeToEquipmentType()}");
                }
            }
            else if (mType.IsSkill())
            {
                return Lance.Atlas.GetSkill(mId);
            }
            else if (mType.IsArtifact() || mType == ItemType.AncientArtifact)
            {
                var data = DataUtil.GetArtifactData(mId);

                return Lance.Atlas.GetItemSlotUISprite(data != null ? data.sprite : "Icon_Artifact_Ring1");
            }
            else if (mType == ItemType.Costume)
            {
                var data = DataUtil.GetCostumeData(mId);

                return Lance.Atlas.GetPlayerSprite(data.uiSprite);
            }
            else if (mType == ItemType.Achievement)
            {
                AchievementData achievementData = Lance.GameData.AchievementData.TryGet(mId);

                return Lance.Atlas.GetUISprite(achievementData.uiSprite);
            }
            else // ¿Á»≠
            {
                if (mType == ItemType.EventCurrency)
                    return Lance.Atlas.GetItemSlotUISprite($"Currency_{mId}");
                else if (mType == ItemType.SpeedMode)
                    return Lance.Atlas.GetUISprite("Icon_Speed_Active");
                else if (mType == ItemType.Buff)
                    return Lance.Atlas.GetUISprite("Icon_Buff");
                else
                {
                    return Lance.Atlas.GetItemSlotUISprite($"Currency_{mType}");
                }
            }
        }
    }

    static class ItemInfoUtil
    {
        public static List<ItemInfo> CreateItemInfos(RewardData rewardData)
        {
            List<ItemInfo> itemInfoList = new List<ItemInfo>();

            if (rewardData.adRemove)
            {
                var itemInfo = new ItemInfo(ItemType.Advertisement);

                itemInfo.SetShowStr(StringTableUtil.Get("UIString_RemoveAd"));

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.speedMode)
            {
                var itemInfo = new ItemInfo(ItemType.SpeedMode);

                itemInfo.SetShowStr(StringTableUtil.Get("UIString_Infinity_SpeedMode"));

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.buff)
            {
                var itemInfo = new ItemInfo(ItemType.Buff);

                itemInfo.SetShowStr(StringTableUtil.Get("UIString_Infinity_Buff"));

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.gold > 0)
            {
                var itemInfo = new ItemInfo(ItemType.Gold, rewardData.gold);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.gem > 0)
            {
                var itemInfo = new ItemInfo(ItemType.Gem, rewardData.gem);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.upgradeStone > 0)
            {
                var itemInfo = new ItemInfo(ItemType.UpgradeStone, rewardData.upgradeStone);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.reforgeStone > 0)
            {
                var itemInfo = new ItemInfo(ItemType.ReforgeStone, rewardData.reforgeStone);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.skillPiece > 0)
            {
                var itemInfo = new ItemInfo(ItemType.SkillPiece, rewardData.skillPiece);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.petFood > 0)
            {
                var itemInfo = new ItemInfo(ItemType.PetFood, rewardData.petFood);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.elementalStone > 0)
            {
                var itemInfo = new ItemInfo(ItemType.ElementalStone, rewardData.elementalStone);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.ticketBundle > 0)
            {
                var itemInfo = new ItemInfo(ItemType.TicketBundle, rewardData.ticketBundle);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.ancientEssence > 0)
            {
                var itemInfo = new ItemInfo(ItemType.AncientEssence, rewardData.ancientEssence);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.joustingCoin > 0)
            {
                var itemInfo = new ItemInfo(ItemType.JoustingCoin, rewardData.joustingCoin);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.joustingTicket > 0)
            {
                var itemInfo = new ItemInfo(ItemType.JoustingTicket, rewardData.joustingTicket);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.gloryToken > 0)
            {
                var itemInfo = new ItemInfo(ItemType.GloryToken, rewardData.gloryToken);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.soulStone > 0)
            {
                var itemInfo = new ItemInfo(ItemType.SoulStone, rewardData.soulStone);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.manaEssence > 0)
            {
                var itemInfo = new ItemInfo(ItemType.ManaEssence, rewardData.manaEssence);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.costumeUpgrade > 0)
            {
                var itemInfo = new ItemInfo(ItemType.CostumeUpgrade, rewardData.costumeUpgrade);

                itemInfoList.Add(itemInfo);
            }

            if (rewardData.eventCurrency.IsValid())
            {
                string[] eventCurrencys = rewardData.eventCurrency.SplitByDelim();
                for (int i = 0; i < eventCurrencys.Length; ++i)
                {
                    var data = Lance.GameData.EventData.TryGet(eventCurrencys[i]);
                    if (data == null)
                    {
                        if (eventCurrencys[i].SplitWord('*', out string id, out string count))
                        {
                            data = Lance.GameData.EventData.TryGet(id);
                            if (data != null)
                            {
                                AddItemInfo(data.id, count.ToIntSafe(1));
                            }
                        }
                    }
                    else
                    {
                        AddItemInfo(data.id, 1);
                    }

                    void AddItemInfo(string id, int count)
                    {
                        var itemInfo = new ItemInfo(ItemType.EventCurrency, count)
                            .SetId(id);

                        itemInfoList.Add(itemInfo);
                    }
                }
            }

            if (rewardData.dungeonTicket.Any(x => x > 0))
            {
                for(int i = 0; i < rewardData.dungeonTicket.Length; ++i)
                {
                    if (rewardData.dungeonTicket[i] > 0)
                    {
                        DungeonType type = (DungeonType)i;

                        var itemInfo = new ItemInfo(type.ChangeToItemType(), rewardData.dungeonTicket[i]);

                        itemInfoList.Add(itemInfo);
                    }
                }
            }

            if (rewardData.demonicRealmStone.Any(x => x > 0))
            {
                for (int i = 0; i < rewardData.demonicRealmStone.Length; ++i)
                {
                    if (rewardData.demonicRealmStone[i] > 0)
                    {
                        DemonicRealmType type = (DemonicRealmType)i;

                        var itemInfo = new ItemInfo(type.ChangeToItemType(), rewardData.demonicRealmStone[i]);

                        itemInfoList.Add(itemInfo);
                    }
                }
            }

            if (rewardData.essence.Any(x => x > 0))
            {
                for (int i = 0; i < rewardData.essence.Length; ++i)
                {
                    if (rewardData.essence[i] > 0)
                    {
                        EssenceType type = (EssenceType)i;

                        var itemInfo = new ItemInfo(type.ChangeToItemType(), rewardData.essence[i]);

                        itemInfoList.Add(itemInfo);
                    }
                }
            }

            if (rewardData.equipment.IsValid())
            {
                string[] equipments = rewardData.equipment.SplitByDelim();
                for(int i = 0; i < equipments.Length; ++i)
                {
                    var data = DataUtil.GetEquipmentData(equipments[i]);
                    if (data == null)
                    {
                        if (equipments[i].SplitWord('*', out string id, out string count))
                        {
                            data = DataUtil.GetEquipmentData(id);
                            if (data != null)
                            {
                                AddItemInfo(data, count.ToIntSafe(1));
                            }
                        }
                    }
                    else
                    {
                        AddItemInfo(data, 1);
                    }

                    void AddItemInfo(EquipmentData data, int count)
                    {
                        var itemInfo = new ItemInfo(data.type, count)
                            .SetId(data.id).SetGrade(data.grade).SetSubGrade(data.subGrade);

                        itemInfoList.Add(itemInfo);
                    }
                }
            }

            if (rewardData.accessory.IsValid())
            {
                string[] accessorys = rewardData.accessory.SplitByDelim();
                for (int i = 0; i < accessorys.Length; ++i)
                {
                    var data = DataUtil.GetAccessoryData(accessorys[i]);
                    if (data == null)
                    {
                        if (accessorys[i].SplitWord('*', out string id, out string count))
                        {
                            data = DataUtil.GetAccessoryData(id);
                            if (data != null)
                            {
                                AddItemInfo(data, count.ToIntSafe(1));
                            }
                        }
                    }
                    else
                    {
                        AddItemInfo(data, 1);
                    }

                    void AddItemInfo(AccessoryData data, int count)
                    {
                        var itemInfo = new ItemInfo(data.type, count)
                            .SetId(data.id).SetGrade(data.grade).SetSubGrade(data.subGrade);

                        itemInfoList.Add(itemInfo);
                    }
                }
            }

            if (rewardData.skill.IsValid())
            {
                string[] skills = rewardData.skill.SplitByDelim();
                for (int i = 0; i < skills.Length; ++i)
                {
                    var data = DataUtil.GetSkillData(skills[i]);
                    if (data == null)
                    {
                        if (skills[i].SplitWord('*', out string id, out string count))
                        {
                            data = DataUtil.GetSkillData(id);
                            if (data != null)
                            {
                                AddItemInfo(data, count.ToIntSafe(1));
                            }
                        }
                    }
                    else
                    {
                        AddItemInfo(data, 1);
                    }

                    void AddItemInfo(SkillData data, int count)
                    {
                        var itemInfo = new ItemInfo(ItemType.Skill, count)
                            .SetId(data.id).SetGrade(data.grade);

                        itemInfoList.Add(itemInfo);
                    }
                }
            }
            
            if (rewardData.artifact.IsValid())
            {
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
                                AddItemInfo(data, count.ToIntSafe(1));
                            }
                        }
                    }
                    else
                    {
                        AddItemInfo(data, 1);
                    }

                    void AddItemInfo(ArtifactData data, int count)
                    {
                        bool isAncientArtifact = DataUtil.IsAncientArtifact(data.id);

                        var itemInfo = new ItemInfo(isAncientArtifact ? ItemType.AncientArtifact : ItemType.Artifact, count)
                            .SetId(data.id);

                        itemInfoList.Add(itemInfo);
                    }
                }
            }
            
            if (rewardData.randomEquipment.IsValid())
            {
                RandomEquipmentRewardData data = Lance.GameData.RandomEquipmentRewardData.TryGet(rewardData.randomEquipment);
                if (data != null)
                {
                    var itemInfo = new ItemInfo(ItemType.Random_Equipment, 1)
                        .SetId(data.id).SetGrade(data.grade).SetSubGrade(data.subGrade);

                    itemInfoList.Add(itemInfo);
                }
            }

            if (rewardData.randomAccessory.IsValid())
            {
                var data = Lance.GameData.RandomAccessoryRewardData.TryGet(rewardData.randomAccessory);
                if (data != null)
                {
                    var itemInfo = new ItemInfo(ItemType.Random_Accessory, 1)
                        .SetId(data.id).SetGrade(data.grade).SetSubGrade(data.subGrade);

                    itemInfoList.Add(itemInfo);
                }
            }

            if (rewardData.randomSkill.IsValid())
            {
                var data = Lance.GameData.RandomSkillRewardData.TryGet(rewardData.randomSkill);
                if (data != null)
                {
                    var itemInfo = new ItemInfo(ItemType.Random_Skill, 1)
                        .SetId(data.id).SetGrade(data.grade);

                    itemInfoList.Add(itemInfo);
                }
            }

            if (rewardData.costume.IsValid())
            {
                var data = DataUtil.GetCostumeData(rewardData.costume);
                if (data != null)
                {
                    var itemInfo = new ItemInfo(ItemType.Costume, 1)
                        .SetId(data.id);

                    itemInfoList.Add(itemInfo);
                }
            }

            if (rewardData.achievement.IsValid())
            {
                var data = Lance.GameData.AchievementData.TryGet(rewardData.achievement);
                if (data != null)
                {
                    var itemInfo = new ItemInfo(ItemType.Achievement, 1)
                        .SetId(data.id);

                    itemInfoList.Add(itemInfo);
                }
            }

            if (rewardData.petEquipment.IsValid())
            {
                string[] petEquipments = rewardData.petEquipment.SplitByDelim();
                for (int i = 0; i < petEquipments.Length; ++i)
                {
                    var data = Lance.GameData.PetEquipmentData.TryGet(petEquipments[i]);
                    if (data == null)
                    {
                        if (petEquipments[i].SplitWord('*', out string id, out string count))
                        {
                            data = Lance.GameData.PetEquipmentData.TryGet(id);
                            if (data != null)
                            {
                                AddItemInfo(data, count.ToIntSafe(1));
                            }
                        }
                    }
                    else
                    {
                        AddItemInfo(data, 1);
                    }

                    void AddItemInfo(PetEquipmentData data, int count)
                    {
                        var itemInfo = new ItemInfo(ItemType.PetEquipment, count)
                            .SetId(data.id).SetGrade(data.grade).SetSubGrade(data.subGrade);

                        itemInfoList.Add(itemInfo);
                    }
                }
            }

            return itemInfoList;
        }

        public static ItemInfo CreateItemInfo(ItemType itemType, string itemId, double amount)
        {
            var itemInfo = new ItemInfo(itemType, amount);

            if (itemType.IsEquipment())
            {
                if (itemId.IsValid())
                {
                    var data = DataUtil.GetEquipmentData(itemId);
                    if (data != null)
                    {
                        return itemInfo.SetId(itemId).SetGrade(data.grade).SetSubGrade(data.subGrade);
                    }
                }    
            }
            else if (itemType.IsAccessory())
            {
                if (itemId.IsValid())
                {
                    var data = DataUtil.GetAccessoryData(itemId);
                    if (data != null)
                    {
                        return itemInfo.SetId(itemId).SetGrade(data.grade).SetSubGrade(data.subGrade);
                    }
                }
            }
            else if (itemType.IsSkill())
            {
                if (itemId.IsValid())
                {
                    var data = DataUtil.GetSkillData(itemId);
                    if (data != null)
                    {
                        return itemInfo.SetId(itemId).SetGrade(data.grade);
                    }
                }
            }
            else if (itemType == ItemType.Artifact || itemType == ItemType.AncientArtifact)
            {
                if (itemId.IsValid())
                {
                    var data = DataUtil.GetArtifactData(itemId);
                    if (data != null)
                    {
                        return itemInfo.SetId(itemId);
                    }
                }
            }
            else if (itemType == ItemType.EventCurrency)
            {
                if (itemId.IsValid())
                {
                    var data = Lance.GameData.EventData.TryGet(itemId);
                    if (data != null)
                    {
                        return itemInfo.SetId(itemId);
                    }
                }
            }
            else if (itemType == ItemType.Costume)
            {
                if (itemId.IsValid())
                {
                    var data = DataUtil.GetCostumeData(itemId);
                    if (data != null)
                    {
                        return itemInfo.SetId(itemId);
                    }
                }
            }
            else if (itemType == ItemType.Achievement)
            {
                if (itemId.IsValid())
                {
                    var data = Lance.GameData.AchievementData.TryGet(itemId);
                    if (data != null)
                    {
                        return itemInfo.SetId(itemId);
                    }
                }
            }
            else if (itemType.IsPetEquipment())
            {
                if (itemId.IsValid())
                {
                    var data = Lance.GameData.PetEquipmentData.TryGet(itemId);
                    if (data != null)
                    {
                        return itemInfo.SetId(itemId).SetGrade(data.grade);
                    }
                }
            }

            return itemInfo;
        }
    }
}