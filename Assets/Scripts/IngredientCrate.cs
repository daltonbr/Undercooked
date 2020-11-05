using UnityEngine;

namespace Undercooked
{
    public class IngredientCrate : Interactable
    { 
        [SerializeField] private Ingredient ingredientPrefab;
        [SerializeField] private Animator animator;
        private static readonly int OpenHash = Animator.StringToHash("Open");
        
        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            if (CurrentPickable != null) return false;
            
            CurrentPickable = pickableToDrop;
            CurrentPickable.gameObject.transform.SetParent(Slot);
            pickableToDrop.gameObject.transform.SetPositionAndRotation(Slot.position, Quaternion.identity);
            return true;
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            if (CurrentPickable == null)
            {
                animator.SetTrigger(OpenHash);
                return Instantiate(ingredientPrefab, Slot.transform.position, Quaternion.identity);
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
