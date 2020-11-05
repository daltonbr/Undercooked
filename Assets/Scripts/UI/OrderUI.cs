using System.Collections.Generic;
using System.Threading.Tasks;
using Lean.Transition;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Undercooked.UI
{
    public class OrderUI : MonoBehaviour
    {
        [SerializeField] private RectTransform rootRectTransform;
        [SerializeField] private RectTransform basePanel;
        [SerializeField] private RectTransform bottomPanel;
        [SerializeField] private Image orderImage;
        [SerializeField] private Slider slider;
        [SerializeField] private List<Image> ingredientImages = new List<Image>(); 
        
        [SerializeField] AudioClip popAudio;
        [SerializeField] AudioClip notificationAudio;

        [SerializeField] private Image[] _images;
        private Image sliderFill;
        private const float UIWidth = 190f;

        private int ShakeIntervalTimeMs = 600;
        private bool _shake;
        private float InitialRemainingTime;
        
        public float CurrentAnchorX { get; private set; }

        // private Coroutine _shakeCoroutine;
        
        // how to handle color lerp in slider
        [SerializeField] private Color sliderInitialColor;
        [SerializeField] private Color sliderFinalColor;
        
        private Material _uiMaterial;
        
        public OrderData OrderData { get; }
        public float SizeDeltaX => rootRectTransform.sizeDelta.x;

        public Order Order { get; private set; }
        
        // public delegate void Delivered(Order order, int timeRemaining);
        // public event Delivered OnDelivered;

        private void Awake()
        {
            DuplicateMaterial();

            //TODO: how to lerp the slider color individually?
            sliderFill = slider.fillRect.gameObject.GetComponent<Image>();
            sliderFill.material.color = sliderFinalColor;
            rootRectTransform = GetComponent<RectTransform>();
        }

        private async void HandleExpired(Order order)
        {
            StopShake();
            //TODO: play buzzer audio
            await RotateAlertBasePanelAsync(Color.red, null);
            // Timer will be reset
        }

        private void HandleAlertTime(Order order)
        {
            StartShake();
        }

        private void DuplicateMaterial()
        {
            _images = GetComponentsInChildren<Image>();

            if (_images.Length > 0)
            {
                _uiMaterial = Instantiate(_images[0].material);
                foreach (var image in _images)
                {
                    image.material = _uiMaterial;
                }
            }
        }

        public void Setup(Order order)
        {
            Order = order;
            rootRectTransform.anchoredPosition = new Vector2(Screen.width + 300f, 0f);
            var sizeDelta = rootRectTransform.sizeDelta;
            rootRectTransform.sizeDelta = new Vector2(UIWidth, sizeDelta.y);
            var randomRotation = Random.Range(-45f, +45f);
            basePanel.localRotation = Quaternion.Euler(0f, 0f, randomRotation);
            
            basePanel.localPosition = new Vector3(basePanel.localPosition.x, 0f, 0f);

            orderImage.sprite = Order.OrderData.sprite;
            InitialRemainingTime = Order.InitialRemainingTime;

            //TODO: how to verify if we have exactly the same amount of ingredients and images?
            for (var i = 0; i < Order.OrderData.ingredients.Count; i++)
            {
                ingredientImages[i].sprite = Order.OrderData.ingredients[i].sprite;
            }

            Order.OnDelivered += HandleDelivered;
            Order.OnAlertTime += HandleAlertTime;
            Order.OnExpired += HandleExpired;
            Order.OnUpdatedCountdown += HandleUpdatedCountdown;
        }

        private void HandleUpdatedCountdown(float remainingTime)
        {
            slider.value = remainingTime / InitialRemainingTime;
            //sliderFill.material.color = Color.Lerp(sliderFinalColor, sliderInitialColor, slider.value);
        }

        private void HandleDelivered(Order order)
        {
            HandleDeliveredAsync(order);
        }

        private async void HandleDeliveredAsync(Order order)
        {
            //TODO: play a positive feedback audio
            await RotateAlertBasePanelAsync(Color.green, null);
            SlideUp();
        }

        private void OnDestroy()
        {
            Order.OnDelivered -= HandleDelivered;
            Order.OnAlertTime -= HandleAlertTime;
            Order.OnExpired -= HandleExpired;
            Order.OnUpdatedCountdown -= HandleUpdatedCountdown;
        }

        public void SlideInAnimation(float desiredX)
        {
            CurrentAnchorX = desiredX;
            float initialSlideDuration = 1f;
            
            Vector2 small = new Vector2(0.8f, 1f);

            rootRectTransform
                .anchoredPositionTransition_X(desiredX, initialSlideDuration, LeanEase.Decelerate)
                .JoinTransition()
                .PlaySoundTransition(popAudio);
            
            basePanel.
                localRotationTransition(Quaternion.identity, initialSlideDuration, LeanEase.Decelerate)
                .JoinTransition()
                .localScaleTransition_XY(small, 0.125f, LeanEase.Elastic)
                .JoinTransition()
                .localScaleTransition_XY(Vector2.one, 0.125f, LeanEase.Smooth);

            bottomPanel
                .JoinDelayTransition(initialSlideDuration + 0.25f)
                .PlaySoundTransition(notificationAudio)
                .JoinTransition()
                .anchoredPositionTransition_Y(-75f, 0.3f, LeanEase.Bounce);
        }

        public void SlideLeft(float desiredX)
        {
            CurrentAnchorX = desiredX;
            float initialSlideDuration = 0.5f;
            
            // Vector2 small = new Vector2(0.8f, 1f);

            rootRectTransform
                .anchoredPositionTransition_X(desiredX, initialSlideDuration, LeanEase.Decelerate);
        }
         
        [ContextMenu("Start Shake")]
        public void StartShake()
        {
            _shake = true;
            ShakeAsync();
            //_shakeCoroutine = StartCoroutine(ShakeCoroutine());
        }
        
        private async Task ShakeAsync()
        {
            const float deltaX = 7f;

            while (_shake)
            {
                basePanel
                    .anchoredPositionTransition_X(-deltaX, 0.15f)
                    .JoinTransition()
                    .anchoredPositionTransition_X(deltaX, 0.3f)
                    .JoinTransition()
                    .anchoredPositionTransition_X(0, 0.15f)
                    .JoinTransition();
                await Task.Delay(ShakeIntervalTimeMs);
                // yield return _shakeWait;
            }

            basePanel.anchoredPositionTransition_X(0, 0.15f);
            //yield break;
        }

        [ContextMenu("StopShake")]
        public void StopShake()
        {
            _shake = false;
            // if (_shakeCoroutine != null)
            // {
            //     StopCoroutine(_shakeCoroutine);
            // }
        }
        
        private async Task RotateAlertBasePanelAsync(Color flickerColor, AudioClip audioClip = null)
        {
            const string colorProperty = "_Color";
            const float deltaZ = 10f;
            const int minorDelayMs = 200;
            const float minorDelaySeconds = minorDelayMs/1000f;
            
            basePanel
                .localRotationTransition(Quaternion.Euler(0f, 0f, deltaZ), minorDelaySeconds)
                .JoinTransition()
                .localRotationTransition(Quaternion.Euler(0f, 0f, -deltaZ), minorDelaySeconds)
                .JoinTransition()
                .localRotationTransition(Quaternion.Euler(0f, 0f, deltaZ), minorDelaySeconds)
                .JoinTransition()
                .localRotationTransition(Quaternion.Euler(0f, 0f, -deltaZ), minorDelaySeconds)
                .JoinTransition()
                .localRotationTransition(Quaternion.identity, .1f);
            
            _uiMaterial.colorTransition(colorProperty, flickerColor, minorDelaySeconds);
            await Task.Delay(minorDelayMs);
            _uiMaterial.colorTransition(colorProperty, Color.white, minorDelaySeconds);
            await Task.Delay(minorDelayMs);
            _uiMaterial.colorTransition(colorProperty, flickerColor, minorDelaySeconds);
            await Task.Delay(minorDelayMs);
            _uiMaterial.colorTransition(colorProperty, Color.white, minorDelaySeconds);
            await Task.Delay(minorDelayMs);

            if (audioClip != null)
            {
                basePanel.PlaySoundTransition(audioClip);
            }
            
        }
        
        [ContextMenu("SlideUp")]
        private void SlideUp()
        {
            const float deltaY = 400f;
            basePanel
                .localScaleTransition_XY(new Vector2(1.1f, 1.1f),.3f, LeanEase.Bounce)
                .JoinTransition()
                .localPositionTransition_Y(deltaY, .5f, LeanEase.Decelerate);
        }
        
    }
}