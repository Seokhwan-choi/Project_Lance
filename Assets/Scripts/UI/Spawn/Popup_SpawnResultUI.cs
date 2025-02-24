using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using DG.Tweening;

namespace Lance
{
    class Popup_SpawnResultUI : PopupBase
    {
        const float AutoTime = 1.5f;

        bool mInMotion;
        float mAutoTime;
        ItemType mItemType;
        SpawnType mSpawnType;
        GameObject mButtonsObj;
        GameObject mFinishMotionObj;
        GameObject mAutoSpawn;
        GameObject mSkipSpawnMotion;
        List<SpawnSlotUI> mSpawnRewardSlotUIs;
        List<GameObject> mSpawnSlotFX;
        List<Result_SpawnButton> mSpawnButtons;
        public void Init(ItemType itemType, SpawnType spawnType, RewardResult result)
        {
            mItemType = itemType;
            mSpawnType = spawnType;

            mFinishMotionObj = gameObject.FindGameObject("FinishMotion");

            InitButtons(itemType);
            InitAutoSpawn(itemType);
            InitSkipSpawnMotion();

            mSpawnRewardSlotUIs = new List<SpawnSlotUI>();
            mSpawnSlotFX = new List<GameObject>();

            StartCoroutine(PlayMotion(itemType, result));
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            if (mInMotion)
                return;
            
            Lance.Lobby.RefreshTab();

            if (mSpawnSlotFX != null && mSpawnSlotFX.Count > 0)
            {
                foreach (var spawnSlotFX in mSpawnSlotFX)
                {
                    Lance.ObjectPool.ReleaseUI(spawnSlotFX);
                }
            }

            mSpawnSlotFX = null;

            foreach (var rewardSlotUI in mSpawnRewardSlotUIs)
            {
                rewardSlotUI.OnRelease();
            }

            mSpawnRewardSlotUIs = null;

            base.Close(immediate, hideMotion);
        }

        public override void OnBackButton(bool immediate = false, bool hideMotion = true)
        {
            base.OnBackButton(immediate, hideMotion);

            //Lance.BackEnd.UpdateAllAccountInfos();

            SpawnUtil.OnSpawnFinish(mItemType, mSpawnType);

            CheckGuideQuest(mItemType);
        }

        void InitAutoSpawn(ItemType itemType)
        {
            var autoSpawnPriceData = DataUtil.GetSpawnPriceData(itemType, itemType == ItemType.AncientArtifact ? SpawnType.AncientEssence : SpawnType.Gem, true);

            mAutoSpawn = gameObject.FindGameObject("AutoSpawn");

            var imageCheck = mAutoSpawn.FindComponent<Image>("Image_Check");
            imageCheck.gameObject.SetActive(SpawnUtil.AutoSpawn);

            var button = mAutoSpawn.FindComponent<Button>("Button_AutoSpawn");
            button.SetButtonAction(() => 
            {
                SpawnUtil.ToggleAutoSpawn();

                imageCheck.gameObject.SetActive(SpawnUtil.AutoSpawn);

                AutoSpawn();
            });

            var textAutoSpawn = mAutoSpawn.FindComponent<TextMeshProUGUI>("Text_AutoSpawn");

            StringParam param = new StringParam("spawnCount", autoSpawnPriceData.spawnCount);

            textAutoSpawn.text = StringTableUtil.Get("UIString_AutoSpawn", param);
        }

        void InitSkipSpawnMotion()
        {
            mSkipSpawnMotion = gameObject.FindGameObject("SkipSpawnMotion");

            var imageCheck = mSkipSpawnMotion.FindComponent<Image>("Image_Check");
            imageCheck.gameObject.SetActive(SaveBitFlags.SkipSpawnMotion.IsOn());

            var button = mSkipSpawnMotion.FindComponent<Button>("Button_SkipSpawnMotion");
            button.SetButtonAction(() =>
            {
                SpawnUtil.ToggleSkipSpawnMotion();

                imageCheck.gameObject.SetActive(SaveBitFlags.SkipSpawnMotion.IsOn());
            });
        }

