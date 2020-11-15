using System.Threading.Tasks;
using Lean.Transition;
using UnityEngine;

namespace Undercooked.Managers
{
    public class MoveDollyTarget : MonoBehaviour
    {
        private Transform _transform;
        [SerializeField] private float deltaX = 9f;
        [SerializeField] private float oscillationDuration = 11f;
        private bool _oscillate;

        private void Awake()
        {
            _transform = transform;
        }

        private void OnEnable()
        {
            _oscillate = true;
            StartLerping();
        }

        private void OnDisable()
        {
            _oscillate = false;
        }

        private async Task StartLerping()
        {
            while (_oscillate)
            {
                _transform
                    .positionTransition_X(deltaX, oscillationDuration, LeanEase.Smooth)
                    .JoinTransition()
                    .positionTransition_X(-deltaX, oscillationDuration, LeanEase.Smooth);
            
                await Task.Delay((int)oscillationDuration * 1000 * 2);   
            }
        }

    }
}
