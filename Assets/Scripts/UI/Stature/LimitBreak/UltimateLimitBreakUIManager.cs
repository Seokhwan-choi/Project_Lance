using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


namespace Lance
{
    class UltimateLimitBreakUIManager : MonoBehaviour
    {
        Dictionary<int, UltimateLimitBreakEdgeUI> mEdgeUIDics;
        public void Init()
        {
            mEdgeUIDics = new Dictionary<int, UltimateLimitBreakEdgeUI>();

            // ±Ø ÇÑ°èµ¹ÆÄ´Â ÃÑ 7´Ü°è
            for (int i = 0; i < Lance.GameData.CommonData.ultimateLimitBreakMaxLevel; ++i)
            {
                int edgeNum = i + 1;

                var edge = gameObject.FindGameObject($"Image_UltimateLimitBreak_{edgeNum:00}");

                var edgeUI = edge.GetOrAddComponent<UltimateLimitBreakEdgeUI>();
                edgeUI.Init();

                mEdgeUIDics.Add(edgeNum, edgeUI);
            }
        }

        public void Refresh()
        {
            int limitBreak = Lance.Account.ExpLevel.GetUltimateLimitBreak();

            ActiveLimitBreak(limitBreak);
        }

        public void ActiveLimitBreak(int limitBreak)
        {
            for (int i = 0; i < Lance.GameData.CommonData.ultimateLimitBreakMaxLevel; ++i)
            {
                int step = i + 1;
                bool isActive = step <= limitBreak;

                var edgeUI = mEdgeUIDics.TryGet(step);
                edgeUI.SetActive(isActive);
            }
        }

        public IEnumerator PlayActiveMotion(int step)
        {
            ActiveLimitBreak(step - 1);

            yield return new WaitForSecondsRealtime(2f);

            var edgeUI = mEdgeUIDics.TryGet(step);
            if (edgeUI != null)
            {
                yield return edgeUI.PlayActiveMotion();
            }

            yield return new WaitForSecondsRealtime(1f);
        }
    }
    class UltimateLimitBreakEdgeUI : MonoBehaviour
    {
        Image mImageActive;
        GameObject mLimitBreakFX;
        public void Init()
        {
            mImageActive = gameObject.FindComponent<Image>("Active");
            mLimitBreakFX = gameObject.FindGameObject("UltimateLimitBreak");

            SetActiveImage(false);
        }

        public void SetActive(bool isActive)
        {
            SetActiveImage(isActive);
            SetActiveFX(isActive);
        }

        public void SetActiveFX(bool isActive)
        {
            mLimitBreakFX.SetActive(isActive);
        }

        public void SetActiveImage(bool isActive)
        {
            mImageActive.gameObject.SetActive(isActive);
        }

        public IEnumerator PlayActiveMotion()
        {
            SetActiveImage(true);
            SetActiveFX(false);

            SoundPlayer.PlayLimitBreakReady();

            Lance.ParticleManager.AquireUI("UltimateLimitBreakActive", mImageActive.rectTransform);

            yield return new WaitForSecondsRealtime(1f);

            SetActiveFX(true);

            yield return new WaitForSecondsRealtime(1.1f);

            SoundPlayer.PlayLimitBreakActive();
        }
    }
}