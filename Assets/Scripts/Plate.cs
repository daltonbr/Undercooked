using UnityEngine;

namespace Undercooked
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Plate : Interactable, IPickable
    {
        //TODO: plates can hold processed ingredients
        // how to store a soup?
        // plates can be dirty, empty or with ingredients
        // do we need a plate slot? to put extra models on top of it?
        
        // plates have up to four UI icons to represent the plate's content
        
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
            
            //data.status = IngredientStatus.Raw;
            //_meshFilter.mesh = data.rawMesh;
            ///_meshRenderer.material = data.ingredientMaterial;
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

        public override bool TryToDropIntoSlot(IPickable pickable)
        {
            // plates will receive pickables (processed ingredients), not when they are on the floor
            // for now we don't receive anything
            Debug.Log("[Plate] TryToDrop not implemented");
            return false;
        }

        public override IPickable TryToPickUpFromSlot()
        {
            // can we pick something from the plate slot?
            return this;
        }
    }
}