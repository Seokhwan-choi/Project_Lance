using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

namespace Lance
{
    class Lobby_RewardListUI : MonoBehaviour
    {
        List<Vector2> mMovePosList;
        List<Lobby_RewardItemUI> mRewardItemUIList;

        List<ItemInfo> mReservedRewards;
        List<Lobby_RewardItemUI> mVisibleItemUIs;
        public void Init()
        {
            mMovePosList = new List<Vector2>();
            mRewardItemUIList = new List<Lobby_RewardItemUI>();
            mReservedRewards = new List<ItemInfo>();
            mVisibleItemUIs = new List<Lobby_RewardItemUI>();

            for (int i = 0; i < 4; ++i)
            {
                var rewardItemUIObj = gameObject.FindGameObject($"RewardItemUI_{i + 1}");
                var rewardItemUI = rewardItemUIObj.GetOrAddComponent<Lobby_RewardItemUI>();
                rewardItemUI.Init();

                var rectTm = rewardItemUIObj.GetComponent<RectTransform>();

                mMovePosList.Add(rectTm.anchoredPosition);

                mRewardItemUIList.Add(rewardItemUI);
            }
        }

        public void OnUpdate(float dt)
        {
            if (gameObject.activeSelf == false)
                return;

            if (mReservedRewards.Count > 0)
            {
                // �ٷ� ����� �� �ִ� UI�� ������ ������ �������
                Lobby_RewardItemUI itemUI = GetRestingItemUI();
                if (itemUI != null)
                {
                    // ����Ǿ� �ִ� ������ ������
                    var reward = PopReward();
                    if (reward != null)
                    {
                        itemUI.Refresh(reward);
                        itemUI.SetActive(true);

                        reward = null;

                        // ���� ����
                        itemUI.PlayShowMotion();

                        // ������ �� �տ� �־�����
                        mVisibleItemUIs.Insert(0, itemUI);

                        // ���Ӱ� �����ֱ�� �߱� ������
                        // UI���� ��ġ�� ���������ش�.
                        UpdateVisibleItemUIsPos();
                    }
                }
            }

            if (mVisibleItemUIs.Count > 0)
            {
                List<Lobby_RewardItemUI> removeList = new List<Lobby_RewardItemUI>();

                foreach (var itemUI in mVisibleItemUIs)
                {
                    itemUI.OnUpdate(dt);

                    if (itemUI.IsResting)
                    {
                        removeList.Add(itemUI);
                    }
                }

                if (removeList.Count > 0)
                {
                    foreach (var remove in removeList)
                    {
                        // ������� ����
                        remove.PlayHideMotion();

                        mVisibleItemUIs.Remove(remove);
                    }
                }

                removeList = null;
            }
        }

        public void ClearReservedReward()
        {
            mReservedRewards.Clear();
        }

        public void ReserveReward(RewardResult reward)
        {
            if (mReservedRewards.Count > 50)
                mReservedRewards.Clear();

            var itemInfos = reward.Split();
            if (itemInfos != null && itemInfos.Count > 0)
            {
                foreach(var itemInfo in itemInfos)
                {
                    mReservedRewards.Add(itemInfo);
                }

                itemInfos = null;
            }
        }

        void UpdateVisibleItemUIsPos()
        {
            for(int i = 0; i < mVisibleItemUIs.Count; ++i)
            {
                // ��ġ �̵�
                mVisibleItemUIs[i].MoveToPos(mMovePosList[i], i == 0);
            }
        }

        Lobby_RewardItemUI GetRestingItemUI()
        {
            foreach(Lobby_RewardItemUI itemUI in mRewardItemUIList)
            {
                if (itemUI.IsResting)
                    return itemUI;
            }

            return null;
        }

        ItemInfo PopReward()
        {
            if (mReservedRewards.Count > 0)
            {
                var reward = mReservedRewards[0];

                mReservedRewards.RemoveAt(0);

                return reward;
            }

            return null;
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }

    class Lobby_RewardItemUI : MonoBehaviour
    {
        bool mInRemoving;
        float mLifetime;
        Image mImageRewardIcon;
        TextMeshProUGUI mTextName;
        TextMeshProUGUI mTextAmount;
        public bool IsResting => mInRemoving == false && mLifetime <= 0f;
        public void Init()
        {
            mImageRewardIcon = gameObject.FindComponent<Image>("Image_RewardIcon");
            mTextName = gameObject.FindComponent<TextMeshProUGUI>("Text_RewardName");
            mTextAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_RewardAmount");

            gameObject.SetActive(false);
        }

        public void OnUpdate(float dt)
        {
            mLifetime -= dt;
        }

        public void Refresh(ItemInfo itemInfo)
        {
            mImageRewardIcon.sprite = itemInfo.GetSprite();
            mTextName.text = itemInfo.GetName();
            mTextAmount.text = itemInfo.GetAmountString();
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);

            mLifetime = isActive ? 0.75f : 0f;
        }
        
        public void MoveToPos(Vector2 pos, bool immediately, Action onComplete = null)
        {
            var rectTm = gameObject.GetComponent<RectTransform>();
            if (immediately)
            {
                rectTm.anchoredPosition = new Vector2(pos.x, pos.y);
            }
            else
            {
                var tween = rectTm.DOAnchorPos(pos, 0.2f)
                .SetEase(Ease.InExpo);

                if (onComplete != null)
                    tween.OnComplete(() => onComplete?.Invoke());
            }
        }

        public void PlayShowMotion()
        {
            var rectTm = gameObject.GetComponent<RectTransform>();
            rectTm.localScale = Vector3.zero;
            rectTm.DOScale(1f, 0.2f)
                .SetDelay(0.2f)
                .SetAutoKill(false);
        }

        public void PlayHideMotion()
        {
            mInRemoving = true;

            var rectTm = gameObject.GetComponent<RectTransform>();
            var movePos = new Vector2(rectTm.anchoredPosition.x - 300, rectTm.anchoredPosition.y);

            MoveToPos(movePos, immediately: false, () => mInRemoving = false);
        }
    }
}

