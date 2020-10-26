using System;
using UnityEngine;

namespace Undercooked
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
                switch (CurrentPickable)
                {
                    case CookingPot cookingPot:
                        Debug.Log("[Hob] Try drop cookingPot into Hob");
                        return cookingPot.TryToDropIntoSlot(pickableToDrop);
                        break;
                    case Ingredient ingredient:
                        break;
                    case Plate plate:
                        break;
                    default:
                        Debug.Log("[Hob] Trying tp drop into an unrecognized item in Hob");
                        break;
                }
                return false;
            }
            // if (CurrentPickable != null)
            // {
            //     var pan = CurrentPickable as CookingPot;
            //     if (pan != null)
            //     {
            //         return pan.TryToDropIntoSlot(pickableToDrop);
            //     }
            // }
            
            switch (pickableToDrop)
            {
                case CookingPot cookingPot:
                    //TODO: accept all types of Pans, like Frying pans
                    return TryDropIfNotOccupied(pickableToDrop);
                    break;
                default:
                    Debug.LogWarning("[Hob] IPickable not accepted. Refuse by default", this);
                    return false;
            }
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            if (CurrentPickable == null)
            {
                return null;
            }
            else
            {
                var output = CurrentPickable;
                var interactable = CurrentPickable as Interactable;
                interactable?.ToggleHighlightOff();
                
                // stop cooking process
                var pan = CurrentPickable as CookingPot;
                pan?.RemovedFromHob();
                
                CurrentPickable = null;
                return output;
            }
        }
        
        //TODO: convert this method to a Interactable method? Since it equals most of the time
        private bool TryDropIfNotOccupied(IPickable pickable)
        {
            if (CurrentPickable != null)
            {
                Debug.Log("[Hob] Try to drop into Hob, but it's already occupied");
                return false;
            }
            CurrentPickable = pickable;
            CurrentPickable.gameObject.transform.SetParent(slot);
            CurrentPickable.gameObject.transform.SetPositionAndRotation(slot.position, Quaternion.identity);
            
            // start cooking
            var pan = CurrentPickable as CookingPot;
            pan?.DroppedIntoHob();
            return true;
        }
    }
}
