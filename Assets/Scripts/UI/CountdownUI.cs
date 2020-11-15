using System;
using Lean.Transition;
using TMPro;
using Undercooked.Managers;
using UnityEngine;
using UnityEngine.Assertions;

namespace Undercooked.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CountdownUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text; 
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            #if UNITY_EDITOR
                  Assert.IsNotNull(text);
                  Assert.IsNotNull(_canvasGroup);
            #endif
            
            _canvasGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            GameManager.OnCountdownTick += HandleCountdownTick;
            GameManager.OnLevelStart += HandleLevelStart;
            GameManager.OnTimeIsOver += HandleTimeOver;
        }

        private void OnDisable()
        {
            GameManager.OnCountdownTick -= HandleCountdownTick;
            GameManager.OnLevelStart -= HandleLevelStart;
            GameManager.OnTimeIsOver -= HandleTimeOver;
        }
        
        private void HandleLevelStart()
        {
            _canvasGroup.alphaTransition(1f, 1f);
        }

        private void HandleTimeOver()
        {
            _canvasGroup.alphaTransition(0f, 1f);
        }

        private void HandleCountdownTick(int timeRemaining)
        {
            var timespan = TimeSpan.FromSeconds(timeRemaining);
            text.text = timespan.ToString(@"m\:ss");
        }
    }
}