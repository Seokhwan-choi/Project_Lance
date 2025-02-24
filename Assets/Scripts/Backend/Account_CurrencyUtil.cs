using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    public partial class Account
    {
        public void AddGem(double gem)
        {
            if (gem <= 0)
                return;

            Currency.AddGem(gem);
        }

        public void AddGold(double gold)
        {
            if (gold <= 0)
                return;

            Currency.AddGold(gold);
        }

        public void AddUpgradeStone(double upgradeStone)
        {
            if (upgradeStone <= 0)
                return;

            Currency.AddUpgradeStones(upgradeStone);

        }

        public bool IsEnoughGold(double gold)
        {
            return Currency.IsEnoughGold(gold);
        }

        public bool IsEnoughGem(double gem)
        {
            return Currency.IsEnoughGem(gem);
        }

        public bool IsEnoughSoulStone(int soulStone)
        {
            return Currency.IsEnoughSoulStone(soulStone);
        }

        public bool IsEnoughJoustCoin(int coin)
        {
            return Currency.IsEnoughJoustingCoin(coin);
        }

        public bool IsEnoughJoustGloryToken(int count)
        {
            return Currency.IsEnoughGloryToken(count);
        }

        public bool IsEnoughAncientEssence(int essence)
        {
            return Currency.IsEnoughAncientEssence(essence);
        }

        public bool IsEnoughUpgradeStones(double stones)
        {
            return Currency.IsEnoughUpgradeStones(stones);
        }

        public bool IsEnoughReforgeStones(double stones)
        {
            return Currency.IsEnoughReforgeStone(stones);
        }

        public bool IsEnoughElementalStones(int stones)
        {
            return Currency.IsEnoughElementalStone(stones);
        }

        public bool IsEnoughCostumeUpgrade(double costumeUpgrade)
        {
            return Currency.IsEnoughCostumeUpgrade(costumeUpgrade);
        }

        public bool IsEnoughManaEssence(int manaEssence)
        {
            return Currency.IsEnoughManaEssence(manaEssence);
        }

        public bool UseGold(double gold)
        {
            if (IsEnoughGold(gold) == false)
                return false;

            Currency.UseGold(gold);

            return true;
        }

        public bool UseGem(double gem)
        {
            if (IsEnoughGem(gem) == false)
                return false;

            Currency.UseGem(gem);

            return true;
        }

        public bool UseSoulStone(int soulStone)
        {
            if (IsEnoughSoulStone(soulStone) == false)
                return false;

            Currency.UseSoulStone(soulStone);

            return true;
        }

        public bool UseAncientEssence(int essence)
        {
            if (IsEnoughAncientEssence(essence) == false)
                return false;

            Currency.UseAncientEssence(essence);

            return true;
        }

        public bool UseUpgradeStones(double stones)
        {
            if (IsEnoughUpgradeStones(stones) == false)
                return false;

            Currency.UseUpgradeStones(stones);

            return true;
        }

        public bool UseReforgeStones(double stones)
        {
            if (IsEnoughReforgeStones(stones) == false)
                return false;

            Currency.UseReforgeStone(stones);

            return true;
        }

        public bool UseElementalStones(int stones)
        {
            if (IsEnoughElementalStones(stones) == false)
                return false;

            Currency.UseElementalStone(stones);

            return true;
        }

        public bool UseJoustCoin(int coin)
        {
            if (IsEnoughJoustCoin(coin) == false)
                return false;

            Currency.UseJoustingCoin(coin);

            return true;
        }

        public bool UseJoustGloryToken(int token)
        {
            if (IsEnoughJoustGloryToken(token) == false)
                return false;

            Currency.UseGloryToken(token);

            return true;
        }

        public bool UseCostumeUpgrade(double costumeUpgrade)
        {
            if (IsEnoughCostumeUpgrade(costumeUpgrade) == false)
                return false;

            Currency.UseCostumeUpgrade(costumeUpgrade);

            return true;
        }

        public bool UseManaEssence(int manaEssence)
        {
            if (IsEnoughManaEssence(manaEssence) == false)
                return false;

            Currency.UseManaEssence(manaEssence);

            return true;
        }

        public void AddDungeonTicket(StageType type, int count)
        {
            Currency.AddDungeonTicket(type, count);
        }

        public int GetDungeonTicket(StageType type)
        {
            return Currency.GetDungeonTicket(type);
        }

        public bool IsEnoughDungeonTicket(StageType type, int count)
        {
            return Currency.IsEnoughDungeonTicket(type, count);
        }

        public bool UseDungeonTicket(StageType type, int count)
        {
            return Currency.UseDungeonTicket(type, count);
        }

        public void AddDemonicRealmStone(StageType type, int count)
        {
            Currency.AddDemonicRealmStone(type, count);
        }

        public int GetDemonicRealmStone(StageType type)
        {
            return Currency.GetDemonicRealmStone(type);
        }

        public bool IsEnoughDemonicRealmStone(StageType type, int count)
        {
            return Currency.IsEnoughDemonicRealmStone(type, count);
        }

        public bool UseDemonicRealmStone(StageType type, int count)
        {
            return Currency.UseDemonicRealmStone(type, count);
        }

#if UNITY_EDITOR
        public void SetGem(double gem)
        {

        }

#endif
    }
}