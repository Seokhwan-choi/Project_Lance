using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;


namespace Lance
{
    class Statistics_StackedDamageTabUI : StatisticsTabUI
    {
        TextMeshProUGUI mTextStackedDamageValue;
        RectTransform mDamageListRectTm;
        double mStackedTotalDamage;

        Dictionary<string, double> mStackedDamageDics;
        Dictionary<string, int> mStackedDamageCountDics;
        List<StatisticsDamageItemUI> mStackedDamageItemUIList;
        public override void Init(StatisticsTab tab)
        {
            base.Init(tab);

            mTextStackedDamageValue = gameObject.FindComponent<TextMeshProUGUI>("Text_StackedDamageValue");
            mDamageListRectTm = gameObject.FindComponent<RectTransform>("DamageList");
            mDamageListRectTm.AllChildObjectOff();

            mStackedDamageDics = new Dictionary<string, double>();
            mStackedDamageCountDics = new Dictionary<string, int>();
            mStackedDamageItemUIList = new List<StatisticsDamageItemUI>();
        }

        public override void ResetInfo()
        {
            base.ResetInfo();

            mStackedTotalDamage = 0;

            RefreshTotalStackedDamage();

            if(mStackedDamageItemUIList != null && mStackedDamageItemUIList.Count > 0)
            {
                foreach(var itemUI in mStackedDamageItemUIList)
                {
                    Lance.ObjectPool.ReleaseUI(itemUI.gameObject);
                }

                mStackedDamageItemUIList.Clear();
            }

            mStackedDamageDics.Clear();
            mStackedDamageCountDics.Clear();

            RefreshStackedDamageItemUIList();
        }

        public override void StackDamage(string id, double damage)
        {
            base.StackDamage(id, damage);

            mStackedTotalDamage += damage;

            RefreshTotalStackedDamage();

            if (mStackedDamageDics.ContainsKey(id))
            {
                mStackedDamageDics[id] += damage;
                mStackedDamageCountDics[id] += 1;
            }
            else
            {
                mStackedDamageCountDics.Add(id, 1);
                mStackedDamageDics.Add(id, damage);
            }

            RefreshStackedDamageItemUIList();
        }

        void RefreshTotalStackedDamage()
        {
            mTextStackedDamageValue.text = mStackedTotalDamage.ToAlphaString();
        }

        void RefreshStackedDamageItemUIList()
        {
            if (mCanvas.enabled == false || mGraphicRaycaster.enabled == false)
                return;

            if (mStackedDamageDics.Count != mStackedDamageItemUIList.Count && mStackedDamageDics.Count > mStackedDamageItemUIList.Count)
            {
                int needCount = mStackedDamageDics.Count - mStackedDamageItemUIList.Count;

                for(int i = 0; i < needCount; ++i)
                {
                    var damageItemObj = Lance.ObjectPool.AcquireUI("DamageItemUI", mDamageListRectTm);

                    var damageItemUI = damageItemObj.GetOrAddComponent<StatisticsDamageItemUI>();
                    damageItemUI.Init();

                    mStackedDamageItemUIList.Add(damageItemUI);
                }
            }

            if (mStackedDamageDics.Count > 0)
            {
                var damageKeyValues = mStackedDamageDics.OrderByDescending(x => x.Value).ToArray();

                for(int j = 0; j < damageKeyValues.Length; ++j)
                {
                    if (mStackedDamageItemUIList.Count > j)
                    {
                        string id = damageKeyValues[j].Key;
                        double damage = damageKeyValues[j].Value;

                        mStackedDamageItemUIList[j].Refresh(id, mStackedTotalDamage, damage);
                    }
                }
            }
        }
    }

    class StatisticsDamageItemUI : MonoBehaviour
    {
        GameObject mNormalAtkObj;
        Image mImageDamageIcon;
        TextMeshProUGUI mTextDamage;
        Slider mSliderDamage;

        public void Init()
        {
            mNormalAtkObj = gameObject.FindGameObject("NormalAtk");
            mImageDamageIcon = gameObject.FindComponent<Image>("Image_DamageIcon");
            mSliderDamage = gameObject.FindComponent<Slider>("Slider_Damage");
            mTextDamage = gameObject.FindComponent<TextMeshProUGUI>("Text_Damage");
        }

        public void Refresh(string id, double totalDamage, double damage)
        {
            if (id.IsValid() == false)
                return;

            if (id == "NormalAtk")
            {
                mNormalAtkObj.SetActive(true);
                mImageDamageIcon.gameObject.SetActive(false);
            }
            else
            {
                mNormalAtkObj.SetActive(false);
                mImageDamageIcon.gameObject.SetActive(true);

                var skillData = DataUtil.GetSkillData(id);
                if (skillData != null)
                {
                    mImageDamageIcon.sprite = Lance.Atlas.GetSkill(id);
                }
                else
                {
                    skillData = DataUtil.GetPetSkillData(id);

                    mImageDamageIcon.sprite = Lance.Atlas.GetSkill(skillData?.uiSprite ?? id);
                }
            }

            float damageRatio = (float)(damage / totalDamage);

            mSliderDamage.value = damageRatio;
            mTextDamage.text = $"{damage.ToAlphaString()} ({damageRatio * 100f:F2}%)";
        }
    }
}