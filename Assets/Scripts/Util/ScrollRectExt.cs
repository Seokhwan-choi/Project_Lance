using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Lance
{
    static class ScrollRectExt
    {
        public static void ScrollToTarget(this ScrollRect scrollRect, RectTransform target)
        {
            Canvas.ForceUpdateCanvases();

            float endValue = scrollRect.transform.InverseTransformPoint(scrollRect.content.position).y
                    - scrollRect.transform.InverseTransformPoint(target.position).y - (target.rect.height * 0.5f + 25f);

            scrollRect.content.anchoredPosition = new Vector2(0f, endValue);

            scrollRect.normalizedPosition = new Vector2(0f, Mathf.Clamp01(scrollRect.normalizedPosition.y));
        }
        
        public static void CenterOnItem(this ScrollRect scroll, RectTransform target, Vector3? targetOffsetPos = null)
        {
            RectTransform scrollTm = scroll.transform as RectTransform;
            RectTransform viewPort = scroll.viewport;
            RectTransform content = scroll.content;

            // Item is here
            var itemCenterPositionInScroll = GetWorldPointInWidget(scrollTm, GetWidgetWorldPoint(isVertical:scroll.vertical, isTarget:true, target, targetOffsetPos));
            // But must be here
            var targetPositionInScroll = GetWorldPointInWidget(scrollTm, GetWidgetWorldPoint(isVertical: scroll.vertical, isTarget:false, viewPort));
            // So it has to move this distance
            var difference = targetPositionInScroll - itemCenterPositionInScroll;
            difference.z = 0f;

            //clear axis data that is not enabled in the scrollrect
            if (!scroll.horizontal)
            {
                difference.x = 0f;
            }
            if (!scroll.vertical)
            {
                difference.y = 0f;
            }

            var normalizedDifference = new Vector2(
                difference.x / (content.rect.size.x - scrollTm.rect.size.x),
                difference.y / (content.rect.size.y - scrollTm.rect.size.y));

            var newNormalizedPosition = scroll.normalizedPosition - normalizedDifference;
            if (scroll.movementType != ScrollRect.MovementType.Unrestricted)
            {
                newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
                newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);

                if (!scroll.horizontal)
                {
                    newNormalizedPosition.x = 0f;
                }
                if (!scroll.vertical)
                {
                    newNormalizedPosition.y = 0f;
                }
            }

            scroll.normalizedPosition = newNormalizedPosition;


            static Vector3 GetWidgetWorldPoint(bool isVertical, bool isTarget, RectTransform target, Vector3? targetOffsetPos = null)
            {
                //pivot position + item size has to be included
                var pivotOffset = new Vector3(
                    (0.5f - target.pivot.x) * target.rect.size.x,
                    (0.5f - target.pivot.y) * target.rect.size.y,
                    0f);
                var localPosition = target.localPosition + pivotOffset;

                if (targetOffsetPos != null && isTarget)
                {
                    localPosition += targetOffsetPos.Value;
                }

                return target.parent.TransformPoint(localPosition);
            }

            static Vector3 GetWorldPointInWidget(RectTransform target, Vector3 worldPoint)
            {
                return target.InverseTransformPoint(worldPoint);
            }
        }

        public static void ScrollToCenterElement(this ScrollRect scroll, RectTransform element)
        {
            // ScrollRect의 가운데 위치를 계산합니다.
            Vector2 centerPosition = new Vector2(scroll.viewport.rect.width / 2f, scroll.viewport.rect.height / 2f);

            // 스크롤할 요소의 위치를 계산합니다.
            Vector2 targetPosition = (Vector2)scroll.transform.InverseTransformPoint(element.position)
                - (Vector2)scroll.transform.InverseTransformPoint(scroll.content.position);

            // 요소가 가운데로 스크롤되도록 스크롤 위치를 계산합니다.
            Vector2 scrollToCenter = centerPosition - targetPosition;

            Vector2 normalizedPosition = Vector2.zero;

            if (scroll.horizontal)
            {
                normalizedPosition.x = Mathf.Clamp01(scrollToCenter.x / (scroll.content.rect.size.x - scroll.viewport.rect.size.x));
            }

            if (scroll.vertical)
            {
                normalizedPosition.y = Mathf.Clamp01(scrollToCenter.y / (scroll.content.rect.size.y - scroll.viewport.rect.size.y));
            }

            // ScrollRect의 스크롤 위치를 조정하여 해당 요소가 가운데로 스크롤되도록 합니다.
            scroll.normalizedPosition = normalizedPosition;
        }

        public static void ScrollToCenterElement2(this ScrollRect scrollRect, RectTransform targetElement)
        {
            // ScrollRect의 Content 내의 요소가 ScrollRect의 가운데로 스크롤되도록 합니다.
            Vector2 targetPosition = (Vector2)scrollRect.transform.InverseTransformPoint(targetElement.position)
                - (Vector2)scrollRect.transform.InverseTransformPoint(scrollRect.content.position);

            // 스크롤할 위치를 계산하여 요소가 ScrollRect의 가운데로 오도록 조정합니다.
            Vector2 scrollToCenter = scrollRect.content.anchoredPosition - targetPosition + scrollRect.viewport.rect.size * 0.5f;

            Vector2 normalizedPosition = Vector2.zero;

            if (scrollRect.horizontal)
            {
                normalizedPosition.x = Mathf.Clamp01(scrollToCenter.x / (scrollRect.content.rect.size.x - scrollRect.viewport.rect.size.x));
            }

            if (scrollRect.vertical)
            {
                normalizedPosition.y = Mathf.Clamp01(scrollToCenter.y / (scrollRect.content.rect.size.y - scrollRect.viewport.rect.size.y));
            }
            // ScrollRect의 스크롤 위치를 조정하여 해당 요소가 가운데로 스크롤되도록 합니다.
            scrollRect.normalizedPosition = normalizedPosition;
        }
    }


}
