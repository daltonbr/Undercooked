using System.Collections;
using System.Collections.Generic;
using Lean.Transition;
using UnityEngine;
using UnityEngine.Assertions;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

namespace Undercooked
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
        // each time we add an ingredient, we raise the liquid level by this amount
        private const float LiquidHeightRaise = 0.12f;
        
        [Header("FX's")] 
        [SerializeField] private ParticleSystem whiteSmoke;
        [SerializeField] private ParticleSystem blackSmoke;
        [SerializeField] private AudioClip burnWarnCountdown;
        [SerializeField] private AudioClip beep;
        [SerializeField] private AudioClip fireBurning;

        private AudioSource _audioSource;
        
        // Timers
        private float _totalCookTime;
        private float _currentCookTime;
        
        // We hold a timer, based on the ingredients cookTime
        private float _currentBurnTime;

        private Coroutine _cookCoroutine;
        private Coroutine _burnCoroutine;
        
        private const int MaxNumberIngredients = 3;
        private readonly List<Ingredient> _ingredients = new List<Ingredient>(MaxNumberIngredients);

        // Visually all the soups are equal except for the floating icons on the plate
        [SerializeField] private Interactable _soupPrefab; // Ingredient? IPickable? or Dish?
        
        // Flags
        private bool _isBurned;
        private bool _isCookFinished;
        private bool _onHob;
        private bool _isCooking;
        private bool _inBurnProcess;
        
        private Rigidbody _rigidbody;
        private Collider _collider;

        public bool IsCookFinished => _isCookFinished;
        public bool IsBurned => _isBurned;

        //Dish Class? Status?
        //recipe concept? (Recipe ScriptableObject
        // e.g. Onion soup x 3 onion boiled
        // e.g. burger - chop bread, chop meat (+pan), optional: lettuce, tomato
        
        // [GameDesign]
        // if there is a mixed soup (e.g. 2x onions 1x tomato) we can't pickup the soup (it's locked),
        // the only option is to trash it
        // we only deliver single ingredient soups
        
        // finalized dish is a IPickable that can be put in a plate,
        // some can be put into a slot (or drop into floor) like a regular IPickable
        // others like soup can only be put in a plate.
        // for soup, we can drop the CookingPot into a plate
        // or PickUp from the CookingPot with an empty plate in hands
        
        // TODO: [N2H] Fire and FX's
        // fire
        // starting smoke fire (investigate more)
        
        protected override void Awake()
        {
            #if UNITY_EDITOR
            Assert.IsNotNull(ingredientUISlots);
            Assert.IsTrue(ingredientUISlots.Count == MaxNumberIngredients);
            #endif
            
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _audioSource = GetComponent<AudioSource>();
            Setup();
        }
        
        private void Setup()
        {
            // Rigidbody is kinematic almost all the time, except when we drop it on the floor
            // re-enabling when picked up.
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
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
                    break;
                case Plate plate:
                    Debug.Log("[CookingPot] Dragging a plate into CookingPot");
                    if (_isCookFinished && !_isBurned)
                    {
                        if (plate.IsEmpty() == false)
                        {
                            Debug.Log("[CookingPot] Plate is full");
                            //TODO:
                            // check if it's a soup
                            // then try to throw it back into the CookingPot
                            return false;
                        }

                        if (_isBurned)
                        {
                            Debug.Log("[CookingPot] Burned soup! Can only be thrash it away");
                            return false;
                        }
                        
                        if (plate.IsClean == false)
                        {
                            Debug.Log("[CookingPot] Plate is dirty");
                            return false;
                        }
                        
                        Debug.Log("[CookingPot] Plate is empty and clean");

                        bool isSoup = Plate.CheckSoupIngredients(this._ingredients);

                        if (isSoup)
                        {
                            plate.AddIngredients(this._ingredients);
                            EmptyPan();
                            return false;    
                        }
                        else
                        {
                            Debug.Log("[CookingPot] Mixed ingredients! Not a soup");
                            return false;
                        }
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
            //TODO: how to verify that user has a Plate?
            // we can only pick a soup when it's ready and Player has a Plate in hands, otherwise refuse

            if (!_isCookFinished || _isBurned) return null;
            
            //TODO: we also "lock" a soup if there are different ingredients. Player has to trash it away

            if (_ingredients[0].Type != _ingredients[1].Type ||
                _ingredients[1].Type != _ingredients[2].Type ||
                _ingredients[0].Type != _ingredients[2].Type)
            {
                Debug.Log("[CookingPot] Soup with mixed ingredients! You must thrash it away! What a waste!");
                return null;
            }

            if (!(playerHoldPickable is Plate plate)) return null;
            if (!plate.IsClean || !plate.IsEmpty()) return null;
                
            //TODO: what to do with the ingredients? Destroy or put into soup?
            //TODO: if we are swapping, we don't need to instantiate a soup again. 
            //TODO: Plate is now in charge of instantiating soupPrefab
            var soup = Instantiate(_soupPrefab, slot.transform.position, Quaternion.identity);
            plate.AddIngredients(_ingredients);

            Debug.Log("[CookingPot] Filling plate");
            EmptyPan();
            return null;
        }

        public void EmptyPan()
        {
            if (_cookCoroutine != null) StopCoroutine(_cookCoroutine);
            if (_burnCoroutine != null) StopCoroutine(_burnCoroutine);
            
            _ingredients.Clear();
            
            liquidSurface.localPosition = Vector3.zero;
            _currentCookTime = 0f;
            _currentBurnTime = 0f;
            _totalCookTime = 0f;
            _isBurned = false;
            _isCookFinished = false;
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
            Debug.Log("[CookingPot] We just have been dropped into fire!");
            _onHob = true;
            warningPopup.enabled = false;

            if (_ingredients.Count == 0 || _isBurned) return;
            
            // after cook, we burn
            if (_isCookFinished)
            {
                _burnCoroutine = StartCoroutine(Burn());
                Debug.Log("[CookingPot] Resume Burn");
                return;
            }

            // ...or restart cooking
            _cookCoroutine = StartCoroutine(Cook());
            Debug.Log("[CookingPot] Resume Cook");
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
            //_isBurned = true;
            _inBurnProcess = false;
            //_currentBurnTime = 0f;
            
            UpdateIngredientsUI();
        }
        
        public void RemovedFromHob()
        {
            // Debug.Log("[CookingPot] We just have been removed from fire!");
            // stop cooking process
            _onHob = false;
            warningPopup.enabled = false;;

            if (_inBurnProcess)
            {
                TryStopBurn();
                // Debug.Log($"[CookingPot] Stop burning at {_currentBurnTime}");
                return;
            }

            if (_isCooking)
            {
                TryStopCook();
                // Debug.Log($"[CookingPot] Stop cooking at {_currentCookTime}");
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
            
            if (_ingredients.Count == MaxNumberIngredients)
            {
                TriggerSuccessfulCook();
                yield break;
            }

            Debug.Log("[CookingPot] Finish partial cooking");
            // finish partial cooking
            
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
            _isBurned = true;
            _inBurnProcess = false;
            _currentBurnTime = 0f;
            
            UpdateIngredientsUI();
            
            whiteSmoke.gameObject.SetActive(false);
            blackSmoke.gameObject.SetActive(true);
            Debug.Log("[CookingPot] The food is burnt!");
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
            _isCookFinished = true;
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
            if (_ingredients.Count >= MaxNumberIngredients)
            {
                Debug.Log("[CookingPot] Try to drop into a full CookingPot");
                return false;
            }

            var ingredient = pickable as Ingredient;
            _ingredients.Add(ingredient);
            
            // add time to FinalCookTime
            _totalCookTime += ingredient.CookTime;
            
            // raise liquid level
            liquidSurface.localPosition += new Vector3(0f, LiquidHeightRaise, 0f);
            liquidMaterial.color = ingredient.BaseColor;
            
            // hide ingredient mesh
            ingredient.SetMeshRendererEnabled(false);
            ingredient.gameObject.transform.SetParent(slot);
            
            // reset burnProcess, if any
            _currentBurnTime = 0f;
            if (_inBurnProcess && _burnCoroutine != null)
            {
                Debug.Log("[CookingPot] Stop burning");
                //StopCoroutine(_burnCoroutine);
                TryStopBurn();
                // resume cooking, because we are already burning
                _cookCoroutine = StartCoroutine(Cook());
                return true;
            }
            
            // (re)start cooking?
            if (_onHob && !_isCooking)
            {
                _cookCoroutine = StartCoroutine(Cook());
            }
            
            Debug.Log("[CookingPot] Added ingredient into CookingPot");
            
            UpdateIngredientsUI();
            return true;
        }

        private void UpdateIngredientsUI()
        {
            ingredientUISlots[1].enabled = !_isBurned;
            ingredientUISlots[2].enabled = !_isBurned;
            if (_isBurned)
            {
                ingredientUISlots[0].sprite = burnIcon;
                return;
            }

            for (int i = 0; i < MaxNumberIngredients; i++)
            {
                if (i < _ingredients.Count)
                {
                    ingredientUISlots[i].sprite = _ingredients[i] == null ? plusIcon : _ingredients[i].SpriteUI;
                }
                else
                {
                    ingredientUISlots[i].sprite = plusIcon;
                }
            }
        }
        
    } 
}
