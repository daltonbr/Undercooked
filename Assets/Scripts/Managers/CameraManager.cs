using Cinemachine;
using Lean.Transition;
using Undercooked.Player;
using UnityEngine;
using UnityEngine.Assertions;

namespace Undercooked.Managers
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera virtualCamera1;
        [SerializeField] private CinemachineVirtualCamera virtualCamera2;
        [SerializeField] private const float TransitionDelayInSeconds = 0.75f;
        [SerializeField] private ParticleSystem starGlowParticleSystem;

        private Transform _particleSystemTransform;
        private Transform _avatar1;
        private Transform _avatar2;
        private PlayerInputController.PlayerControllerIndex _lastActivatedPlayerController;
        
        private void Awake()
        {
            #if UNITY_EDITOR
            Assert.IsNotNull(virtualCamera1);
            Assert.IsNotNull(virtualCamera2);
            Assert.IsNotNull(starGlowParticleSystem);
            #endif
            
            starGlowParticleSystem.Play();
            _particleSystemTransform = starGlowParticleSystem.transform;
            _avatar1 = virtualCamera1.Follow;
            _avatar2 = virtualCamera2.Follow;
            _particleSystemTransform.position = _avatar1.position;
        }

        private void OnEnable()
        {
            PlayerInputController.OnSwitchPlayerController += HandleSwitchPlayerController;
        }

        private void OnDisable()
        {
            PlayerInputController.OnSwitchPlayerController -= HandleSwitchPlayerController;
        }

        private void HandleSwitchPlayerController(PlayerInputController.PlayerControllerIndex playerControllerIndex)
        {
            SwitchFocus(playerControllerIndex);
        }

        private void SwitchFocus(PlayerInputController.PlayerControllerIndex playerControllerIndex)
        {
            switch (playerControllerIndex)
            {
                case PlayerInputController.PlayerControllerIndex.First:
                    if (_lastActivatedPlayerController != PlayerInputController.PlayerControllerIndex.None)
                    {
                        MoveStarParticle(_avatar2, _avatar1, TransitionDelayInSeconds);
                    }
                    virtualCamera1.gameObject.SetActive(true);
                    virtualCamera2.gameObject.SetActive(false);
                    break;
                case PlayerInputController.PlayerControllerIndex.Second:
                    if (_lastActivatedPlayerController != PlayerInputController.PlayerControllerIndex.None)
                    {
                        MoveStarParticle(_avatar1, _avatar2, TransitionDelayInSeconds);
                    }
                    virtualCamera1.gameObject.SetActive(false);
                    virtualCamera2.gameObject.SetActive(true);
                    break;
                default:
                    Debug.LogWarning("[CameraManager] Unexpected player controller index", this);
                    break;
            }
            _lastActivatedPlayerController = playerControllerIndex;
        }

        private void MoveStarParticle(Transform from, Transform to, float delayInSeconds = 1f)
        {
            starGlowParticleSystem.Play();
            _particleSystemTransform.position = from.position;
            _particleSystemTransform
                .positionTransition(to.position, delayInSeconds, LeanEase.Smooth);
        }
    }
}
