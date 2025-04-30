using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class ManaHeartTabUI : MonoBehaviour
    {
        Canvas mCanvas;
        GraphicRaycaster mGraphicRaycaster;

        ParticleSystem mUpgradeManaHeartInstFX;
        ParticleSystem mUpgradeManaHeartFX;

        List<ManaHeartStatValueInfoUI> mManaHeartStatValueInfoList;

        TextMeshProUGUI mTextMyManaEssence;
        TextMeshProUGUI mTextManaHeartStep;
        ManaHeartExpUI mManaHearExpUI;

        List<GameObject> mManaHeartCircles;

        Button mButtonUpgradeManaHeart;
        TextMeshProUGUI mTextRequireLevel;
        GameObject mUpgradeManaHeartObj;
        GameObject mMaxStepManaHeartObj;
        GameObject mUpgradeManaHeartRedDot;

        Button mButtonUpgradeManaHeartInst;
        TextMeshProUGUI mTextRequireManaEssence;
        GameObject mUpgradeManaHeartInstRedDot;
        public void Init()
        {
            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();

            mTextMyManaEssence = gameObject.FindComponent<TextMeshProUGUI>("Text_MyManaEssence");
            mTextManaHeartStep = gameObject.FindComponent<TextMeshProUGUI>("Text_ManaHeart");
            mManaHearExpUI = gameObject.FindComponent<ManaHeartExpUI>("Image_ManaHeartExp");
            mManaHearExpUI.Init();

            var upgradeManaHeartInstFXObj = gameObject.FindGameObject("UpgradeManaHeartInstFX");
            mUpgradeManaHeartInstFX = upgradeManaHeartInstFXObj.FindComponent<ParticleSystem>("UpgradeManaHeartInst");
            mUpgradeManaHeartInstFX.Stop();

            var upgradeManaHearFXObj = gameObject.FindGameObject("UpgradeManaHeartFX");
            mUpgradeManaHeartFX = upgradeManaHearFXObj.FindComponent<ParticleSystem>("UpgradeManaHeart");
            mUpgradeManaHeartFX.Stop();

            mManaHeartStatValueInfoList = new List<ManaHeartStatValueInfoUI>();

            var statValues = gameObject.FindGameObject("StatValues");

            for (int i = 0; i < (int)ManaHeartType.Count; ++i)
            {
                ManaHeartType type = (ManaHeartType)i;

                var data = Lance.GameData.ManaHeartData.TryGet(type);
                if (data != null)
                {
                    var statValueObj = gameObject.FindGameObject($"StatUI_{data.valueType}");

                    var statValueUI = statValueObj.GetOrAddComponent<ManaHeartStatValueInfoUI>();
                    statValueUI.Init(type);

                    mManaHeartStatValueInfoList.Add(statValueUI);
                }
            }

            mManaHeartCircles = new List<GameObject>();

            for (int i = 0; i < DataUtil.GetManaHeartMaxStep(); ++i)
            {
                int index = i + 1;

                var manaHeartCircleObj = gameObject.FindGameObject($"ManaHeart_Circle_{index}");

                mManaHeartCircles.Add(manaHeartCircleObj);
            }

            mButtonUpgradeManaHeart = gameObject.FindComponent<Button>("Button_UpgradeManaHeart");
            mButtonUpgradeManaHeart.SetButtonAction(OnUpgradeManaHeart);
            mUpgradeManaHeartObj = mButtonUpgradeManaHeart.gameObject.FindGameObject("Upgrade");
            mUpgradeManaHeartRedDot = mUpgradeManaHeartObj.FindGameObject("RedDot");
            mTextRequireLevel = mUpgradeManaHeartObj.FindComponent<TextMeshProUGUI>("Text_RequireLevel");
            mMaxStepManaHeartObj = mButtonUpgradeManaHeart.gameObject.FindGameObject("MaxStep");

            mButtonUpgradeManaHeartInst = gameObject.FindComponent<Button>("Button_UpgradeManaHeartInst");
            mButtonUpgradeManaHeartInst.SetButtonAction(OnUpgradeManaHeartInst);
            mTextRequireManaEssence = mButtonUpgradeManaHeartInst.FindComponent<TextMeshProUGUI>("Text_UpgradeRequire");
            mUpgradeManaHeartInstRedDot = mButtonUpgradeManaHeartInst.gameObject.FindGameObject("RedDot");

            var buttonInfo = gameObject.FindComponent<Button>("Button_Info");
            buttonInfo.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_DescUI>();

                popup.Init(StringTableUtil.Get("UIString_ManaHeart"), StringTableUtil.GetDesc("ManaHeart"));
            });

            Refresh();
        }

        public void Localize()
        {
            Refresh();
        }

        const float PlayerStatUpdateTime = 0.5f;

        float mPlayerStatUpdateTime;
        public void OnUpdate()
        {
            if (mPlayerStatUpdateTime > 0)
            {
                mPlayerStatUpdateTime -= Time.deltaTime;
                if (mPlayerStatUpdateTime <= 0f)
                {
                    Lance.GameManager.UpdatePlayerStat();
                }
            }
        }

        public void OnLeave()
        {
            if (mPlayerStatUpdateTime > 0)
            {
                mPlayerStatUpdateTime = 0;

                Lance.GameManager.UpdatePlayerStat();
            }
        }

        public void Refresh()
        {
            bool isMaxUltimateLimitBreak = Lance.Account.ExpLevel.IsMaxUltimateLimitBreak();
            if (isMaxUltimateLimitBreak)
            {
                if (mCanvas.enabled != isMaxUltimateLimitBreak)
                {
                    mCanvas.enabled = true;
                    mGraphicRaycaster.enabled = true;
                }
            }
            else
            {
                if (mCanvas.enabled != isMaxUltimateLimitBreak)
                {
                    mCanvas.enabled = false;
                    mGraphicRaycaster.enabled = false;
                }
            }

            mTextMyManaEssence.text = $"{Lance.Account.Currency.GetManaEssence()}";

            foreach (var statUI in mManaHeartStatValueInfoList)
            {
                statUI.Refresh();
            }

            int manaHeartStep = Lance.Account.ManaHeart.GetStep();
            int manaHeartMaxStep = DataUtil.GetManaHeartMaxStep();
            int manaHeartUpgradeStep = Lance.Account.ManaHeart.GetUpgradeStep();

            for (int i = 0; i < manaHeartMaxStep; ++i)
            {
                bool active = (i + 1) <= manaHeartStep;
                if (mManaHeartCircles[i].activeSelf != active)
                    mManaHeartCircles[i].SetActive(active);
            }

            StringParam param = new StringParam("step", manaHeartStep);

            mTextManaHeartStep.text = StringTableUtil.Get("UIString_ManaHeartStep", param);

            var stepData = Lance.GameData.ManaHeartStepData.TryGet(manaHeartStep);

            if (manaHeartStep > 0)
            {
                int prevStep = manaHeartStep - 1;
                var prevStepData = Lance.GameData.ManaHeartStepData.TryGet(prevStep);

                int calcUpgradeStep = (manaHeartUpgradeStep - prevStepData.maxUpgradeStep);
                int calcUpgradeMaxStep = (stepData.maxUpgradeStep - prevStepData.maxUpgradeStep);

                mManaHearExpUI.SetRatio((float)calcUpgradeStep / (float)calcUpgradeMaxStep);
            }
            else
            {
                mManaHearExpUI.SetRatio((float)manaHeartUpgradeStep / (float)stepData.maxUpgradeStep);
            }

            bool isMaxStepManaHeart = manaHeartStep == manaHeartMaxStep;
            bool isMaxStepManaHeartInst = stepData.maxUpgradeStep < manaHeartUpgradeStep;

            if (isMaxStepManaHeart == false && isMaxStepManaHeartInst == false || isMaxStepManaHeart && isMaxStepManaHeartInst == false)
            {
                mButtonUpgradeManaHeart.gameObject.SetActive(false);
                mButtonUpgradeManaHeartInst.gameObject.SetActive(true);

                int upgradeRequire = Lance.Account.ManaHeart.GetUpgradeRequire();
                upgradeRequire = upgradeRequire == int.MaxValue ? -1 : upgradeRequire;

                bool isEnoughManaEssence = Lance.Account.Currency.IsEnoughManaEssence(upgradeRequire);

                mButtonUpgradeManaHeartInst.SetActiveFrame(isEnoughManaEssence);
                mUpgradeManaHeartInstRedDot.SetActive(isEnoughManaEssence);
                mTextRequireManaEssence.text = $"{upgradeRequire}";
                mTextRequireManaEssence.SetColor(isEnoughManaEssence ? Const.EnoughTextColor : Const.NotEnoughTextColor);
            }
            else if (isMaxStepManaHeart == false && isMaxStepManaHeartInst )
            {
                mButtonUpgradeManaHeart.gameObject.SetActive(true);
                mButtonUpgradeManaHeartInst.gameObject.SetActive(false);

                mUpgradeManaHeartObj.SetActive(true);
                mMaxStepManaHeartObj.SetActive(false);

                int requireLevel = stepData.requireLevel;
                int myLevel = Lance.Account.ExpLevel.GetLevel();
                bool isEnoughLevel = requireLevel <= myLevel;

                mButtonUpgradeManaHeart.SetActiveFrame(isEnoughLevel);
                mUpgradeManaHeartRedDot.SetActive(isEnoughLevel);

                StringParam param2 = new StringParam("level", requireLevel);

                mTextRequireLevel.text = StringTableUtil.Get("UIString_RequireLevel", param2);
            }
            else
            {
                mButtonUpgradeManaHeart.gameObject.SetActive(true);
                mButtonUpgradeManaHeartInst.gameObject.SetActive(false);

                mUpgradeManaHeartObj.SetActive(false);
                mMaxStepManaHeartObj.SetActive(true);

                mButtonUpgradeManaHeart.SetActiveFrame(false);
                mUpgradeManaHeartRedDot.SetActive(false);
            }
        }

        void OnUpgradeManaHeart()
        {
            if (Lance.GameManager.UpgradeManaHeart())
            {
                StartCoroutine(StartUpgradeManaHeartMotion());
            }
        }

        IEnumerator StartUpgradeManaHeartMotion()
        {
            SoundPlayer.PlayUpgradeManaHeartCharge();

            Lance.HideTouchBlock.SetActive(true);

            mUpgradeManaHeartFX.Stop();
            mUpgradeManaHeartFX.Play();

            yield return new WaitForSecondsRealtime(1f);

            Lance.HideTouchBlock.SetActive(false);

            mPlayerStatUpdateTime = PlayerStatUpdateTime;

            SoundPlayer.PlayUpgradeManaHeartFinish();

            Refresh();
        }

        void OnUpgradeManaHeartInst()
        {
            if (Lance.GameManager.UpgradeManaHeartInst())
            {
                mPlayerStatUpdateTime = PlayerStatUpdateTime;

                mUpgradeManaHeartInstFX.Stop();
                mUpgradeManaHeartInstFX.Play();

                Refresh();

                SoundPlayer.PlayUpgradeManaHeartInst();
            }
        }
    }

    class ManaHeartStatValueInfoUI : MonoBehaviour
    {
        ManaHeartType mType;
        StatType mStatType;

        GameObject mNextLevelObj;
        TextMeshProUGUI mTextNextCurrentValue;
        TextMeshProUGUI mTextNextValue;
        GameObject mCurrentLevelObj;
        TextMeshProUGUI mTextCurrentValue;
        public void Init(ManaHeartType type)
        {
            mType = type;

            var data = Lance.GameData.ManaHeartData.TryGet(type);

            var textStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
            textStatName.text = StringTableUtil.GetName($"{data.valueType}");

            mStatType = data.valueType;

            mNextLevelObj = gameObject.FindGameObject("NextLevel");
            mTextNextCurrentValue = mNextLevelObj.FindComponent<TextMeshProUGUI>("Text_NextCurrentValue");
            mTextNextValue = mNextLevelObj.FindComponent<TextMeshProUGUI>("Text_NextValue");

            mCurrentLevelObj = gameObject.FindGameObject("CurrentLevel");
            mTextCurrentValue = mCurrentLevelObj.FindComponent<TextMeshProUGUI>("Text_CurrentValue");
        }

        public void Refresh()
        {
            int curStep = Lance.Account.ManaHeart.GetUpgradeStep();

            var data = Lance.GameData.ManaHeartUpgradeStepData.TryGet(curStep);
            if (data != null)
            {
                if (mNextLevelObj.activeSelf != (data.type == mType))
                    mNextLevelObj.SetActive(data.type == mType);
                if (mCurrentLevelObj.activeSelf != (data.type != mType))
                    mCurrentLevelObj.SetActive(data.type != mType);

                if (data.type == mType)
                {
                    double curValue = Lance.Account.ManaHeart.GetStatValue(mType);
                    double nextValue = Lance.Account.ManaHeart.GetNextStatValue(mType);

                    mTextNextCurrentValue.text = mStatType.IsPercentType() ? $"{curValue * 100f:F2}%" :
                mStatType.IsNoneAlphaType() ? $"{curValue:F2}" : $"{curValue.ToAlphaString(showDp: ((mStatType == StatType.Atk || mStatType == StatType.Hp) ? ShowDecimalPoint.GoldTrain : ShowDecimalPoint.Default))}";

                    mTextNextValue.text = mStatType.IsPercentType() ? $"{nextValue * 100f:F2}%" :
                mStatType.IsNoneAlphaType() ? $"{nextValue:F2}" : $"{nextValue.ToAlphaString(showDp: ((mStatType == StatType.Atk || mStatType == StatType.Hp) ? ShowDecimalPoint.GoldTrain : ShowDecimalPoint.Default))}";
                }
                else
                {
                    double curValue = Lance.Account.ManaHeart.GetStatValue(mType);

                    mTextCurrentValue.text = mStatType.IsPercentType() ? $"{curValue * 100f:F2}%" :
                mStatType.IsNoneAlphaType() ? $"{curValue:F2}" : $"{curValue.ToAlphaString(showDp: ((mStatType == StatType.Atk || mStatType == StatType.Hp) ? ShowDecimalPoint.GoldTrain : ShowDecimalPoint.Default))}";
                }
            }
            else
            {
                if (mNextLevelObj.activeSelf != false)
                    mNextLevelObj.SetActive(false);
                if (mCurrentLevelObj.activeSelf != true)
                    mCurrentLevelObj.SetActive(true);

                double curValue = Lance.Account.ManaHeart.GetStatValue(mType);

                mTextCurrentValue.text = mStatType.IsPercentType() ? $"{curValue * 100f:F2}%" :
                mStatType.IsNoneAlphaType() ? $"{curValue:F2}" : $"{curValue.ToAlphaString(showDp: ((mStatType == StatType.Atk || mStatType == StatType.Hp) ? ShowDecimalPoint.GoldTrain : ShowDecimalPoint.Default))}";
            }
        }
    }
}
