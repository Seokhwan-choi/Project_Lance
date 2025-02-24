using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BackEnd;

namespace Lance
{
    static class SpawnUtil
    {
        static int mStackedSpawnCount;
        static double mStackedUseCurrencyAmount;
        public static bool AutoSpawn;
        public static void OnSpawnFinish(ItemType itemType, SpawnType spawnType)
        {
            Param param = new Param();
            param.Add("spawnType", $"{spawnType}");
            param.Add("spawnItemType", $"{itemType}");
            param.Add("spawnCount", mStackedSpawnCount);
            param.Add("remainCurrency", spawnType == SpawnType.Gem ? Lance.Account.Currency.GetGem() : Lance.Account.Currency.GetAncientEssence());
            param.Add("usedCurrency", mStackedUseCurrencyAmount);

            Lance.BackEnd.InsertLog("Spawn", param, 7);

            Lance.Account.UpdatePassRewardValue(PassType.Spawn);

            Lance.Lobby.RefreshPassRedDot();

            if (itemType.IsEquipment() || itemType.IsAccessory())
            {
                Lance.Lobby.RefreshTabRedDot(LobbyTab.Inventory);
            }
            else if (itemType.IsArtifact())
            {
                Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
            }
            else if (itemType.IsSkill())
            {
                Lance.Lobby.RefreshTabRedDot(LobbyTab.Skill);
            }

            mStackedSpawnCount = 0;
            mStackedUseCurrencyAmount = 0;
            AutoSpawn = false;
        }

        public static void ToggleAutoSpawn()
        {
            AutoSpawn = !AutoSpawn;
        }

        public static void ToggleSkipSpawnMotion()
        {
            SaveBitFlags.SkipSpawnMotion.Toggle();
        }

        public static void Spawn(SpawnPriceData data)
        {
            Spawn(data.type, data.spawnType, data.spawnCount);

            if (data.spawnType == SpawnType.Gem || data.spawnType == SpawnType.AncientEssence)
            {
                mStackedSpawnCount += data.spawnCount;
                mStackedUseCurrencyAmount += data.price;
            }
        }

        static void Spawn(ItemType itemType, SpawnType spawnType, int spawnCount)
        {
            SpawnData spawnData = Lance.GameData.SpawnData.TryGet(itemType);
            if (spawnData == null)
                return;

            SpawnPriceData spawnPriceData = DataUtil.GetSpawnPriceData(spawnData.type, spawnType, spawnCount);
            if (spawnPriceData == null)
                return;

            bool isPurchasedRemoveAd = false;
            if (spawnPriceData.spawnType == SpawnType.Ad)
            {
                isPurchasedRemoveAd = Lance.Account.PackageShop.IsPurchasedRemoveAD() || Lance.IAPManager.IsPurchasedRemoveAd();

                int remainWathAdCount = Lance.Account.GetSpawnDailyWatchAdRemainCount(spawnPriceData.type);

                spawnCount = spawnCount * remainWathAdCount;
            }

            RewardResult result = Lance.Account.SpawnItem(spawnPriceData, spawnData.probId, spawnCount, isPurchasedRemoveAd);
            if (result.IsEmpty() == false)
            {
                // 보상을 지급하자
                Lance.Account.GiveReward(result);

                // 이미 지급된 보상에 대해서 화면 연출만 보여주자
                ShowSpawnResult(itemType, spawnType, result);

                // 퀘스트
                Lance.GameManager.CheckQuest(QuestType.Spawn, spawnCount);

                if (itemType.IsEquipment())
                {
                    Lance.GameManager.CheckQuest(QuestType.SpawnEquipment, spawnCount);

                    if (itemType == ItemType.Weapon)
                    {
                        Lance.GameManager.CheckQuest(QuestType.SpawnWeapon, spawnCount);
                    }
                    else if (itemType == ItemType.Armor)
                    {
                        Lance.GameManager.CheckQuest(QuestType.SpawnArmor, spawnCount);
                    }
                    else if (itemType == ItemType.Gloves)
                    {
                        Lance.GameManager.CheckQuest(QuestType.SpawnGloves, spawnCount);
                    }
                    else
                    {
                        Lance.GameManager.CheckQuest(QuestType.SpawnShoes, spawnCount);
                    }
                }
                else if (itemType.IsSkill())
                {
                    Lance.GameManager.CheckQuest(QuestType.SpawnSkill, spawnCount);
                }
                else if (itemType == ItemType.Artifact)
                {
                    Lance.GameManager.CheckQuest(QuestType.SpawnArtifact, spawnCount);
                }

                Lance.Lobby.UpdateCurrencyUI();
            }
        }

        public static void ShowSpawnResult(ItemType itemType, SpawnType spawnType, RewardResult result)
        {
            Popup_SpawnResultUI resultPopup = Lance.PopupManager.CreatePopup<Popup_SpawnResultUI>();

            resultPopup.Init(itemType, spawnType, result);
        }
    }
}
