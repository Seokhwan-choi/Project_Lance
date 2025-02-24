using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Spawn_AccessoryTabUI : SpawnTabUI
    {
        List<SpawnItemUI> mSpawnItemUI;
        public override void Init(SpawnTab tab)
        {
            base.Init(tab);

            GameObject spawnItemUIList = gameObject.FindGameObject("SpawnItemUIList");

            spawnItemUIList.AllChildObjectOff();

            mSpawnItemUI = new List<SpawnItemUI>();

            for (int i = 0; i < (int)ItemType.Count; ++i)
            {
                ItemType itemType = (ItemType)i;

                if (itemType.IsAccessory() == false)
                    continue;

                SpawnData spawnData = Lance.GameData.SpawnData.TryGet(itemType);
                if (spawnData == null)
                    continue;

                GameObject spawnItemObj = Util.InstantiateUI("AccessorySpawnItemUI", spawnItemUIList.transform);
                SpawnItemUI spawnItemUI = spawnItemObj.GetOrAddComponent<SpawnItemUI>();
                spawnItemUI.Init(spawnData);

                mSpawnItemUI.Add(spawnItemUI);
            }
        }

        public override void RefreshContentsLockUI()
        {
            foreach (var spawnItemUI in mSpawnItemUI)
            {
                spawnItemUI.RefreshContentsLockUI();
            }
        }

        public override void Localize()
        {
            foreach (var spawnItemUI in mSpawnItemUI)
            {
                spawnItemUI.Localize();
            }
        }

        public override void Refresh()
        {
            base.Refresh();

            foreach (var spawnItemUI in mSpawnItemUI)
            {
                spawnItemUI.Refresh();
            }
        }
    }
}