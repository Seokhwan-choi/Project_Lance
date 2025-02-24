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
            SetResolution(); // �ʱ⿡ ���� �ػ� ����
        }

        public void SetResolution()
        {
            int deviceWidth = Screen.width; // ��� �ʺ� ����
            int deviceHeight = Screen.height; // ��� ���� ����

            Screen.SetResolution(width, (int)(((float)deviceHeight / deviceWidth) * width), true); // SetResolution �Լ� ����� ����ϱ�

            if ((float)width / height < (float)deviceWidth / deviceHeight) // ����� �ػ� �� �� ū ���
            {
                float newWidth = ((float)width / height) / ((float)deviceWidth / deviceHeight); // ���ο� �ʺ�
                Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // ���ο� Rect ����
            }
            else // ������ �ػ� �� �� ū ���
            {
                float newHeight = ((float)deviceWidth / deviceHeight) / ((float)width / height); // ���ο� ����
                Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // ���ο� Rect ����
            }
        }
    }
}


