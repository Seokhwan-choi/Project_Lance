using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lance
{
    public class MultiScrollRect : ScrollRect
    {
        bool mIsParent;
        [SerializeField]
        ScrollRect mParentScrollRect;

        protected override void Start()
        {
            base.Start();

            foreach(var scrollRect in GetComponentsInParent<ScrollRect>())
            {
                if (scrollRect == this)
                    continue;

                mParentScrollRect = scrollRect;

                break;
            }
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            mParentScrollRect.OnInitializePotentialDrag(eventData);

            base.OnInitializePotentialDrag(eventData);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (!horizontal && Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y))
                mIsParent = true;
            else if (!vertical && Mathf.Abs(eventData.delta.x) < Mathf.Abs(eventData.delta.y))
                mIsParent = true;
            else
                mIsParent = false;

            if (mIsParent)
                mParentScrollRect.OnBeginDrag(eventData);
            else
                base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (mIsParent)
                mParentScrollRect.OnDrag(eventData);
            else
                base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (mIsParent)
                mParentScrollRect.OnEndDrag(eventData);
            else
                base.OnEndDrag(eventData);
        }
    }
}