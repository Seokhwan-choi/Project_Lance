using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace Lance
{
    // Âü°í : https://youtu.be/XJJl19N2KFM
    class CutoutMaskUI : Image
    {
        public override Material materialForRendering
        {
            get
            {
                var material = new Material(base.materialForRendering);

                material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);

                return material;
            }
        }
    }
}