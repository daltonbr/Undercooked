using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Undercooked
{
    public class ChoppingBoard : Interactable
    {
        [SerializeField] private Transform knife;
        [SerializeField] private Slider slider;
        private float _finalProcessTime;
        private float _currentProcessTime;
        private Coroutine _chopCoroutine;
        private Ingredient _ingredient;
        private bool _isChopping;

        protected override void Awake()
        {
            base.Awake();
            slider.gameObject.SetActive(false);
        }

        public override void Interact()
        {
            base.Interact();
            if (CurrentPickable == null)
            {
                Debug.Log("[ChoppingBoard] There is nothing to chop");
                return;
            }

            if (_ingredient == null) return;
            
            switch (_ingredient.Status)
            {
                case IngredientStatus.Raw:
                    // start/resume chopping
                    if (_chopCoroutine == null)
                    {
                        _finalProcessTime = _ingredient.ProcessTime;
                        _currentProcessTime = 0f;
                        slider.value = 0f;
                        slider.gameObject.SetActive(true);
                        _chopCoroutine = StartCoroutine(Chop());
                        return;
                    }
                    else
                    {
                        if (_isChopping == false)
                        {
                            Debug.Log("[ChoppingBoard] Resume Chopping");
                            _chopCoroutine = StartCoroutine(Chop());
                        }
                        else
                        {
                            Debug.Log("[ChoppingBoard] Coroutine already in progress. Ignoring");        
                        }
                        return;
                    }
                    break;
                case IngredientStatus.Processed:
                    Debug.Log("[ChoppingBoard] Ingredient already chopped. Ignoring");
                    break;
                default:
                    Debug.Log("[ChoppingBoard] IPickable wasn't expected", this);
                    break;
            }
        }
        
        //TODO: stop chopping when break outside collider
        public void PauseChop()
        {
            _isChopping = false;
            StopCoroutine(_chopCoroutine);
        }
        
        private void SetSliderActive(bool active)
        {
            slider.gameObject.SetActive(active);
        }

        private IEnumerator Chop()
        {
            _isChopping = true;
            Debug.Log($"[ChoppingBoard] current:{_currentProcessTime} final:{_finalProcessTime}");
            //TODO: also check if player is still in range (or maybe at a minimum distance of chopping board)
            while (_currentProcessTime < _finalProcessTime)
            {
                slider.value = _currentProcessTime / _finalProcessTime;
                _currentProcessTime += Time.deltaTime;
                yield return null;
            }

            //we finish
           _ingredient.ChangeToProcessed();
            slider.gameObject.SetActive(false);
            // TODO: what more?
            _isChopping = false;
            _chopCoroutine = null;
        }
        
        public override bool TryToDropIntoSlot(IPickable pickable)
        {
            switch (pickable)
            {
                case Ingredient ingredient:
                    return TryDropIfNotOccupied(pickable);
                    break;
                default:
                    Debug.LogWarning("[ChoppingBoard] Refuse everything that is not raw ingredient");
                    return false;
            }
        }

        public override IPickable TryToPickUpFromSlot()
        {
            //TODO: only allow Pickup after we finish chopping the ingredient. Essentially locking it in place.
            if (CurrentPickable == null)
            {
                return null;
            }
            else
            {
                var output = CurrentPickable;
                _ingredient = null;
                var interactable = CurrentPickable as Interactable;
                interactable?.ToggleHighlightOff();
                CurrentPickable = null;
                knife.gameObject.SetActive(true);
                return output;
            }
        }
        
        private bool TryDropIfNotOccupied(IPickable pickable)
        {
            if (CurrentPickable != null)
            {
                Debug.Log("[ChoppingBoard] Try to drop into ChoppingBoard, but it's already occupied");
                return false;
            }
            CurrentPickable = pickable;
            _ingredient = pickable as Ingredient;
            if (_ingredient == null)
            {
                Debug.Log("[ChoppingBoard] IPickable is not an Ingredient", this);    
            }

            _finalProcessTime = _ingredient.ProcessTime;
            
            CurrentPickable.gameObject.transform.SetParent(slot);
            CurrentPickable.gameObject.transform.SetPositionAndRotation(slot.position, Quaternion.identity);
            knife.gameObject.SetActive(false);
            return true;
        }
    }
}
