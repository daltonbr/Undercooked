using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lean.Transition;
using Undercooked.Managers;
using Undercooked.Model;
using UnityEngine;
using UnityEngine.Assertions;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

namespace Undercooked.Appliances
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class CookingPot : Interactable, IPickable
    {
        [Header("UI")]
        [SerializeField] private Slider slider;
        [SerializeField] private List<Image> ingredientUISlots;
        [Tooltip("green popup when finished cooking, red alert for when it's about to burn")]
        [SerializeField] private Image warningPopup;
        [SerializeField] private Image greenCheckPopup;
        [SerializeField] private Sprite plusIcon;
        [SerializeField] private Sprite burnIcon;
        
        [Header("Liquid")]
        [SerializeField] private Transform liquidSurface;
        [SerializeField] private Material liquidMaterial;
        [SerializeField] private Color burnLiquid;
        // each ingredient raise the liquid level by this amount
        private const float LiquidHeightRaise = 0.12f;
        
        [Header("FX's")] 
        [SerializeField] private ParticleSystem whiteSmoke;
        [SerializeField] private ParticleSystem blackSmoke;
        [SerializeField] private AudioClip burnWarnCountdown;
        [SerializeField] private AudioClip beep;
        [SerializeField] private AudioClip fireBurning;

        // Timers
        private float _totalCookTime;
        private float _currentCookTime;
        
        // We hold a timer, based on the ingredients cookTime
        private float _currentBurnTime;

        private Coroutine _cookCoroutine;
        private Coroutine _burnCoroutine;
        
        private const int MaxNumberIngredients = 3;

        // Flags
        private bool _onHob;
        private bool _isCooking;
        private bool _inBurnProcess;
        
        private Rigidbody _rigidbody;
        private Collider _collider;
        private CanvasGroup _canvasGroup;

        public bool IsCookFinished { get; private set; }
        public bool IsBurned { get; private set; }
        public bool IsEmpty() =>Ingredients.Count == 0;
        public List<Ingredient> Ingredients { get; } = new List<Ingredient>(MaxNumberIngredients);

        // [GameDesign]
        // if there is a mixed soup (e.g. 2x onions 1x tomato) we can't pickup the soup (it's locked),
        // the only option is to trash it
        // we only deliver single ingredient soups
        
        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _canvasGroup = GetComponent<CanvasGroup>();
            
            #if UNITY_EDITOR
                Assert.IsNotNull(_canvasGroup);
                Assert.IsNotNull(ingredientUISlots);
                Assert.IsTrue(ingredientUISlots.Count == MaxNumberIngredients);
            #endif
            
            Setup();
        }
        
        private void Setup()
        {
            // Rigidbody is kinematic almost all the time, except when we drop it on the floor
            // re-enabling when picked up.
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
            _canvasGroup.alpha = 0f;
        }
        
        private void OnEnable()
        {
            GameManager.OnLevelStart += HandleLevelStart;
            GameManager.OnTimeIsOver += HandleTimeOver;
        }

        private void OnDisable()
        {
            GameManager.OnLevelStart -= HandleLevelStart;
            GameManager.OnTimeIsOver -= HandleTimeOver;
        }
        
        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            switch (pickableToDrop)
            {
                case Ingredient ingredient:
                    if (ingredient.Status != IngredientStatus.Processed)
                    {
                        Debug.Log("[CookingPot] Only accept chopped/processed ingredients");
                        return false;
                    }
                    if (ingredient.Type == IngredientType.Onion ||
                        ingredient.Type == IngredientType.Tomato ||
                        ingredient.Type == IngredientType.Mushroom)
                    {
                        return TryDrop(pickableToDrop);    
                    }
                    Debug.Log("[CookingPot] Only accept Onions, tomatoes or Mushrooms");
                    return false;
                case Plate plate:
                    
                    if (IsEmpty())
                    {
                        if (plate.IsEmpty() == false && Plate.CheckSoupIngredients(plate.Ingredients))
                        {
                            // Drop soup back into CookingPot
                            TryAddIngredients(plate.Ingredients);
                            plate.RemoveAllIngredients();
                            return false;
                        }
                    }
                    
                    if (IsCookFinished && !IsBurned)
                    {
                        if (IsBurned) return false;
                        if (plate.IsClean == false) return false;
                        
                        bool isSoup = Plate.CheckSoupIngredients(this.Ingredients);

                        if (!isSoup) return false;
                        
                        plate.AddIngredients(this.Ingredients);
                        EmptyPan();
                        return false;
                    }
                    break;
                
                default:
                    Debug.LogWarning("[ChoppingBoard] Refuse everything else");
                    return false;
            }
            return false;
        }
        
        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            // we can only pick a soup when it's ready and Player has a Plate in hands, otherwise refuse
            if (!IsCookFinished || IsBurned) return null;
            
            // we "lock" a soup if there are different ingredients. Player has to trash it away
            if (Ingredients[0].Type != Ingredients[1].Type ||
                Ingredients[1].Type != Ingredients[2].Type ||
                Ingredients[0].Type != Ingredients[2].Type)
            {
                // Debug.Log("[CookingPot] Soup with mixed ingredients! You must thrash it away! What a waste!");
                return null;
            }

            if (!(playerHoldPickable is Plate plate)) return null;
            if (!plate.IsClean || !plate.IsEmpty()) return null;
            
            plate.AddIngredients(Ingredients);
            EmptyPan();
            return null;
        }

        private bool TryAddIngredients(List<Ingredient> ingredients)
        {
            if (!IsEmpty()) return false;
            if (Plate.CheckSoupIngredients(ingredients) == false) return false;
            Ingredients.AddRange(ingredients);
            
            foreach (var ingredient in Ingredients)
            {
                ingredient.transform.SetParent(Slot);
                ingredient.transform.SetPositionAndRotation(Slot.transform.position, Quaternion.identity);
            }
            
            SetLiquidLevelAndColor();
            
            IsCookFinished = true;
            UpdateIngredientsUI();
            return true;
        }

        public void EmptyPan()
        {
            if (_cookCoroutine != null) StopCoroutine(_cookCoroutine);
            if (_burnCoroutine != null) StopCoroutine(_burnCoroutine);
            
            slider.gameObject.SetActive(false);
            Ingredients.Clear();
            
            liquidSurface.localPosition = Vector3.zero;
            _currentCookTime = 0f;
            _currentBurnTime = 0f;
            _totalCookTime = 0f;
            IsBurned = false;
            IsCookFinished = false;
            _isCooking = false;
            warningPopup.transform.localScale = Vector3.zero;
            greenCheckPopup.transform.localScale = Vector3.zero;
            
            UpdateIngredientsUI();
            
            // deactivate FX's
            whiteSmoke.gameObject.SetActive(false);
            blackSmoke.gameObject.SetActive(false);
        }

        public void DroppedIntoHob()
        {
            _onHob = true;
            warningPopup.enabled = false;

            if (Ingredients.Count == 0 || IsBurned) return;
            
            // after cook, we burn
            if (IsCookFinished)
            {
                _burnCoroutine = StartCoroutine(Burn());
                return;
            }

            // or restart cooking
            _cookCoroutine = StartCoroutine(Cook());
        }

        private void TryStopCook()
        {
            if (_cookCoroutine == null) return;
            
            StopCoroutine(_cookCoroutine);
            _isCooking = false;
        }
        
        private void TryStopBurn()
        {
            if (_burnCoroutine == null) return;
            
            StopCoroutine(_burnCoroutine);
                
            warningPopup.enabled = false;
            greenCheckPopup.enabled = false;
            _inBurnProcess = false;

            UpdateIngredientsUI();
        }
        
        public void RemovedFromHob()
        {
            _onHob = false;
            warningPopup.enabled = false;;

            if (_inBurnProcess)
            {
                TryStopBurn();
                return;
            }

            if (_isCooking)
            {
                TryStopCook();
            }
        }

        private IEnumerator Cook()
        {
            _isCooking = true;
            slider.gameObject.SetActive(true);

            while (_currentCookTime < _totalCookTime)
            {
                slider.value = _currentCookTime / _totalCookTime;
                _currentCookTime += Time.deltaTime;
                yield return null;
            }

            _isCooking = false;
            
            if (Ingredients.Count == MaxNumberIngredients)
            {
                TriggerSuccessfulCook();
                yield break;
            }

            // Debug.Log("[CookingPot] Finish partial cooking");
            
            _burnCoroutine = StartCoroutine(Burn());
        }

        private IEnumerator Burn()
        {
            float[] timeLine = {0f, 4f, 6.3f, 9.3f, 12.3f, 13.3f};
            
            _inBurnProcess = true;
            slider.gameObject.SetActive(false);

            // GreenCheck
            if (_currentBurnTime <= timeLine[0])
            {
                AnimateGreenCheck();    
            }
            whiteSmoke.gameObject.SetActive(true);
            
            while (_currentBurnTime < timeLine[1])
            {
                _currentBurnTime += Time.deltaTime;
                yield return null;
            }
            
            warningPopup.enabled = true;

            var internalCount = 0f;
            
            // pulsating at 2Hz
            while (_currentBurnTime < timeLine[2])
            {
                _currentBurnTime += Time.deltaTime;
                internalCount += Time.deltaTime;
                if (internalCount > 0.5f)
                {
                    internalCount = 0f;
                    PulseAndBeep(1.15f);
                }
                yield return null;
            }
            
            // pulsating at 5Hz
            while (_currentBurnTime < timeLine[3])
            {
                _currentBurnTime += Time.deltaTime;
                internalCount += Time.deltaTime;
                if (internalCount > 0.2f)
                {
                    internalCount = 0f;
                    PulseAndBeep(1.25f);
                }
                yield return null;
            }

            var initialColor = liquidMaterial.color;
            var delta = timeLine[4] - timeLine[3];
            
            // pulsating at 10Hz
            while (_currentBurnTime < timeLine[4])
            {
                
                var interpolate = (_currentBurnTime - timeLine[3]) / delta;
                liquidMaterial.color = Color.Lerp(initialColor, burnLiquid, interpolate);
                _currentBurnTime += Time.deltaTime;
                internalCount += Time.deltaTime;
                if (internalCount > 0.1f)
                {
                    internalCount = 0f;
                    PulseAndBeep(1.35f);
                }
                yield return null;
            }
            
            // FX's
            warningPopup.enabled = false;
            IsBurned = true;
            _inBurnProcess = false;
            _currentBurnTime = 0f;
            
            UpdateIngredientsUI();
            
            whiteSmoke.gameObject.SetActive(false);
            blackSmoke.gameObject.SetActive(true);
             Debug.Log("[CookingPot] The food is burned!");
        }
        
        private void AnimateGreenCheck()
        {
            greenCheckPopup.transform
                .localScaleTransition(Vector3.zero, .25f)
                .localScaleTransition(Vector3.one, .25f)
                .JoinDelayTransition(2.0f)
                .localScaleTransition(Vector3.zero, .25f);
        }

       private void PulseAndBeep(float intensity = 1.1f)
        {
            Vector3 pulseVector = new Vector3(intensity, intensity, intensity);

            warningPopup.transform
                .PlaySoundTransition(beep)
                .localScaleTransition(pulseVector, .03f, LeanEase.Accelerate)
                .JoinDelayTransition(.04f)
                .localScaleTransition(Vector3.one, 0.03f, LeanEase.Accelerate);
        }
       
        private void TriggerSuccessfulCook()
        {
            IsCookFinished = true;
            _currentCookTime = 0f;
            _burnCoroutine = StartCoroutine(Burn());
        }
        
        public void Pick()
        {
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
        }

        public void Drop()
        {
            gameObject.transform.SetParent(null);
            _rigidbody.isKinematic = false;
            _collider.enabled = true;
        }

        private bool TryDrop(IPickable pickable)
        {
            if (Ingredients.Count >= MaxNumberIngredients) return false;

            var ingredient = pickable as Ingredient;
            if (ingredient == null)
            {
                Debug.LogWarning("[CookingPot] Can only drop ingredients into CookingPot", this);
                return false;
            }
            
            Ingredients.Add(ingredient);
            
            _totalCookTime += ingredient.CookTime;
            
            SetLiquidLevelAndColor();

            // hide ingredient mesh
            ingredient.SetMeshRendererEnabled(false);
            ingredient.gameObject.transform.SetParent(Slot);
            
            // reset burnProcess, if any
            _currentBurnTime = 0f;
            if (_inBurnProcess && _burnCoroutine != null)
            {
                TryStopBurn();
                // resume cooking, because we are already burning
                _cookCoroutine = StartCoroutine(Cook());
                return true;
            }
            
            // (re)start cooking
            if (_onHob && !_isCooking)
            {
                _cookCoroutine = StartCoroutine(Cook());
            }

            UpdateIngredientsUI();
            return true;
        }

        private void SetLiquidLevelAndColor()
        {
            var ingredientCount = Ingredients.Count;
            liquidSurface.localPosition = new Vector3(0f, LiquidHeightRaise * ingredientCount, 0f);
            liquidMaterial.color = Ingredients.Last().BaseColor;
        }

        private void UpdateIngredientsUI()
        {
            ingredientUISlots[1].enabled = !IsBurned;
            ingredientUISlots[2].enabled = !IsBurned;
            if (IsBurned)
            {
                ingredientUISlots[0].sprite = burnIcon;
                return;
            }

            for (int i = 0; i < MaxNumberIngredients; i++)
            {
                if (i < Ingredients.Count)
                {
                    ingredientUISlots[i].sprite = Ingredients[i] == null ? plusIcon : Ingredients[i].SpriteUI;
                }
                else
                {
                    ingredientUISlots[i].sprite = plusIcon;
                }
            }
        }
        
        private void HandleLevelStart()
        {
            _canvasGroup.alphaTransition(1f, 1f);
        }

        private void HandleTimeOver()
        {
            _canvasGroup.alphaTransition(0f, 1f);
        }
        
    } 
}
