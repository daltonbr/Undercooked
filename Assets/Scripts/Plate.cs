using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Undercooked
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Plate : Interactable, IPickable
    {
        [SerializeField] private List<Image> ingredientUISlots;
        
        [SerializeField] private Material cleanMaterial;
        [SerializeField] private Material dirtyMaterial;
        [SerializeField] private MeshRenderer _meshRenderer;
        
        [SerializeField] private Transform _soup;
        private Material _soupMaterial;
        public bool IsClean { get; private set; }

        private Rigidbody _rigidbody;
        private Collider _collider;

        private const int MaxNumberIngredients = 4; 
        private List<Ingredient> _ingredients = new List<Ingredient>(MaxNumberIngredients);

        public List<Ingredient> Ingredients => _ingredients;
        public override bool IsEmpty() =>_ingredients.Count == 0;

        protected override void Awake()
        {
#if UNITY_EDITOR
            Assert.IsNotNull(ingredientUISlots);
            Assert.IsTrue(ingredientUISlots.Count == MaxNumberIngredients);
#endif
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            Setup();
        }

        public bool AddIngredients(List<Ingredient> ingredients)
        {
            // check for soup (3 equal cooked ingredients, mushroom, onion or tomato)
            if (!IsEmpty()) return false;
            _ingredients.AddRange(ingredients);
            
            foreach (var ingredient in _ingredients)
            {
                ingredient.transform.SetParent(Slot);
                ingredient.transform.SetPositionAndRotation(Slot.transform.position, Quaternion.identity);
            }
            UpdateIconsUI();
            
            if (CheckSoupIngredients(ingredients))
            {
                EnableSoup(ingredients[0]);
            }
            else
            {
                //it's not a soup
            }
            return true;
        }
        
        public void RemoveAllIngredients()
        {
            DisableSoup();
            _ingredients.Clear();
            UpdateIconsUI();
        }

        private void UpdateIconsUI()
        {
            for (int i = 0; i < ingredientUISlots.Count; i++)
            {
                if (i < _ingredients.Count && _ingredients[i] != null)
                {
                    ingredientUISlots[i].enabled = true;
                    ingredientUISlots[i].sprite = _ingredients[i].SpriteUI;
                }
                else
                {
                    ingredientUISlots[i].enabled = false;    
                }
            }
        }
        
        /// <summary>
        /// Check for exactly 3 equals ingredients, being onions, tomatos or mushrooms.
        /// </summary>
        public static bool CheckSoupIngredients(IReadOnlyList<Ingredient> ingredients)
        {
            if (ingredients == null || ingredients.Count != 3)
            {
                return false;
            }

            if (ingredients[0].Type != IngredientType.Onion &&
                ingredients[0].Type != IngredientType.Tomato &&
                ingredients[0].Type != IngredientType.Mushroom)
            {
                Debug.Log("[Plate] Soup only must contain onion, tomato or mushroom");
                return false;
            }
            
            if (ingredients[0].Type != ingredients[1].Type ||
                ingredients[1].Type != ingredients[2].Type ||
                ingredients[0].Type != ingredients[2].Type)
            {
                Debug.Log("[Plate] Soup with mixed ingredients! You must thrash it away! What a waste!");
                return false;
            }
            
            return true;
        }
        
        private void EnableSoup(in Ingredient ingredientSample)
        {
            _soup.gameObject.SetActive(true); 
            _soupMaterial.color = ingredientSample.BaseColor;
        }

        private void DisableSoup()
        {
            _soup.gameObject.SetActive(false);
        }

        private void Setup()
        {
            // Rigidbody is kinematic almost all the time, except when we drop it on the floor
            // re-enabling when picked up.
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
            _soupMaterial = _soup.gameObject.GetComponent<MeshRenderer>()?.material;
            
            #if UNITY_EDITOR
            Assert.IsNotNull(_soupMaterial);
            #endif

            if (IsClean)
            {
                SetClean();
            }
            else
            {
                SetDirty();
            }
            
            DisableSoup();
        }

        [ContextMenu("SetClean")]
        public void SetClean()
        {
            _meshRenderer.material = cleanMaterial;
            IsClean = true;
        }
        
        [ContextMenu("SetDirty")]
        public void SetDirty()
        {
            _meshRenderer.material = dirtyMaterial;
            IsClean = false;
            DisableSoup();
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
        
        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            // player has empty hands - doesn't make sense
            if (pickableToDrop == null)
            {
                Debug.LogWarning("[Plate] Nothing to drop.", this);
            }
            
            //we can drop soup from plate to plate AND from plate to CookingPot (and viceversa)
            switch (pickableToDrop)
            {
                case CookingPot cookingPot:
                    Debug.Log("[Plate] Dropping CookingPot into Plate");
                    if (cookingPot.IsCookFinished &&
                        cookingPot.IsBurned == false &&
                        CheckSoupIngredients(cookingPot.Ingredients))
                    {
                        AddIngredients(cookingPot.Ingredients);
                        cookingPot.EmptyPan();
                        return false;
                    }
                    break;
                case Ingredient ingredient:
                    Debug.Log("[Plate] Trying to dropping Ingredient into Plate! Not implemented yet");
                    break;
                case Plate plate:
                    //Debug.Log("[Plate] Trying to drop something from a plate into other plate! We basically swap contents");
                    if (this.IsEmpty() == false || this.IsClean == false) return false;
                    this.AddIngredients(plate.Ingredients);
                    plate.RemoveAllIngredients();
                    return false;
                default:
                    Debug.LogWarning("[Plate] Drop not recognized", this);
                    break;
            }
            return false;
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            // We can pickup Ingredients from plates with other plates (effectively swapping content) or from Pans
            
            // in order to pick from a plate slot, we must have something in hands
            if (playerHoldPickable == null)
            {
                return null;
            }
            
            // can we pick something from the plate slot?
            switch (playerHoldPickable)
            {
                // we just pick the soup ingredients, not the CookingPot itself
                case CookingPot cookingPot:
                    Debug.Log("[Plate] Trying to pick from a plate with a CookingPot", this);
                    break;
                case Ingredient ingredient:
                    //TODO: we can pickup some ingredients into plate, not all
                    break;
                // we can swap plate ingredients
                case Plate plate:
                    Debug.Log("[Plate] Trying to pick from a plate with a plate", this);
                    if (plate.IsEmpty())
                    {
                        if (this.IsEmpty())
                        {
                            Debug.Log("[Plate] Trying to pick something from a empty plate! No effect", this);       
                            return null;    
                        }
                        // swap
                        plate.AddIngredients(this._ingredients);

                    }
                    break;
                default:
                    Debug.LogWarning("[Plate] Pickable not recognized", this);
                    break;
            }
            
            //TODO: we can if we implement pick from another plate (see TryToDropIntoSlot above) 
            return null;
        }
    }
}
