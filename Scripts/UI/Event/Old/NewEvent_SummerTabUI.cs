using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

namespace Lance
{
    class NewEvent_SummerTabUI : NewEventTabUI
    {
        TextMeshProUGUI mTextCurrency;
        SummerTabUIManager mTabUIManager;
        public override void Init(NewEventTabUIManager parent, NewEventTab tab)
        {
            base.Init(parent, tab);

            var eventData = Lance.GameData.EventData.TryGet(mEventId);

            var imageTitle = gameObject.FindComponent<Image>("Image_Title");
            imageTitle.sprite = Lance.Atlas.GetUISprite($"Title_{mEventId}");

            var imageCurrency = gameObject.FindComponent<Image>("Image_Currency");
            imageCurrency.sprite = Lance.Atlas.GetItemSlotUISprite($"Currency_{mEventId}");

            var textDesc1 = gameObject.FindComponent<TextMeshProUGUI>("Text_Desc");
            textDesc1.text = StringTableUtil.GetDesc(mEventId);

            StringParam dropParam = new StringParam("prob", $"{eventData.currencyDropProb * 100f:F0}%");

            var textCurrencyProb = gameObject.FindComponent<TextMeshProUGUI>("Text_DropProb");
            textCurrencyProb.text = StringTableUtil.GetDesc($"{mEventId}_CurrencyDropProb", dropParam);

            var textDuration = gameObject.FindComponent<TextMeshProUGUI>("Text_Duration");

            StringParam durationParam = new StringParam("startDate", TimeUtil.GetTimeStr(eventData.startDate));
            durationParam.AddParam("endDate", TimeUtil.GetTimeStr(eventData.endDate));

            textDuration.text = StringTableUtil.Get("UIString_DateRange", durationParam);

            mTextCurrency = gameObject.FindComponent<TextMeshProUGUI>("Text_Amount");
            mTextCurrency.text = $"{Lance.Account.Event.GetCurrency(mEventId)}";

            mTabUIManager = new SummerTabUIManager();
            mTabUIManager.Init(gameObject, mEventId);
        }

        public override void Refresh()
        {
            mTabUIManager.Refresh();

            mTextCurrency.text = $"{Lance.Account.Event.GetCurrency(mEventId)}";
        }

        public override void RefreshRedDots()
        {
            mTabUIManager.RefreshRedDots();
        }
    }
}