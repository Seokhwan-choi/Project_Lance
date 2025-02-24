using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Lance
{
     class RectTransformAspectRatio : UIBehaviour
     {
        protected override void OnRectTransformDimensionsChange()
        {
            var mainObj = gameObject.FindGameObject("Main");

            var mainRectTm = mainObj.GetComponent<RectTransform>();

            var rectTm = GetComponent<RectTransform>();

            if (rectTm.sizeDelta.x > rectTm.sizeDelta.y)
            {
                mainRectTm.sizeDelta = new Vector2(rectTm.sizeDelta.x, rectTm.sizeDelta.x);
            }
            else
            {
                mainRectTm.sizeDelta = new Vector2(rectTm.sizeDelta.y, rectTm.sizeDelta.y);
            }
        }
     }
}


