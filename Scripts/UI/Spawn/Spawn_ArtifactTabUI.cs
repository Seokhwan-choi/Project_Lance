using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Spawn_ArtifactTabUI : SpawnTabUI
    {
        SpawnItemUI mArtifactSpawnItemUI;
        SpawnItemUI mAncientArtifactSpawnItemUI;
        public override void Init(SpawnTab tab)
        {
            base.Init(tab);

            SpawnData spawnData = Lance.GameData.SpawnData.TryGet(ItemType.Artifact);
            if (spawnData != null)
            {
                GameObject spawnItemObj = gameObject.FindGameObject("ArtifactSpawnItemUI");
                mArtifactSpawnItemUI = spawnItemObj.GetOrAddComponent<SpawnItemUI>();
                mArtifactSpawnItemUI.Init(spawnData);
            }

            SpawnData spawnData2 = Lance.GameData.SpawnData.TryGet(ItemType.AncientArtifact);
            if (spawnData2 != null)
            {
                GameObject spawnItemObj = gameObject.FindGameObject("AncientArtifactSpawnItemUI");
                mAncientArtifactSpawnItemUI = spawnItemObj.GetOrAddComponent<SpawnItemUI>();
                mAncientArtifactSpawnItemUI.Init(spawnData2);
            }
        }

        public override void OnEnter()
        {
            Refresh();

            RefreshContentsLockUI();
        }

        public override void RefreshContentsLockUI()
        {
            mArtifactSpawnItemUI?.RefreshContentsLockUI();
            mAncientArtifactSpawnItemUI?.RefreshContentsLockUI();
        }

        public override void Localize()
        {
            mArtifactSpawnItemUI.Localize();
            mAncientArtifactSpawnItemUI.Localize();
        }

        public override void Refresh()
        {
            mArtifactSpawnItemUI?.Refresh();
            mAncientArtifactSpawnItemUI?.Refresh();
        }
    }
}