using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Lobby_CurrencyUI : MonoBehaviour
    {
        List<Lobby_CurrencyItemUI> mCurrenyItemList;
        public void Init()
        {
            mCurrenyItemList = new List<Lobby_CurrencyItemUI>();

            for (int i = 0; i < (int)ItemType.Count; ++i)
            {
                ItemType itemType = (ItemType)i;
                if (itemType.IsLobbyVisibleCurrency())
                {
                    var itemObj = gameObject.FindGameObject($"{itemType}");

                    var itemUI = itemObj.GetOrAddComponent<Lobby_CurrencyItemUI>();
                    itemUI.Init(itemType);

                    mCurrenyItemList.Add(itemUI);
                }
            }

            Refresh();
        }

        public void Refresh()
        {
            foreach(Lobby_CurrencyItemUI item in mCurrenyItemList)
            {
                item.Refresh();
            }
        }

        Lobby_CurrencyItemUI GetCurrencyItem(ItemType itemType)
        {
            foreach(var item in mCurrenyItemList)
            {
                if (item.Type == itemType)
                    return item;
            }

            return null;
        }

        public void UpdateGold()
        {
            var goldItem = GetCurrencyItem(ItemType.Gold);

            goldItem?.Refresh();
        }

        public void UpdateGem()
        {
            var gemItem = GetCurrencyItem(ItemType.Gem);

            gemItem?.Refresh();
        }

        public void UpdateUpgradeStones()
        {
            var upgradeStoneItem = GetCurrencyItem(ItemType.UpgradeStone);

            upgradeStoneItem?.Refresh();
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void OnStartStage(StageData stageData)
        {
            SetActive(!stageData.type.IsJousting());
        }
    }

    class Lobby_CurrencyItemUI : MonoBehaviour
    {
        ItemType mType;
        TextMeshProUGUI mTextAmount;
        public ItemType Type => mType;
        public void Init(ItemType type)
        {
            mType = type;

            Image imgCurrency = gameObject.FindComponent<Image>("Image_Currency");
            imgCurrency.sprite = Lance.Atlas.GetItemSlotUISprite($"Currency_{type}");

            mTextAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_Amount");
        }

        public void Refresh()
        {
            mTextAmount.text = mType == ItemType.Gem ? GetCurrencyAmount().ToString() : GetCurrencyAmount().ToAlphaString();
        }

        double GetCurrencyAmount()
        {
            if (mType == ItemType.UpgradeStone)
            {
                return Lance.Account.Currency.GetUpgradeStones();
            }
            else if (mType == ItemType.Gem)
            {
                return Lance.Account.Currency.GetGem();
            }
            else
            {
                return Lance.Account.Currency.GetGold();
            }
        }
    }
}