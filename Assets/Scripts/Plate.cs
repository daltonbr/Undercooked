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
        private readonly List<Ingredient> _ingredients = new List<Ingredient>(MaxNumberIngredients);

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

        public bool AddIngredient(Ingredient ingredient)
        {
            //TODO: not implemented
            UpdateIconsUI();
            return false;
        }

        public bool AddIngredients(List<Ingredient> ingredients)
        {
            // check for soup (3 equal cooked ingredients, mushroom, onion or tomato)
            if (!IsEmpty()) return false;
            
            _ingredients.AddRange(ingredients);
            foreach (var ingredient in _ingredients)
            {
                ingredient.transform.SetParent(slot);
                ingredient.transform.SetPositionAndRotation(slot.transform.position, Quaternion.identity);
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

        public void CleanIngredients()
        {
            _ingredients.Clear();
            DisableSoup();
            UpdateIconsUI();
        }

        private void UpdateIconsUI()
        {
            for (int i = 0; i < _ingredients.Count; i++)
            {
                if (_ingredients[i] == null)
                {
                    ingredientUISlots[i].enabled = false;
                }
                else
                {
                    ingredientUISlots[i].enabled = true;
                    ingredientUISlots[i].sprite = _ingredients[i].SpriteUI;
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
            
            SetClean();
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
                    //TODO: check if CookingPot finish cook and have a valid cook soup
                    break;
                case Ingredient ingredient:
                    Debug.Log("[Plate] Trying to dropping Ingredient into Plate! Not implemented yet");
                    break;
                case Plate plate:
                    // TODO: drop something from a plate into other plate! We basically swap contents
                    Debug.Log("[Plate] Trying to drop something from a plate into other plate! We basically swap contents");
                    if (plate.IsEmpty()) return false;
                    plate.AddIngredients(this._ingredients);
                    this.CleanIngredients();
                    return false;
                    break;
                default:
                    Debug.LogWarning("[Plate] Drop not recognized", this);
                    break;
            }
            // plates will receive pickables (processed ingredients), not when they are on the floor
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
                    // if (cookingPot.IsCookFinished == false || cookingPot.IsBurned) return null;
                    // List<Ingredient> ingredients = cookingPot.Pickables.Cast<Ingredient>().ToList();
                    // if (CheckSoupIngredients(ingredients))
                    // {
                    //     // soup is ready
                    //     //_pickables = ingredients as IPickable;
                    //     _ingredients.Clear();
                    //     _ingredients.AddRange(ingredients);
                    //     foreach (var ingredient in ingredients)
                    //     {
                    //         ingredient.transform.SetParent(slot);
                    //         ingredient.transform.SetPositionAndRotation(slot.position, Quaternion.identity);
                    //     }
                    //     EnableSoup(ingredients[0]);
                    //     return null;
                    // }
                    break;
                case Ingredient ingredient:
                    //TODO: we can pickup some ingredients into plate, not all
                    break;
                // we can swap plate ingredients
                case Plate plate:
                    Debug.Log("[Plate] Trying to pick from a plate with a plate", this);
                    if (plate.IsEmpty())
                    {
                        // the other plate is empty we can swap contents (if any)
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