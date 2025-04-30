using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;

namespace Lance
{
    class CharacterMessageManager
    {
        GameObject mCharacterMessageObj;
        Action mOnTouchScreen;
        bool mFinishMotion;

        GameObject mBubbleUpObj;
        TextMeshProUGUI mTextBubbleUpMessage;
        Image mImageUpFinish;

        GameObject mBubbleDownObj;
        TextMeshProUGUI mTextBubbleDownMessage;
        Image mImageDownFinish;

        GameObject mCharacterObj;
        Image mImageCharacterFace;

        int mCurrentStep;
        CharacterMessageInst mCurrMessage;
        List<CharacterMessageInst> mMessageList;
        public bool InMessage => mMessageList.Count > 0 || mCurrMessage != null;
        public void Init(GuideActionManager guideActionManager)
        {
            mMessageList = new List<CharacterMessageInst>();

            var alwaysFrontCanvas = GameObject.Find("AlwaysFront_Canvas");

            mCharacterMessageObj = alwaysFrontCanvas.FindGameObject("CharacterMessage");
            var buttonTouchScreen = mCharacterMessageObj.GetComponent<Button>();
            buttonTouchScreen.SetButtonAction(() =>
            {
                mOnTouchScreen?.Invoke();
            });

            mBubbleUpObj = mCharacterMessageObj.FindGameObject("Bubble_Up");
            mTextBubbleUpMessage = mBubbleUpObj.FindComponent<TextMeshProUGUI>("Text_Up_Message");
            mImageUpFinish = mBubbleUpObj.FindComponent<Image>("Image_UpFinish");
            mImageUpFinish.gameObject.SetActive(false);
            mBubbleUpObj.SetActive(false);

            mBubbleDownObj = mCharacterMessageObj.FindGameObject("Bubble_Down");
            mTextBubbleDownMessage = mBubbleDownObj.FindComponent<TextMeshProUGUI>("Text_Down_Message");
            mImageDownFinish = mBubbleDownObj.FindComponent<Image>("Image_DownFinish");
            mImageDownFinish.gameObject.SetActive(false);
            mBubbleDownObj.SetActive(false);

            mCharacterObj = mCharacterMessageObj.FindGameObject("Character");
            mImageCharacterFace = mCharacterObj.FindComponent<Image>("Image_CharacterFace");

            var buttonSkipButton = mCharacterMessageObj.FindComponent<Button>("Skip");
            buttonSkipButton.SetButtonAction(() =>
            {
                guideActionManager?.OnSkip();

                OnSkip();
            });
        }

        public void StartMessage(string id)
        {
            Time.timeScale = 0f;

            IEnumerable<CharacterMessageData> datas = Lance.GameData.CharacterMessageData.Where(x => x.id == id);

            if (datas != null && datas.Count() > 0)
            {
                mCharacterMessageObj.SetActive(true);

                mCurrentStep = 1;

                // 순서대로 동작해야 되기때문에 혹시 모르니 정렬해주자
                foreach (CharacterMessageData data in datas.OrderBy(x => x.step))
                {
                    var inst = new CharacterMessageInst();

                    inst.Init(this, data);

                    mMessageList.Add(inst);
                }

                SoundPlayer.PlayCharacterAppear();

                mCharacterObj.transform.localPosition = Vector3.zero;
                mCharacterObj.transform.localScale = Vector3.zero;
                mCharacterObj.transform.DOScale(1f, 0.5f)
                    .SetEase(Ease.InOutCirc)
                    .SetUpdate(isIndependentUpdate: true).timeScale = 1f;
            }
        }
        bool mFaceIsLeft;
        public void OnUpdate(float dt)
        {
            if (mCurrMessage != null)
            {
                mCurrMessage.OnUpdate(dt);

                if (mCurrMessage.IsFinish)
                {
                    mCurrMessage.OnFinish();
                    mCurrMessage = null;

                    if (mMessageList.Count <= 0)
                    {
                        EndMessage();
                    }
                }
            }
            else
            {
                mCurrMessage = PopupMessage();
                if (mCurrMessage != null)
                {
                    mCurrMessage.OnStart();
                }
            }
        }

        CharacterMessageInst PopupMessage()
        {
            if (mMessageList.Count > 0)
            {
                var first = mMessageList.FirstOrDefault();

                mMessageList.RemoveAt(0);

                return first;
            }

            return null;
        }

        public void EndMessage()
        {
            if (mCurrentStep != 0)
            {
                mCurrentStep = 0;

                mCharacterObj.transform.localScale = Vector3.one;
                mCharacterObj.transform.DOScale(0f, 0.5f)
                    .SetEase(Ease.InOutCirc)
                    .SetUpdate(isIndependentUpdate: true)
                    .OnComplete(() =>
                    {
                        mCharacterMessageObj.SetActive(false);

                        Time.timeScale = Lance.Account.SpeedMode.InSpeedMode() ? Lance.GameData.CommonData.speedModeValue : 1f;
                    }).timeScale = 1f;

                SoundPlayer.PlayHidePopup();
            }
        }

        public void OnSkip()
        {
            mCurrMessage = null;
            mMessageList.Clear();

            mCurrentStep = 0;
            mCharacterObj.transform.localScale = Vector3.zero;
            mCharacterMessageObj.SetActive(false);
            Time.timeScale = Lance.Account.SpeedMode.InSpeedMode() ? Lance.GameData.CommonData.speedModeValue : 1f;
        }

