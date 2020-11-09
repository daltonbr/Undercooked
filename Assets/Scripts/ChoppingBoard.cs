using System.Collections;
using UnityEngine;
using Slider = UnityEngine.UI.Slider;

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

        public delegate void ChoppingStatus();
        public static event ChoppingStatus OnChoppingStart;
        public static event ChoppingStatus OnChoppingStop;

        protected override void Awake()
        {
            base.Awake();
            slider.gameObject.SetActive(false);
        }

        public override void Interact()
        {
            base.Interact();
            if (CurrentPickable == null ||
                _ingredient == null ||
                _ingredient.Status != IngredientStatus.Raw) return;
            
            if (_chopCoroutine == null)
            {
                _finalProcessTime = _ingredient.ProcessTime;
                _currentProcessTime = 0f;
                slider.value = 0f;
                slider.gameObject.SetActive(true);
                StartChopCoroutine();
                return;
            }

            if (_isChopping == false)
            {
                StartChopCoroutine();
            }
        }

        private void StartChopCoroutine()
        {
            OnChoppingStart?.Invoke();
            _chopCoroutine = StartCoroutine(Chop());
        }

        private void StopChopCoroutine()
        {
            OnChoppingStop?.Invoke();
            _isChopping = false;
            if (_chopCoroutine != null) StopCoroutine(_chopCoroutine);
        }

        public override void ToggleHighlightOff()
        {
            base.ToggleHighlightOff();
            StopChopCoroutine();
        }
        
        private IEnumerator Chop()
        {
            _isChopping = true;
            while (_currentProcessTime < _finalProcessTime)
            {
                slider.value = _currentProcessTime / _finalProcessTime;
                _currentProcessTime += Time.deltaTime;
                yield return null;
            }

            // finished
            _ingredient.ChangeToProcessed();
            slider.gameObject.SetActive(false);
            _isChopping = false;
            _chopCoroutine = null;
            OnChoppingStop?.Invoke();
        }
        
        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            if (pickableToDrop is Ingredient)
            {
                return TryDropIfNotOccupied(pickableToDrop);
            }
            return false;
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            // only allow Pickup after we finish chopping the ingredient. Essentially locking it in place.
            if (CurrentPickable == null) return null;
            if (_chopCoroutine != null) return null;
            
            var output = CurrentPickable;
            _ingredient = null;
            var interactable = CurrentPickable as Interactable;
            interactable?.ToggleHighlightOff();
            CurrentPickable = null;
            knife.gameObject.SetActive(true);
            return output;
        }
        
        private bool TryDropIfNotOccupied(IPickable pickable)
        {
            if (CurrentPickable != null) return false;
            CurrentPickable = pickable;
            _ingredient = pickable as Ingredient;
            if (_ingredient == null) return false;

            _finalProcessTime = _ingredient.ProcessTime;
            
            CurrentPickable.gameObject.transform.SetParent(Slot);
            CurrentPickable.gameObject.transform.SetPositionAndRotation(Slot.position, Quaternion.identity);
            knife.gameObject.SetActive(false);
            return true;
        }
    }
}