        void InitButtons(ItemType itemType)
        {
            mButtonsObj = mFinishMotionObj.FindGameObject("Buttons");

            Button buttonClose = mButtonsObj.FindComponent<Button>("Button_Close");
            buttonClose.SetButtonAction(() =>
            {
                Close();

                // 바로 계정 정보를 저장해주자
                //Lance.BackEnd.UpdateAllAccountInfos();  // 닫기 버튼을 직접 눌렀다면

                SpawnUtil.OnSpawnFinish(mItemType, mSpawnType);

                CheckGuideQuest(itemType);
            });

            // 소환 횟수가 제일 많은 것의 2개만 가져오자
            IEnumerable<SpawnPriceData> spawPriceDatas = DataUtil.GetSpawnPriceDatas(itemType);

            // 내림차순을 정렬하고
            spawPriceDatas = spawPriceDatas.OrderByDescending(x => x.spawnCount);

            // 2개를 가져온다.
            SpawnPriceData[] newDatas = spawPriceDatas.Take(3).ToArray();

            mSpawnButtons = new List<Result_SpawnButton>();

            for(int i = 0; i < newDatas.Length; ++i)
            {
                int buttonInx = i + 1;

                SpawnPriceData data = newDatas[i];

                GameObject buttonSpawnObj = mButtonsObj.FindGameObject($"Button_Spawn{buttonInx}");

                Result_SpawnButton buttonSpawnUI = buttonSpawnObj.GetOrAddComponent<Result_SpawnButton>();

                buttonSpawnUI.Init(this, data);

                mSpawnButtons.Add(buttonSpawnUI);
            }
        }

