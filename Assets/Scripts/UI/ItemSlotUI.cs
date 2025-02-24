using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class ItemSlotUI : MonoBehaviour
    {
        ItemInfo mItemInfo;
        Image mImageCurrency;
        Image mImageEquipment;
        Image mImageSkill;
        Image mImageArtifact;
        Image mImageRandomEquipment;
        Image mImageCostumeBody;
        Image mImageCostumeWeapon;
        Image mImageAchievement;
        Image mImageType;

        GameObject mItemSparkles;

        Image mImageFrameBack;
        Image mImageFrameFront;
        Image mImageGrade;
        TextMeshProUGUI mTextAmount;
        TextMeshProUGUI mTextCostume;
        TextMeshProUGUI mTextAchievement;
        SubGradeManager mSubGradeManager;
        public ItemType Type => mItemInfo.Type;
        public Grade Grade => mItemInfo.GetGrade();
        public string Id => mItemInfo.Id;
        public void Init(ItemInfo itemInfo)
        {
            InternalInit();

            Refresh(itemInfo);
        }

        public void SetActiveItemSparkle(bool isActive)
        {
            mItemSparkles.gameObject.SetActive(isActive);
        }

        public void Init()
        {
            InternalInit();
        }

        public void Refresh(ItemInfo itemInfo)
        {
            mItemInfo = itemInfo;

            InitSubGrade();
            SetItemSprite(itemInfo.GetSprite());
            InitSkillType();
            InitItemGrade();

            mTextCostume.gameObject.SetActive(itemInfo.Type == ItemType.Costume);
            mTextAchievement.gameObject.SetActive(itemInfo.Type == ItemType.Achievement);
            mTextAmount.gameObject.SetActive(itemInfo.Amount > 0 || itemInfo.ShowStr.IsValid());
            mTextAmount.text = itemInfo.GetAmountString();
        }

        void InitSubGrade()
        {
            if (mItemInfo.Type.IsEquipment() || mItemInfo.Type.IsAccessory() || mItemInfo.Type.IsRandomEquipment() ||  mItemInfo.Type.IsRandomAccessory() || mItemInfo.Type.IsPetEquipment())
            {
                if (mItemInfo.Type.IsEquipment() && mItemInfo.Id.IsValid())
                {
                    var data = DataUtil.GetEquipmentData(mItemInfo.Id);

                    mSubGradeManager.SetSubGrade(data.subGrade);
                }
                else if (mItemInfo.Type.IsAccessory() && mItemInfo.Id.IsValid())
                {
                    var data = DataUtil.GetAccessoryData(mItemInfo.Id);

                    mSubGradeManager.SetSubGrade(data.subGrade);
                }
                else if (mItemInfo.Type.IsPetEquipment() && mItemInfo.Id.IsValid())
                {
                    var data = Lance.GameData.PetEquipmentData.TryGet(mItemInfo.Id);

                    mSubGradeManager.SetSubGrade(data.subGrade);
                }
                else
                {
                    mSubGradeManager.SetSubGrade(mItemInfo.SubGrade);
                }

                mSubGradeManager.SetActive(true);
            }
            else
            {
                mSubGradeManager.SetActive(false);
            }
        }

        public void SetItemSprite(Sprite sprite)
        {
            InitItemSprite(sprite);
        }

        void InitItemSprite(Sprite sprite)
        {
            bool isEquipmnt = mItemInfo.Type.IsEquipment() || mItemInfo.Type.IsPetEquipment();
            bool isAccessory = mItemInfo.Type.IsAccessory();
            bool isSkill = mItemInfo.Type.IsSkill();
            bool isRandom = mItemInfo.Type.IsRandom();
            bool isArtifact = mItemInfo.Type.IsArtifact() || mItemInfo.Type == ItemType.AncientArtifact;
            bool isCurrency = mItemInfo.Type.IsCurrency();
            bool isCostume = mItemInfo.Type == ItemType.Costume;
            bool isAchievement = mItemInfo.Type == ItemType.Achievement;

            mImageEquipment.gameObject.SetActive(isEquipmnt || isAccessory);
            mImageSkill.gameObject.SetActive(isSkill);
            mImageCurrency.gameObject.SetActive(isCurrency);
            mImageArtifact.gameObject.SetActive(isArtifact);
            mImageRandomEquipment.gameObject.SetActive(isRandom);
            mImageCostumeBody.gameObject.SetActive(isCostume);
            mImageCostumeWeapon.gameObject.SetActive(isCostume);
            mImageAchievement.gameObject.SetActive(isAchievement);

            if (isEquipmnt || isAccessory)
            {
                mImageEquipment.sprite = sprite;
            }
            else if (isSkill)
            {
                mImageSkill.sprite = sprite;
            }
            else if (isArtifact)
            {
                mImageArtifact.sprite = sprite;
            }
            else if (isRandom)
            {
                mImageRandomEquipment.sprite = sprite;
            }   
            else if (isCostume)
            {
                var data = DataUtil.GetCostumeData(mItemInfo.Id);
                if (data.type == CostumeType.Body || data.type == CostumeType.Etc)
                {
                    mImageCostumeBody.gameObject.SetActive(true);
                    mImageCostumeWeapon.gameObject.SetActive(false);
                    mImageCostumeBody.sprite = sprite;
                }
                else
                {
                    mImageCostumeBody.gameObject.SetActive(false);
                    mImageCostumeWeapon.gameObject.SetActive(true);
                    mImageCostumeWeapon.sprite = sprite;
                }
            }
            else if (isAchievement)
            {
                mImageAchievement.sprite = sprite;
            }
            else
            {
                mImageCurrency.sprite = sprite;
            }
        }

        void InitSkillType()
        {
            bool isSkill = mItemInfo.Type.IsSkill();

            mImageType.gameObject.SetActive(isSkill);

            if (mItemInfo.Type.IsSkill())
            {
                var data = DataUtil.GetSkillData(mItemInfo.Id);
                if (data != null)
                {
                    mImageType.sprite = Lance.Atlas.GetItemSlotUISprite($"Skill_{data.type}");
                }
            }
        }

        void InitItemGrade()
        {
            // 이거 조건문 왜 이따구지 ㅡ ㅡ ?
            mImageGrade.sprite = mItemInfo.GetGradeSprite();
            mImageGrade.gameObject.SetActive(
                mItemInfo.Type.IsArtifact() == false && 
                mItemInfo.Type != ItemType.AncientArtifact && 
                mItemInfo.Type.IsCurrency() == false && 
                mItemInfo.Type != ItemType.Costume &&
                mItemInfo.Type != ItemType.Achievement);

            mImageFrameBack.sprite = GetFrameBack();

            if (mItemInfo.Type.IsEquipment() || 
                mItemInfo.Type.IsAccessory() ||
                mItemInfo.Type.IsSkill() || 
                mItemInfo.Type.IsRandom() || mItemInfo.Type.IsArtifact() || 
                mItemInfo.Type == ItemType.AncientArtifact || mItemInfo.Type == ItemType.Costume || 
                mItemInfo.Type.IsPetEquipment())
            {
                mImageFrameFront.gameObject.SetActive(true);
                mImageFrameFront.sprite = mItemInfo.GetGradeBackground();
            }
            else
            {
                mImageFrameFront.gameObject.SetActive(false);
            }

            if (mItemInfo.GetGrade() >= Grade.A)
            {
                SetActiveItemSparkle(true);

                for(int i = (int)Grade.A; i < (int)Grade.Count; ++i)
                {
                    Grade grade = (Grade)i;

                    var ps = mItemSparkles.FindComponent<ParticleSystem>($"ItemSparkle_{grade}");

                    ps.gameObject.SetActive(mItemInfo.GetGrade() == grade);
                }
            }
            else
            {
                SetActiveItemSparkle(false);
            }

            Sprite GetFrameBack()
            {
                if (mItemInfo.Type.IsEquipment() || mItemInfo.Type.IsAccessory() 
                    || mItemInfo.Type.IsArtifact() || mItemInfo.Type.IsRandom() 
                    || mItemInfo.Type.IsPetEquipment())
                {
                    return Lance.Atlas.GetItemSlotUISprite("Frames_Item_Back");
                }
                else if (mItemInfo.Type.IsSkill())
                {
                    return Lance.Atlas.GetItemSlotUISprite("Rectangle_Black");
                }
                else
                {
                    return Lance.Atlas.GetItemSlotUISprite("Frame_Transparents");
                }
            }
        }

        public void Init(MultiReward reward)
        {
            InternalInit();

            Init(ItemInfoUtil.CreateItemInfo(reward.ItemType, reward.Id, reward.Count));
        }

        public void OnRelease()
        {
            mImageFrameBack = null;
            mImageFrameFront = null;
            mImageGrade = null;
            mImageCurrency = null;
            mImageEquipment = null;
            mImageSkill = null;
            mImageArtifact = null;
            mImageRandomEquipment = null;
            mImageType = null;
            mTextAmount = null;
            mItemSparkles = null;
            mSubGradeManager = null;
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        void InternalInit()
        {
            mImageFrameBack = gameObject.FindComponent<Image>("Image_Frame_Deco_Back");
            mImageFrameFront = gameObject.FindComponent<Image>("Image_Frame_Deco_Front");
            mImageGrade = gameObject.FindComponent<Image>("Image_Grade");
            mImageCurrency = gameObject.FindComponent<Image>("Image_Currency");
            mImageEquipment = gameObject.FindComponent<Image>("Image_Equipment");
            mImageSkill = gameObject.FindComponent<Image>("Image_Skill");
            mImageArtifact = gameObject.FindComponent<Image>("Image_Artifact");
            mImageRandomEquipment = gameObject.FindComponent<Image>("Image_Random_Equipment");
            mImageCostumeBody = gameObject.FindComponent<Image>("Image_Costume_Body");
            mImageCostumeWeapon = gameObject.FindComponent<Image>("Image_Costume_Weapon");
            mImageAchievement = gameObject.FindComponent<Image>("Image_Achievement");
            mImageType = gameObject.FindComponent<Image>("Image_Type");
            mTextAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_Amount");
            mTextCostume = gameObject.FindComponent<TextMeshProUGUI>("Text_Costume");
            mTextAchievement = gameObject.FindComponent<TextMeshProUGUI>("Text_Achievement");
            mItemSparkles = gameObject.FindGameObject("ItemSparkles");

            var subGradeObj = gameObject.FindGameObject("SubGrade");
            mSubGradeManager = new SubGradeManager();
            mSubGradeManager.Init(subGradeObj);
        }
    }
}


