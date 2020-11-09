using UnityEngine;

namespace Undercooked
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Ingredient : Interactable, IPickable
    {
        [SerializeField] private IngredientData data;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;

        public IngredientStatus Status { get; private set; }
        public IngredientType Type => data.type;
        public Color BaseColor => data.baseColor;

        [SerializeField] private IngredientStatus _startingStatus = IngredientStatus.Raw; 

        public float ProcessTime => data.processTime;
        public float CookTime => data.cookTime;
        public Sprite SpriteUI => data.sprite;

        protected override void Awake()
        {
            base.Awake();
            
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
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
            
            Status = IngredientStatus.Raw;
            _meshFilter.mesh = data.rawMesh;
            _meshRenderer.material = data.ingredientMaterial;

            if (_startingStatus == IngredientStatus.Processed)
            {
                ChangeToProcessed();
            }
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
        
        public void ChangeToProcessed()
        {
            Status = IngredientStatus.Processed;
            _meshFilter.mesh = data.processedMesh;
        }

        public void ChangeToCooked()
        {
            Status = IngredientStatus.Cooked;
            var cookedMesh = data.cookedMesh;
            if (cookedMesh == null) return;
            
            _meshFilter.mesh = cookedMesh;
            SetMeshRendererEnabled(true);
        }

        public void SetMeshRendererEnabled(bool enable)
        {
            _meshRenderer.enabled = enable;
        }

        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            // Ingredients normally don't get any pickables dropped into it.
            // Debug.Log("[Ingredient] TryToDrop into an Ingredient isn't possible by design");
            return false;
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            // Debug.Log($"[Ingredient] Trying to PickUp {gameObject.name}");
            _rigidbody.isKinematic = true;
            return this;
        }
    }
}