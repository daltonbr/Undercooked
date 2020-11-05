using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Undercooked
{
    // -- Particular Features --
    // we can drop a pile of plates into Sink
    // when player breaks contact cleaning process is paused (it could be resumed)
    // when one plate  is cleaned the next one starts automatically
    
    public class Sink : Interactable
    {
        [SerializeField] private Slider slider;
        [SerializeField] private List<Transform> dirtySlots = new List<Transform>();

        private readonly Stack<Plate> _cleanPlates = new Stack<Plate>();
        private readonly Stack<Plate> _dirtyPlates = new Stack<Plate>();

        private const float CleaningTime = 3f;
        private float _currentCleaningTime;
        private Coroutine _cleanCoroutine;

        protected override void Awake()
        {
            base.Awake();
            
            #if UNITY_EDITOR
            Assert.IsNotNull(dirtySlots);
            Assert.IsNotNull(_cleanPlates);
            Assert.IsNotNull(_dirtyPlates);
            Assert.IsNotNull(slider);
            #endif
        }

        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            switch (pickableToDrop)
            {
                case Plate plate:
                    if (!plate.IsEmpty() || plate.IsClean)
                    {
                        Debug.Log("[Sink] Plate is not empty or clean. Refusing it!");
                        return false;
                    }
                    AddPileDirtyPlatesRecursively(plate);
                    return true;
                    break;
                default:
                    Debug.Log("[Sink] Only accept dirty empty plates.");
                    break;
            }
            return false;
        }

        /// <summary>
        /// The first two dirty plate slots are visible, all subsequent are placed out of player's sight
        /// </summary>
        private void AddPileDirtyPlatesRecursively(Plate plate)
        {
            Plate nextPlate = plate.Slot.GetComponentInChildren<Plate>();
            if (nextPlate != null)
            {
                nextPlate.transform.SetParent(null);
                AddPileDirtyPlatesRecursively(nextPlate);
            }
            
            _dirtyPlates.Push(plate);
            var dirtySize = _dirtyPlates.Count;
            
            Transform dirtySlot = dirtySize <= 2 ? dirtySlots[dirtySize - 1] : dirtySlots[2];
            
            plate.transform.SetParent(dirtySlot);
            plate.transform.SetPositionAndRotation(dirtySlot.transform.position, dirtySlot.transform.rotation);
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            if (playerHoldPickable != null) return null;
            
            return _cleanPlates.Count > 0 ? _cleanPlates.Pop() : null;
        }

        public override void Interact()
        {
            base.Interact();

            if (_dirtyPlates.Count == 0) return;
            
            if (_cleanCoroutine == null)
            {
                _currentCleaningTime = 0f;
                slider.value = 0f;
                slider.gameObject.SetActive(true);
                _cleanCoroutine = StartCoroutine(Clean());
                return;
            }
            
            PauseCleanCoroutine();
            _cleanCoroutine = StartCoroutine(Clean());
        }

        private IEnumerator Clean()
        {
            slider.gameObject.SetActive(true);
            while (_currentCleaningTime < CleaningTime)
            {
                slider.value = _currentCleaningTime / CleaningTime;
                _currentCleaningTime += Time.deltaTime;
                yield return null;
            }
            
            // clean the top of the _dirtyPlates stack
            var plateToClean = _dirtyPlates.Pop();
            plateToClean.SetClean();
            
            // put the clean plate into the top of the cleanPile (physically)
            var topStackSlot = _cleanPlates.Count == 0 ? Slot : _cleanPlates.Peek().Slot;
            
            // all plates parented to the base Slot, but physically positioned on the top of the stack
            plateToClean.transform.SetParent(Slot);
            plateToClean.transform.SetPositionAndRotation(topStackSlot.transform.position, Quaternion.identity);

            _cleanPlates.Push(plateToClean);
            
            _cleanCoroutine = null;
            slider.gameObject.SetActive(false);
            _currentCleaningTime = 0f;

            // Clean next plate
            if (_dirtyPlates.Count > 0)
            {
                _cleanCoroutine = StartCoroutine(Clean());
            }
        }
        
        public override void ToggleHighlightOff()
        {
            base.ToggleHighlightOff();
            PauseCleanCoroutine();
        }
        
        private void PauseCleanCoroutine()
        {
            slider.gameObject.SetActive(false);
            if (_cleanCoroutine != null) StopCoroutine(_cleanCoroutine);
        }
        
    }
}
