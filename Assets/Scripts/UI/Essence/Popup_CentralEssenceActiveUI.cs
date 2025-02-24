using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Popup_CentralEssenceActiveUI : PopupBase
    {
        public void Init()
        {
            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get("Title_ActiveCentralEssence"));

            int centralEssenceStep = Lance.Account.Essence.GetStep(EssenceType.Central);

            // ���� ��ȭ ǥ��
            var currencyUIs = new Dictionary<EssenceType, EssenceCurrencyUI>();
            for (int i = 0; i < (int)EssenceType.Count; ++i)
            {
                EssenceType type = (EssenceType)i;

                var currencyObj = gameObject.FindGameObject($"Currency_{type}");

                var currencyUI = currencyObj.GetOrAddComponent<EssenceCurrencyUI>();
                currencyUI.Init(type);
            }

            // ������ ����  �̹���
            var essencesObj = gameObject.FindGameObject("Essences");
            var essenceItemObj = essencesObj.FindGameObject($"Essence_{EssenceType.Central}");
            var essenceItemUI = essenceItemObj.GetOrAddComponent<EssenceItemUI>();
            essenceItemUI.Init(EssenceType.Central, OnSelectEssence);

            var activeRequireData = Lance.GameData.CentralEssenceActiveRequireData.TryGet(centralEssenceStep);
            var statValueData = Lance.GameData.CentralEssenceStatValueData.TryGet(centralEssenceStep);
                
            var textDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_Desc");
            StringParam descParam = new StringParam("essenceAmount", activeRequireData.requireAllEssenceAmount);
            descParam.AddParam("duration", Lance.GameData.EssenceCommonData.centralEssenceDurationTime);
            descParam.AddParam("activeValue", Lance.GameData.EssenceCommonData.centralEssenceActiveValue);
            textDesc.text = StringTableUtil.Get("Desc_ActiveCentralEssence", descParam);

            // ������ ����
            var textCurrentValue = gameObject.FindComponent<TextMeshProUGUI>("Text_CurrentValue");
            textCurrentValue.text = $"{statValueData.value * 100:F2}%";

            // Ȱ��ȭ ������ ����
            var textNextValue = gameObject.FindComponent<TextMeshProUGUI>("Text_NextValue");
            textNextValue.text = $"{statValueData.value * 100 * Lance.GameData.EssenceCommonData.centralEssenceActiveValue:F2}%";

            // Ȱ��ȭ ���� Ƚ��
            var textRemainDailyLimitCount = gameObject.FindComponent<TextMeshProUGUI>("Text_RemainDailyLimitCount");
            StringParam dailyParam = new StringParam("remainCount", Lance.Account.Essence.GetRemainActiveCount());
            dailyParam.AddParam("limitCount", Lance.GameData.EssenceCommonData.centralEssenceDailyLimitCount);
            textRemainDailyLimitCount.text = StringTableUtil.Get("UIString_DailyActiveLimitCount", dailyParam);

            // Ȱ��ȭ ��ư
            var buttonActiveCentralEssence = gameObject.FindComponent<Button>("Button_ActiveCentralEssence");
            buttonActiveCentralEssence.SetActiveFrame(Lance.Account.CanActiveCentralEssence());
            buttonActiveCentralEssence.SetButtonAction(() =>
            {
                if (Lance.GameManager.ActiveCentralEssence())
                {
                    Close();
                }
            });
        }

        public void OnSelectEssence(EssenceType selectType)
        {

        }
    }
}