using System.Collections.Generic;
using System.Linq;
using Undercooked.UI;
using UnityEngine;

namespace Undercooked
{
    public class OrdersPanelUI : MonoBehaviour
    {
        [SerializeField] private OrderUI orderUIPrefab;
        private readonly List<OrderUI> _ordersUI = new List<OrderUI>();
        private readonly Queue<OrderUI> _orderUIPool = new Queue<OrderUI>();

        private OrderUI GetOrderUIFromPool()
        {
            return _orderUIPool.Count > 0 ? _orderUIPool.Dequeue() : Instantiate(orderUIPrefab, transform);
        }
        
        private void OnEnable()
        {
            OrderManager.OnOrderSpawned += HandleOrderSpawned;    
        }

        private void OnDisable()
        {
            OrderManager.OnOrderSpawned -= HandleOrderSpawned;
        }

        private void HandleOrderSpawned(Order order)
        {
            var rightmostX = GetRightmostXFromLastElement();
            var orderUI = GetOrderUIFromPool();
            orderUI.Setup(order);
            _ordersUI.Add(orderUI);
            orderUI.SlideInSpawn(rightmostX);
        }

        private float GetRightmostXFromLastElement()
        {
            if (_ordersUI.Count == 0) return 0;
            
            float rightmostX = 0f;
            
            List<OrderUI> orderUisNotDeliveredOrderedByLeftToRight = _ordersUI
                .Where(x => x.Order.IsDelivered == false)
                .OrderBy(y => y.CurrentAnchorX).ToList();

            if (orderUisNotDeliveredOrderedByLeftToRight.Count == 0) return 0;
            
            var last = orderUisNotDeliveredOrderedByLeftToRight.Last();
            rightmostX = last.CurrentAnchorX + last.SizeDeltaX;

            return rightmostX;
        }
        
        public void RegroupPanelsLeft()
        {
            float leftmostX = 0f;

            for (var i = 0; i < _ordersUI.Count; i++)
            {
                var orderUI = _ordersUI[i];
                if (orderUI.Order.IsDelivered)
                {
                    _orderUIPool.Enqueue(orderUI);
                    _ordersUI.RemoveAt(i);
                    i--;
                }
                else
                {
                    orderUI.SlideLeft(leftmostX);
                    leftmostX += orderUI.SizeDeltaX;
                }
            }
        }
        
    }
}
