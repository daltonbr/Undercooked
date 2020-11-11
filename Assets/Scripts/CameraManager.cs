using System;
using Cinemachine;
using ICSharpCode.NRefactory.Ast;
using Lean.Transition;
using UnityEngine;
using UnityEngine.Assertions;

namespace Undercooked
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _virtualCamera1;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera2;
        [SerializeField] private float transitionDelayInSeconds = 0.75f;
        
        [SerializeField] private ParticleSystem starGlowParticleSystem;

        private Transform _particleSystemTransform;
        private Transform _avatar1;
        private Transform _avatar2;

        private PlayerControllerIndex _lastActivatedPlayerController;
        
        private void Awake()
        {
            starGlowParticleSystem.Play();
            _particleSystemTransform = starGlowParticleSystem.transform;
            _avatar1 = _virtualCamera1.Follow;
            _avatar2 = _virtualCamera2.Follow;
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

        private void HandleSwitchPlayerController(PlayerControllerIndex playerControllerIndex)
        {
            SwitchFocus(playerControllerIndex);
        }

        private void SwitchFocus(PlayerControllerIndex playerControllerIndex)
        {
            switch (playerControllerIndex)
            {
                case PlayerControllerIndex.First:
                    if (_lastActivatedPlayerController != PlayerControllerIndex.None)
                    {
                        MoveStarParticle(_avatar2, _avatar1, transitionDelayInSeconds);
                    }
                    _virtualCamera1.gameObject.SetActive(true);
                    _virtualCamera2.gameObject.SetActive(false);
                    break;
                case PlayerControllerIndex.Second:
                    if (_lastActivatedPlayerController != PlayerControllerIndex.None)
                    {
                        MoveStarParticle(_avatar1, _avatar2, transitionDelayInSeconds);
                    }
                    _virtualCamera1.gameObject.SetActive(false);
                    _virtualCamera2.gameObject.SetActive(true);
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
