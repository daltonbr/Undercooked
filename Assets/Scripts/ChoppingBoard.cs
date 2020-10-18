using UnityEngine;

namespace Undercooked
{
    public class ChoppingBoard : Interactable
    {
        [SerializeField] private Transform knife;
        
        public override bool TryToDropIntoSlot(IPickable pickable)
        {
            switch (pickable)
            {
                case Ingredient ingredient:
                    return TryDropIfNotOccupied(pickable);
                    break;
                default:
                    Debug.LogWarning("[ChoppingBoard] Refuse everything that is not raw ingredient");
                    return false;
            }
        }

        public override IPickable TryToPickUpFromSlot()
        {
            //TODO: only allow Pickup after we finish chopping the ingredient. Essentially locking it in place.
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
                knife.gameObject.SetActive(true);
                return output;
            }
        }
        
        private bool TryDropIfNotOccupied(IPickable pickable)
        {
            if (CurrentPickable != null)
            {
                Debug.Log("[ChoppingBoard] Try to drop into ChoppingBoard, but it's already occupied");
                return false;
            }
            CurrentPickable = pickable;
            CurrentPickable.gameObject.transform.SetParent(slot);
            CurrentPickable.gameObject.transform.SetPositionAndRotation(slot.position, Quaternion.identity);
            knife.gameObject.SetActive(false);
            return true;
        }
    }
}
