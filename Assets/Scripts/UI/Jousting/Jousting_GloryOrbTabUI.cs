using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class Jousting_GloryOrbTabUI : JoustingTabUI
    {
        bool mOnUpgraded;
        List<GloryOrbItemUI> mGloryOrbItemList;
        List<JoustGloryOrbStatValueInfoUI> mGloryOrbStatValueInfoList;

        Button mButtonUpgrade;
        TextMeshProUGUI mTextRequire;
        GameObject mUpgradeRedDot;
        public override void Init(JoustingTabUIManager parent, JoustingTab tab)
        {
            base.Init(parent, tab);

            mGloryOrbItemList = new List<GloryOrbItemUI>();
            mGloryOrbStatValueInfoList = new List<JoustGloryOrbStatValueInfoUI>();

            var gloryOrbs = gameObject.FindGameObject("GloryOrbs");
            var statValues = gameObject.FindGameObject("StatValues");
            statValues.AllChildObjectOff();

            for (int i = 0; i < (int)JoustGloryOrbType.Count; ++i)
            {
                JoustGloryOrbType type = (JoustGloryOrbType)i;

                var gloryOrbObj = gloryOrbs.FindGameObject($"{type}");

                var gloryOrbItemUI = gloryOrbObj.GetOrAddComponent<GloryOrbItemUI>();
                gloryOrbItemUI.Init(type);

                mGloryOrbItemList.Add(gloryOrbItemUI);

                var statValueObj = Util.InstantiateUI("GloryOrbStatValueInfoUI", statValues.transform);
                var statValueUI = statValueObj.GetOrAddComponent<JoustGloryOrbStatValueInfoUI>();
                statValueUI.Init(type);

                mGloryOrbStatValueInfoList.Add(statValueUI);
            }

            mButtonUpgrade = gameObject.FindComponent<Button>("Button_Upgrade");
            mButtonUpgrade.SetButtonAction(OnUpgrade);

            mTextRequire = mButtonUpgrade.FindComponent<TextMeshProUGUI>("Text_RequireAmount");
            mUpgradeRedDot = mButtonUpgrade.gameObject.FindGameObject("RedDot");

            mOnUpgraded = false;

            Refresh();
        }

        public override void OnEnter()
        {
            Refresh();

            mOnUpgraded = false;
        }

        public override void OnLeave()
        {
            base.OnLeave();

            if (mOnUpgraded)
            {
                mOnUpgraded = false;

                Lance.GameManager.UpdatePlayerStat();
            }
        }

        public override void OnClose()
        {
            base.OnClose();

            OnLeave();
        }

        public override void Refresh()
        {
            foreach(var itemUI in mGloryOrbItemList)
            {
                itemUI.Refresh();
            }

            foreach(var statUI in mGloryOrbStatValueInfoList)
            {
                statUI.Refresh();
            }

            bool canUpgrade = Lance.Account.CanUpgradeJoustGloryOrb();
            int require = Lance.Account.JoustGloryOrb.GetUpgradeRequire();
            require = require == int.MaxValue ? -1 : require;
            bool isEnoughToken = Lance.Account.Currency.IsEnoughGloryToken(require);
            
            mButtonUpgrade.SetActiveFrame(canUpgrade);
            mUpgradeRedDot.SetActive(canUpgrade);
            mTextRequire.text = $"{require}";
            mTextRequire.SetColor(isEnoughToken ? Const.EnoughTextColor : Const.NotEnoughTextColor);
        }

        void OnUpgrade()
        {
            int step = Lance.Account.JoustGloryOrb.GetStep();
            var stepData = Lance.GameData.JoustingGloryOrbStepData.TryGet(step);

            if (Lance.GameManager.UpgradeGloryOrb())
            {
                mOnUpgraded = true;
                // 강화 사운드
                SoundPlayer.PlayGloryOrbUpgrade(stepData.type);

                // 강화 이펙트
                foreach (var item in mGloryOrbItemList)
                {
                    if (item.Type == stepData.type)
                    {
                        item.PlayUpgradeFX();
                        break;
                    }
                }

                Refresh();

                mParent.RefreshCurrency();
                mParent.RefreshRedDots();
            }
        }
    }

    class GloryOrbItemUI : MonoBehaviour
    {
        JoustGloryOrbType mType;

        Image mImageCanUpgrade;
        TextMeshProUGUI mTextLevel;
        ParticleSystem mUpgradeFX;
        public JoustGloryOrbType Type => mType;
        public void Init(JoustGloryOrbType type)
        {
            mType = type;

            mImageCanUpgrade = gameObject.FindComponent<Image>("CanUpgrade");
            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            mUpgradeFX = gameObject.FindComponent<ParticleSystem>($"Upgrade{type}");
        }

        public void Refresh()
        {
            int curStep = Lance.Account.JoustGloryOrb.GetStep();

            var data = Lance.GameData.JoustingGloryOrbStepData.TryGet(curStep);
            if (data != null)
                SetActiveCanUpgrade(data.type == mType);
            else
                SetActiveCanUpgrade(false);

            mTextLevel.text = $"{Lance.Account.JoustGloryOrb.GetLevel(mType)}";
        }

        public void PlayUpgradeFX()
        {
            mUpgradeFX?.Stop();
            mUpgradeFX?.Play();
        }

        public void SetActiveCanUpgrade(bool active)
        {
            mImageCanUpgrade.gameObject.SetActive(active);
        }
    }

    class JoustGloryOrbStatValueInfoUI : MonoBehaviour
    {
        JoustGloryOrbType mType;
        StatType mStatType;

        GameObject mNextLevelObj;
        TextMeshProUGUI mTextNextCurrentValue;
        TextMeshProUGUI mTextNextValue;
        GameObject mCurrentLevelObj;
        TextMeshProUGUI mTextCurrentValue;
        public void Init(JoustGloryOrbType type)
        {
            mType = type;

            var data = Lance.GameData.JoustingGloryOrbData.TryGet(type);

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
            int curStep = Lance.Account.JoustGloryOrb.GetStep();

            var data = Lance.GameData.JoustingGloryOrbStepData.TryGet(curStep);
            if (data != null)
            {
                mNextLevelObj.SetActive(data.type == mType);
                mCurrentLevelObj.SetActive(data.type != mType);

                if (data.type == mType)
                {
                    double curValue = Lance.Account.JoustGloryOrb.GetStatValue(mType);
                    double nextValue = Lance.Account.JoustGloryOrb.GetNextStatValue(mType);

                    mTextNextCurrentValue.text = mStatType.IsPercentType() ? $"{curValue * 100:F2}%" : $"{curValue.ToAlphaString()}";
                    mTextNextValue.text = mStatType.IsPercentType() ? $"{nextValue * 100:F2}%" : $"{nextValue.ToAlphaString()}";
                }
                else
                {
                    double curValue = Lance.Account.JoustGloryOrb.GetStatValue(mType);

                    mTextCurrentValue.text = mStatType.IsPercentType() ? $"{curValue * 100:F2}%" : $"{curValue.ToAlphaString()}";
                }
            }
            else
            {
                mNextLevelObj.SetActive(false);
                mCurrentLevelObj.SetActive(true);

                double curValue = Lance.Account.JoustGloryOrb.GetStatValue(mType);

                mTextCurrentValue.text = mStatType.IsPercentType() ? $"{curValue * 100:F2}%" : $"{curValue.ToAlphaString()}";
            }
        }
    }
}
