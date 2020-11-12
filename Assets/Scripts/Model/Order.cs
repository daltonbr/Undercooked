using System.Collections;
using System.Collections.Generic;
using Undercooked.Data;
using UnityEngine;

namespace Undercooked.Model
{
    public class Order : MonoBehaviour
    {
        private OrderData _orderData;
        private const float AlertLimitTime = 5f;
        
        public bool IsDelivered { get; private set; }
        public float RemainingTime { get; private set; }
        public float ArrivalTime { get; private set; }
        public float InitialRemainingTime { get; private set; } = 50f;

        public OrderData OrderData => _orderData;
        public List<IngredientData> Ingredients => _orderData.ingredients;
        public float TimeRemainingWhenDelivered { get; private set; }

        private Coroutine _countdownCoroutine;
        
        public delegate void Expired(Order order);
        public event Expired OnExpired;
                
        public delegate void Delivered(Order order);
        public event Delivered OnDelivered;
        
        public delegate void AlertTime(Order order);
        public event AlertTime OnAlertTime;

        public delegate void UpdatedCountdown(float remainingTime);
        public event UpdatedCountdown OnUpdatedCountdown;

        public void Setup(OrderData orderData)
        {
            IsDelivered = false;
            _orderData = orderData;
            ArrivalTime = Time.time;
            SetCountdownTime(InitialRemainingTime);
            StartCountdown();
        }

        private void SetCountdownTime(float seconds)
        {
            InitialRemainingTime = seconds;
            RemainingTime = InitialRemainingTime;
        }

        private void StartCountdown()
        {
            _countdownCoroutine = StartCoroutine(CountdownCoroutine());
        }

        private void StopCountdown()
        {
            if (_countdownCoroutine != null)
            {
                StopCoroutine(CountdownCoroutine());
            }
        }

        private void ResumeCountdown()
        {
            if (_countdownCoroutine != null)
            {
                StartCoroutine(CountdownCoroutine());
            }   
        }

        private void ResetCountdown()
        {
            ArrivalTime = Time.time;
            RemainingTime = InitialRemainingTime;
            StopCoroutine(CountdownCoroutine());
            StartCountdown();
        }

        private IEnumerator CountdownCoroutine()
        {
            while (RemainingTime > AlertLimitTime)
            {
                RemainingTime -= Time.deltaTime;
                OnUpdatedCountdown?.Invoke(RemainingTime);
                yield return null;
            }
            
            OnAlertTime?.Invoke(this);
            
            while (RemainingTime > 0)
            {
                RemainingTime -= Time.deltaTime;
                OnUpdatedCountdown?.Invoke(RemainingTime);
                yield return null;
            }
            
            OnExpired?.Invoke(this);
            ResetCountdown();
        }

        public void SetOrderDelivered()
        {
            TimeRemainingWhenDelivered = RemainingTime;
            IsDelivered = true;
            StopCountdown();
            OnDelivered?.Invoke(this);
        }
        
    }
}
