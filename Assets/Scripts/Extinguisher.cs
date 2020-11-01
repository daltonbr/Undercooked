using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Undercooked
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Extinguisher : Interactable,  IPickable
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
            Setup();
        }
        
        private void Setup()
        {
            // Rigidbody is kinematic almost all the time, except when we drop it on the floor
            // re-enabling when picked up.
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
        }
        
        public override void Interact()
        {
            base.Interact();
            
            // set animation (should this be player's responsibility?
            //TODO: implement Extinguisher
            Debug.Log("[Extinguisher] Not implemented", this);
        }
        
        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            return false;
        }

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
