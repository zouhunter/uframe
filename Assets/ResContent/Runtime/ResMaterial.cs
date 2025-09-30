using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace UFrame.ResContent
{
    public class ResMaterial : ResBase<Material>
    {
        public void Charge(Graphic targetGraphic)
        {
            if (targetGraphic == null)
                return;
            targetGraphic.material = Value;
        }
        public void Charge(Renderer renderer)
        {
            if(renderer)
            {
                renderer.material = Value;
            }
        }
    }
}