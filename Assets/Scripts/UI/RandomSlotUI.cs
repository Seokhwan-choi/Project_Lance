using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class RandomSlotUI : MonoBehaviour
    {
        public void Init(ItemType itemType)
        {
            var image = GetComponent<Image>();
            image.sprite = Lance.Atlas.GetItemSlotUISprite($"Icon_Shop_Random_{itemType}");
            
            TextMeshProUGUI textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            textName.text = StringTableUtil.GetName($"{itemType}");
        }
    }
}