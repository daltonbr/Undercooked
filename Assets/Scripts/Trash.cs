using System.Collections;
using UnityEngine;

namespace Undercooked
{
    public class Trash : Interactable
    {
        private const float TotalAnimTime = 1f;
        private const float AngularSpeed = 1f;
        
        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            switch (pickableToDrop)
            {
                case CookingPot cookingPot:
                    cookingPot.EmptyPan();
                    break;
                case Ingredient ingredient:
                    StartCoroutine(AnimateAndDestroy(ingredient));
                    return true;
                case Plate plate:
                    if (plate.IsEmpty()) return false;
                    plate.RemoveAllIngredients();
                    return false;
            }
            return false;
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable) => null;

        private IEnumerator AnimateAndDestroy(IPickable pickable)
        {
            if (pickable == null) yield break;

            // put pickable into slot
            pickable.gameObject.transform.SetParent(Slot);
            pickable.gameObject.transform.SetPositionAndRotation(Slot.position, Quaternion.identity);
            
            float currentTime = 0f;

            while (currentTime < TotalAnimTime)
            {
                // rotate and shrink pickable
                pickable.gameObject.transform.SetParent(null);
                pickable.gameObject.transform.Rotate(0f, AngularSpeed, 0f, Space.Self);
                pickable.gameObject.transform.localScale =
                    Vector3.Lerp(Vector3.one, Vector3.zero, currentTime / TotalAnimTime);
                currentTime += Time.deltaTime;
                yield return null;
            }

            Destroy(pickable.gameObject);
        }
    }
}
