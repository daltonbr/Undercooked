using System.Collections;
using System.Threading.Tasks;
using DaltonLima.Core;
using Undercooked.Appliances;
using Undercooked.Data;
using Undercooked.Model;
using Undercooked.Player;
using Undercooked.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Undercooked.Managers
{
    [RequireComponent(typeof(OrderManager))]
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private DishTray dishTray;
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private LevelData level1;
        [SerializeField] private CameraManager cameraManager;
        [SerializeField] private InputController inputController;
        [SerializeField] private PlayerInput playerInput;
        
        private const int BaseScorePerPlate = 20;
        private const int PenaltyExpiredScore = 10;
        private const float TimeToReturnPlateSeconds = 3f;
        
        private Coroutine _countdownCoroutine;
        private readonly WaitForSeconds _timeToReturnPlate = new WaitForSeconds(TimeToReturnPlateSeconds);
        private readonly WaitForSeconds _oneSecondWait = new WaitForSeconds(1f);
        private static int _score;
        private int _timeRemaining;

        public static int Score
        {
            get => _score;
            private set
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

        public static LevelData LevelData => Instance.level1; 

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
                Assert.IsNotNull(cameraManager);
                Assert.IsNotNull(inputController);
            #endif
        }

        private async void Start()
        {
            await GameLoop();
        }

        private async Task GameLoop()
        {
            await StartMainMenuAsync();  // maybe move this out to Start()
            await StartLevelAsync(level1);
        }

        private bool _userPressedStart;
        
        private async Task StartMainMenuAsync()
        {
            await Task.Delay(1000);
            MenuPanelUI.InitialMenuSetActive(true);
            cameraManager.SwitchDollyCamera();
            
            // activate MenuControls
            inputController.EnableMenuControls();
            inputController.OnStartPressedAtMenu += HandleStartAtMenu;
            
            while (_userPressedStart == false)
            {
                Debug.Log("[GameManager] Wait for start...");
                await Task.Delay(2000);
            }
            MenuPanelUI.InitialMenuSetActive(false);
            inputController.OnStartPressedAtMenu -= HandleStartAtMenu;
        }

        private void HandleStartAtMenu()
        {
            _userPressedStart = true;
        }

        private async Task StartLevelAsync(LevelData levelData)
        {
            //_startAtMenuAction = playerInput.currentActionMap["Start@Menu"];
            // _startAtMenuAction.performed += HandleStart;
            
            cameraManager.FocusFirstPlayer();
            
            Score = 0;
            _timeRemaining = levelData.durationTime;
            
            await DisplayInitialNotifications();
            orderManager.Init(levelData);

            OnLevelStart?.Invoke();
            // Unlock player movement
            inputController.EnableGameplayControls();
            inputController.EnableFirstPlayerController();

            inputController.OnStartPressedAtPlayer += HandlePausePressed;
            
            _countdownCoroutine = StartCoroutine(CountdownTimer(_timeRemaining));

            //TODO: handle pause (pause timer coroutine and restart)
        }

        private void HandlePausePressed()
        {
            MenuPanelUI.PauseUnpause();
        }

        private InputAction _startAtPlayerAction;
        private bool _hasSubscribedPlayerActions;
        private bool _hasSubscribedMenuActions;
        
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

            MenuPanelUI.OnQuitButton += HandleQuitButton;
            MenuPanelUI.OnRestartButton += HandleRestartButton;
            MenuPanelUI.OnResumeButton += HandleResumeButton;
        }

        private void OnDisable()
        {
            DeliverCountertop.OnPlateDropped -= HandlePlateDropped;
            OrderManager.OnOrderExpired -= HandleOrderExpired;
            OrderManager.OnOrderDelivered -= HandleOrderDelivered;
            
            MenuPanelUI.OnQuitButton -= HandleQuitButton;
            MenuPanelUI.OnRestartButton -= HandleRestartButton;
            MenuPanelUI.OnResumeButton -= HandleResumeButton;
        }

        private static void HandleResumeButton()
        {
            MenuPanelUI.PauseUnpause();
        }

        private void HandleRestartButton()
        {
            //Start();
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            //SceneManager.UnloadSceneAsync(sceneIndex);
            SceneManager.LoadScene(sceneIndex);
        }

        private static void HandleQuitButton()
        {
            Application.Quit();
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
            //Time.timeScale = 0;
            MenuPanelUI.PauseUnpause();
            //StopCoroutine(_countdownCoroutine);
        }

        public void Unpause()
        {
            // restore player input (event?)
            //Time.timeScale = 1;
            MenuPanelUI.PauseUnpause();
            //_countdownCoroutine = StartCoroutine(CountdownTimer(_timeRemaining));
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
            yield return _timeToReturnPlate;
            dishTray.AddDirtyPlate(plate);
        }

        private IEnumerator CountdownTimer(int timeInSeconds)
        {
            var timeRemaining = timeInSeconds;
            while (timeRemaining > 0)
            {
                timeRemaining--;
                yield return _oneSecondWait;
                OnCountdownTick?.Invoke(timeRemaining);
            }

            TimeOver();
        }

        private async void TimeOver()
        {
            // Take control from player
            inputController.DisableAllPlayerControllers();
            
            await NotificationUI.DisplayCenterNotificationAsync("Time Over!", new Color(.66f, .367f, .15f), 3f);

            inputController.OnStartPressedAtMenu -= HandlePausePressed;
            inputController.EnableMenuControls();

            // pause time?
            //Time.timeScale = 0;
            MenuPanelUI.GameOverMenu();
            
            // Stop OrderManager
            orderManager.StopAndClear();
            
            // TODO: Show summary end screen - prompt to play again
            
            
        }
    }
}