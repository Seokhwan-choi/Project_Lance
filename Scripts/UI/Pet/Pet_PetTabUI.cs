using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lance
{
    class Pet_PetTabUI : PetTabUI
    {
        List<PetItemUI> mPetItemUIList;

        public override void Init(PetTab tab)
        {
            base.Init(tab);

            mPetItemUIList = new List<PetItemUI>();

            var petItemList = gameObject.FindGameObject("PetItemList");

            petItemList.AllChildObjectOff();

            foreach (var data in Lance.GameData.PetData.Values)
            {
                var petItemObj = Util.InstantiateUI("PetItemUI", petItemList.transform);

                var petItemUI = petItemObj.GetOrAddComponent<PetItemUI>();

                petItemUI.Init(data.id);

                mPetItemUIList.Add(petItemUI);
            }
        }

        public override void Localize()
        {
            base.Localize();

            foreach (var petItemUI in mPetItemUIList)
            {
                petItemUI.Localize();
            }
        }

        public override void Refresh()
        {
            base.Refresh();

            foreach (var petItemUI in mPetItemUIList)
            {
                petItemUI.Refresh();
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            Refresh();
        }

        public override void RefreshRedDots()
        {
            base.RefreshRedDots();

            foreach (var petItemUI in mPetItemUIList)
            {
                petItemUI.RefreshRedDot();
            }
        }

        public RectTransform GetBestPetItemUI()
        {
            int maxLevel = 0;
            RectTransform maxLevelRectTm = null;

            foreach (PetItemUI petItemUI in mPetItemUIList)
            {
                var level = Lance.Account.Pet.GetLevel(petItemUI.Id);
                if (maxLevel < level)
                {
                    maxLevel = level;

                    maxLevelRectTm = petItemUI.transform as RectTransform;
                }
            }

            return maxLevelRectTm;
        }
    }
}