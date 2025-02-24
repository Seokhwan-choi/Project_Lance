using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lance
{
    class GuideActionTag : MonoBehaviour
    {
        public GuideActionType Tag;
    }

    static class GuideActionTagExt
    {
		public static RectTransform FindGuideActionTag(this Component comp, GuideActionType tag)
		{
			var tags = comp.GetComponentsInChildren<GuideActionTag>(true);
			for (int i = 0; i < tags.Length; i++)
			{
				if (tags[i].Tag == tag)
				{
					return tags[i].GetComponent<RectTransform>();
				}
			}

			return null;
		}

		public static T FindGuideActionTag<T>(this Component comp, GuideActionType tag) where T : Component
		{
			var tagObj = FindGuideActionTag(comp, tag);
			if (tagObj != null)
			{
				return tagObj.GetComponent<T>();
			}

			return null;
		}

		public static RectTransform FindGuideActionTag(this GameObject go, GuideActionType tag)
		{
			return go.transform.FindGuideActionTag(tag);
		}
	}
}