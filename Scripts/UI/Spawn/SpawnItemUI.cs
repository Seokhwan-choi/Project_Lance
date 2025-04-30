using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class SpawnItemUI : MonoBehaviour
    {
        SpawnData mData;
        TextMeshProUGUI mText_SpawnLevel;
        Slider mSlider_SpawnCount;
        TextMeshProUGUI mText_StackedSpawnCount;
        TextMeshProUGUI mTextMyAncientEssence;
        GameObject mRedDotObj;
        GameObject mLockObj;
        
        List<SpawnButton> mButtons;
        public void Init(SpawnData data)
        {
            mData = data;

            TextMeshProUGUI textSpawnName = gameObject.FindComponent<TextMeshProUGUI>("Text_SpawnName");
            StringParam param = new StringParam("itemName", StringTableUtil.Get($"Name_{mData.type}"));
            textSpawnName.text = StringTableUtil.Get("UIString_SpawnName", param);

            var imageSpawnItem = gameObject.FindComponent<Image>("Image_SpawnItem");
            imageSpawnItem.sprite = Lance.Atlas.GetSpawnSprite(data.sprite, data.type.IsEquipment(), data.type.IsAccessory());

            mSlider_SpawnCount = gameObject.FindComponent<Slider>("Slider_StackedSpawn");
            mText_SpawnLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_SpawnLevel");
            mText_StackedSpawnCount = gameObject.FindComponent<TextMeshProUGUI>("Text_StackedSpawn");
            mRedDotObj = gameObject.FindGameObject("RedDot");
            var buttonSpawnReward = gameObject.FindComponent<Button>("Button_SpawnReward");
            buttonSpawnReward.SetButtonAction(OnSpawnRewardButton);

            if (data.type == ItemType.AncientArtifact)
            {
                var ancientObj = gameObject.FindGameObject("AncientEssence");

                mTextMyAncientEssence = ancientObj.FindComponent<TextMeshProUGUI>("Text_Amount");
            }

            mLockObj = gameObject.FindGameObject("Lock");

            var textUnlock = mLockObj.FindComponent<TextMeshProUGUI>("Text_Unlock");
            if (mData.type == ItemType.AncientArtifact)
                textUnlock.text = StringTableUtil.Get("UIString_RequireAllArtifactMaxLevel");
            else
                textUnlock.text = ContentsLockUtil.GetContentsLockMessage(data.type.ChangeToContentsLockType());

            GameObject buttonsObj = gameObject.FindGameObject("Buttons");

            buttonsObj.AllChildObjectOff();

            mButtons = new List<SpawnButton>();

            foreach (SpawnPriceData priceData in DataUtil.GetSpawnPriceDatas(mData.type))
            {
                if (priceData.onlyResult)
                    continue;

                GameObject buttonObj = Util.InstantiateUI("Button_Spawn", buttonsObj.transform);

                SpawnButton buttonUI = buttonObj.GetOrAddComponent<SpawnButton>();
                buttonUI.Init(this, priceData, OnSpawnButton);

                mButtons.Add(buttonUI);
            }

            Refresh();
        }

        public void RefreshContentsLockUI()
        {
            bool isLockContents = mData.type == ItemType.AncientArtifact ?
                Lance.Account.Artifact.IsAllArtifactMaxLevel() == false :
                ContentsLockUtil.IsLockContents(mData.type.ChangeToContentsLockType());

            mLockObj.SetActive(isLockContents);
        }

        public void Localize()
        {
            TextMeshProUGUI textSpawnName = gameObject.FindComponent<TextMeshProUGUI>("Text_SpawnName");
            StringParam param = new StringParam("itemName", StringTableUtil.Get($"Name_{mData.type}"));
            textSpawnName.text = StringTableUtil.Get("UIString_SpawnName", param);

            var textUnlock = mLockObj.FindComponent<TextMeshProUGUI>("Text_Unlock");
            if (mData.type == ItemType.AncientArtifact)
                textUnlock.text = StringTableUtil.Get("UIString_RequireAllArtifactMaxLevel");
            else
                textUnlock.text = ContentsLockUtil.GetContentsLockMessage(mData.type.ChangeToContentsLockType());

            foreach (SpawnButton button in mButtons)
            {
                button.Localize();
            }
        }

        public void Refresh()
        {
            StackedSpawnInfo info = Lance.Account.Spawn.GetInfo(mData.type);
            if (info == null)
                return;

            SpawnGradeProbData probData = DataUtil.GetSpawnGradeProbData(mData.probId, info.GetStackedSpawnCount());
            if (probData != null)
            {
                SpawnGradeProbData nextProbData = DataUtil.GetNextLevelSpawnGradeProbdata(mData.probId, probData?.level ?? 1);

                int requireCount = probData.requireStack;
                int myCount = info.GetStackedSpawnCount() - DataUtil.GetSpawnTotalRequireCount(mData.probId, probData.level);
                if (nextProbData != null)
                {
                    requireCount = nextProbData.requireStack;
                }
                else
                {
                    var rewardData = DataUtil.GetSpawnRewardData(mData.type, probData.level);
                    if (rewardData != null && rewardData.repeatReward.IsValid())
                    {
                        myCount = info.GetRepeatStackedCount(probData.level);
                        requireCount = rewardData.repeatRewardRequire;
                    }
                }

                mText_SpawnLevel.text = $"Lv. {probData?.level ?? 1}";
                mText_StackedSpawnCount.text = $"{myCount} / {requireCount}";
                mSlider_SpawnCount.value = (float)myCount / (float)requireCount;
                mRedDotObj.SetActive(info.AnyCanReceiveReward());
            }

            if (mData.type == ItemType.AncientArtifact)
            {
                mTextMyAncientEssence.text = $"{Lance.Account.Currency.GetAncientEssence()}";
            }

            foreach (SpawnButton button in mButtons)
            {
                button.Refresh();
            }
        }

        void OnSpawnButton()
        {
            Refresh();
        }

        void OnSpawnRewardButton()
        {
            if (mData.type.IsEquipment())
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_EquipProbUI>();

                popup.Init(this, mData);
            }
            else if (mData.type.IsAccessory())
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_AccessoryProbUI>();

                popup.Init(this, mData);
            }
            else if (mData.type.IsSkill())
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_SkillProbUI>();

                popup.Init(this, mData);
            }
            else if (mData.type.IsArtifact())
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_ArtifactProbUI>();

                popup.Init(Lance.GameData.ArtifactData.Values, Lance.GameData.ArtifactProbData);
            }
            else if (mData.type == ItemType.AncientArtifact)
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_ArtifactProbUI>();

                popup.Init(Lance.GameData.AncientArtifactData.Values, Lance.GameData.AncientArtifactProbData);
            }
        }
    }

    class SpawnButton : MonoBehaviour
    {
        SpawnPriceData mData;
        Action mOnSpawnAction;
        GameObject mFreeObj;
        GameObject mPayObj;

        Image mImageGemFrame;
        Image mImageAdFrame;

        GameObject mRedDotObj;
        TextMeshProUGUI mTextSpawnCount;
        TextMeshProUGUI mTextGemSpawnPrice;
        TextMeshProUGUI mTextAdSpawnPrice;
        public void Init(SpawnItemUI parent, SpawnPriceData data, Action onSpawnAction)
        {
            mData = data;
            mOnSpawnAction = onSpawnAction;

            mFreeObj = gameObject.FindGameObject("Free");
            mPayObj = gameObject.FindGameObject("Pay");
            
            mRedDotObj = gameObject.FindGameObject("RedDot");
            GameObject gemObj = gameObject.FindGameObject("Gem");
            GameObject adObj = gameObject.FindGameObject("Ad");

            mTextSpawnCount = gameObject.FindComponent<TextMeshProUGUI>("Text_SpawnCount");
            TextMeshProUGUI textSpawnCount2 = gameObject.FindComponent<TextMeshProUGUI>("Text_FreeSpawnCount");
            StringParam param = new StringParam("spawnCount", $"{mData.spawnCount}");
            string spawnCount = StringTableUtil.Get("UIString_SpawnCount", param);
            mTextSpawnCount.text = spawnCount;
            textSpawnCount2.text = spawnCount;

            var imageCurrency = gameObject.FindComponent<Image>("Image_Gem");

            // 잼
            if (mData.spawnType == SpawnType.Gem)
            {
                mTextGemSpawnPrice = gemObj.FindComponent<TextMeshProUGUI>("Text_SpawnPrice");

                mImageGemFrame = gemObj.FindComponent<Image>("Gem");

                imageCurrency.sprite = Lance.Atlas.GetItemSlotUISprite("Currency_Gem");
            }
            // 고대 정수
            else if (mData.spawnType == SpawnType.AncientEssence)
            {
                mTextGemSpawnPrice = gemObj.FindComponent<TextMeshProUGUI>("Text_SpawnPrice");

                mImageGemFrame = gemObj.FindComponent<Image>("Gem");

                imageCurrency.sprite = Lance.Atlas.GetItemSlotUISprite("Currency_AncientEssence");
            }
            // 광고
            else
            {
                mTextAdSpawnPrice = adObj.FindComponent<TextMeshProUGUI>("Text_AdCount");

                mImageAdFrame = adObj.FindComponent<Image>("Ad");
            }

            gemObj.SetActive(mData.spawnType == SpawnType.Gem || mData.spawnType == SpawnType.AncientEssence);
            adObj.SetActive(mData.spawnType == SpawnType.Ad);

            Button button = GetComponent<Button>();
            button.SetButtonAction(() =>
            {
                OnSpawnButton();

                onSpawnAction.Invoke();
            });

            if (mData.guideButton)
            {
                var guideActionTag = button.GetOrAddComponent<GuideActionTag>();

                GuideActionType tag = mData.type.ChangeToGuideActionHighlightSpawnButtonType();

                guideActionTag.Tag = tag;

                if (tag.IsNeedParent())
                {
                    var guideActionParentTag = parent.GetOrAddComponent<GuideActionTag>();
                    guideActionParentTag.Tag = tag.ChangeToParentType();
                }
            }

            Refresh();
        }

        public void Localize()
        {
            mTextSpawnCount = gameObject.FindComponent<TextMeshProUGUI>("Text_SpawnCount");
            TextMeshProUGUI textSpawnCount2 = gameObject.FindComponent<TextMeshProUGUI>("Text_FreeSpawnCount");
            StringParam param = new StringParam("spawnCount", $"{mData.spawnCount}");
            string spawnCount = StringTableUtil.Get("UIString_SpawnCount", param);
            mTextSpawnCount.text = spawnCount;
            textSpawnCount2.text = spawnCount;
        }

        public void Refresh()
        {
            bool isFree = Lance.Account.Spawn.IsFreeSpawn(mData.type, mData.id);

            mFreeObj.SetActive(isFree);
            mPayObj.SetActive(!isFree);

            if (mData.spawnType == SpawnType.Gem)
            {
                bool isEnoughGem = Lance.Account.IsEnoughGem(mData.price);
                bool canSpawn = true;
                string btnFrameName = (isEnoughGem && canSpawn) ? Const.DefaultActiveButtonFrame : Const.DefaultInactiveButtonFrame;
                mImageGemFrame.sprite = Lance.Atlas.GetUISprite(btnFrameName);

                mTextGemSpawnPrice.SetColor((isEnoughGem && canSpawn) ? Const.EnoughTextColor : Const.NotEnoughTextColor);
                mTextGemSpawnPrice.text = mData.price.ToString();

                mTextSpawnCount.SetColor((isEnoughGem && canSpawn) ? Const.DefaultActiveTextColor : Const.DefaultInactiveTextColor);
            }
            else if (mData.spawnType == SpawnType.AncientEssence)
            {
                bool isEnoughAncientEssence = Lance.Account.IsEnoughAncientEssence((int)mData.price);
                bool canSpawn = true;
                string btnFrameName = (isEnoughAncientEssence && canSpawn) ? Const.DefaultActiveButtonFrame : Const.DefaultInactiveButtonFrame;
                mImageGemFrame.sprite = Lance.Atlas.GetUISprite(btnFrameName);

                mTextGemSpawnPrice.SetColor((isEnoughAncientEssence && canSpawn) ? Const.EnoughTextColor : Const.NotEnoughTextColor);
                mTextGemSpawnPrice.text = mData.price.ToString();

                mTextSpawnCount.SetColor((isEnoughAncientEssence && canSpawn) ? Const.DefaultActiveTextColor : Const.DefaultInactiveTextColor);
            }
            else
            {
                StackedSpawnInfo info = Lance.Account.Spawn.GetInfo(mData.type);
                if (info != null)
                {
                    bool isEnoughAdCount = Lance.Account.IsEnoughSpawnWatchAdCount(mData.type);
                    bool canWatchAd = true;
                    string btnFrameName = (isEnoughAdCount && canWatchAd) ? "Button_Sky" : Const.DefaultInactiveButtonFrame;
                    mImageAdFrame.sprite = Lance.Atlas.GetUISprite(btnFrameName);

                    mTextAdSpawnPrice.SetColor((isEnoughAdCount && canWatchAd) ? Const.EnoughTextColor : Const.NotEnoughTextColor);
                    mTextAdSpawnPrice.text = $"{info.GetDailyWatchAdRemainCount()} / {info.GetDailyWatchAdMaxCount()}";

                    mTextSpawnCount.SetColor((isEnoughAdCount && canWatchAd) ? Const.DefaultActiveTextColor : Const.DefaultInactiveTextColor);
                }
            }

            RefreshRedDot();
        }

        public void RefreshRedDot()
        {
            bool isEnoughAdCount = Lance.Account.IsEnoughSpawnWatchAdCount(mData.type);
            bool canWatchAd = true;

            mRedDotObj.SetActive(mData.spawnType == SpawnType.Ad && isEnoughAdCount && canWatchAd);
        }

        void OnSpawnButton()
        {
            if (Lance.Account.Spawn.IsFreeSpawn(mData.type, mData.id))
            {
                Spawn();
            }
            else
            {
                //if (mData.type == ItemType.Artifact)
                //{
                //    if (Lance.Account.Artifact.IsAllArtifactMaxLevel())
                //    {
                //        UIUtil.ShowSystemErrorMessage("IsAllArtifactMaxLevel");

                //        return;
                //    }
                //}

                // 잼
                if (mData.spawnType == SpawnType.Gem)
                {
                    // 잼이 충분한지 확인
                    if (Lance.Account.IsEnoughGem(mData.price) == false)
                    {
                        UIUtil.ShowSystemErrorMessage("IsNotEnoughGem");

                        return;
                    }

                    Spawn();
                }
                else if (mData.spawnType == SpawnType.AncientEssence)
                {
                    // 고대 정수가 충분한지
                    if (Lance.Account.IsEnoughAncientEssence((int)mData.price) == false)
                    {
                        UIUtil.ShowSystemErrorMessage("IsNotEnoughAncientEssence");

                        return;
                    }

                    Spawn();
                }
                // 광고
                else
                {
                    // 광고 횟수가 충분한지 확인
                    if (Lance.Account.IsEnoughSpawnWatchAdCount(mData.type) == false)
                    {
                        UIUtil.ShowSystemErrorMessage("IsNotEnoughWatchAdCount");

                        return;
                    }

                    Lance.GameManager.ShowRewardedAd(mData.type.ChangeToAdType(), () =>
                    {
                        Spawn();

                        Lance.Lobby.RefreshTabRedDot(LobbyTab.Spawn);

                        Lance.Lobby.RefreshNavBarRedDot();
                    });
                }
            }
            
            
            void Spawn()
            {
                SpawnUtil.Spawn(mData);

                mOnSpawnAction?.Invoke();
            }
        }
    }
}