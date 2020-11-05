using UnityEngine;

namespace Undercooked.UI
{
    /// <summary>
    /// UI elements in Overlay mode, will follow the target in world space
    /// </summary>
    public class FollowTarget : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset;
        [SerializeField] private Camera cam;
        
        private void Awake()
        {
            cam = Camera.main;
            
        }

        private void LateUpdate()
        {
            Vector3 position = cam.WorldToScreenPoint(target.position + offset);
            
            if (transform.position != position)
            {
                transform.position = position;
            }
        }
        
    }
    
}
