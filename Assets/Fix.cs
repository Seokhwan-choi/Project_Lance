using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public class Fix : MonoBehaviour
    {
        public int width = 1080;
        public int height = 1920;
        private void Start()
        {
            SetResolution(); // 초기에 게임 해상도 고정
        }

        public void SetResolution()
        {
            int deviceWidth = Screen.width; // 기기 너비 저장
            int deviceHeight = Screen.height; // 기기 높이 저장

            Screen.SetResolution(width, (int)(((float)deviceHeight / deviceWidth) * width), true); // SetResolution 함수 제대로 사용하기

            if ((float)width / height < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
            {
                float newWidth = ((float)width / height) / ((float)deviceWidth / deviceHeight); // 새로운 너비
                Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
            }
            else // 게임의 해상도 비가 더 큰 경우
            {
                float newHeight = ((float)deviceWidth / deviceHeight) / ((float)width / height); // 새로운 높이
                Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
            }
        }
    }
}


