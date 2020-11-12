using System.Collections.Generic;
using System.Linq;
using Undercooked.Model;
using UnityEngine;

namespace Undercooked.Appliances
{
    public class DishTray : Interactable
    {
        private readonly List<Plate> _dirtyPlates = new List<Plate>();

        public override bool TryToDropIntoSlot(IPickable pickableToDrop) => false;

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            if (playerHoldPickable != null) return null;
            if (_dirtyPlates.Count == 0) return null;
            
            var bottomPlate = _dirtyPlates[0];
            _dirtyPlates.Clear();
            return bottomPlate;
        }

        public void AddDirtyPlate(Plate plate)
        {
            // put the clean plate into the top of the cleanPile (physically)
            var topPileSlot = _dirtyPlates.Count == 0 ? Slot : _dirtyPlates.Last().Slot;
            
            // plates are parented to the Slot of the previous one
            plate.transform.SetParent(topPileSlot);
            plate.transform.SetPositionAndRotation(topPileSlot.transform.position, Quaternion.identity);
            _dirtyPlates.Add(plate);
        }
    }
}
