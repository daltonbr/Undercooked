using Undercooked.Model;
using UnityEngine;

namespace Undercooked.Appliances
{
    public class Hob : Interactable
    {
        private void Start()
        {
            var pan = CurrentPickable as CookingPot;
            pan?.DroppedIntoHob();
        }

        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            if (CurrentPickable != null)
            {
                if (CurrentPickable is CookingPot cookingPot) return cookingPot.TryToDropIntoSlot(pickableToDrop);
                return false;
            }

            if (pickableToDrop is CookingPot cookingPot1) return TryDropIfNotOccupied(pickableToDrop);
            return false;
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            if (CurrentPickable == null) return null;

            var output = CurrentPickable;
            var interactable = CurrentPickable as Interactable;
            interactable?.ToggleHighlightOff();
                
            // stop cooking process
            var pan = CurrentPickable as CookingPot;
            pan?.RemovedFromHob();
                
            CurrentPickable = null;
            return output;
        }
        
        private bool TryDropIfNotOccupied(IPickable pickable)
        {
            if (CurrentPickable != null) return false;
            
            CurrentPickable = pickable;
            CurrentPickable.gameObject.transform.SetParent(Slot);
            CurrentPickable.gameObject.transform.SetPositionAndRotation(Slot.position, Quaternion.identity);
            
            // start cooking
            var pan = CurrentPickable as CookingPot;
            pan?.DroppedIntoHob();
            return true;
        }
    }
}
