using UnityEngine;

namespace Undercooked
{
    /// <summary>
    /// Can be picked/dropped by the player into the floor or into an <see cref="Interactable"/>
    /// </summary>
    public interface IPickable
    {
        GameObject gameObject { get; }
        public void Pick();
        public void Drop();
    }
}