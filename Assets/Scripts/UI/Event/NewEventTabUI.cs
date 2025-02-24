using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lance
{
    class NewEventTabUI : MonoBehaviour
    {
        Canvas mCanvas;
        GraphicRaycaster mGraphicRaycaster;
        protected NewEventTabUIManager mParent;
        protected NewEventTab mTab;
        protected string mEventId;
        public virtual void Init(NewEventTabUIManager parent, NewEventTab tab)
        {
            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();

            mParent = parent;
            mTab = tab;
            mEventId = DataUtil.GetEventId(tab.ChangeToEventType());
        }
        public virtual void OnEnter()
        {
            Refresh();
        }
        public virtual void OnLeave() { }
        public virtual void Refresh() { }
        public virtual void RefreshRedDots() { }
        public string GetEventId() { return mEventId; }
        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }

        public virtual QuestData GetQuestData(int index)
        {
            return null;
        }

        public virtual void PlayQuestItemReceiveMotion(string questId)
        {
            var questItemUIs = gameObject.GetComponentsInChildren<NewEventQuestItemUI>();

            foreach(var questItemUI in questItemUIs)
            {
                if (questItemUI.Id == questId)
                {
                    questItemUI.PlayReceiveMotion();
                }
            }
        }
    }
}
