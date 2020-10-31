using System.Collections;
using Lean.Transition;
using UnityEngine;
using UnityEngine.UI;

namespace Undercooked.UI
{
    public class OrderUI : MonoBehaviour
    {
        [SerializeField] private RectTransform _mainPanel;
        [SerializeField] private RectTransform basePanel;
        [SerializeField] private RectTransform bottomPanel;
        [SerializeField] private Image orderImage;
        [SerializeField] private Slider slider;

        private Image sliderFill;
        
        // how to handle color lerp in slider
        [SerializeField] private Color sliderInitialColor;
        [SerializeField] private Color sliderFinalColor;
        // single or multiple bottomPanel?
        
        public Order Order { get; }

        private void Awake()
        {
            //TODO: how to lerp the slider color individually?
            // sliderFill = slider.fillRect.gameObject.GetComponent<Image>();
            // sliderFill.material.color = sliderFinalColor;
            _mainPanel = GetComponent<RectTransform>();
        }

        public void Setup(Order order)
        {
            orderImage.sprite = order.sprite;

            // resize the basePanel according to the amount of Ingredients
            
            // spawn individual ingredients Image
            
            foreach (IngredientData ingredientData in order.ingredients)
            {
                // fill the image sprites
            }
        }

        [ContextMenu("Shake a lot")]
        public void StartShake()
        {
            StartCoroutine(ShakeCoroutine());
        }

        private WaitForSeconds _shakeWait = new WaitForSeconds(0.25f);
        
        private IEnumerator ShakeCoroutine()
        {
            for (int i = 0; i < 10; i++)
            {
                Shake();
                yield return _shakeWait;    
            }
        }
        
        // TODO: Shake OrderUI - should this be here?
        [ContextMenu("Shake")]
        public void Shake()
        {
            // oscillate this Panel horizontally for a how long?
            // blink it red - need verification
            Vector3 pulseVector = new Vector3(1.1f, 1.1f, 1.1f);
            float origin_X = this.transform.position.x;
            Vector3 deltaVector = new Vector3(0.1f, 0.0f, 0.0f);
            float delta = 10f;
            Debug.Log($"[OrderUI] Shake position: {transform.position} x:{origin_X}");

            var left = origin_X - delta;
            var right = origin_X + delta;
            
            _mainPanel.transform
                .positionTransition_X(right, 0.1f, LeanEase.Accelerate)
                .JoinTransition()
                .positionTransition_X(left, 0.1f, LeanEase.Accelerate)
                //.JoinTransition()
                // .positionTransition_X(right, .08f, LeanEase.Accelerate)
                // .JoinTransition()
                // .positionTransition_X(left, 0.09f, LeanEase.Accelerate)
                // .JoinTransition()
                // .positionTransition_X(right, 0.10f, LeanEase.Accelerate)
                // .JoinTransition()
                // .positionTransition_X(left, 0.11f, LeanEase.Accelerate)
                .JoinTransition()
                .positionTransition_X(origin_X, 0.05f, LeanEase.Accelerate);
            //.PlaySoundTransition(beep)
            //.positionTransition_X(origin_X - delta, 0.5f, LeanEase.Smooth)
            //.positionTransition_X(origin_X, 0.5f, LeanEase.Smooth);
            //.localPositionTransition(-delta, 0.5f, LeanEase.Smooth)
            //.localScaleTransition(pulseVector, .03f, LeanEase.Accelerate)
            //.JoinDelayTransition(.04f)
            //.localPositionTransition(delta, 0.5f, LeanEase.Smooth)
            //.localPositionTransition(Vector3.zero, 0.25f, LeanEase.Smooth);
            //.localScaleTransition(Vector3.one, 0.03f, LeanEase.Accelerate);

        }
        
        // hide and show bottomPanel? here?
        
    }
}