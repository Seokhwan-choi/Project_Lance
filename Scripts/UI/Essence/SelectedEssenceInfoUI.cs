using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class SelectedEssenceInfoUI : MonoBehaviour
    {
        EssenceType mSelectedEssence;
        Image mImageSelectedEssence;
        TextMeshProUGUI mTextSelectedEssenceName;
        //TextMeshProUGUI mTextSelectedEssenceMaxLevel;
        EssenceStatValueInfoUI mStatValueInfo;
        TextMeshProUGUI mTextRequireCondition;

        Button mButtonUpgrade;
        GameObject mCanUpgradeObj;
        GameObject mRedDotObj;
        GameObject mCurrencyObj;
        Image mImageRequireCurrency;
        TextMeshProUGUI mTextRequireAmount;

        public EssenceType SelectedEssence => mSelectedEssence;
        public void Init(Action<EssenceType> onUpgrade)
        {
            mImageSelectedEssence = gameObject.FindComponent<Image>("Image_Essence");
            mTextSelectedEssenceName = gameObject.FindComponent<TextMeshProUGUI>("Text_SelectedEssenceName");
            //mTextSelectedEssenceMaxLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_SelectedEssenceMaxLevel");

            var statValueInfoObj = gameObject.FindGameObject("StatValueInfoUI");

            mStatValueInfo = statValueInfoObj.GetOrAddComponent<EssenceStatValueInfoUI>();
            mStatValueInfo.Init();

            mTextRequireCondition = gameObject.FindComponent<TextMeshProUGUI>("Text_RequireCondition");

            mButtonUpgrade = gameObject.FindComponent<Button>("Button_Upgrade");
            mButtonUpgrade.SetButtonAction(() => OnUpgradeButton(onUpgrade));
            mCanUpgradeObj = mButtonUpgrade.gameObject.FindGameObject("CanUpgrade");
            mRedDotObj = mButtonUpgrade.gameObject.FindGameObject("RedDot");
            mCurrencyObj = gameObject.FindGameObject("Currency");
            mImageRequireCurrency = mCurrencyObj.FindComponent<Image>("Image_Currency");
            mTextRequireAmount = mCurrencyObj.FindComponent<TextMeshProUGUI>("Text_RequireAmount");
        }

        public void OnSelectEssence(EssenceType selectEssence)
        {
            mSelectedEssence = selectEssence;

            Refresh();
        }

        public void Refresh()
        {
            int step = Lance.Account.Essence.GetStep(mSelectedEssence);

            mImageSelectedEssence.sprite = Lance.Atlas.GetItemSlotUISprite($"Essence_{mSelectedEssence}_{step + 1}");

            StringParam stepParam = new StringParam("step", step + 1);

            mTextSelectedEssenceName.text = $"{StringTableUtil.GetName($"Essence_{mSelectedEssence}")} {StringTableUtil.Get("UIString_Step", stepParam)}";

            mStatValueInfo.ChangeType(mSelectedEssence);

            bool canUpgrade = Lance.Account.CanUpgradeEssence(mSelectedEssence);

            mButtonUpgrade.SetActiveFrame(canUpgrade);
            mRedDotObj.SetActive(canUpgrade);
            mCanUpgradeObj.SetActive(canUpgrade);
            mTextRequireCondition.gameObject.SetActive(!canUpgrade);

            if (!canUpgrade)
            {
                bool isMaxStep = Lance.Account.Essence.IsMaxStep(mSelectedEssence);

                if (mSelectedEssence == EssenceType.Central)
                {
                    if (isMaxStep)
                    {
                        mTextRequireCondition.text = StringTableUtil.Get("UIString_IsMaxStepEssence");
                    }
                    else
                    {
                        CentralEssenceStepData essenceStepData = Lance.GameData.CentralEssenceStepData.TryGet(step);

                        StringParam param = new StringParam("level", essenceStepData.requireAllEssenceLevel);

                        mTextRequireCondition.text = StringTableUtil.Get("UIString_RequireCentralEssenceCondition", param);
                    }
                }
                else
                {
                    bool isMaxLevel = Lance.Account.Essence.IsMaxLevel(mSelectedEssence);

                    if (isMaxStep && isMaxLevel)
                    {
                        mTextRequireCondition.text = StringTableUtil.Get("UIString_IsMaxStepEssence");
                    }
                    else if (isMaxLevel)
                    {
                        EssenceStepData essenceStepData = DataUtil.GetEssenceStepData(mSelectedEssence, step + 1);

                        StringParam param = new StringParam("step", essenceStepData.requireCentralStep + 1);

                        mTextRequireCondition.text = StringTableUtil.Get("UIString_RequireEssenceCondition", param);
                    }
                    else
                    {
                        mCanUpgradeObj.SetActive(true);
                        mTextRequireCondition.gameObject.SetActive(false);
                    }
                }
            }

            if (mSelectedEssence == EssenceType.Central)
            {
                mCurrencyObj.SetActive(false);
            }
            else
            {
                mCurrencyObj.SetActive(true);
                int myEssenceElement = Lance.Account.Currency.GetEssence(mSelectedEssence);
                int requireEssenceElement = Lance.Account.Essence.GetUpgradeRequireElements(mSelectedEssence);
                bool isEnough = myEssenceElement >= requireEssenceElement;

                mTextRequireAmount.text = $"{myEssenceElement}/{requireEssenceElement}";
                mTextRequireAmount.SetColor(isEnough ? Const.EnoughTextColor : Const.NotEnoughTextColor);

                mImageRequireCurrency.sprite = Lance.Atlas.GetItemSlotUISprite($"Currency_Essence_{mSelectedEssence}");
            }
        }

        void OnUpgradeButton(Action<EssenceType> onUpgrade)
        {
            if (Lance.GameManager.UpgradeEssence(mSelectedEssence))
            {
                onUpgrade?.Invoke(mSelectedEssence);
            }
        }
    }
}