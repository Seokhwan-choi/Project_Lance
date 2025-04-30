using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Artifact_ExcaliburTabUI : ArtifactTabUI
    {
        ParticleSystem mUpgradeFX;
        Button mButtonNextStep;
        GameObject mMaxStepObj;
        GameObject mNextStepObj;
        GameObject mNextStepRedDotObj;
        TextMeshProUGUI mTextExcaliburStep;
        TextMeshProUGUI mTextMyAncientEssenceAmount;
        TextMeshProUGUI mTextRequireUltimateLimitBreak;
        List<ExcaliburForceItemUI> mExcaliburForceItemUIList;
        public override void Init(ArtifactTabUIManager parent, ArtifactTab tab)
        {
            base.Init(parent, tab);

            mUpgradeFX = gameObject.FindComponent<ParticleSystem>("UpgradeExcalibur");
            mTextExcaliburStep = gameObject.FindComponent<TextMeshProUGUI>("Text_ExcaliburStep");
            mTextMyAncientEssenceAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_MyAncientEssence");

            mMaxStepObj = gameObject.FindGameObject("MaxStep");
            mNextStepObj = gameObject.FindGameObject("NextStep");
            mButtonNextStep = gameObject.FindComponent<Button>("Button_NextStep");
            mNextStepRedDotObj = mButtonNextStep.gameObject.FindGameObject("RedDot");
            mButtonNextStep.SetButtonAction(() => 
            {
                if (Lance.GameManager.UpgradeExcalibur())
                {
                    Lance.GameManager.StartCoroutine(PlayExcaliburUpgradeMotion());
                }
            });
            mTextRequireUltimateLimitBreak = gameObject.FindComponent<TextMeshProUGUI>("Text_RequireUltimateLimitBreak");

            mExcaliburForceItemUIList = new List<ExcaliburForceItemUI>();

            var excaliburForceItemListObj = gameObject.FindGameObject("ForceList");

            excaliburForceItemListObj.AllChildObjectOff();

            foreach (var data in Lance.GameData.ExcaliburData.Values)
            {
                GameObject itemObj = Util.InstantiateUI("ExcaliburForceItemUI", excaliburForceItemListObj.transform);

                var itemUI = itemObj.GetOrAddComponent<ExcaliburForceItemUI>();
                itemUI.Init(this, data.type);

                mExcaliburForceItemUIList.Add(itemUI);
            }
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void OnLeave()
        {
            base.OnLeave();

            foreach (var item in mExcaliburForceItemUIList)
            {
                item.OnTabLeave();
            }
        }

        public override void Localize()
        {
            int excaliburStep = Lance.Account.Excalibur.GetStep();

            StringParam stepParam = new StringParam("step", excaliburStep);

            mTextExcaliburStep.text = StringTableUtil.Get("UIString_ExcaliburStep", stepParam);

            var stepData = Lance.GameData.ExcaliburStepData.TryGet(excaliburStep);

            StringParam stepParam2 = new StringParam("step", stepData.requireUltimateLimitBreakStep);

            mTextRequireUltimateLimitBreak.text = StringTableUtil.Get("UIString_RequireUltmateLimitBreak", stepParam2);

            foreach (var item in mExcaliburForceItemUIList)
            {
                item.Localize();
            }
        }

        public override void Refresh()
        {
            base.Refresh();

            bool isMaxStepExcalibur = Lance.Account.Excalibur.IsMaxStepExcalibur();

            int excaliburStep = Lance.Account.Excalibur.GetStep();

            StringParam stepParam = new StringParam("step", excaliburStep);

            mTextExcaliburStep.text = StringTableUtil.Get("UIString_ExcaliburStep", stepParam);

            mTextMyAncientEssenceAmount.text = $"{Lance.Account.Currency.GetAncientEssence()}";

            if (isMaxStepExcalibur)
            {
                mMaxStepObj.SetActive(true);
                mNextStepObj.SetActive(false);
            }
            else
            {
                mMaxStepObj.SetActive(false);
                mNextStepObj.SetActive(true);

                bool canUpgradeExcalibur = Lance.Account.CanUpgradeExcalibur();

                mButtonNextStep.SetActiveFrame(canUpgradeExcalibur);
                mNextStepRedDotObj.SetActive(canUpgradeExcalibur);

                var stepData = Lance.GameData.ExcaliburStepData.TryGet(excaliburStep);

                StringParam stepParam2 = new StringParam("step", stepData.requireUltimateLimitBreakStep);

                mTextRequireUltimateLimitBreak.text = StringTableUtil.Get("UIString_RequireUltmateLimitBreak", stepParam2);
            }

            foreach (var item in mExcaliburForceItemUIList)
            {
                item.Refresh();
            }
        }

        IEnumerator PlayExcaliburUpgradeMotion()
        {
            //Lance.TouchBlock.SetActive(true);

            mUpgradeFX?.Stop();
            mUpgradeFX?.Play();

            SoundPlayer.PlaySpawnItem(Grade.SS);

            yield return new WaitForSecondsRealtime(1f);

            Refresh();

            foreach (var itemUI in mExcaliburForceItemUIList)
            {
                itemUI.PlayUpgradeMotion();
            }

            //Lance.TouchBlock.SetActive(false);
        }
    }
}