using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lance
{
    class PatternMover : MonoBehaviour
    {
        RawImage mImage;

        public float MoveSpeed = 0.05f;
        public bool IsLeft = false;
        void Start()
        {
            mImage = GetComponent<RawImage>();
        }

        void Update()
        {
            Rect uvRect = mImage.uvRect;

            uvRect.x += Time.unscaledDeltaTime * (IsLeft ? -MoveSpeed : MoveSpeed);
            //uvRect.y += Time.deltaTime * MoveSpeed;

            mImage.uvRect = uvRect;
        }
    }
}