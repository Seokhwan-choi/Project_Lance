using UnityEngine.U2D;
using UnityEngine;
using System.Resources;
using System;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    class AtlasManager
    {
        SpriteAtlas mUIAtlas;
        SpriteAtlas mPlayerAtlas;
        SpriteAtlas mPetAtlas;
        SpriteAtlas mItemSlotUIAtlas;
        SpriteAtlas mBackgroundAtlas;
        public void Init()
        {
            mUIAtlas = Resources.Load<SpriteAtlas>("Atlas/UI");
            mPlayerAtlas = Resources.Load<SpriteAtlas>("Atlas/Player");
            mPetAtlas = Resources.Load<SpriteAtlas>("Atlas/Pet");
            mItemSlotUIAtlas = Resources.Load<SpriteAtlas>("Atlas/ItemSlot");
            mBackgroundAtlas = Resources.Load<SpriteAtlas>("Atlas/Background");
        }

        public Sprite GetBackground(StageType stageType, int chapter, int index)
        {
            if (stageType.IsNormal())
            {
                return mBackgroundAtlas.GetSprite($"Background_{chapter:00}{index:00}");
            }
            else if (stageType.IsDungeon())
            {
                if (stageType == StageType.Gold ||
                    stageType == StageType.Stone ||
                    stageType == StageType.Ancient)
                {
                    return mBackgroundAtlas.GetSprite($"Background_{stageType}_01{index:00}");
                }
                else if (stageType == StageType.Pet)
                {
                    return mBackgroundAtlas.GetSprite($"Background_Stone_01{index:00}");
                }
                else if (stageType == StageType.Reforge)
                {
                    return mBackgroundAtlas.GetSprite($"Background_LimitBreak_03{index:00}");
                }
                else if (stageType == StageType.Growth)
                {
                    return mBackgroundAtlas.GetSprite($"Background_LimitBreak_05{index:00}");
                }
                else
                {
                    if (chapter == 1) // Fire
                    {
                        return mBackgroundAtlas.GetSprite($"Background_LimitBreak_05{index:00}");
                    }
                    else if (chapter == 2) // Grass
                    {
                        return mBackgroundAtlas.GetSprite($"Background_LimitBreak_01{index:00}");
                    }
                    else // chapter == 3, Water
                    {
                        return mBackgroundAtlas.GetSprite($"Background_Raid_01{index:00}");
                    }
                }
            }
            else if (stageType.IsDemonicRealm())
            {
                // Background_DemonicRealm_0101
                return mBackgroundAtlas.GetSprite($"Background_DemonicRealm_01{index:00}");
            }
            else if (stageType.IsJousting())
            {
                return mBackgroundAtlas.GetSprite($"Background_{stageType}_01{index:00}");
            }
            else if (stageType == StageType.UltimateLimitBreak)
            {
                return mBackgroundAtlas.GetSprite($"Background_UltimateLimitBreak_01{index:00}");
            }
            else // 한계 돌파
            {
                return mBackgroundAtlas.GetSprite($"Background_LimitBreak_{chapter:00}{index:00}");
            }
        }

        public Sprite GetSpawnSprite(string name, bool isEquipment, bool isAccessory)
        {
            if (isEquipment || isAccessory)
            {
                return mItemSlotUIAtlas.GetSprite(name);
            }
            else
            {
                return mUIAtlas.GetSprite(name);
            }
        }

        public Sprite GetPetSprite(string name)
        {
            return mPetAtlas.GetSprite(name);
        }

        public Sprite GetDungeonThumbnail(StageType stageType)
        {
            if (stageType == StageType.Raid)
            {
                ElementalType type = (ElementalType)DataUtil.GetNowRaidBossElementalTypeIndex();

                return mUIAtlas.GetSprite($"Image_Dungeon_{stageType}_{type}");
            }
            else
            {
                return mUIAtlas.GetSprite($"Image_Dungeon_{stageType}");
            }
        }

        public Sprite GetDemonicRealmThumbnail(StageType stageType)
        {
            return mUIAtlas.GetSprite($"Image_DemonicRealm_{stageType}");
        }

        public Sprite GetPlayerSprite(string name)
        {
            return mPlayerAtlas.GetSprite(name);
        }

        public Sprite GetUISprite(string name)
        {
            return mUIAtlas.GetSprite(name);
        }

        public Sprite GetRankIcon(int rank)
        {
            string rankSpriteName = $"Icon_Rank_{GetRankPrefix(rank)}";

            return Lance.Atlas.GetUISprite(rankSpriteName);

            string GetRankPrefix(int rank)
            {
                if (rank == 1)
                    return "1st";
                else if (rank == 2)
                    return "2nd";
                else
                    return "3rd";
            }
        }

        public Sprite GetItemSlotUISprite(string name)
        {
            return mItemSlotUIAtlas.GetSprite(name);
        }

        public Sprite GetIconGrade(Grade grade)
        {
            return GetItemSlotUISprite($"Icon_Grade_{grade}");
        }

        public Sprite GetIconGrade(EquipmentOptionStatGrade grade)
        {
            return GetItemSlotUISprite($"Icon_Grade_{grade}");
        }

        public Sprite GetIconGrade(PetEvolutionStatGrade grade)
        {
            return GetItemSlotUISprite($"Icon_Grade_{grade}");
        }

        public Sprite GetDungeonTicket(ItemType type)
        {
            if (type == ItemType.GoldTicket)
                return GetItemSlotUISprite("Currency_DungeonKey_05");    // 노란색
            else if (type == ItemType.StoneTicket)
                return GetItemSlotUISprite("Currency_DungeonKey_04");    // 보라색
            else if (type == ItemType.ReforgeTicket)
                return GetItemSlotUISprite("Currency_DungeonKey_03");    // 파란색
            else if (type == ItemType.PetTicket)
                return GetItemSlotUISprite("Currency_DungeonKey_02");    // 초록색
            else if (type == ItemType.RaidTicket)
                return GetItemSlotUISprite("Currency_DungeonKey_01");    // 붉은색
            else if (type == ItemType.GrowthTicket)
                return GetItemSlotUISprite("Currency_DungeonKey_06");    // 회남색?
            else if (type == ItemType.AncientTicket)
                return GetItemSlotUISprite("Currency_DungeonKey_09");    // 하늘색
            else
                return GetItemSlotUISprite("Currency_DungeonKeys_01");   // 열쇠 묶음
        }

        public Sprite GetTicket(StageType type)
        {
            if (type == StageType.Gold)
                return GetItemSlotUISprite("Currency_DungeonKey_05");    // 노란색
            else if (type == StageType.Stone)
                return GetItemSlotUISprite("Currency_DungeonKey_04");    // 보라색
            else if (type == StageType.Pet)
                return GetItemSlotUISprite("Currency_DungeonKey_02");    // 초록색
            else if (type == StageType.Reforge)
                return GetItemSlotUISprite("Currency_DungeonKey_03");    // 파란색
            else if (type == StageType.Growth)
                return GetItemSlotUISprite("Currency_DungeonKey_06");    // 회남색?
            else if (type == StageType.Ancient)
                return GetItemSlotUISprite("Currency_DungeonKey_09");    // 하늘색
            else
                return GetItemSlotUISprite("Currency_DungeonKey_01");    // 붉은색
        }

        public Sprite GetDemonicRealmStone(StageType type)
        {
            // Currency_DemonicRealmStone_Accessory
            return GetItemSlotUISprite($"Currency_DemonicRealmStone_{type}");
        }

        public Sprite GetCountry(string countryCode)
        {
            return mUIAtlas.GetSprite($"Image_Countries_{countryCode}");
        }

        public Sprite GetDemonicRealmStone(ItemType itemType)
        {
            return GetItemSlotUISprite($"Currency_{itemType}");
        }

        public Sprite GetFrameGrade(Grade grade)
        {
            return GetItemSlotUISprite($"Frame_GradeSlot_{grade}");
        }

        public Sprite GetSkill(string skillId)
        {
            return GetItemSlotUISprite(skillId);
        }

        public Sprite GetBuffIcon(StatType type)
        {
            if (type == StatType.AddDmg)
            {
                return mUIAtlas.GetSprite("Image_Potion_Atk");
            }
            else if (type == StatType.GoldAmount)
            {
                return mUIAtlas.GetSprite("Image_Potion_Gold");
            }
            else
            {
                return mUIAtlas.GetSprite("Image_Potion_EXP");
            }
        }
    }
}

