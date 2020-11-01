using UnityEngine;

namespace Undercooked
{
    public class Countertop : Interactable
    {

        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            if (CurrentPickable != null)
            {
                switch (CurrentPickable)
                {
                    case CookingPot cookingPot:
                        Debug.Log("[Countertop] Drop into CookingPot(on Countertop)");
                        return cookingPot.TryToDropIntoSlot(pickableToDrop);
                        break;
                    case Ingredient ingredient:
                        Debug.Log("[Countertop] Try to drop into Ingredient(on Countertop)");
                        return ingredient.TryToDropIntoSlot(pickableToDrop);
                        break;
                    case Plate plate:
                        Debug.Log("[Countertop] Drop into Plate(on Countertop)");
                        return plate.TryToDropIntoSlot(pickableToDrop);
                        break;
                    default:
                        Debug.Log("[Countertop] Trying tp drop into an unrecognized item in Countertop");
                        break;
                }
                return false;
            }
            
            switch (pickableToDrop)
            {
                case Ingredient ingredient:
                    // we can filter ingredients here, by type/status.  For now we accept all
                    return TryDropIfNotOccupied(pickableToDrop);
                    break;
                case Plate plate:
                    return TryDropIfNotOccupied(pickableToDrop);
                    break;
                case CookingPot cookingPot:
                    return TryDropIfNotOccupied(pickableToDrop);
                    break;
                case Extinguisher extinguisher:
                    return TryDropIfNotOccupied(pickableToDrop);
                default:
                    Debug.LogWarning("[Countertop] IPickable not recognized. Refuse by default", this);
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
                CurrentPickable = null;
                return output;
            }
        }

        private bool TryDropIfNotOccupied(IPickable pickable)
        {
            if (CurrentPickable != null)
            {
                Debug.Log("[Countertop] Try to drop into Countertop, but it's already occupied");
                return false;
            }
            CurrentPickable = pickable;
            CurrentPickable.gameObject.transform.SetParent(Slot);
            CurrentPickable.gameObject.transform.SetPositionAndRotation(Slot.position, Quaternion.identity);
            return true;
        }
    }
}
