using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;


namespace Lance
{
    class Popup_LottoUI : PopupBase
    {
        LottoMainManager mMainManager;
        public void Init()
        {
            SetUpCloseAction();
            
            var scratchObj = gameObject.FindGameObject("Scratch");

            mMainManager = scratchObj.GetOrAddComponent<LottoMainManager>();
            mMainManager.Init(gameObject);

            // 로또 확률 표기
            var probTableObj = gameObject.FindGameObject("ProbTable");
            var probs = probTableObj.FindGameObject("Probs");
            probs.AllChildObjectOff();

            foreach(var data in Lance.GameData.LottoRewardData.Values.OrderBy(x => x.priority))
            {
                var probItemObj = Util.InstantiateUI("LottoProbItemUI", probs.transform);
                var probItemUI = probItemObj.GetOrAddComponent<LottoProbItemUI>();
                probItemUI.Init(data);
            }
        }
    }

    class LottoProbItemUI : MonoBehaviour
    {
        public void Init(LottoRewardData data)
        {
            var reward = Lance.GameData.RewardData.TryGet(data.reward);
            if (reward != null)
            {
                var textGemAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_GemAmount");
                textGemAmount.text = $"{reward.gem:N0}";

                var textProb = gameObject.FindComponent<TextMeshProUGUI>("Text_Prob");
                textProb.text = $"{data.prob * 100f}%";
            }
        }
    }
}