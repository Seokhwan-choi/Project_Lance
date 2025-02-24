using UnityEngine;
using UnityEngine.UI;
using Mosframe;
using System.Collections.Generic;

namespace Lance
{
    class AccessoryProbItemUI : MonoBehaviour, IDynamicScrollViewItem
    {
        Popup_AccessoryProbUI mParent;
        List<ItemSlotUI> mProbItemSlotUI;
        public void Init()
        {
            mProbItemSlotUI = new List<ItemSlotUI>();

            for (int i = 0; i < 3; ++i)
            {
                int index = i + 1;

                var slotObj = gameObject.FindGameObject($"ProbItemSlotUI_{index}");
                var slotUI = slotObj.GetOrAddComponent<ItemSlotUI>();
                slotUI.Init();

                mProbItemSlotUI.Add(slotUI);
            }

            mParent = GetComponentInParent<Popup_AccessoryProbUI>();
        }

        public void OnUpdateItem(int index)
        {
            if (mParent == null)
                mParent = GetComponentInParent<Popup_AccessoryProbUI>();

            if (mParent != null)
            {
                var probInfo = mParent.GetProbInfo(index);
                if (probInfo != null)
                {
                    var itemType = probInfo.GetItemType();
                    var grade = probInfo.GetGrade();
                    var gradeProb = probInfo.GetGradeProb();

                    for (int i = 0; i < (int)SubGrade.Four_Star; ++i)
                    {
                        if (mProbItemSlotUI.Count <= i)
                            continue;

                        SubGrade subGrade = (SubGrade)i;

                        float subGradeProb = probInfo.GetSubGradeProb(subGrade);

                        float totalProb = gradeProb * subGradeProb;

                        var accessoryData = DataUtil.GetAccessoryData(itemType, grade, subGrade);

                        var itemInfo = new ItemInfo(itemType).SetId(accessoryData.id).SetGrade(grade).SetSubGrade(subGrade)
                            .SetShowStr($"{totalProb * 100f:F2}%");

                        mProbItemSlotUI[i].Refresh(itemInfo);
                    }
                }
            }
        }
    }
}