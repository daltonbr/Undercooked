using Lean.Transition;
using UnityEngine;
using TMPro;

namespace Undercooked
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreBaseText;
        [SerializeField] private TextMeshProUGUI scoreAnimatedText;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("notification colors")]
        [SerializeField] private Color positiveColorOutline;
        [SerializeField] private Color positiveColorBase;
        [SerializeField] private Color negativeColorOutline;
        [SerializeField] private Color negativeColorBase;

        private Vector3 _initialAnimatedTextLocalPosition;

        private void Awake()
        {
            _initialAnimatedTextLocalPosition = scoreAnimatedText.transform.localPosition;
        }

        private void OnEnable()
        {
            GameManager.OnScoreUpdate += HandleScoreUpdate;
        }

        private void OnDisable()
        {
            GameManager.OnScoreUpdate -= HandleScoreUpdate;
        }

        private void HandleScoreUpdate(int score, int delta)
        {
            if (delta == 0) return;
            
            scoreBaseText.text = score.ToString();
            if (delta < 0)
            {
                ScrollAndFadeText(delta.ToString(), negativeColorOutline, negativeColorBase);
                return;
            }
            ScrollAndFadeText($"+{delta.ToString()}", positiveColorOutline, positiveColorBase);
        }
        
        private void ScrollAndFadeText(string textToDisplay, Color baseColor, Color outlineColor, float timeToDisplayInSeconds = 2f)
        {
            scoreAnimatedText.transform.localPosition = _initialAnimatedTextLocalPosition;
            _canvasGroup.alpha = 1f;
            scoreAnimatedText.text = textToDisplay;
            scoreAnimatedText.color = baseColor;
            scoreAnimatedText.outlineColor = outlineColor;

            scoreAnimatedText.rectTransform
                .localScaleTransition(new Vector3(1.2f, 1.2f, 1.2f), 0.2f, LeanEase.Decelerate)
                .JoinTransition()
                .localScaleTransition(Vector3.one, .2f, LeanEase.Smooth);
            
            scoreAnimatedText.rectTransform
                 .localPositionTransition_Y(200f, timeToDisplayInSeconds, LeanEase.Smooth);
            
            _canvasGroup.alphaTransition(0f, timeToDisplayInSeconds, LeanEase.Smooth);
        }
    }
}
