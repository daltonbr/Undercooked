using Lean.Transition;
using Undercooked.Model;
using UnityEngine;
using UnityEngine.Assertions;

namespace Undercooked.Appliances
{
    public class DeliverCountertop : Interactable
    {

        [SerializeField] private ParticleSystem starParticle;
        [SerializeField] private AudioClip positiveFeedbackAudio;
        
        public delegate void PlateDropped(Plate plate);
        public static event PlateDropped OnPlateDropped;
        public delegate void PlateMissing();
        public static event PlateMissing OnPlateMissing;

        protected override void Awake()
        {
            base.Awake();
            
            #if UNITY_EDITOR
                Assert.IsNotNull(starParticle);
                Assert.IsNotNull(positiveFeedbackAudio);
            #endif
        }

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
                    OnPlateMissing?.Invoke();
                    return false;
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
