using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Undercooked
{
    // Features
    // we can carry a pile of plates from DishTray
    // we can drop a pile of plates into Sink
    // break line-of-sight pauses cleaning process
    // if one plate is cleaning the next one starts automatically
    
    public class Sink : Interactable
    {
        [SerializeField] private Slider slider;
        [SerializeField] private List<Transform> dirtySlots = new List<Transform>();

        private readonly Stack<Plate> _cleanPlates = new Stack<Plate>();
        private readonly Stack<Plate> _dirtyPlates = new Stack<Plate>();

        private const float CleaningTime = 3f;
        private float _currentCleaningTime;
        private Coroutine _cleanCoroutine;
        private bool _isCleaning;
        
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
            // we can only pick clean plates (if we have available ones). Potentially a pile
            if (playerHoldPickable != null)
            {
                Debug.Log("[Sink] Player need empty hands to pick something from sink");
                return null;
            }
            
            if (_cleanPlates.Count > 0)
            {
                // Debug.Log("[Sink] Picked a clean plate");
                return _cleanPlates.Pop();
            }
            else
            {
                // Debug.Log("[Sink] There is no clean plates to pick");
                return null;    
            }
        }

        public override void Interact()
        {
            base.Interact();
            
            // set animation (should this be player's responsibility?

            if (_dirtyPlates.Count == 0)
            {
                Debug.Log("[Sink] There are no dirty plates");
                return;
            }

            if (_cleanCoroutine == null)
            {
                _currentCleaningTime = 0f;
                slider.value = 0f;
                slider.gameObject.SetActive(true);
                _cleanCoroutine = StartCoroutine(Clean());
                return;
            }
            else
            {
                PauseCleanCoroutine();
                _cleanCoroutine = StartCoroutine(Clean());
                return;
            }
        }

        private IEnumerator Clean()
        {
            _isCleaning = true;
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
            _isCleaning = false;

            // Clean next plate
            if (_dirtyPlates.Count > 0)
            {
                _cleanCoroutine = StartCoroutine(Clean());
            }
            yield break;
        }
        
        public override void ToggleHighlightOff()
        {
            base.ToggleHighlightOff();
            PauseCleanCoroutine();
        }
        
        private void PauseCleanCoroutine()
        {
            slider.gameObject.SetActive(false);
            _isCleaning = false;
            if (_cleanCoroutine != null) StopCoroutine(_cleanCoroutine);
        }
        
    }
}
