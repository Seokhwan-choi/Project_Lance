using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


namespace Lance
{
    class LimitBreakUIManager : MonoBehaviour
    {
        Dictionary<int, LimitBreakLineUI> mLineUIDics;
        Dictionary<int, LimitBreakEdgeUI> mEdgeUIDics;
        public void Init()
        {
            // 라인은 총 11개
            mLineUIDics = new Dictionary<int, LimitBreakLineUI>();
            for (int i = 0; i < 11; ++i)
            {
                int lineNum = i + 1;

                var line = gameObject.FindGameObject($"Image_Line_{lineNum:00}");

                var lineUI = line.GetOrAddComponent<LimitBreakLineUI>();
                lineUI.Init();

                mLineUIDics.Add(lineNum, lineUI);
            }

            mEdgeUIDics = new Dictionary<int, LimitBreakEdgeUI>();
            
            // 한계돌파는 총 12단계
            for (int i = 0; i < Lance.GameData.CommonData.limitBreakMaxLevel; ++i)
            {
                int edgeNum = i + 1;

                var edge = gameObject.FindGameObject($"Image_LimitBreak_{edgeNum:00}");

                var edgeUI = edge.GetOrAddComponent<LimitBreakEdgeUI>();
                edgeUI.Init();

                mEdgeUIDics.Add(edgeNum, edgeUI);
            }
        }

        public void Refresh()
        {
            int limitBreak = Lance.Account.ExpLevel.GetLimitBreak();

            ActiveLimitBreak(limitBreak);
        }

        public void ActiveLimitBreak(int limitBreak)
        {
            for (int i = 0; i < Lance.GameData.CommonData.limitBreakMaxLevel; ++i)
            {
                int step = i + 1;
                bool isActive = step <= limitBreak;

                var edgeUI = mEdgeUIDics.TryGet(step);
                edgeUI.SetActive(isActive);

                if (isActive)
                {
                    int[] activeLines = DataUtil.GetLimitBreakActiveLines(step);
                    if (activeLines != null)
                    {
                        for (int j = 0; j < activeLines.Length; ++j)
                        {
                            int lineNum = activeLines[j];

                            var lineUI = mLineUIDics.TryGet(lineNum);

                            lineUI.SetActive(true);
                        }
                    }
                }
            }
        }

        public IEnumerator PlayActiveMotion(int step)
        {
            ActiveLimitBreak(step - 1);

            yield return new WaitForSecondsRealtime(2f);

            int[] activeLines = DataUtil.GetLimitBreakActiveLines(step);
            if (activeLines != null)
            {
                for (int j = 0; j < activeLines.Length; ++j)
                {
                    int lineNum = activeLines[j];

                    var lineUI = mLineUIDics.TryGet(lineNum);

                    lineUI.PlayActiveMotion();
                }

                yield return new WaitForSecondsRealtime(0.4f);
            }

            var edgeUI = mEdgeUIDics.TryGet(step);
            if (edgeUI != null)
            {
                yield return edgeUI.PlayActiveMotion();
            }

            yield return new WaitForSecondsRealtime(1f);
        }
    }

    class LimitBreakLineUI : MonoBehaviour
    {
        Image mImageActive;

        public void Init()
        {
            mImageActive = gameObject.FindComponent<Image>("Active");

            SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            mImageActive.gameObject.SetActive(isActive);
        }

        public void PlayActiveMotion()
        {
            SetActive(true);

            mImageActive.fillAmount = 0f;

            DOTween.To(() => mImageActive.fillAmount, f => mImageActive.fillAmount = f, 1f, 0.5f);
        }
    }

    class LimitBreakEdgeUI : MonoBehaviour
    {
        Image mImageActive;
        GameObject mLimitBreakFX;
        public void Init()
        {
            mImageActive = gameObject.FindComponent<Image>("Active");
            mLimitBreakFX = gameObject.FindGameObject("LimitBreak");

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

            Lance.ParticleManager.AquireUI("LimitBreakActive", mImageActive.rectTransform);

            yield return new WaitForSecondsRealtime(1f);

            SetActiveFX(true);

            yield return new WaitForSecondsRealtime(1.1f);

            SoundPlayer.PlayLimitBreakActive();
        }
    }
}