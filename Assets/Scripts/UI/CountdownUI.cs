using System;
using TMPro;
using UnityEngine;

namespace Undercooked.UI
{
    public class CountdownUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private void OnEnable()
        {
            GameManager.OnCountdownTick += HandleCountdownTick;
        }

        private void OnDisable()
        {
            GameManager.OnCountdownTick -= HandleCountdownTick;
        }

        private void HandleCountdownTick(int timeRemaining)
        {
            var timespan = TimeSpan.FromSeconds(timeRemaining);
            text.text = timespan.ToString(@"m\:ss");
        }
    }
}