using UnityEngine;

namespace Undercooked
{
    /// <summary>
    /// Structures that the user can highlight and optionally interact.
    /// Could allow certain items to be dropped/pickedup <see cref="IPickable"/>s into it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public abstract class Interactable : MonoBehaviour
    {
        protected MeshFilter _meshFilter;
        protected MeshRenderer _meshRenderer;
        
        [Tooltip("Pivot where IPickables could be dropped/pickedUp")]
        [SerializeField] protected Transform slot;
        // TODO: we may have a starting item.
        protected IPickable CurrentPickable;

        private MaterialPropertyBlock _materialBlock;
        private static readonly int Highlight = Shader.PropertyToID("Highlight_");
        
        protected virtual void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _materialBlock = new MaterialPropertyBlock();
            
            CheckSlotOccupied();
        }

        private void CheckSlotOccupied()
        {
            if (slot == null) return;
            foreach (Transform child in slot)
            {
                CurrentPickable = child.GetComponent<IPickable>();
            }
        }

        private void ChangePropertyBlock(bool highlight)
        {
            _materialBlock.SetInt(Highlight, highlight ? 1 : 0);
            _meshRenderer.SetPropertyBlock(_materialBlock);
        }

        public virtual void Interact()
        {
            //Debug.Log($"[Interactable] Interact with {gameObject.name}");
        }
        
        public void ToggleHighlightOn()
        {
            ChangePropertyBlock(true);
            var interactable = CurrentPickable as Interactable;
            interactable?.ToggleHighlightOn();
        }

        public void ToggleHighlightOff()
        {
            ChangePropertyBlock(false);
            var interactable = CurrentPickable as Interactable;
            interactable?.ToggleHighlightOff();
        }

        public abstract bool TryToDropIntoSlot(IPickable pickable);
        public abstract IPickable TryToPickUpFromSlot();
    }
}