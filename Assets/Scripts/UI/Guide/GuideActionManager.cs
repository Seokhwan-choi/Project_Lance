using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using DG.Tweening;

namespace Lance
{
    class GuideActionManager
    {
        GameObject mGuideObj;
        Image mImageTouchBlock;
        GameObject mHighlightObj;
        Image mImageHighlightModal;
        Image mImageHighlightBox;
        GameObject mTouchFingerObj;
        Button mButtonHighlightTouch;

        int mCurrentGuideStep;

        GuideAction mCurrentGuide;
        Dictionary<GuideActionType, RectTransform> mGuideTargets;
        List<GuideAction> mCurrentGuideStepList;
        public GameObject HighlightObj => mHighlightObj;
        public bool InGuide => mCurrentGuide != null || mCurrentGuideStepList.Count > 0;
        public void Init()
        {
            mCurrentGuideStepList = new List<GuideAction>();

            var alwaysFrontCanvas = GameObject.Find("AlwaysFront_Canvas");
            mGuideObj = alwaysFrontCanvas.FindGameObject("Guide");
            mImageTouchBlock = mGuideObj.FindComponent<Image>("Image_TouchBlock");
            
            mHighlightObj = mGuideObj.FindGameObject("Highlight");
            mHighlightObj.SetActive(false);
            mImageHighlightBox = mHighlightObj.FindComponent<Image>("Image_Box");
            mImageHighlightModal = mHighlightObj.FindComponent<Image>("Image_Modal");
            mTouchFingerObj = mHighlightObj.FindGameObject("Image_TouchFinger");
            mTouchFingerObj.SetActive(false);
            mButtonHighlightTouch = mHighlightObj.FindComponent<Button>("Button_HighLightTouch");
            mGuideTargets = new Dictionary<GuideActionType, RectTransform>();

            for(int i = 0; i < (int)GuideActionType.Count; ++i)
            {
                GuideActionType tag = (GuideActionType)i;

                var target = Lance.Lobby.FindGuideActionTag(tag);
                if (target != null)
                {
                    mGuideTargets.Add(tag, target);
                }
            }

            var buttonSkipButton = mGuideObj.FindComponent<Button>("Skip");
            buttonSkipButton.SetButtonAction(OnSkip);
        }

        public void StartGuide(int step, bool isAuto)
        {
            var guideData = Lance.GameData.GuideData.TryGet(step);

            string action = isAuto ? guideData.autoAction : guideData.action;

            if (action.IsValid())
            {
                mGuideObj.SetActive(true);

                mCurrentGuideStep = step;

                mCurrentGuideStepList.Clear();

                IEnumerable<GuideActionData> datas = Lance.GameData.GuideActionData.Where(x => x.id == action);

                // 순서대로 동작해야 되기때문에 혹시 모르니 정렬해주자
                foreach (GuideActionData data in datas.OrderBy(x => x.step))
                {
                    mCurrentGuideStepList.Add(GuideActionCreater.Create(this, data));
                }
            }
        }

        public void Localize()
        {
            var readerUIs = mGuideObj.GetComponentsInChildren<StringTableUIReader>(true);
            foreach (var readerUI in readerUIs)
            {
                readerUI.Localize();
            }
        }

        public void StartGuide(string action)
        {
            if (action.IsValid())
            {
                mGuideObj.SetActive(true);

                mCurrentGuideStep = 0;

                IEnumerable<GuideActionData> datas = Lance.GameData.GuideActionData.Where(x => x.id == action);

                // 순서대로 동작해야 되기때문에 혹시 모르니 정렬해주자
                foreach (GuideActionData data in datas.OrderBy(x => x.step))
                {
                    mCurrentGuideStepList.Add(GuideActionCreater.Create(this, data));
                }
            }
        }

