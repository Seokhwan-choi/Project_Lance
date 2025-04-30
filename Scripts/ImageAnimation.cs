using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lance
{
    class ImageAnimation : MonoBehaviour
    {
		public Sprite[] Sprites;
		public int SpritePerFrame = 6;
		public bool Loop = true;

		int mIndex = 0;
		Image mImage;
		int mFrame = 0;

		void Awake()
		{
			mImage = GetComponent<Image>();
		}

		void Update()
		{
			mFrame++;
			if (mFrame < SpritePerFrame) 
				return;

			mImage.sprite = Sprites[mIndex];
			mFrame = 0;
			mIndex++;

			if (mIndex >= Sprites.Length && Loop)
			{
				mIndex = 0;
			}
		}
	}
}


