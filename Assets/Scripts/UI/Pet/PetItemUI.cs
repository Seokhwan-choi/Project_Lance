using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class PetItemUI : MonoBehaviour
    {
        string mId;
        PetUIMotion mPetUIMotion;
        TextMeshProUGUI mTextLevel;
        SubGradeManager mSubGradeManager;
        GameObject mIsEquippedObj;
        GameObject mRedDotObj;

        public string Id => mId;
        public void Init(string id, bool ignoreButton = false)
        {
            mId = id;

            var petData = Lance.GameData.PetData.TryGet(id);

            var buttonInfo = gameObject.GetComponent<Button>();
            if (ignoreButton == false)
                buttonInfo.SetButtonAction(OnButtonAction);

            var imageElementalType = gameObject.FindComponent<Image>("Image_ElementalType");
            imageElementalType.sprite = Lance.Atlas.GetUISprite($"Icon_Ele_{petData.type}");

            var imagePet = gameObject.FindComponent<Image>("Image_Pet");
            mPetUIMotion = imagePet.GetOrAddComponent<PetUIMotion>();
            mPetUIMotion.Init();

            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");

            mSubGradeManager = new SubGradeManager();
            mSubGradeManager.Init(gameObject.FindGameObject("SubGrade"));

            mIsEquippedObj = gameObject.FindGameObject("IsEquipped");
            mRedDotObj = gameObject.FindGameObject("RedDot");

            Refresh();
        }

        void OnButtonAction()
        {
            var popup = Lance.PopupManager.CreatePopup<Popup_PetInfoUI>();
            popup.Init(mId);
            popup.SetOnCloseAction(() =>
            {
                Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.EquipPet);
                Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.LevelUpPet);
                Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.EvolutionPet);
            });
        }

        public void Localize()
        {

        }

        public void Refresh()
        {
            // 착용 여부
            mIsEquippedObj.SetActive(Lance.Account.Pet.IsEquipped(mId));

            // 레벨
            int level = Lance.Account.Pet.GetLevel(mId);
            mTextLevel.text = $"Lv.{level}";

            // 등급
            int step = Lance.Account.Pet.GetStep(mId);
            if (step > 0)
            {
                mSubGradeManager.SetActive(true);
                mSubGradeManager.SetSubGrade((UISubGrade)step - 1);
            }
            else
            {
                mSubGradeManager.SetActive(false);
            }

            var data = Lance.GameData.PetData.TryGet(mId);

            // 펫 이미지
            mPetUIMotion.RefreshSprites(data.type, step);

            RefreshRedDot();
        }

        public void RefreshRedDot()
        {
            SetRedDotActive(Lance.Account.CanEvolutionPet(mId));
        }

        public void SetIsEquippedActive(bool isActive)
        {
            mIsEquippedObj.SetActive(isActive);
        }

        public void SetRedDotActive(bool isActive)
        {
            mRedDotObj.SetActive(isActive);
        }
    }
}