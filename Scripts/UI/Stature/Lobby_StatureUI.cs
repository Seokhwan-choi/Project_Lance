using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class Lobby_StatureUI : LobbyTabUI
    {
        StatureTabUIManager mTabUIManager;
        public override void Init(LobbyTab tab)
        {
            base.Init(tab);

            mTabUIManager = new StatureTabUIManager();
            mTabUIManager.Init(gameObject);
        }

        public void ChangeTab(StatureTab tab)
        {
            mTabUIManager.ChangeTab(tab);
            mTabUIManager.RefreshTabFrame(tab);
        }

        public override void OnEnter()
        {
            mTabUIManager.OnEnter();
        }

        public override void OnUpdate()
        {
            mTabUIManager.OnUpdate();
        }

        public override void Refresh()
        {
            mTabUIManager.Refresh();
        }

        public override void RefreshAll()
        {
            mTabUIManager.RefreshAll();
        }

        public override void Localize()
        {
            mTabUIManager.Localize();
        }

        public override void RefreshContentsLockUI()
        {
            mTabUIManager.RefreshContentsLockUI();
        }

        public override void RefreshRedDots()
        {
            mTabUIManager.RefreshRedDots();
        }

        public void RefreshTab(StatureTab tab)
        {
            mTabUIManager.RefreshTab(tab);
        }

        public Transform GetCanUpgradeArtifactItemUI()
        {
            var artifactTabUI = mTabUIManager.GetTab<Stature_ArtifactTabUI>();

            return artifactTabUI?.GetCanUpgradeArtifactItemUI();
        }

        public Transform GetFirstAbilityItemUI()
        {
            var abilityTabUI = mTabUIManager.GetTab<Stature_AbilityTabUI>();

            return abilityTabUI?.GetFirstAbilityItemUI();
        }
    }
}