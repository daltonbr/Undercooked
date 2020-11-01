using System.Collections;
using System.Threading.Tasks;
using Undercooked.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace Undercooked
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private DishTray dishTray;

        private int _score;
        private int _timeRemaining;

        public int Score
        {
            get => _score;
            private set
            {
                _score = value;
                OnScoreUpdate?.Invoke(_score);
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

        private const int BaseScorePerPlate = 20;
        //TODO: how to compute tips (0/2/4/6) ? consider time remaining
        
        private readonly WaitForSeconds TimeToReturnPlate = new WaitForSeconds(3f);
        private readonly WaitForSeconds OneSecondWait = new WaitForSeconds(1f);

        public delegate void CountdownTick(int timeRemaining);
        public static event CountdownTick OnCountdownTick;
        public delegate void ScoreUpdate(int score);
        public static event ScoreUpdate OnScoreUpdate;
        public delegate void DisplayNotification(string textToDisplay, Color color, float timeToDisplay);
        public static event DisplayNotification OnDisplayNotification;
        

        [SerializeField] private LevelData level1;
        
        private Coroutine _countdownCoroutine;
        
        private void Awake()
        {
            #if UNITY_EDITOR
            Assert.IsNotNull(dishTray);
            #endif
        }

        private void Start()
        {
            StartLevelAsync(level1);
        }

        private async Task StartLevelAsync(LevelData levelData)
        {
            Score = 0;
            _timeRemaining = levelData.time;
            // Start Order spawner
            
            // Unlock player movement
            
            // Countdown - Ready - Go
            // NotificationUI.DisplayNotification("Ready?", new Color(.66f, .367f, .15f), 2f);
            // NotificationUI.DisplayNotification("GO", new Color(.333f, .733f, .196f), 2f);
            
            await StartLevelAsync();
            
            _countdownCoroutine = StartCoroutine(CountdownTimer(_timeRemaining));
            
            //TODO: handle pause (pause timer coroutine and restart)
        }

        private static async Task StartLevelAsync()
        {
            await NotificationUI.DisplayNotificationAsync("Ready?", new Color(.66f, .367f, .15f), 2f);
            await NotificationUI.DisplayNotificationAsync("GO", new Color(.333f, .733f, .196f), 2f);
        }
        
        private void OnEnable()
        {
            DeliverCountertop.OnPlateDropped += HandlePlateDropped;
        }

        private void OnDisable()
        {
            DeliverCountertop.OnPlateDropped -= HandlePlateDropped;
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
            Debug.Log("[GameManager] Plate has been dropped");
            
            // Evaluate plate
            EvaluatePlate(plate);
            //TODO: improve this cycle
            // add score if is the case
                // check for tips
            // play Visual FX's
                // DeliverCounterTop (event?) 
                // update Score (event)
                // update and animate Order UI (event)

                plate.RemoveAllIngredients();
                StartCoroutine(ReturnPlateDirty(plate));
        }

        private void EvaluatePlate(Plate plate)
        {
            if (plate.IsEmpty())
            {
                Debug.Log("[GameManager] Plate is empty");
            }

            if (plate.IsClean == false)
            {
                Debug.Log("[GameManager] Plate is dirty");
            }

            if (Plate.CheckSoupIngredients(plate.Ingredients))
            {
                Score += 20;
            }
        }

        private IEnumerator ReturnPlateDirty(Plate plate)
        {
            plate.gameObject.SetActive(true);
            plate.SetDirty();
            yield return TimeToReturnPlate;
            // delete all ingredients if any
            
            dishTray.AddDirtyPlate(plate);
            
            // reenable the plate in DishTray, we could pile it as well, like Sink
        }

        private IEnumerator CountdownTimer(int timeInSeconds)
        {
            var timeRemaining = timeInSeconds;
            while (timeRemaining > 0)
            {
                timeRemaining--;
                OnCountdownTick?.Invoke(timeRemaining);
                yield return OneSecondWait;
            }

            TimeOver();
        }

        private static async void TimeOver()
        {
            Debug.Log("[GameManager] TimeOver");
            await NotificationUI.DisplayNotificationAsync("Time Over!", new Color(.66f, .367f, .15f), 3f);
        }
    }
}