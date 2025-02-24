using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Lance
{
    class Stature_ArtifactTabUI : StatureTabUI
    {
        ArtifactTabUIManager mTabUIManager;
        
        public override void Init(StatureTab tab)
        {
            base.Init(tab);

            mTabUIManager = new ArtifactTabUIManager();
            mTabUIManager.Init(gameObject);
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void Refresh()
        {
            mTabUIManager.Refresh();
        }

        public override void OnLeave()
        {
            mTabUIManager.OnLeave();
        }

        public override void Localize()
        {
            mTabUIManager.Localize();
        }

        public Transform GetCanUpgradeArtifactItemUI()
        {
            var tab = mTabUIManager.GetTab<Artifact_ArtifactTabUI>();

            return tab.GetCanUpgradeArtifactItemUI();
        }

        public override void RefreshRedDots()
        {
            base.RefreshRedDots();

            mTabUIManager.RefreshRedDots();
        }

        public override void RefreshContentsLockUI()
        {
            base.RefreshContentsLockUI();

            mTabUIManager.RefreshContentsLockUI();
        }
    }
}