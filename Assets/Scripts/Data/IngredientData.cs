using Undercooked.Model;
using UnityEngine;

namespace Undercooked.Data
{
    [CreateAssetMenu(fileName = "IngredientData", menuName = "IngredientData", order = 0)]
    public class IngredientData : ScriptableObject
    {
        public IngredientType type;
        public float processTime = 7.4f;
        public float cookTime = 6f;
        
        [Header("Visuals")]
        public Mesh rawMesh;
        public Mesh processedMesh;
        public Mesh cookedMesh;
        [Tooltip("We are using the same material for raw and processed meshes")]
        public Material ingredientMaterial;
        [Tooltip("Used to tint related objects, like the Plate")]
        public Color baseColor;
        [Tooltip("UI usage")]
        public Sprite sprite;
    }
}