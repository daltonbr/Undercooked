using UnityEngine;

namespace Undercooked
{
    public class Countertop : Interactable
    {

        public override bool TryToDropIntoSlot(IPickable pickable)
        {
            switch (pickable)
            {
                case Ingredient ingredient:
                    // we can filter ingredients here, by type/status.  For now we accept all
                    return TryDropIfNotOccupied(pickable);
                    break;
                case Plate plate:
                    // refuse plate on the Countertop, just a test for now
                    //TODO: allow plates later
                    Debug.Log("[Countertop] Refusing plate");
                    return false;
                    break;
                default:
                    Debug.LogWarning("[Countertop] IPickable not recognized. Refuse by default", this);
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

        private bool TryDropIfNotOccupied(IPickable pickable)
        {
            if (CurrentPickable != null)
            {
                Debug.Log("[Countertop] Try to drop into Countertop, but it's already occupied");
                return false;
            }
            CurrentPickable = pickable;
            CurrentPickable.gameObject.transform.SetParent(slot);
            CurrentPickable.gameObject.transform.SetPositionAndRotation(slot.position, Quaternion.identity);
            return true;
        }
    }
}
