using System;
using System.Threading.Tasks;
using Undercooked.Appliances;
using Undercooked.Data;
using Undercooked.Model;
using Undercooked.Player;
using Undercooked.UI;
using Undercooked.Utils;
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
        
        private const int BaseScorePerPlate = 20;
        private const int PenaltyExpiredScore = 10;
        private const float TimeToReturnPlateSeconds = 3f;
        private const int OneSecondTick = 1;

        private readonly TimeSpan _returnPlateDuration = TimeSpan.FromSeconds(TimeToReturnPlateSeconds);
        private readonly TimeSpan _oneSecondTick = TimeSpan.FromSeconds(OneSecondTick);
        private static int _score;
        private int _timeRemaining;
        private InputAction _startAtPlayerAction;
        private bool _hasSubscribedPlayerActions;
        private bool _hasSubscribedMenuActions;

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

        private void OnDestroy()
        {
            _userPressedStart = true;
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
            
            while (!_userPressedStart)
            {
                await Task.Delay(1000);
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
            cameraManager.FocusFirstPlayer();
            
            Score = 0;
            _timeRemaining = levelData.durationTime;
            
            await DisplayInitialNotifications();
            orderManager.Init(levelData);

            OnLevelStart?.Invoke();

            inputController.EnableGameplayControls();
            inputController.EnableFirstPlayerController();

            inputController.OnStartPressedAtPlayer += HandlePausePressed;
            
            await CountdownTimerAsync(_timeRemaining);
        }

        private static void HandlePausePressed()
        {
            MenuPanelUI.PauseUnpause();
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

        private static void HandleRestartButton()
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(sceneIndex);
        }

        private static void HandleQuitButton()
        {
            Application.Quit();
        }

        private static void HandleOrderDelivered(Order order, int tipCalculated)
        {
            Score += BaseScorePerPlate + tipCalculated;
        }
        
        private static void HandleOrderExpired(Order order)
        {
            Score -= PenaltyExpiredScore;
        }

        public void Pause()
        {
            // player controller ignore input
            MenuPanelUI.PauseUnpause();
        }

        public void Unpause()
        {
            // restore player input (event?)
            MenuPanelUI.PauseUnpause();
        }
        
        private Task HandlePlateDropped(Plate plate)
        {
            if (!plate.IsEmpty() && plate.IsClean)
            {
                orderManager.CheckIngredientsMatchOrder(plate.Ingredients);
            }

            plate.RemoveAllIngredients();
            return ReturnPlateDirtyAsync(plate);
        }
        
        private async Task ReturnPlateDirtyAsync(Plate plate)
        {
            plate.gameObject.SetActive(true);
            plate.SetDirty();
            await Task.Delay(_returnPlateDuration);
            dishTray.AddDirtyPlate(plate);
        }

        private async Task CountdownTimerAsync(int timeInSeconds)
        {
            var timeRemaining = timeInSeconds;
            while (timeRemaining > 0)
            {
                timeRemaining--;
                await Task.Delay(_oneSecondTick);
                OnCountdownTick?.Invoke(timeRemaining);
            }

            OnTimeIsOver?.Invoke();
            await TimeOver();
        }

        private async Task TimeOver()
        {
            // Take control from player
            inputController.DisableAllPlayerControllers();
            
            await NotificationUI.DisplayCenterNotificationAsync(
                textToDisplay: "Time Over!",
                outlineColor: new Color(.66f, .367f, .15f),
                timeToDisplayInSeconds: 3f);

            inputController.OnStartPressedAtMenu -= HandlePausePressed;
            inputController.EnableMenuControls();

            MenuPanelUI.GameOverMenu();

            orderManager.StopAndClear();
        }
    }
}