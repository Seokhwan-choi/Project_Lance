using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Lance
{
    class Artifact_ArtifactTabUI : ArtifactTabUI
    {
        List<ArtifactItemUI> mArtifactItemUIList;
        public override void Init(ArtifactTabUIManager parent, ArtifactTab tab)
        {
            base.Init(parent, tab);

            mArtifactItemUIList = new List<ArtifactItemUI>();

            var artifactItemListObj = gameObject.FindGameObject("ArtifactList");

            artifactItemListObj.AllChildObjectOff();

            foreach (var data in Lance.GameData.ArtifactData.Values)
            {
                GameObject itemObj = Util.InstantiateUI("ArtifactItemUI", artifactItemListObj.transform);

                var itemUI = itemObj.GetOrAddComponent<ArtifactItemUI>();
                itemUI.Init(this, data.id);

                mArtifactItemUIList.Add(itemUI);
            }

            var buttonAllSell = gameObject.FindComponent<Button>("Button_AllSell");
            buttonAllSell.SetButtonAction(OnSellAllButton);

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

        public override void Refresh()
        {
            base.Refresh();

            foreach(var item in mArtifactItemUIList)
            {
                item.Refresh();
            }
        }

        public override void Localize()
        {
            foreach (var item in mArtifactItemUIList)
            {
                item.Localize();
            }
        }

        public Transform GetCanUpgradeArtifactItemUI()
        {
            foreach (var item in mArtifactItemUIList)
            {
                if (Lance.Account.Artifact.CanLevelUp(item.Id))
                    return item.transform;
            }

            return mArtifactItemUIList.First().transform;
        }

        void OnSellAllButton()
        {
            if (Lance.Account.Artifact.AnyCanSell() == false)
            {
                UIUtil.ShowSystemErrorMessage("AnyCanNotSellArtifact");

                return;
            }

            Lance.GameManager.SellAllArtifact();

            Refresh();

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
        }

        void OnDismantleAllButton()
        {
            if (Lance.Account.Artifact.AnyCanDismantle() == false)
            {
                UIUtil.ShowSystemErrorMessage("AnyCanNotDismantleArtifact");

                return;
            }

            Lance.GameManager.DismantleAllArtifact();

            Refresh();

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
        }
    }
}