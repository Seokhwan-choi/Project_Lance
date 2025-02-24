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
                Debug.LogError($"{gameObject.name}�� Category or Index�� �� �� �Էµ�");
            }
        }

        public void Localize()
        {
            Refresh();
        }
    }
}