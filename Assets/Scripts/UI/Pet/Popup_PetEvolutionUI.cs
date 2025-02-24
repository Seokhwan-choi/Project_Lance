using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace Lance
{
    class Popup_PetEvolutionUI : PopupBase
    {
        bool mInMotion;
        public void Init(ElementalType type, int step)
        {
            mInMotion = true;

            var buttonModal = gameObject.FindComponent<Button>("Modal");
            buttonModal.SetButtonAction(() => Close());

            StartCoroutine(PlayEvolutionMotion(type, step));
        }

        IEnumerator PlayEvolutionMotion(ElementalType type, int step)
        {
            var imagePet = gameObject.FindComponent<Image>("Image_Pet");

            int spriteStep = step;

            if (step == 6)
                spriteStep = 5;
            else if (step > 6)
                spriteStep = 6;
            else if (step < 6)
                spriteStep = spriteStep - 1;

            imagePet.sprite = Lance.Atlas.GetPetSprite($"Pet{type}_{spriteStep}_0");

            var imageWhite = gameObject.FindComponent<Image>("Image_White");
            imageWhite.DOFade(1f, 0.5f);

            var fxCharge = gameObject.FindComponent<ParticleSystem>($"MagicCharge_{type}");
            fxCharge.gameObject.SetActive(true);

            SoundPlayer.PlayEvolutionCharge();

            yield return new WaitForSeconds(1f);

            imagePet.transform.DOScale(0f, 1f).SetEase(Ease.InBounce);

            yield return new WaitForSeconds(2f);

            int spriteResultStep = step;

            if (step > 5 && step < 10)
                spriteResultStep = 6;
            else if (step >= 10)
                spriteResultStep = 7;
            else
                spriteResultStep = step;

            imagePet.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
            imagePet.sprite = Lance.Atlas.GetPetSprite($"Pet{type}_{spriteResultStep}_0");
            imageWhite.DOFade(0f, 0.5f);

            SoundPlayer.PlayEvolutionNova();

            var fxNova = gameObject.FindComponent<ParticleSystem>($"Nova_{type}");
            fxNova.gameObject.SetActive(true);

            var successObj = gameObject.FindGameObject("Success");
            successObj.SetActive(true);
            successObj.transform.localScale = Vector3.zero;
            successObj.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

            // 이름
            var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            textName.text = StringTableUtil.GetName($"Pet_{type}_{step}");

            // 등급
            var subGradeManager = new SubGradeManager();
            subGradeManager.Init(gameObject.FindGameObject("Step"));
            subGradeManager.SetSubGrade((SubGrade)step - 1);

            yield return new WaitForSeconds(0.5f);

            mInMotion = false;
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            if (mInMotion)
                return;

            base.Close(immediate, hideMotion);
        }

        public override void OnBackButton(bool immediate = false, bool hideMotion = true)
        {
            if (mInMotion)
                return;

            base.OnBackButton(immediate, hideMotion);
        }
    }
}