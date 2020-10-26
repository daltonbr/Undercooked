using UnityEngine;

namespace Undercooked
{
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