        void CheckGuideQuest(ItemType itemType)
        {
            if (itemType.IsEquipment())
            {
                Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.SpawnEquipment);

                if (itemType == ItemType.Weapon)
                {
                    Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.SpawnWeapon);
                }
                else if (itemType == ItemType.Armor)
                {
                    Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.SpawnArmor);
                }
                else if (itemType == ItemType.Gloves)
                {
                    Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.SpawnGloves);
                }
                else
                {
                    Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.SpawnShoes);
                }
            }
            else if (itemType.IsSkill())
            {
                Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.SpawnSkill);
            }
            else if (itemType == ItemType.Artifact)
            {
                Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.SpawnArtifact);
            }
        }

        IEnumerator PlayMotion(ItemType itemType, RewardResult result)
        {
            mAutoTime = AutoTime;

            mInMotion = true;

            mFinishMotionObj.SetActive(false);

            // 마무리 결과물 연출
            yield return FinishMotion(itemType, result);
        }

        IEnumerator FinishMotion(ItemType itemType, RewardResult result)
        {
            mFinishMotionObj.SetActive(true);
            mButtonsObj.SetActive(false);
            mAutoSpawn.SetActive(false);
            mSkipSpawnMotion.SetActive(false);

            var rewardsObj = gameObject.FindGameObject("Rewards");
            rewardsObj.AllChildObjectOff();

            mSpawnRewardSlotUIs = CreateSpawnSlotUIList();

            if (SpawnUtil.AutoSpawn == false && SaveBitFlags.SkipSpawnMotion.IsOn() == false)
            {
                // 보상이 순서대로 위에서 떨어지도록
                foreach (var spawnSlotUI in mSpawnRewardSlotUIs)
                {
                    spawnSlotUI.gameObject.SetActive(true);

                    if (spawnSlotUI.Grade == Grade.A)
                    {
                        spawnSlotUI.ItemSlotTm.localScale = Vector3.zero;
                        spawnSlotUI.ItemSlotTm.DOScale(Vector3.one, 0.3f)
                            .SetUpdate(isIndependentUpdate: true).timeScale = 1f;

                        Lance.ParticleManager.AquireUI($"Spawn_Grade_{Grade.A}", spawnSlotUI.Tm);

                        SoundPlayer.PlaySpawnItem(Grade.A);

                        yield return new WaitForSecondsRealtime(0.1f);
                    }
                    else if (spawnSlotUI.Grade == Grade.S || spawnSlotUI.IsAncientArtifact)
                    {
                        spawnSlotUI.ItemSlotTm.localScale = Vector3.zero;
                        spawnSlotUI.ItemSlotTm.DOScale(Vector3.one, 0.3f)
                            .SetUpdate(isIndependentUpdate: true).timeScale = 1f;

                        Lance.ParticleManager.AquireUI($"Spawn_Grade_{Grade.S}", spawnSlotUI.Tm);

                        SoundPlayer.PlaySpawnItem(Grade.S);

                        yield return new WaitForSecondsRealtime(0.15f);
                    }
                    else if (spawnSlotUI.Grade == Grade.SS || spawnSlotUI.Grade == Grade.SSS)
                    {
                        spawnSlotUI.ItemSlotTm.localScale = Vector3.zero;
                        spawnSlotUI.ItemSlotTm.DOScale(Vector3.one, 0.3f)
                            .SetUpdate(isIndependentUpdate: true)
                            .SetDelay(0.75f).timeScale = 1f;

                        Lance.ParticleManager.AquireUI($"Spawn_Grade_{Grade.SS}", spawnSlotUI.Tm);

                        SoundPlayer.PlaySpawnItem(Grade.SS);

                        yield return new WaitForSecondsRealtime(1f);
                    }
                    else if (spawnSlotUI.Grade == Grade.SR)
                    {
                        spawnSlotUI.ItemSlotTm.localScale = Vector3.zero;
                        spawnSlotUI.ItemSlotTm.DOScale(Vector3.one, 0.3f)
                            .SetUpdate(isIndependentUpdate: true)
                            .SetDelay(0.75f).timeScale = 1f;

                        Lance.ParticleManager.AquireUI($"Spawn_Grade_{Grade.SR}", spawnSlotUI.Tm);

                        SoundPlayer.PlayUpgradeManaHeartCharge();

                        yield return new WaitForSecondsRealtime(1f);

                        SoundPlayer.PlayUpgradeManaHeartFinish();

                        yield return new WaitForSecondsRealtime(0.15f);

                        SoundPlayer.PlayGloryOrbUpgrade(JoustGloryOrbType.GloryOrb_1);
                    }
                    else if (spawnSlotUI.Grade == Grade.SSR)
                    {
                        spawnSlotUI.ItemSlotTm.localScale = Vector3.zero;
                        spawnSlotUI.ItemSlotTm.DOScale(Vector3.one, 0.3f)
                            .SetUpdate(isIndependentUpdate: true)
                            .SetDelay(0.75f).timeScale = 1f;

                        Lance.ParticleManager.AquireUI($"Spawn_Grade_{Grade.SSR}", spawnSlotUI.Tm);

                        SoundPlayer.PlayUpgradeManaHeartCharge();

                        yield return new WaitForSecondsRealtime(1f);

                        SoundPlayer.PlayUpgradeManaHeartFinish();

                        yield return new WaitForSecondsRealtime(0.15f);

                        SoundPlayer.PlayGloryOrbUpgrade(JoustGloryOrbType.GloryOrb_3);
                    }
                    else
                    {
                        spawnSlotUI.transform.localScale = Vector3.one * 3f;
                        spawnSlotUI.transform.DOScale(Vector3.one, 0.3f)
                            .SetUpdate(isIndependentUpdate: true).timeScale = 1f;

                        yield return new WaitForSecondsRealtime(0.05f);

                        SoundPlayer.PlaySpawnItem(spawnSlotUI.Grade);
                    }
                }

                yield return new WaitForSecondsRealtime(0.25f);
            }

            // 보상이 모두 떨어지면 자연스럽게 위아래로 흔들리도록
            rewardsObj.transform.DOLocalMoveY(30f, 0.5f)
                    .SetUpdate(isIndependentUpdate:true)
                    .SetLoops(-1, LoopType.Yoyo).timeScale = 1f;

            mButtonsObj.SetActive(true);
            mAutoSpawn.SetActive(true);
            mSkipSpawnMotion.SetActive(true);

            mInMotion = false;

            List<SpawnSlotUI> CreateSpawnSlotUIList()
            {
                MultiReward[] rewards;
                if (itemType.IsEquipment())
                {
                    rewards = result.equipments.GatherReward();
                }
                else if (itemType.IsAccessory())
                {
                    rewards = result.accessorys.GatherReward();
                }
                else if (itemType.IsSkill())
                {
                    rewards = result.skills.GatherReward();
                }
                else
                {
                    rewards = result.artifacts.GatherReward();
                }

                List<SpawnSlotUI> list = new List<SpawnSlotUI>();

                RectTransform parent = rewardsObj.transform as RectTransform;

                foreach (MultiReward reward in rewards)
                {
                    var spawnSlotObj = Lance.ObjectPool.AcquireUI("SpawnSlotUI", parent);

                    var spawnSlotUI = spawnSlotObj.GetOrAddComponent<SpawnSlotUI>();
                    spawnSlotUI.gameObject.SetActive(SpawnUtil.AutoSpawn || SaveBitFlags.SkipSpawnMotion.IsOn());
                    spawnSlotUI.Init(reward);

                    list.Add(spawnSlotUI);
                }

                return list;
            }
        }

        private void Update()
        {
            if (mInMotion)
                return;

            float dt = Time.unscaledDeltaTime;

            if (SpawnUtil.AutoSpawn && mAutoTime > 0)
            {
                mAutoTime -= dt;
                if (mAutoTime <= 0f)
                {
                    mAutoTime = AutoTime;

                    AutoSpawn();
                }
            }
        }

        void AutoSpawn()
        {
            if (SpawnUtil.AutoSpawn == false)
                return;

            if (mItemType == ItemType.AncientArtifact)
            {
                var spawnPriceData = DataUtil.GetSpawnPriceData(mItemType, SpawnType.AncientEssence, true);
                if (spawnPriceData != null)
                {
                    double spawnPrice = spawnPriceData.price;

                    if (Lance.Account.IsEnoughAncientEssence((int)spawnPrice) == false)
                    {
                        UIUtil.ShowSystemErrorMessage("IsNotEnoughAncientEssence");

                        SpawnUtil.ToggleAutoSpawn();

                        var imageCheck = mAutoSpawn.FindComponent<Image>("Image_Check");
                        imageCheck.gameObject.SetActive(SpawnUtil.AutoSpawn);

                        return;
                    }

                    Close(true);

                    SpawnUtil.Spawn(spawnPriceData);
                }
            }
            else
            {
                var spawnPriceData = DataUtil.GetSpawnPriceData(mItemType, SpawnType.Gem, true);
                if (spawnPriceData != null)
                {
                    double gemPrice = spawnPriceData.price;

                    if (Lance.Account.IsEnoughGem(gemPrice) == false)
                    {
                        UIUtil.ShowSystemErrorMessage("IsNotEnoughGem");

                        SpawnUtil.ToggleAutoSpawn();

                        var imageCheck = mAutoSpawn.FindComponent<Image>("Image_Check");
                        imageCheck.gameObject.SetActive(SpawnUtil.AutoSpawn);

                        return;
                    }

                    Close(true);

                    SpawnUtil.Spawn(spawnPriceData);
                }
            }
        }
    }

    class SpawnSlotUI : MonoBehaviour
    {
        RectTransform mRectTm;
        ItemSlotUI mItemSlotUI;
        public Grade Grade => mItemSlotUI.Grade;
        public bool IsAncientArtifact => DataUtil.IsAncientArtifact(mItemSlotUI.Id);
        public RectTransform Tm => mRectTm;
        public Transform ItemSlotTm => mItemSlotUI.transform;
        public void Init(MultiReward reward)
        {
            mRectTm = GetComponent<RectTransform>();

            var itemSlotObj = gameObject.FindGameObject("ItemSlotUI");

            mItemSlotUI = itemSlotObj.GetOrAddComponent<ItemSlotUI>();
            mItemSlotUI.Init(reward);
        }

        public void OnRelease()
        {
            mRectTm = null;
            mItemSlotUI = null;

            Lance.ObjectPool.ReleaseUI(gameObject);
        }
    }

    class Result_SpawnButton : MonoBehaviour
    {
        Popup_SpawnResultUI mParnet;
        SpawnPriceData mSpawnPriceData;

        TextMeshProUGUI mTextPrice;

        public void Init(Popup_SpawnResultUI parent, SpawnPriceData spawnPriceData)
        {
            mParnet = parent;

            mSpawnPriceData = spawnPriceData;

            Button button = gameObject.FindComponent<Button>("SpawnResult_Button_Spawn");

            button.SetButtonAction(OnBuyButtonAction);

            TextMeshProUGUI textSpawnCount = gameObject.FindComponent<TextMeshProUGUI>("Text_SpawnCount");

            StringParam param = new StringParam("spawnCount", $"{spawnPriceData.spawnCount}");

            textSpawnCount.text = StringTableUtil.Get("UIString_SpawnCount", param);

            mTextPrice = gameObject.FindComponent<TextMeshProUGUI>("Text_Price");

            RefreshTextPrice();
        }

        public void RefreshTextPrice()
        {
            bool isGemType = mSpawnPriceData.spawnType == SpawnType.Gem;
            bool isEnoughCurrency = isGemType ? Lance.Account.IsEnoughGem(mSpawnPriceData.price) : Lance.Account.IsEnoughAncientEssence((int)mSpawnPriceData.price);

            var imageFrame = gameObject.FindComponent<Image>("SpawnResult_Button_Spawn");
            imageFrame.sprite = Lance.Atlas.GetUISprite(isEnoughCurrency ? Const.DefaultActiveButtonFrame : Const.DefaultInactiveButtonFrame);

            var textSpawnCount = gameObject.FindComponent<TextMeshProUGUI>("Text_SpawnCount");
            textSpawnCount.SetColor(UIUtil.GetActiveTextColor(isEnoughCurrency));

            var imageCurrency = gameObject.FindComponent<Image>("Image_Currency");
            imageCurrency.sprite = Lance.Atlas.GetItemSlotUISprite(isGemType ? "Currency_Gem" : "Currency_AncientEssence");

            // 재화 부족하면
            mTextPrice.SetColor(UIUtil.GetEnoughTextColor(isEnoughCurrency));
            mTextPrice.text = $"{mSpawnPriceData.price}";
        }

        void OnBuyButtonAction()
        {
            if (mSpawnPriceData == null)
                return;

            double spawnPrice = mSpawnPriceData.price;

            if (mSpawnPriceData.spawnType == SpawnType.Gem)
            {
                if (Lance.Account.IsEnoughGem(spawnPrice) == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughGem");

                    return;
                }
            }
            else if (mSpawnPriceData.spawnType == SpawnType.AncientEssence)
            {
                if (Lance.Account.IsEnoughAncientEssence((int)spawnPrice) == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughAncientEssence");

                    return;
                }
            }

            mParnet.Close(true);

            SpawnUtil.Spawn(mSpawnPriceData);
        }
    }
}