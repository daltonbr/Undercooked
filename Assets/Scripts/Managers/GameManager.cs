using System.Collections;
using System.Threading.Tasks;
using Undercooked.Appliances;
using Undercooked.Data;
using Undercooked.Model;
using Undercooked.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace Undercooked.Managers
{
    [RequireComponent(typeof(OrderManager))]
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private DishTray dishTray;
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private LevelData level1;
        
        private const int BaseScorePerPlate = 20;
        private const int PenaltyExpiredScore = 10;
        private const float TimeToReturnPlateSeconds = 3f;
        
        private Coroutine _countdownCoroutine;
        private readonly WaitForSeconds TimeToReturnPlate = new WaitForSeconds(TimeToReturnPlateSeconds);
        private readonly WaitForSeconds OneSecondWait = new WaitForSeconds(1f);
        private int _score;
        private int _timeRemaining;

        private int Score
        {
            get => _score;
            set
            {
                var previous = _score;
                _score = value;
                OnScoreUpdate?.Invoke(_score, _score - previous);
            }
        }

        public int TimeRemaining
        {
            get => _timeRemaining;
            private set
            {
                _timeRemaining = value;
                OnCountdownTick?.Invoke(_timeRemaining);
            }
        }
        
        public delegate void CountdownTick(int timeRemaining);
        public static event CountdownTick OnCountdownTick;
        public delegate void ScoreUpdate(int score, int delta);
        public static event ScoreUpdate OnScoreUpdate;
        public delegate void DisplayNotification(string textToDisplay, Color color, float timeToDisplay);
        public static event DisplayNotification OnDisplayNotification;
        public delegate void TimeIsOver();
        public static event TimeIsOver OnTimeIsOver;
        public delegate void LevelStart();
        public static event LevelStart OnLevelStart;
        
        private void Awake()
        {
            #if UNITY_EDITOR
                Assert.IsNotNull(dishTray);
                Assert.IsNotNull(orderManager);
                Assert.IsNotNull(level1);
            #endif
        }

        private void Start()
        {
            GameLoop();
        }

        private async Task StartLevelAsync(LevelData levelData)
        {
            Score = 0;
            _timeRemaining = levelData.durationTime;
            
            await DisplayInitialNotifications();
            //OnLevelStart?.Invoke();
            orderManager.Init(levelData);
            
            // TODO: Start "Order spawner"
            
            // TODO: Unlock player movement
            
            _countdownCoroutine = StartCoroutine(CountdownTimer(_timeRemaining));

            //TODO: handle pause (pause timer coroutine and restart)
        }

        private async Task GameLoop()
        {
            StartLevelAsync(level1);
        }

        private static async Task DisplayInitialNotifications()
        {
            await NotificationUI.DisplayCenterNotificationAsync("Ready?", new Color(.66f, .367f, .15f), 2f);
            await NotificationUI.DisplayCenterNotificationAsync("GO", new Color(.333f, .733f, .196f), 2f);
        }
        
        private void OnEnable()
        {
            DeliverCountertop.OnPlateDropped += HandlePlateDropped;
            OrderManager.OnOrderExpired += HandleOrderExpired;
            OrderManager.OnOrderDelivered += HandleOrderDelivered;
        }
        
        private void OnDisable()
        {
            DeliverCountertop.OnPlateDropped -= HandlePlateDropped;
            OrderManager.OnOrderExpired -= HandleOrderExpired;
            OrderManager.OnOrderDelivered -= HandleOrderDelivered;
        }

        private void HandleOrderDelivered(Order order, int tipCalculated)
        {
            Score += BaseScorePerPlate + tipCalculated;
        }
        
        private void HandleOrderExpired(Order order)
        {
            Score -= PenaltyExpiredScore;
        }

        public void Pause()
        {
            // player controller ignore input
            Time.timeScale = 0;
            StopCoroutine(_countdownCoroutine);
        }

        public void Unpause()
        {
            // restore player input (event?)
            Time.timeScale = 1;
            _countdownCoroutine = StartCoroutine(CountdownTimer(_timeRemaining));
        }
        
        private void HandlePlateDropped(Plate plate)
        {
            if (plate.IsEmpty() || plate.IsClean == false)
            {
                plate.RemoveAllIngredients();
                StartCoroutine(ReturnPlateDirty(plate));
                return;
            }
            
            orderManager.CheckIngredientsMatchOrder(plate.Ingredients);
            plate.RemoveAllIngredients();
            StartCoroutine(ReturnPlateDirty(plate));
        }

        private IEnumerator ReturnPlateDirty(Plate plate)
        {
            plate.gameObject.SetActive(true);
            plate.SetDirty();
            yield return TimeToReturnPlate;
            dishTray.AddDirtyPlate(plate);
        }

        private IEnumerator CountdownTimer(int timeInSeconds)
        {
            var timeRemaining = timeInSeconds;
            while (timeRemaining > 0)
            {
                timeRemaining--;
                yield return OneSecondWait;
                OnCountdownTick?.Invoke(timeRemaining);
            }

            TimeOver();
        }

        private async void TimeOver()
        {
            Debug.Log("[GameManager] TimeOver");
            await NotificationUI.DisplayCenterNotificationAsync("Time Over!", new Color(.66f, .367f, .15f), 3f);
            
            // Take control from player
            
            // Show summary end screen - prompt to play again
            
            // Stop OrderManager
            orderManager.StopAndClear();
        }
    }
}