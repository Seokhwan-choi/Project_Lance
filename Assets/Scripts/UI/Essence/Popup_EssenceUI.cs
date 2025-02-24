using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Popup_EssenceUI : PopupBase
    {
        Dictionary<EssenceType, EssenceCurrencyUI> mCurrencyUIs;
        Dictionary<EssenceType, EssenceItemUI> mEssenceItemUIs;
        SelectedEssenceInfoUI mEssenceSeletedUI;
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_Essence"));

            mCurrencyUIs = new Dictionary<EssenceType, EssenceCurrencyUI>();
            mEssenceItemUIs = new Dictionary<EssenceType, EssenceItemUI>();

            var essencesObj = gameObject.FindGameObject("Essences");

            for (int i = 0; i < (int)EssenceType.Count; ++i)
            {
                EssenceType type = (EssenceType)i;

                InitCurrencyEssence(type);
                InitEssenceItemUI(type);
            }

            InitEssenceItemUI(EssenceType.Central);

            void InitCurrencyEssence(EssenceType essenceType)
            {
                var currencyObj = gameObject.FindGameObject($"Currency_{essenceType}");

                var currencyUI = currencyObj.GetOrAddComponent<EssenceCurrencyUI>();
                currencyUI.Init(essenceType);

                mCurrencyUIs.Add(essenceType, currencyUI);
            }

            void InitEssenceItemUI(EssenceType essenceType)
            {
                var essenceItemObj = essencesObj.FindGameObject($"Essence_{essenceType}");
                var essenceItemUI = essenceItemObj.GetOrAddComponent<EssenceItemUI>();
                essenceItemUI.Init(essenceType, OnSelectEssence);

                mEssenceItemUIs.Add(essenceType, essenceItemUI);
            }

            var selectedObj = gameObject.FindGameObject("SelectedInfo");

            mEssenceSeletedUI = selectedObj.GetOrAddComponent<SelectedEssenceInfoUI>();
            mEssenceSeletedUI.Init(OnUpgrade);

            var buttonInfo = gameObject.FindComponent<Button>("Button_Info");
            buttonInfo.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_DescUI>();

                popup.Init(StringTableUtil.Get("Title_Essence"), StringTableUtil.Get("Desc_Essence"));
            });

            OnSelectEssence(EssenceType.Central);
        }

        public void OnSelectEssence(EssenceType selectType)
        {
            if (selectType == EssenceType.Central)
            {
                if (selectType == mEssenceSeletedUI.SelectedEssence)
                {
                    var popup = Lance.PopupManager.CreatePopup<Popup_CentralEssenceActiveUI>();

                    popup.Init();

                    return;
                }
            }

            mEssenceSeletedUI.OnSelectEssence(selectType);

            foreach (EssenceItemUI essenceItemUI in mEssenceItemUIs.Values)
            {
                essenceItemUI.SetSelected(essenceItemUI.Type == selectType);
            }
        }

        public void OnUpgrade(EssenceType upgradeType)
        {
            if (upgradeType == EssenceType.Central)
            {
                StartCoroutine(PlayUpgradeCentralEssenceMotion());
            }
            else
            {
                Refresh();

                var esscenItemUI = mEssenceItemUIs.TryGet(upgradeType);

                esscenItemUI.PlayMotion();

                SoundPlayer.PlaySkillLevelUp();
            }
        }

        IEnumerator PlayUpgradeCentralEssenceMotion()
        {
            var centralEssence = mEssenceItemUIs.TryGet(EssenceType.Central);

            Lance.ParticleManager.AquireUI($"Spawn_Grade_SS", centralEssence.transform as RectTransform);

            SoundPlayer.PlaySpawnItem(Grade.SS);

            yield return new WaitForSecondsRealtime(1f);

            Refresh();

            foreach (EssenceItemUI essenceItemUI in mEssenceItemUIs.Values)
            {
                essenceItemUI.PlayMotion();
            }

            SoundPlayer.PlaySkillLevelUp();
        }

        public void Refresh()
        {
            foreach(var currencyUI in mCurrencyUIs.Values)
            {
                currencyUI.Refresh();
            }

            foreach(EssenceItemUI essenceItemUI in mEssenceItemUIs.Values)
            {
                essenceItemUI.Refresh();
            }

            mEssenceSeletedUI.Refresh();
        }
    }

    class EssenceCurrencyUI : MonoBehaviour
    {
        EssenceType mType;
        TextMeshProUGUI mTextAmount;
        public void Init(EssenceType essenceType)
        {
            var imageCurrency = gameObject.FindComponent<Image>("Image_Currency");
            imageCurrency.sprite = Lance.Atlas.GetItemSlotUISprite($"Currency_Essence_{essenceType}");

            mType = essenceType;
            mTextAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_Amount");

            Refresh();
        }

        public void Refresh()
        {
            mTextAmount.text = $"{Lance.Account.Currency.GetEssence(mType)}";
        }
    }
}