using System;
using TMPro;
using Undercooked.Managers;
using UnityEngine;
using UnityEngine.Assertions;

namespace Undercooked.UI
{
    public class CountdownUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private void Awake()
        {
            #if UNITY_EDITOR
                  Assert.IsNotNull(text);
            #endif
        }

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