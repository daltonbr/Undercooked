using UnityEngine;
using TMPro;

namespace Undercooked
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private void OnEnable()
        {
            GameManager.OnScoreUpdate += HandleScoreUpdate;
        }

        private void OnDisable()
        {
            GameManager.OnScoreUpdate -= HandleScoreUpdate;
        }

        private void HandleScoreUpdate(int score)
        {
            text.text = score.ToString();
        }
    }
}
