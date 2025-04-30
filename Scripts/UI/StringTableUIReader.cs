using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Lance
{
    class StringTableUIReader : MonoBehaviour
    {
        public string Category;
        public string Index;

        private void Start()
        {
            Refresh();
        }

        void Refresh()
        {
            string str = StringTableUtil.Get($"{Category}_{Index}");
            if (str.IsValid())
            {
                var text = gameObject.GetOrAddComponent<TextMeshProUGUI>();

                text.text = str;
            }
            else
            {
                Debug.LogError($"{gameObject.name}의 Category or Index가 잘 못 입력됨");
            }
        }

        public void Localize()
        {
            Refresh();
        }
    }
}