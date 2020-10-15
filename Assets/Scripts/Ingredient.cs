using UnityEngine;

namespace Undercooked
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Ingredient : Interactable, IPickable
    {
        [SerializeField] private IngredientData data;
        private float _timeRemainingToProcess;
        private Rigidbody _rigidbody;
        private Collider _collider;

        public IngredientStatus Status => data.status;
        public IngredientType Type => data.type;

        protected override void Awake()
        {
            base.Awake();
            
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _timeRemainingToProcess = data.timeToProcess;
            
            Setup();
        }

        private void Setup()
        {
            // Rigidbody is kinematic almost all the time, except when we drop it on the floor
            // re-enabling when picked up.
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
            
            data.status = IngredientStatus.Raw;
            _meshFilter.mesh = data.rawMesh;
            _meshRenderer.material = data.ingredientMaterial;
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

        //TODO: how to handle the timer? Coroutine?
        // we need to keep a reference to an amount of time passed,
        // because the processing can be interrupted and restarted
        public void ChangeToProcessed()
        {
            data.status = IngredientStatus.Processed;
            _meshFilter.mesh = data.processedMesh;
        }

        public override bool TryToDropIntoSlot(IPickable pickable)
        {
            // Ingredients normally don't get any pickables dropped into it.
            
            Debug.Log("[Ingredient] TryToDrop into an Ingredient isn't possible by design");
            return false;
        }

        public override IPickable TryToPickUpFromSlot()
        {
            Debug.Log($"[Ingredient] Trying to PickUp {gameObject.name}");
            _rigidbody.isKinematic = true;
            return this;
        }
    }
}