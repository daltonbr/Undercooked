using System.Collections.Generic;
using UnityEngine;

namespace Undercooked.Data
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "LevelData", order = 1)]
    public class LevelData : ScriptableObject
    {
        public string levelName;
        [Tooltip("Orders that going to be randomly spawned")]
        public List<OrderData> orders;
        [Tooltip("Level duration time in seconds")]
        public int durationTime;
        [Header("Star Ratings")]
        public int star1Score;
        public int star2Score;
        public int star3Score;
    }
}