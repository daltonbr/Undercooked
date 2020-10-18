using UnityEngine;

namespace Undercooked
{
    public class Hob : Interactable
    {
        public override bool TryToDropIntoSlot(IPickable pickable)
        {
            switch (pickable)
            {
                case Ingredient ingredient:
                    return false;
                    break;
                case Plate plate:
                    //TODO: remove this later
                    return TryDropIfNotOccupied(pickable);
                    break;
                //TODO: receive only Pan's
                default:
                    Debug.LogWarning("[Hob] IPickable not recognized. Refuse by default", this);
                    return false;
            }
        }

        public override IPickable TryToPickUpFromSlot()
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
            return true;
        }
    }
}
