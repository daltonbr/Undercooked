using System.Collections.Generic;
using JetBrains.Annotations;
using Undercooked.Player;
using UnityEngine;

namespace Undercooked.Model
{
    /// <summary>
    /// Structures that the user can highlight and optionally interact with.
    /// Could allow certain items to be dropped/pickedup <see cref="IPickable"/>s into it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class Interactable : MonoBehaviour
    {
        [Tooltip("Pivot where IPickables could be dropped/pickedUp")]
        [SerializeField] protected Transform slot;

        protected IPickable CurrentPickable { get; set; }
        protected PlayerController LastPlayerControllerInteracting;
        private readonly List<MeshRenderer> _meshes = new List<MeshRenderer>();
        private MaterialPropertyBlock _materialBlock;
        private static readonly int Highlight = Shader.PropertyToID("Highlight_");
        
        public Transform Slot => slot;
        
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
            
            CacheMeshRenderersRecursivelyIgnoringSlot(transform);
        }

        private void CacheMeshRenderersRecursivelyIgnoringSlot(Transform root)
        {
            foreach (Transform child in root)
            {
                if (child.gameObject.name == "Slot") continue;
                
                var meshRenderer = child.GetComponent<MeshRenderer>();
                if (meshRenderer != null) 
                {
                    _meshes.Add(meshRenderer);
                }
                
                CacheMeshRenderersRecursivelyIgnoringSlot(child);
            }
        }

        private void CheckSlotOccupied()
        {
            if (Slot == null) return;
            foreach (Transform child in Slot)
            {
                CurrentPickable = child.GetComponent<IPickable>();
                if (CurrentPickable != null) return;
            }
        }

        private void ChangePropertyBlock(bool highlight)
        {
            _materialBlock?.SetInt(Highlight, highlight ? 1 : 0);
            
            foreach (var mesh in _meshes)
            {
                if (mesh == null) return;
                mesh.SetPropertyBlock(_materialBlock);
            }
        }

        public virtual void Interact(PlayerController playerController)
        {
            LastPlayerControllerInteracting = playerController;
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

        public abstract bool TryToDropIntoSlot(IPickable pickableToDrop);
        [CanBeNull] public abstract IPickable TryToPickUpFromSlot(IPickable playerHoldPickable);
    }
}