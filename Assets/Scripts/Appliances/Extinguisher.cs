using Undercooked.Model;
using Undercooked.Player;
using UnityEngine;
using UnityEngine.Assertions;

namespace Undercooked.Appliances
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Extinguisher : Interactable, IPickable
    {
        [SerializeField] private ParticleSystem smoke;
        [SerializeField] private AudioClip wooshClip;
        
        private Rigidbody _rigidbody;
        private Collider _collider;
        
        protected override void Awake()
        {
            base.Awake();
            
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            
            #if UNITY_EDITOR
                //Assert.IsNotNull(smoke);
                //Assert.IsNotNull(wooshClip);    
                Assert.IsNotNull(_rigidbody);
                Assert.IsNotNull(_collider);
            #endif
            
            Setup();
        }
        
        private void Setup()
        {
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
        }
        
        public override void Interact(PlayerController playerController)
        {
            base.Interact(playerController);
            //TODO: implement Extinguisher
            Debug.Log("[Extinguisher] Not implemented", this);
        }

        public override bool TryToDropIntoSlot(IPickable pickableToDrop) => false;
        
        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            return playerHoldPickable == null ? this : null;
        }

        public void Pick()
        {
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
        }

        public void Drop()
        {
            gameObject.transform.SetParent(null);
            _rigidbody.isKinematic = false;
            _collider.enabled = true;
        }
    }
}
