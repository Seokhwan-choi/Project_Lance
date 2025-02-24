using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace Lance
{
    class PassTabUI : MonoBehaviour
    {
        protected PassTab mTab;
        protected PassSubTabUIManager mSubTabUIManager;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;

        PassTabUIManager mParent;
        Slider mSliderMyRewardValue;
        TextMeshProUGUI mTextMyRewardValue;
        public PassTab Tab => mTab;
        public PassTabUIManager Parent => mParent;
        public virtual void Init(PassTabUIManager parent, PassTab tab)
        {
            mParent = parent;
            mTab = tab;
            mCanvas = GetComponentInChildren<Canvas>();
            mGraphicRaycaster = GetComponentInChildren<GraphicRaycaster>();
            mTextMyRewardValue = gameObject.FindComponent<TextMeshProUGUI>("Text_MyRewardValue");
            mSliderMyRewardValue = gameObject.FindComponent<Slider>("Slider_MyRewardValue");
            
            mSubTabUIManager = new PassSubTabUIManager();
            mSubTabUIManager.Init(this);
        }

        public void OnRelease()
        {
            mSubTabUIManager.OnRelease();
            mSubTabUIManager = null;
        }

        public void RefreshMyRewardValue()
        {
            string selectedId = mSubTabUIManager.SelectedId;
            if (selectedId.IsValid())
            {
                double myValue = Lance.Account.Pass.GetRewardValue(selectedId);

                mTextMyRewardValue.text = $"{GetMyRewardValueString()}";

                IEnumerable<PassStepData> datas = DataUtil.GetPassStepDatas(selectedId);
                double maxValue = datas.Max(x => x.requireValue);
                double minValue = datas.Min(x => x.requireValue);
                maxValue -= minValue;
                if (minValue > myValue)
                    myValue = 0;
                else
                    myValue -= minValue;

                mSliderMyRewardValue.value = (float)(myValue / maxValue);

                string GetMyRewardValueString()
                {
                    PassData passData = Lance.GameData.PassData.TryGet(selectedId);

                    if (passData.type == PassType.PlayTime)
                    {
                        return $"{myValue}m";
                    }
                    else
                    {
                        return $"{myValue}";
                    }
                }
            }
        }
        public virtual void OnEnter() 
        {
            RefreshMyRewardValue();

            mSubTabUIManager.Refresh();
        }
        public virtual void OnLeave() { }
        public virtual void Refresh() { }
        public virtual void OnUpdate() { }
        public PassStepData GetStepData(int index)
        {
            return mSubTabUIManager?.GetSelectedPassStepData(index);
        }

        public void RefreshRedDots()
        {
            mSubTabUIManager?.RefreshRedDots();
        }

        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }
}
