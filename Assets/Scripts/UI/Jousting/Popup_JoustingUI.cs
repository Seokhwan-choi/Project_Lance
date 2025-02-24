using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Popup_JoustingUI : PopupBase
    {
        const float IntervalTime = 1f;
        float mIntervalTime;
        JoustingTabUIManager mTabUIManager;
        public void Init()
        {
            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get("Title_Jousting"));

            mIntervalTime = IntervalTime;

            mTabUIManager = new JoustingTabUIManager();
            mTabUIManager.Init(gameObject);

            Lance.GameManager.CheckQuest(QuestType.ConfirmJousting, 1);
        }

        public override void OnClose()
        {
            base.OnClose();

            mTabUIManager.OnClose();
        }

        private void Update()
        {
            mIntervalTime -= Time.unscaledDeltaTime;
            if (mIntervalTime <= 0f)
            {
                mTabUIManager.RefreshWeeklyUpdteRemainTime();

                mIntervalTime = IntervalTime;
            }
        }

        public void RefreshCurrencyUI()
        {
            mTabUIManager.RefreshCurrency();
        }
    }
}