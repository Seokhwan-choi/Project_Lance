using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;

namespace Lance
{
    class Stature_AbilityTabUI : StatureTabUI
    {
        AbilityInfoUI mAbilityInfoUI;
        Dictionary<int, AbilityItemUI> mItemDics;
        public override void Init(StatureTab tab)
        {
            base.Init(tab);

            var infoObj = gameObject.FindGameObject("AbilityInfo");

            mAbilityInfoUI = infoObj.GetOrAddComponent<AbilityInfoUI>();
            mAbilityInfoUI.Init(this);

            var itemList = gameObject.FindGameObject("AbilityItemList");

            itemList.AllChildObjectOff();

            mItemDics = new Dictionary<int, AbilityItemUI>();

            foreach (var data in Lance.GameData.AbilityData.Values)
            {
                int step = data.step;

                if (mItemDics.ContainsKey(data.step))
                    continue;

                var abilityItemObj = Util.InstantiateUI("AbilityItemUI", itemList.transform);

                var abilityItemUI = abilityItemObj.GetOrAddComponent<AbilityItemUI>();

                abilityItemUI.Init(step, ChangeId);

                mItemDics.Add(data.step, abilityItemUI);
            }
        }

        void ChangeId(string id)
        {
            mAbilityInfoUI.ChangeId(id);

            foreach (var item in mItemDics.Values)
            {
                item.SetActiveSelected(id);
                item.Refresh();
            }
        }

        public Transform GetFirstAbilityItemUI()
        {
            // ³»°¡ Áö±Ý ÂïÀº ÃÖ°í ½ºÅÇ
            (string id, int step) result = Lance.Account.Ability.GetBestStepAbility();

            AbilityItemUI itemUI = mItemDics.TryGet(result.step);

            return itemUI.GetSlot(result.id)?.transform;
        }

        public void RefreshItems()
        {
            foreach (var item in mItemDics.Values)
            {
                item.Refresh();
            }
        }

        public override void Localize()
        {
            mAbilityInfoUI.Localize();
        }

        public override void Refresh()
        {
            RefreshItems();

            mAbilityInfoUI.Refresh();
        }

        public override void OnEnter()
        {
            // ³»°¡ Áö±Ý ÂïÀº ÃÖ°í ½ºÅÇ
            (string id, int step) result = Lance.Account.Ability.GetBestStepAbility();
            if (result.id.IsValid())
            {
                StartCoroutine(ScrollToBestStep(result.step));

                ChangeId(result.id);
            }
            else
            {
                var firstItemUI = mItemDics.Values.FirstOrDefault();

                ChangeId(firstItemUI?.GetFirstAbilityId() ?? string.Empty);
            }
        }

        IEnumerator ScrollToBestStep(int bestStep)
        {
            yield return null;

            AbilityItemUI itemUI = mItemDics.TryGet(bestStep);
            if (itemUI != null)
            {
                RectTransform target = itemUI.GetComponent<RectTransform>();

                var scrollRect = gameObject.FindComponent<ScrollRect>("ScrollView");

                scrollRect.CenterOnItem(target, Vector3.up * -160f);
            }
        }
    }
}