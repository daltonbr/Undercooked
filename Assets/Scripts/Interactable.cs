using System.Collections.Generic;
using UnityEngine;

namespace Undercooked
{
    /// <summary>
    /// Structures that the user can highlight and optionally interact.
    /// Could allow certain items to be dropped/pickedup <see cref="IPickable"/>s into it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class Interactable : MonoBehaviour
    {
        [Tooltip("Pivot where IPickables could be dropped/pickedUp")]
        [SerializeField] protected Transform slot;
        // TODO: we may have a starting item.
        protected IPickable CurrentPickable;

        private readonly List<MeshRenderer> _meshes = new List<MeshRenderer>();
        private MaterialPropertyBlock _materialBlock;
        private static readonly int Highlight = Shader.PropertyToID("Highlight_");
        
        
        protected virtual void Awake()
        {
            _materialBlock = new MaterialPropertyBlock();

            CacheMeshRenderers();
            CheckSlotOccupied();
        }

        private void CacheMeshRenderers()
        {
            var baseMesh = transform.GetComponent<MeshRenderer>();
            if (baseMesh != null) _meshes.Add(baseMesh);
            foreach (Transform child in transform)
            {
                var childMesh = child.GetComponent<MeshRenderer>();
                if (childMesh != null) _meshes.Add(childMesh);
            }
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
            //_meshRenderer.SetPropertyBlock(_materialBlock);
            foreach (var mesh in _meshes)
            {
                mesh.SetPropertyBlock(_materialBlock);
            }
        }

        public virtual void Interact()
        {
            //Debug.Log($"[Interactable] Interact with {gameObject.name}");
        }
        
        public virtual void ToggleHighlightOn()
        {
            ChangePropertyBlock(true);
            var interactable = CurrentPickable as Interactable;
            interactable?.ToggleHighlightOn();
        }

        public virtual void ToggleHighlightOff()
        {
            ChangePropertyBlock(false);
            var interactable = CurrentPickable as Interactable;
            interactable?.ToggleHighlightOff();
        }

        public abstract bool TryToDropIntoSlot(IPickable pickable);
        public abstract IPickable TryToPickUpFromSlot();
    }
}