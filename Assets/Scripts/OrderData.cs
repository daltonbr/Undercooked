using System.Collections.Generic;
using UnityEngine;

namespace Undercooked
{
    [CreateAssetMenu(fileName = "OrderData", menuName = "OrderData", order = 2)]
    public class OrderData : ScriptableObject
    {
        public Sprite sprite;
        public List<IngredientData> ingredients;
    }
}