        public void MoveToHighlight(Action onFinish)
        {
            var guideHighLightObj = Lance.Lobby.Guide.HighlightObj;

            if (guideHighLightObj.activeSelf)
            {
                var characterRectTm = mCharacterObj.GetComponent<RectTransform>();
                var guideRectTm = guideHighLightObj.GetComponent<RectTransform>();

                characterRectTm.anchoredPosition = Vector3.zero;

                Vector3 vDist = guideRectTm.anchoredPosition - Vector2.zero;
                Vector3 dir = vDist.normalized;
                float dist = vDist.magnitude;

                Vector3 endValue = dir * dist * 0.8f;

                SoundPlayer.PlayCharacterJump();

                characterRectTm.DOLocalJump(endValue, 60f, 2, 1f)
                    .SetUpdate(isIndependentUpdate: true)
                    .OnComplete(() => onFinish?.Invoke()).timeScale = 1f;

                if (endValue.x < 0f)
                {
                    mImageCharacterFace.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
                }
                else
                {
                    mImageCharacterFace.rectTransform.localScale = new Vector3(1f, 1f, 1f);
                }
            }
            else
            {
                onFinish?.Invoke();
            }
        }
        TweenerCore<Quaternion, Vector3, QuaternionOptions> mShakeTween;
        public void ShowMessage(string message, Action onFinish)
        {
            var charaterRectTm = mImageCharacterFace.rectTransform;
            mOnTouchScreen = OnTouchScreen;

            mFinishMotion = false;
            // 현재 mCharacterObj 좌표를 가지고 어떤 말풍선 사용할지 결정하고 보여주자
            ShowMessage(charaterRectTm.anchoredPosition.y <= 0, message, skip:false);

            void ShowMessage(bool isDown, string message, bool skip)
            {
                mBubbleDownObj.SetActive(!isDown);
                mBubbleUpObj.SetActive(isDown);

                if (!skip)
                {
                    PlayMotion(isDown);
                }
                else
                {
                    FinishMotion(isDown);
                }

                void PlayMotion(bool isDown)
                {
                    if (!isDown)
                    {
                        mImageDownFinish.gameObject.SetActive(false);

                        mTextBubbleDownMessage.text = string.Empty;
                        mTextBubbleDownMessage.DOText(message, 1f)
                            .SetUpdate(isIndependentUpdate: true)
                            .OnComplete(() =>
                            {
                                FinishMotion(isDown);
                            }).timeScale = 1f;
                    }
                    else
                    {
                        mImageUpFinish.gameObject.SetActive(false);

                        mTextBubbleUpMessage.text = string.Empty;
                        mTextBubbleUpMessage.DOText(message, 1f)
                            .SetUpdate(isIndependentUpdate: true)
                            .OnComplete(() =>
                            {
                                FinishMotion(isDown);
                            }).timeScale = 1f;
                    }

                    SoundPlayer.PlayMessageSound();

                    if (mShakeTween != null)
                    {
                        mShakeTween.Rewind();
                        mShakeTween.timeScale = 1f;
                        mShakeTween.Play();
                    }
                    else
                    {
                        mShakeTween = charaterRectTm.DOLocalRotate(new Vector3(0, 0, -5f), 0.125f)
                        .SetUpdate(isIndependentUpdate: true)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutCirc)
                        .SetAutoKill(false);
                        mShakeTween.timeScale = 1f;
                    }
                }

                void FinishMotion(bool isDown)
                {
                    if (!isDown)
                    {
                        mTextBubbleDownMessage.DOKill();
                        mTextBubbleDownMessage.text = message;

                        mImageDownFinish.gameObject.SetActive(true);
                    }
                    else
                    {
                        mTextBubbleUpMessage.DOKill();
                        mTextBubbleUpMessage.text = message;

                        mImageUpFinish.gameObject.SetActive(true);
                    }

                    mFinishMotion = true;

                    mShakeTween.Rewind();
                    mShakeTween.Pause();

                    charaterRectTm.localEulerAngles = Vector3.zero;
                }
            }

            void OnTouchScreen()
            {
                if (mFinishMotion)
                {
                    onFinish?.Invoke();

                    mBubbleDownObj.SetActive(false);
                    mBubbleUpObj.SetActive(false);
                }
                else
                {
                    ShowMessage(charaterRectTm.anchoredPosition.y <= 0, message, skip: true);
                }
            }
        }
        
        string mSavedFace;
        public void ChangeFace(string face)
        {
            if (mSavedFace != face)
            {
                mSavedFace = face;
                mImageCharacterFace.sprite = Lance.Atlas.GetUISprite(face);
                //mImageCharacterFace.transform.DOPunchScale(Vector3.one * 0.25f, 1f)
                //    .SetUpdate(isIndependentUpdate: true);
            }
        }
    }

    class CharacterMessageInst
    {
        bool mIsFinish;
        CharacterMessageManager mParent;
        CharacterMessageData mData;
        public bool IsFinish => mIsFinish;
        public void Init(CharacterMessageManager parent, CharacterMessageData data)
        {
            mParent = parent;
            mData = data;
        }

        public void OnStart()
        {
            if (mData.action == CharacterMessageActionType.MoveToHighlight)
            {
                mParent.MoveToHighlight(() => mIsFinish = true);
            }
            else
            {
                string message = StringTableUtil.Get($"GuideMessage_{mData.message}");

                mParent.ShowMessage(message, () => mIsFinish = true);
            }

            mParent.ChangeFace(mData.character);
        }

        public void OnUpdate(float dt)
        {

        }

        public void OnFinish()
        {

        }
    }

    public enum CharacterMessageActionType
    {
        MoveToHighlight,
        ShowMessage,
    }
}