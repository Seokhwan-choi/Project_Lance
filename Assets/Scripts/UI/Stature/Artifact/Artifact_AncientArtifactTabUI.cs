using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lance
{
    class Artifact_AncientArtifactTabUI : ArtifactTabUI
    {
        List<ArtifactItemUI> mArtifactItemUIList;
        public override void Init(ArtifactTabUIManager parent, ArtifactTab tab)
        {
            base.Init(parent, tab);

            mArtifactItemUIList = new List<ArtifactItemUI>();

            var artifactItemListObj = gameObject.FindGameObject("ArtifactList");

            artifactItemListObj.AllChildObjectOff();

            foreach (var data in Lance.GameData.AncientArtifactData.Values)
            {
                GameObject itemObj = Util.InstantiateUI("ArtifactItemUI", artifactItemListObj.transform);

                var itemUI = itemObj.GetOrAddComponent<ArtifactItemUI>();
                itemUI.Init(this, data.id);

                mArtifactItemUIList.Add(itemUI);
            }

            var buttonAllDismantle = gameObject.FindComponent<Button>("Button_AllDismantle");
            buttonAllDismantle.SetButtonAction(OnDismantleAllButton);
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void OnLeave()
        {
            base.OnLeave();

            foreach (var item in mArtifactItemUIList)
            {
                item.OnTabLeave();
            }
        }

        public override void Localize()
        {
            foreach (var item in mArtifactItemUIList)
            {
                item.Localize();
            }
        }

        public override void Refresh()
        {
            base.Refresh();

            foreach (var item in mArtifactItemUIList)
            {
                item.Refresh();
            }
        }

        void OnDismantleAllButton()
        {
            if (Lance.Account.AncientArtifact.AnyCanDismantle() == false)
            {
                UIUtil.ShowSystemErrorMessage("AnyCanNotDismantleAncientArtifact");

                return;
            }

            Lance.GameManager.DismantleAllAncientArtifact();

            Refresh();

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
        }
    }
}