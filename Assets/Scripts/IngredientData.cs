using UnityEngine;

namespace Undercooked
{
    [CreateAssetMenu(fileName = "IngredientData", menuName = "IngredientData", order = 0)]
    public class IngredientData : ScriptableObject
    {
        public IngredientType type;
        public IngredientStatus status;
        public float timeToProcess = 4f;
        
        [Header("Visuals")]
        public Mesh rawMesh;
        public Mesh processedMesh;
        [Tooltip("We are using the same material for raw and processed meshes")]
        public Material ingredientMaterial;
        [Tooltip("Used to tint related objects, like the Plate")] // we may put this into the plate itself later
        public Color baseColor;

    }
}