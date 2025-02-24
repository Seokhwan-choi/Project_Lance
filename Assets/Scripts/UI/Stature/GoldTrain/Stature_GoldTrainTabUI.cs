using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Lance
{
    class Stature_GoldTrainTabUI : StatureTabUI
    {
        GoldTrainCountUIManager mCountUIManager;
        List<GoldTrainItemUI> mGoldTrainItemList;

        Image mImageMaxLevelHideCheck;
        public int GoldTrainCount => mCountUIManager.GoldTrainCount;
        public override void Init(StatureTab tab)
        {
            base.Init(tab);

            mCountUIManager = new GoldTrainCountUIManager();
            mCountUIManager.Init(this);

            var buttonMaxLevelHide = gameObject.FindComponent<Button>("MaxLevelHide");
            buttonMaxLevelHide.SetButtonAction(OnToggleMaxLevelHide);

            mImageMaxLevelHideCheck = buttonMaxLevelHide.FindComponent<Image>("Image_Check");
            mImageMaxLevelHideCheck.gameObject.SetActive(SaveBitFlags.GoldTrainMaxLevelHide.IsOn());

            mGoldTrainItemList = new List<GoldTrainItemUI>();

            var goldTrainItemListObj = gameObject.FindGameObject("GoldTrainList");

            goldTrainItemListObj.AllChildObjectOff();

            for (int i = 0; i < (int)StatType.Count; ++i)
            {
                StatType type = (StatType)i;

                if (type.IsGoldTrainType() == false)
                    continue;

                GameObject itemObj = Util.InstantiateUI("GoldTrainItemUI", goldTrainItemListObj.transform);

                var itemUI = itemObj.GetOrAddComponent<GoldTrainItemUI>();
                itemUI.Init(this, type);

                mGoldTrainItemList.Add(itemUI);
            }
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void Localize()
        {
            foreach (var item in mGoldTrainItemList)
            {
                item.Localize();
            }
        }

        public override void Refresh()
        {
            foreach (var item in mGoldTrainItemList)
            {
                item.Refresh();
            }
        }

        public void UpdateRequireGold()
        {
            foreach (var item in mGoldTrainItemList)
            {
                item.UpdateRequireGold();
                item.UpdateRequireType();
            }
        }

        public override void OnLeave()
        {
            foreach (var item in mGoldTrainItemList)
            {
                item.OnTabLeave();
            }
        }

        public void OnChangeGoldTrainCount()
        {
            foreach (var item in mGoldTrainItemList)
            {
                item.OnChangeGoldTrainCount();
            }
        }

        void OnToggleMaxLevelHide()
        {
            SaveBitFlags.GoldTrainMaxLevelHide.Toggle();

            mImageMaxLevelHideCheck.gameObject.SetActive(SaveBitFlags.GoldTrainMaxLevelHide.IsOn());

            Refresh();
        }
    }
}