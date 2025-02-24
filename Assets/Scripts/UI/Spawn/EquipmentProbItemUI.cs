using UnityEngine;
using UnityEngine.UI;
using Mosframe;
using System.Collections.Generic;

namespace Lance
{
    class EquipmentProbItemUI : MonoBehaviour, IDynamicScrollViewItem
    {
        Popup_EquipProbUI mParent;
        List<ItemSlotUI> mProbItemSlotUI;
        public void Init()
        {
            mProbItemSlotUI = new List<ItemSlotUI>();
            
            for(int i = 0; i < 5; ++i)
            {
                int index = i + 1;

                var slotObj = gameObject.FindGameObject($"ProbItemSlotUI_{index}");
                var slotUI = slotObj.GetOrAddComponent<ItemSlotUI>();
                slotUI.Init();

                mProbItemSlotUI.Add(slotUI);
            }

            mParent = GetComponentInParent<Popup_EquipProbUI>();
        }

        public void OnUpdateItem(int index)
        {
            if (mParent == null)
                mParent = GetComponentInParent<Popup_EquipProbUI>();

            if (mParent != null)
            {
                var probInfo = mParent.GetProbInfo(index);
                if (probInfo != null)
                {
                    var itemType = probInfo.GetItemType();
                    var grade = probInfo.GetGrade();
                    var gradeProb = probInfo.GetGradeProb();

                    for (int i = 0; i < (int)SubGrade.Count; ++i)
                    {
                        if (mProbItemSlotUI.Count <= i)
                            continue;

                        SubGrade subGrade = (SubGrade)i;

                        float subGradeProb = probInfo.GetSubGradeProb(subGrade);

                        float totalProb = gradeProb * subGradeProb;

                        var equipmentData = DataUtil.GetEquipmentData(itemType, grade, subGrade);

                        var itemInfo = new ItemInfo(itemType).SetId(equipmentData.id).SetGrade(grade).SetSubGrade(subGrade)
                            .SetShowStr($"{totalProb * 100f:F2}%");

                        mProbItemSlotUI[i].Refresh(itemInfo);
                    }
                }
            }
        }
    }
}