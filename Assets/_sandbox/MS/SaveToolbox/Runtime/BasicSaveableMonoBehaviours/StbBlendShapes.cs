using System;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
    [AddComponentMenu("SaveToolbox/SavingBehaviours/StbBlendShapes")]
    public class StbBlendShapes : SaveableMonoBehaviour
    {
        [SerializeField]
        private SkinnedMeshRenderer skinnedMeshRenderer;

        public override object Serialize()
        {
            if (skinnedMeshRenderer == null)
            {
                if (!TryGetComponent(out skinnedMeshRenderer)) throw new Exception($"Could not serialize object of type SkinnedMeshRenderer as there isn't one referenced or attached to the game object.");
            }

            var blendShapeWeightsCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
            var blendShapeWeightArray = new float[blendShapeWeightsCount];
            for (var index = 0; index < blendShapeWeightArray.Length; index++)
            {
                blendShapeWeightArray[index] = skinnedMeshRenderer.GetBlendShapeWeight(index);
            }

            return blendShapeWeightArray;
        }

        public override void Deserialize(object data)
        {
            var blendShapeWeightsArray = (float[])data;
            for (var index = 0; index < blendShapeWeightsArray.Length; index++)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(index, blendShapeWeightsArray[index]);
            }
        }
    }
}
