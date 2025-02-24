using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Stature_LimitBreakTabUI : StatureTabUI
    {
        LimitBreakTabUI mLimitBreakTabUI;
        UltimateLimitBreakTabUI mUltimateLimitBreakTabUI;
        ManaHeartTabUI mManaHeartTabUI;
        public override void Init(StatureTab tab)
        {
            base.Init(tab);

            var limitBreakTabUI = gameObject.FindGameObject("LimitBreakTab");

            mLimitBreakTabUI = limitBreakTabUI.GetOrAddComponent<LimitBreakTabUI>();
            mLimitBreakTabUI.Init();

            var ultimateLimitBreakTabUI = gameObject.FindGameObject("UltimateLimitBreakTab");

            mUltimateLimitBreakTabUI = ultimateLimitBreakTabUI.GetOrAddComponent<UltimateLimitBreakTabUI>();
            mUltimateLimitBreakTabUI.Init();

            var manaHeartTabUI = gameObject.FindGameObject("ManaHeartTab");

            mManaHeartTabUI = manaHeartTabUI.GetOrAddComponent<ManaHeartTabUI>();
            mManaHeartTabUI.Init();

            Refresh();
        }

        public override void OnEnter()
        {
            base.OnEnter();

            Refresh();
        }

        public override void OnLeave()
        {
            base.OnLeave();

            mManaHeartTabUI.OnLeave();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            mManaHeartTabUI.OnUpdate();
        }

        public override void Refresh()
        {
            mLimitBreakTabUI.Refresh();
            mUltimateLimitBreakTabUI.Refresh();
            mManaHeartTabUI.Refresh();
        }

        public override void Localize()
        {
            mLimitBreakTabUI.Localize();
            mUltimateLimitBreakTabUI.Localize();
            mManaHeartTabUI.Localize();
        }
    }
}