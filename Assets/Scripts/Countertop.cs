using UnityEngine;

namespace Undercooked
{
    [RequireComponent(typeof(MeshRenderer))]
    public class Countertop : Interactable
    {
        private MaterialPropertyBlock _materialBlock;
        private MeshRenderer _meshRenderer;
        private static readonly int Highlight = Shader.PropertyToID("Highlight_");
        
        private void Awake()
        {
            //TODO: reuse this materialPropertyBlock
            _materialBlock = new MaterialPropertyBlock();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void ChangePropertyBlock(bool highlight)
        {
            _materialBlock.SetInt(Highlight, highlight ? 1 : 0);
            _meshRenderer.SetPropertyBlock(_materialBlock);
        }

        public override void Interact()
        {
            Debug.Log($"[Countertop] Interact with Countertop {gameObject.name}");
        }

        public override void ToggleOn()
        {
            ChangePropertyBlock(true);
        }

        public override void ToggleOff()
        {
            ChangePropertyBlock(false);
        }
    }
}