        public void OnUpdate(float dt)
        {
            if (mCurrentGuide != null)
            {
                mCurrentGuide.OnUpdate(dt);

                if (mCurrentGuide.IsFinish)
                {
                    if (Lance.Lobby.InMessageMode())
                        return;

                    mCurrentGuide.OnFinish();
                    mCurrentGuide = null;

                    if (mCurrentGuideStepList.Count <= 0)
                    {
                        EndGuide();
                    }
                }
            }
            else
            {
                mCurrentGuide = PopupGuide();
                if (mCurrentGuide != null)
                {
                    //Lance.GameManager.StartTouchBlock(100f);

                    mCurrentGuide.OnStart();
                }
            }
        }

        GuideAction PopupGuide()
        {
            if (mCurrentGuideStepList.Count > 0)
            {
                var first = mCurrentGuideStepList.FirstOrDefault();

                mCurrentGuideStepList.RemoveAt(0);

                return first;
            }

            return null;
        }

        public void EndGuide()
        {
            if (mCurrentGuideStep != 0)
            {
                mCurrentGuideStep = 0;

                mGuideObj.SetActive(false);
            }
        }

        public void OnSkip()
        {
            mCurrentGuide?.OnFinish();
            mCurrentGuide = null;

            mCurrentGuideStepList.Clear();

            mCurrentGuideStep = 0;

            mGuideObj.SetActive(false);
        }

        public void ShowHighlight(GuideActionType type, Action onHighlightTouchAction)
        {
            RectTransform target = GetGuideTarget(type);

            if (target != null)
            {
                if (type == GuideActionType.Highlight_ChallengeBoss)
                {
                    if (Lance.GameManager.StageManager.IsBossStage)
                    {
                        onHighlightTouchAction.Invoke();
                    }
                }

                if (type.IsEnsureVisibleType())
                {
                    if (type.IsNeedParent())
                    {
                        var parent = GetGuideTarget(type.ChangeToParentType());
                        if (parent != null)
                        {
                            Lance.GameManager.StartCoroutine(EnsureVisibleTarget(parent));
                        }
                        else
                        {
                            Lance.GameManager.StartCoroutine(EnsureVisibleTarget(target));
                        }
                    }
                    else
                    {
                        Lance.GameManager.StartCoroutine(EnsureVisibleTarget(target));
                    }
                }
                else
                {
                    InternalShowHighlight();
                }
            }
            else
            {
                onHighlightTouchAction.Invoke();
            }

            IEnumerator EnsureVisibleTarget(RectTransform target)
            {
                // 타겟의 부모에 스크롤 뷰가 있다면
                foreach (var scrollRect in target.GetComponentsInParent<ScrollRect>())
                {
                    if (target.IsChildOf(scrollRect.content))
                    {
                        // 스크롤 뷰의 자식인지 확인하고
                        // 무조건 타겟을 스크롤뷰 영역안에 보이도록 스크롤링해주자
                        scrollRect.ScrollToTarget(target);

                        break;
                    }
                }

                yield return null;

                InternalShowHighlight();
            }

            void InternalShowHighlight()
            {
                ShowHighlight(target, () =>
                {
                    var button = target.GetComponent<Button>();
                    button?.onClick.Invoke();

                    onHighlightTouchAction.Invoke();
                });
            }
        }

        public void ShowHighlight(Vector3 pos, Action onHighlightTouchAction)
        {
            ShowHighlight(onHighlightTouchAction);

            SetHighlightObjPos(pos);
        }

        public void ShowHighlight(RectTransform rectTm, Action onHighlightTouchAction)
        {
            ShowHighlight(onHighlightTouchAction);

            SetHighlightObjPos(rectTm);

            SetHighlightSize(rectTm);
        }

        void ShowHighlight(Action onHighlightTouchAction)
        {
            if (mHighlightObj.activeSelf == false)
            {
                mHighlightObj.SetActive(true);

                foreach (var tween in mHighlightObj.GetComponentsInChildren<DOTweenAnimation>())
                {
                    tween.DORewind();
                    tween.DOPlay();
                }
            }

            mTouchFingerObj.SetActive(onHighlightTouchAction != null);
            mButtonHighlightTouch.SetButtonAction(() => onHighlightTouchAction?.Invoke());
        }

