using UnityEngine;

namespace Undercooked
{
    public abstract class Interactable : MonoBehaviour
    {
        public virtual void Interact() {}
        public virtual void ToggleOn() {}
        public virtual void ToggleOff() {}
    }
}