using UnityEngine;

namespace Undercooked
{
    public class IngredientCrate : Interactable
    { 
        //TODO: how to display the prefab onto the crate? Probably a texture into Ingredient Data Scriptable Object
        
        [SerializeField] private Ingredient ingredientPrefab;
        private Animator _animator;
        private static readonly int OpenHash = Animator.StringToHash("Open");
        private IngredientType IngredientType => ingredientPrefab.Type;

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
        }

        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            Debug.Log($"[IngredientCrate] Try to drop {pickableToDrop.gameObject.name} into {this.gameObject.name}");

            // it's empty, player can drop something here
            if (CurrentPickable == null)
            {
                CurrentPickable = pickableToDrop;
                CurrentPickable.gameObject.transform.SetParent(slot);
                pickableToDrop.gameObject.transform.SetPositionAndRotation(slot.position, Quaternion.identity);
                return true;
            }
            else
            {
                // we are occupied
                return false;
            }
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            if (CurrentPickable == null)
            {
                _animator.SetTrigger(OpenHash);
                return Instantiate(ingredientPrefab, slot.transform.position, Quaternion.identity);
            }
            else
            {
                var output = CurrentPickable;
                var interactable = CurrentPickable as Interactable;
                interactable?.ToggleHighlightOff();
                CurrentPickable = null;
                return output;
            }
        }

        public override void Interact()
        {
            base.Interact();
            Debug.Log($"[IngredientCrate] Interact with {gameObject.name}");
        }
    }
}