        public void SetHighlightObjPos(Vector3 pos)
        {
            mHighlightObj.transform.position = pos;
        }

        void SetHighlightSize(RectTransform rectTm)
        {
            Vector2 highlightSize = rectTm.rect.size * 1.1f;

            mImageHighlightBox.rectTransform.sizeDelta = highlightSize;
        }

        void SetHighlightObjPos(RectTransform rectTm)
        {
            RectTransform tm = mHighlightObj.GetComponent<RectTransform>();

            tm.position = rectTm.position;
        }

        public void SetActivHighlightObj(bool active)
        {
            mHighlightObj.SetActive(active);
        }

        public void SetActiveTouchBlock(bool active)
        {
            mImageTouchBlock.raycastTarget = active;
        }

        public void SetTouchBlockAlpha(float alpha)
        {
            var newColor = mImageTouchBlock.color;

            newColor.a = alpha;

            mImageTouchBlock.color = newColor;
        }

        public void SetHighlightModalAlpha(float alpha)
        {
            var newColor = mImageHighlightModal.color;

            newColor.a = alpha;

            mImageHighlightModal.color = newColor;
        }

        RectTransform GetGuideTarget(GuideActionType type)
        {
            RectTransform target;

            if (type == GuideActionType.Highlight_BestEquipment)
            {
                var inventoryTabUI = Lance.Lobby.GetLobbyTabUI<Lobby_InventoryUI>();

                target = inventoryTabUI.GetBestEquipmentItemUI()?.GetComponent<RectTransform>();
            }
            else if (type == GuideActionType.Highlight_BestSkill)
            {
                var skillInventoryTabUI = Lance.Lobby.GetLobbyTabUI<Lobby_SkillInventoryUI>();

                var result = skillInventoryTabUI.GetBestSkillItemUI();

                target = result.transform as RectTransform;
            }
            else if (type == GuideActionType.Highlight_CanEquipBestSkill)
            {
                var skillInventoryTabUI = Lance.Lobby.GetLobbyTabUI<Lobby_SkillInventoryUI>();

                var result = skillInventoryTabUI.GetCanEquipBestSkillItemUI();

                target = result?.transform as RectTransform;
            }
            else if (type == GuideActionType.Highlight_CanUpgradeSkill)
            {
                var skillInventoryTabUI = Lance.Lobby.GetLobbyTabUI<Lobby_SkillInventoryUI>();

                var result = skillInventoryTabUI.GetCanUpgradeSkillItemUI();

                target = result?.transform as RectTransform;
            }
            else if (type == GuideActionType.Highlight_CanUpgradeArtifact)
            {
                var statureTabUI = Lance.Lobby.GetLobbyTabUI<Lobby_StatureUI>();

                var result = statureTabUI.GetCanUpgradeArtifactItemUI();

                target = result.transform as RectTransform;
            }
            else if (type == GuideActionType.Highlight_FirstAbility)
            {
                var statureTabUI = Lance.Lobby.GetLobbyTabUI<Lobby_StatureUI>();

                var result = statureTabUI.GetFirstAbilityItemUI();

                target = result.transform as RectTransform;
            }
            else if (type == GuideActionType.Highlight_BestPet)
            {
                var petTabUI = Lance.Lobby.GetLobbyTabUI<Lobby_PetUI>();

                target = petTabUI.GetBestPetItemUI();
            }
            else
            {
                // 미리 찾아놓은 것 중에 있는지 확인한다.
                target = mGuideTargets.TryGet(type);
                if (target == null)
                {
                    // 팝업중에 있는지 찾아보자
                    target = Lance.PopupManager.Parent.FindGuideActionTag(type);
                }
            }

            return target;
        }
    }
}