using UnityEngine;

namespace Undercooked
{
    [RequireComponent(typeof(MeshRenderer))]
    public class Countertop : MonoBehaviour
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
        
        private void OnTriggerEnter(Collider other)
        {
            //Debug.Log($"[Countertop] PlayerEnter {other.gameObject.name}");
            ChangePropertyBlock(true);
        }

        private void OnTriggerExit(Collider other)
        {
            //Debug.Log("[Countertop] PlayerExit");
            ChangePropertyBlock(false);
        }

        private void ChangePropertyBlock(bool highlight)
        {
            _materialBlock.SetInt(Highlight, highlight ? 1 : 0);
            _meshRenderer.SetPropertyBlock(_materialBlock);
        }
    }
}
