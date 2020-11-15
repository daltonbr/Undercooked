using Lean.Transition;
using UnityEngine;
using UnityEngine.Assertions;

namespace Undercooked.Utils
{
    public class PlayAudioFromAnimationEvent : MonoBehaviour
    {
        [SerializeField] private AudioClip[] audioClips;

        private void Awake()
        {
            #if UNITY_EDITOR
                Assert.IsNotNull(audioClips);
                foreach (var clip in audioClips)
                {
                    Assert.IsNotNull(clip);
                }
            #endif
        }

        public void PlayAudioHit()
        {
            int randomIndex = Random.Range(0, audioClips.Length);
            this.PlaySoundTransition(audioClips[randomIndex]);
        }
    }
}