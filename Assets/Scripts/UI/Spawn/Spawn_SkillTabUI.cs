using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Spawn_SkillTabUI : SpawnTabUI
    {
        SpawnItemUI mSpawnItemUI;
        public override void Init(SpawnTab tab)
        {
            base.Init(tab);

            SpawnData spawnData = Lance.GameData.SpawnData.TryGet(ItemType.Skill);
            if (spawnData == null)
                return;

            GameObject spawnItemObj = gameObject.FindGameObject("SkillSpawnItemUI");
            mSpawnItemUI = spawnItemObj.GetOrAddComponent<SpawnItemUI>();
            mSpawnItemUI.Init(spawnData);
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void RefreshContentsLockUI()
        {
            mSpawnItemUI.RefreshContentsLockUI();
        }

        public override void Localize()
        {
            mSpawnItemUI.Localize();
        }

        public override void Refresh()
        {
            mSpawnItemUI.Refresh();
        }
    }
}