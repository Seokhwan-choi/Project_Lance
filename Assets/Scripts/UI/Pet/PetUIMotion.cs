using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lance
{
    class PetUIMotion : MonoBehaviour
    {
		const int SpritePerFrame = 8;

		int mIndex = 0;
		int mFrame = 0;

		int mStep;
		ElementalType mElementalType;
		
		Image mImage;
		Sprite[] mSprites;

		public void Init()
        {
			mImage = GetComponent<Image>();

			mElementalType = ElementalType.Normal;
			mStep = -1;
		}

		public void RefreshSprites(ElementalType type, int step)
        {
			if (mElementalType != type || mStep != step)
            {
				mElementalType = type;
				mStep = step;

				if (step > 5 && step < 10)
					step = 6;
				else if (step >= 10)
					step = 7;

				List<Sprite> petSprites = new List<Sprite>();

				for(int i = 0; i < 4; ++i)
                {
					var sprite = Lance.Atlas.GetPetSprite($"Pet{type}_{step}_{i}");

					petSprites.Add(sprite);
				}

				mSprites = petSprites.ToArray();

				mImage.sprite = mSprites[mIndex];
			}
        }

		void Update()
		{
			mFrame++;
			if (mFrame < SpritePerFrame)
				return;

			mImage.sprite = mSprites[mIndex];
			mFrame = 0;
			mIndex++;

			if (mIndex >= mSprites.Length)
			{
				mIndex = 0;
			}
		}
	}
}