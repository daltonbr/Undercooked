using Lean.Transition;
using UnityEngine;

namespace Undercooked
{
    public class DeliverCountertop : Interactable
    {

        [SerializeField] private ParticleSystem starParticle;
        [SerializeField] private AudioClip positiveFeedbackAudio;
        public delegate void PlateDropped(Plate plate);
        public static event PlateDropped OnPlateDropped;
        public delegate void PlateMissing();
        public static event PlateMissing OnPlateMissing;
        
        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            if (pickableToDrop == null) return false;
            
            switch (pickableToDrop)
            {
                case Plate plate:
                    plate.transform.SetParent(null);
                    plate.transform.SetPositionAndRotation(Slot.position, Quaternion.identity);
                    OnPlateDropped?.Invoke(plate);
                    PlayPositiveFeedback();
                    // move the plate out-of sight
                    plate.transform.position = new Vector3(10000f, 10000f, 10000f);
                    return true;
                case Ingredient ingredient:
                    Debug.Log("[DeliverCountertop] Need plate.");
                    OnPlateMissing?.Invoke();
                    return false;
                default:
                    Debug.Log($"[DeliverCountertop] Dropping {pickableToDrop.gameObject.name} isn't accepted.");
                    break;
            }
            return false;
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable) => null;
        
        private void PlayPositiveFeedback()
        {
            starParticle.Play();
            starParticle.PlaySoundTransition(positiveFeedbackAudio);
        }
    }
}
