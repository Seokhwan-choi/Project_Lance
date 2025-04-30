using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class Popup_ArtifactProbUI : PopupBase
    {
        public void Init(IEnumerable<ArtifactData> datas, ArtifactProbData probData)
        {
            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get("Title_SpawnProb"));

            var probList = gameObject.FindGameObject("ProbList");

            probList.AllChildObjectOff();

            int index = 0;

            foreach(ArtifactData data in datas)
            {
                float prob = probData.prob[index];

                var artifactProbItemUIObj = Util.InstantiateUI("ArtifactProbItemUI", probList.transform);

                var artifactProbItemUI = artifactProbItemUIObj.GetOrAddComponent<ArtifactProbItemUI>();

                artifactProbItemUI.Init(data, prob);

                index++;
            }
        }
    }

    class ArtifactProbItemUI : MonoBehaviour
    {
        public void Init(ArtifactData data, float prob)
        {
            Image imageArtifact = gameObject.FindComponent<Image>("Image_Artifact");
            imageArtifact.sprite = Lance.Atlas.GetItemSlotUISprite(data.sprite);

            TextMeshProUGUI textProb = gameObject.FindComponent<TextMeshProUGUI>("Text_Prob");
            textProb.text = $"{prob * 100f:F3}%";
        }
    }
}