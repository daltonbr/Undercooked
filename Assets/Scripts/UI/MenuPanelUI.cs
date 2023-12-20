using Lean.Transition;
using TMPro;
using Undercooked.Data;
using Undercooked.Managers;
using Undercooked.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Undercooked.UI
{
    public class MenuPanelUI : Singleton<MenuPanelUI>
    {
        [Header("InitialMenu")]
        [SerializeField] private GameObject initialMenu;
        private CanvasGroup _initalMenuCanvasGroup;
        [Space]
        
        [Header("PauseMenu")]
        [SerializeField] private GameObject pauseMenu;
        private CanvasGroup _pauseMenuCanvasGroup;
        
        [Header("Buttons")]
        [SerializeField] private GameObject firstSelectedPauseMenu;
        [SerializeField] private Button resumeButton_Pause;
        [SerializeField] private Button restartButton_Pause;
        [SerializeField] private Button quitButton_Pause;
        [Space]
        
        [Header("GameOverMenu")]
        [SerializeField] private GameObject gameOverMenu;
        private CanvasGroup _gameOverMenuCanvasGroup;
        [SerializeField] private GameObject firstSelectedGameOverMenu;
        [SerializeField] private AudioClip successClip;
        
        [Header("Buttons")]
        [SerializeField] private Button restartButton_GameOver;
        [SerializeField] private Button quitButton_GameOver;

        [Header("GameOver Stars")]
        [SerializeField] private Image star1;
        [SerializeField] private Image star2;
        [SerializeField] private Image star3;
        [SerializeField] private TextMeshProUGUI scoreStar1Text;
        [SerializeField] private TextMeshProUGUI scoreStar2Text;
        [SerializeField] private TextMeshProUGUI scoreStar3Text;
        [SerializeField] private TextMeshProUGUI scoreText;

        public delegate void ButtonPressed();
        public static ButtonPressed OnResumeButton;
        public static ButtonPressed OnRestartButton;
        public static ButtonPressed OnQuitButton;
        
        
        private void Awake()
        {
            _initalMenuCanvasGroup = initialMenu.GetComponent<CanvasGroup>();
            _pauseMenuCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
            _gameOverMenuCanvasGroup = gameOverMenu.GetComponent<CanvasGroup>();
            
            #if UNITY_EDITOR
                Assert.IsNotNull(initialMenu);
                Assert.IsNotNull(pauseMenu);
                Assert.IsNotNull(gameOverMenu);
                Assert.IsNotNull(_initalMenuCanvasGroup);
                Assert.IsNotNull(_gameOverMenuCanvasGroup);
                Assert.IsNotNull(_pauseMenuCanvasGroup);
                
                Assert.IsNotNull(resumeButton_Pause);
                Assert.IsNotNull(restartButton_Pause);
                Assert.IsNotNull(quitButton_Pause);
                Assert.IsNotNull(restartButton_GameOver);
                Assert.IsNotNull(quitButton_GameOver);
                
                Assert.IsNotNull(star1);
                Assert.IsNotNull(star2);
                Assert.IsNotNull(star3);
                Assert.IsNotNull(scoreStar1Text);
                Assert.IsNotNull(scoreStar2Text);
                Assert.IsNotNull(scoreStar3Text);
                Assert.IsNotNull(scoreText);
            #endif
            
            initialMenu.SetActive(false);
            pauseMenu.SetActive(false);
            gameOverMenu.SetActive(false);
            _initalMenuCanvasGroup.alpha = 0f;
            _pauseMenuCanvasGroup.alpha = 0f;
            _gameOverMenuCanvasGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            AddButtonListeners();
        }

        private void OnDisable()
        {
            RemoveButtonListeners();
        }

        private void AddButtonListeners()
        {
            resumeButton_Pause.onClick.AddListener(HandleResumeButton);
            restartButton_Pause.onClick.AddListener(HandleRestartButton);
            quitButton_Pause.onClick.AddListener(HandleQuitButton);
            restartButton_GameOver.onClick.AddListener(HandleRestartButton);
            quitButton_GameOver.onClick.AddListener(HandleQuitButton);
        }

        private void RemoveButtonListeners()
        {
            resumeButton_Pause.onClick.RemoveAllListeners();
            restartButton_Pause.onClick.RemoveAllListeners();
            quitButton_Pause.onClick.RemoveAllListeners();
            restartButton_GameOver.onClick.RemoveAllListeners();
            quitButton_GameOver.onClick.RemoveAllListeners();
        }

        private static void HandleResumeButton()
        {
            OnResumeButton?.Invoke();
        }

        private static void HandleRestartButton()
        {
            GameOverMenu();
            OnRestartButton?.Invoke();
        }

        private static void HandleQuitButton()
        {
            OnQuitButton?.Invoke();
        }

        public static void InitialMenuSetActive(bool active)
        {
            Instance.initialMenu.SetActive(active);
            Instance._initalMenuCanvasGroup.alphaTransition(active ? 1f : 0f, 2f);
        }
        
        public static void PauseUnpause()
        {
            if (Instance.pauseMenu.activeInHierarchy == false)
            {
                Instance.pauseMenu.SetActive(true);
                Time.timeScale = 0;
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(Instance.firstSelectedPauseMenu);
                
                Instance.pauseMenu.SetActive(true);
                Instance._pauseMenuCanvasGroup.alphaTransition(1f, .5f);
            }
            else
            {
                Instance.pauseMenu.SetActive(false);
                Instance._pauseMenuCanvasGroup
                    .alphaTransition(0f, .5f)
                    .JoinTransition()
                    .EventTransition(() => Instance.pauseMenu.SetActive(false))
                    .EventTransition(() => Time.timeScale = 1);
            }            
        }

        public static void GameOverMenu()
        { 
            if (Instance.gameOverMenu.activeInHierarchy == false)
            {
                if (Instance.pauseMenu.activeInHierarchy)
                {
                    PauseUnpause();
                }

                Instance.gameOverMenu.SetActive(true);
                Time.timeScale = 0;
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(Instance.firstSelectedGameOverMenu);   
                
                Instance.gameOverMenu.SetActive(true);
                Instance._gameOverMenuCanvasGroup.alphaTransition(1f, .5f);

                UpdateStars();
            }
            else
            {
                Instance.gameOverMenu.SetActive(false);
                Instance._gameOverMenuCanvasGroup
                    .alphaTransition(0f, .5f)
                    .JoinTransition()
                    .EventTransition(() => Instance.gameOverMenu.SetActive(false))
                    .EventTransition(() => Time.timeScale = 1);
            }
        }

        private static void UpdateStars()
        {
            int score = GameManager.Score;
            LevelData levelData = GameManager.LevelData;
            int star1Score = levelData.star1Score;
            int star2Score = levelData.star2Score;
            int star3Score = levelData.star3Score;
            Instance.scoreStar1Text.text = star1Score.ToString();
            Instance.scoreStar2Text.text = star2Score.ToString();
            Instance.scoreStar3Text.text = star3Score.ToString();
            Instance.scoreText.text = $"Score {score.ToString()}";
            
            Instance.star1.gameObject.transform.localScale = Vector3.zero;
            Instance.star2.gameObject.transform.localScale = Vector3.zero;
            Instance.star3.gameObject.transform.localScale = Vector3.zero;
            
            if (score < star1Score) return;
            
            if (score < star2Score)
            {
                Instance.star1.gameObject.transform
                    .localScaleTransition(Vector3.one, 1f, LeanEase.Bounce);
            }
            else if (score < star3Score)
            {
                Instance.star1.gameObject.transform
                    .localScaleTransition(Vector3.one, 1f, LeanEase.Bounce)
                    .JoinTransition();
                Instance.star2.gameObject.transform
                    .localScaleTransition(Vector3.one, 1f, LeanEase.Bounce);
            }
            else
            {
                Instance.star1.gameObject.transform
                    .localScaleTransition(Vector3.one, 1f, LeanEase.Bounce)
                    .JoinTransition();
                Instance.star2.gameObject.transform
                    .localScaleTransition(Vector3.one, 1f, LeanEase.Bounce)
                    .JoinTransition();
                Instance.star3.gameObject.transform
                    .localScaleTransition(Vector3.one, 1f, LeanEase.Bounce);
            }
        }
    }
    
}
