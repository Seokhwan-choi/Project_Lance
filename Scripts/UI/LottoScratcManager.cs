using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace Lance
{
    class LottoMainManager : MonoBehaviour
    {
        const float TimeInterval = 1f;
        const float SoundTimeInterval = 1f;
        const float RevealThreshold = 0.65f; // 긁힌 영역 비율 임계값 (75% 기본 설정)

        int mTotalPixels;
        int mRevealedPixels;
        bool mStartScratch;
        float mSoundTimer;
        AudioSource mScratchSound;

        TextMeshProUGUI mTextRemainCount;
        TextMeshProUGUI mTextCoolTime;
        Image mImageReward;
        TextMeshProUGUI mTextRewardAmount;
        GameObject mLottoRewardItemObj;

        Image mScratchImage;    // 긁을 이미지
        Texture2D mScratchSurface;
        Texture2D mOrgSurface;
        public Texture2D MaskTexture; // 마스크 텍스처
        public void Init(GameObject parent)
        {
            mScratchImage = gameObject.FindComponent<Image>(Lance.LocalSave.LangCode == LangCode.KR ? "Image_Scratch" : "Image_Scratch_Eng");
            mScratchImage.gameObject.SetActive(true);

            mLottoRewardItemObj = gameObject.FindGameObject("LottoRewardItemUI");
            mImageReward = mLottoRewardItemObj.FindComponent<Image>("Image_Reward");
            mTextRewardAmount = mLottoRewardItemObj.FindComponent<TextMeshProUGUI>("Text_RewardAmount");

            mTextRemainCount = parent.FindComponent<TextMeshProUGUI>("Text_RemainCount");
            mTextCoolTime = parent.FindComponent<TextMeshProUGUI>("Text_CoolTime");

            mOrgSurface = Instantiate(mScratchImage.sprite.texture); 
            mScratchSurface = Instantiate(mScratchImage.sprite.texture);
            mScratchImage.sprite = Sprite.Create(mScratchSurface, new Rect(0, 0, mScratchSurface.width, mScratchSurface.height), Vector2.zero);

            mStartScratch = false;
            mLottoRewardItemObj.SetActive(false);

            mTotalPixels = mScratchSurface.width * mScratchSurface.height;
            mRevealedPixels = 0;

            Refresh();
        }

        float mTimeInterval;
        void OnUpdateCoolTime(float dt)
        {
            mTimeInterval -= dt;
            if (mTimeInterval <= 0f)
            {
                mTimeInterval = TimeInterval;

                if (Lance.Account.Lotto.InCoolTime())
                {
                    StringParam param = new StringParam("coolTime", TimeUtil.GetTimeStr(Lance.Account.Lotto.GetRemainCoolTime(), ignoreHour:false));

                    mTextCoolTime.text = StringTableUtil.Get("UIString_LottoInCoolTime", param);
                }
            }
        }

        void RescratchSetting()
        {
            mScratchSound?.Stop();

            //Graphics.CopyTexture(mOrgSurface, mScratchSurface);

            //mScratchSurface.Apply();

            mRevealedPixels = 0;

            int remainCount = Lance.Account.Lotto.GetRemainDailyCount();

            StringParam param = new StringParam("limitCount", Lance.GameData.LottoCommonData.dailyMaxCount);
            param.AddParam("remainCount", remainCount);

            mTextRemainCount.text = StringTableUtil.Get("UIString_DailyLottoLimitCount", param);
            mTextRemainCount.SetColor(remainCount > 0 ? Const.EnoughTextColor : Const.NotEnoughLottoCount);
            mTextCoolTime.gameObject.SetActive(Lance.Account.Lotto.InCoolTime());
        }

        void Refresh()
        {
            var rewardIndex = Lance.Account.Lotto.GetNowRewardIndex();
            if (rewardIndex != -1)
            {
                var lottoRewardData = Lance.GameData.LottoRewardData.TryGet(rewardIndex);
                if (lottoRewardData != null && lottoRewardData.reward.IsValid())
                {
                    var rewardData = Lance.GameData.RewardData.TryGet(lottoRewardData.reward);
                    if (rewardData != null)
                    {
                        mImageReward.sprite = Lance.Atlas.GetUISprite(lottoRewardData.sprite);
                        mTextRewardAmount.text = $"{rewardData.gem:N0}";
                    }
                }
            }

            int remainCount = Lance.Account.Lotto.GetRemainDailyCount();

            StringParam param = new StringParam("limitCount", Lance.GameData.LottoCommonData.dailyMaxCount);
            param.AddParam("remainCount", remainCount);

            mTextRemainCount.text = StringTableUtil.Get("UIString_DailyLottoLimitCount", param);
            mTextRemainCount.SetColor(remainCount > 0 ? Const.EnoughTextColor : Const.NotEnoughLottoCount);
            mTextCoolTime.gameObject.SetActive(Lance.Account.Lotto.InCoolTime());
        }

        float mTouchCoolTime;
        void Update()
        {
            float unDt = Time.unscaledDeltaTime;

            OnUpdateCoolTime(unDt);

            if (mTouchCoolTime > 0f)
                mTouchCoolTime -= unDt;

            if (mSoundTimer > 0f)
                mSoundTimer -= unDt;

#if UNITY_EDITOR
            // 마우스 클릭(혹은 터치) 여부 확인
            if (Input.GetMouseButton(0))
            {
                Vector2 mousePos = Input.mousePosition;

                OnDragStart(mousePos);
            }
#else
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    var touch = Input.GetTouch(0);

                    Vector2 touchPos = touch.position;

                    OnDragStart(touchPos);
                }
            }
#endif
            void OnDragStart(Vector2 touchPos)
            {
                if (mTouchCoolTime > 0)
                    return;

                // 마우스 위치가 이미지 영역 내에 있는지 확인
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(mScratchImage.rectTransform, touchPos, null, out Vector2 localPoint))
                {
                    // 로컬 좌표가 이미지 영역 안에 있는지 확인
                    if (localPoint.x >= -mScratchImage.rectTransform.rect.width / 2 &&
                        localPoint.x <= mScratchImage.rectTransform.rect.width / 2 &&
                        localPoint.y >= -mScratchImage.rectTransform.rect.height / 2 &&
                        localPoint.y <= mScratchImage.rectTransform.rect.height / 2)
                    {
                        if (Lance.Account.Lotto.InCoolTime())
                        {
                            mTouchCoolTime = TimeInterval;

                            UIUtil.ShowSystemErrorMessage("InCoomTimeLotto");

                            return;
                        }

                        if (Lance.Account.Lotto.GetRemainDailyCount() <= 0)
                        {
                            mTouchCoolTime = TimeInterval;

                            UIUtil.ShowSystemErrorMessage("NotEnoughLottoDailyCount");

                            return;
                        }

                        Vector2 textureCoord = new Vector2((localPoint.x / mScratchImage.rectTransform.rect.width) + 0.5f, (localPoint.y / mScratchImage.rectTransform.rect.height) + 0.5f);
                        Vector2 pixelCoord = new Vector2(textureCoord.x * mScratchSurface.width, textureCoord.y * mScratchSurface.height);

                        if (mStartScratch == false)
                        {
                            mStartScratch = true;

                            mLottoRewardItemObj.SetActive(true);
                        }

                        if (mSoundTimer <= 0f)
                        {
                            mSoundTimer = SoundTimeInterval;

                            mScratchSound = SoundPlayer.PlayScratchLotto();
                        }
                            
                        // 긁는 효과 적용
                        EraseAt(pixelCoord);
                    }
                }

                // 긁힌 비율이 임계값을 초과했는지 확인
                if (mRevealedPixels >= mTotalPixels * RevealThreshold)
                {
                    // 보상 주고
                    Lance.GameManager.DrawLotto();

                    // 초기화
                    RescratchSetting();
                }
            }
        }

        void EraseAt(Vector2 pixel)
        {
            int brushSize = 200; // 브러시 크기
            int x = (int)pixel.x - brushSize / 2;
            int y = (int)pixel.y - brushSize / 2;

            for (int i = 0; i < brushSize; i++)
            {
                for (int j = 0; j < brushSize; j++)
                {
                    // 이미지 영역 밖을 벗어나는지 확인
                    if (x + i >= 0 && x + i < mScratchSurface.width && y + j >= 0 && y + j < mScratchSurface.height)
                    {
                        Color maskColor = MaskTexture.GetPixelBilinear((float)i / brushSize, (float)j / brushSize);
                        Color pixelColor = mScratchSurface.GetPixel(x + i, y + j);
                        Color newColor = Color.Lerp(pixelColor, Color.clear, maskColor.a);

                        // 만약 투명하게 만든다면 (즉, 색상이 변했다면)
                        if (pixelColor.a > 0.1f && newColor.a <= 0.1f)
                        {
                            mRevealedPixels++;
                        }

                        mScratchSurface.SetPixel(x + i, y + j, newColor);
                    }
                }
            }

            mScratchSurface.Apply();
        }
    }
}