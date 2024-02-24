using System;
using System.Collections;
using Dev.Scripts.Track;
using UnityEngine;
using UnityEngine.UI;

namespace Dev.Scripts.GameManager
{
    public class GameState:AState
    {
        public Canvas canvas;
        public TrackManager trackManager;
        public AudioClip gameTheme;
        [Header("UI")]
        public Text coinText;
        public Text scoreText;
        public Text distanceText;
        public Text countdownText;
        public RectTransform lifeRectTransform;

        public RectTransform pauseMenu;
        public RectTransform wholeUI;
        public Button pauseButton;
        
        public GameObject gameOverPopup;
        
        
        private bool _finished;
        private Image[] _lifeHearts;
        private RectTransform _countdownRectTransform;
        private bool _wasMoving;
        private int _maxLives = 3;
        private int _currentSegmentObstacleIndex = 0;
        private TrackSegment _nextValidSegment = null;
        private int _obstacleToClear = 3;
        public override void Enter(AState from)
        {
            _countdownRectTransform = countdownText.GetComponent<RectTransform>();
            _lifeHearts = new Image[_maxLives];
            for (int i = 0; i < _maxLives; i++)
            {
                _lifeHearts[i] = lifeRectTransform.GetChild(i).GetComponent<Image>();
            }

            StartGame();
        }

        private void StartGame()
        {
            canvas.gameObject.SetActive(false);
            pauseMenu.gameObject.SetActive(false);
            wholeUI.gameObject.SetActive(true);
            pauseButton.gameObject.SetActive(!trackManager.isTutorial);
            if (!trackManager.isRerun)
            {
                trackManager.characterController.currentLife = _maxLives;
                trackManager.ClearSegments();
            }
            _finished = false;
            StartCoroutine(trackManager.Begin());
        }

        public override void Exit(AState to)
        {
            canvas.gameObject.SetActive(false);
        }

        public override void Tick()
        {
            if (trackManager.isLoaded)
            {
                CharacterControl characterControl = trackManager.characterController;
                if (characterControl.currentLife<=0)
                {
                    pauseButton.gameObject.SetActive(false);
                    StartCoroutine(WaitForGameOver());
                }
                UpdateUI();
            }
        }
        private void UpdateUI()
        {
            coinText.text = trackManager.characterController.coins.ToString();

            for (int i = 0; i < 3; ++i)
            {

                if(trackManager.characterController.currentLife > i)
                {
                    _lifeHearts[i].color = Color.white;
                }
                else
                {
                    _lifeHearts[i].color = Color.black;
                }
            }

            scoreText.text = trackManager.score.ToString();

            distanceText.text = Mathf.FloorToInt(trackManager.worldDistance).ToString() + "m";

            if (trackManager.timeToStart >= 0)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = Mathf.Ceil(trackManager.timeToStart).ToString();
                _countdownRectTransform.localScale = Vector3.one * (1.0f - (trackManager.timeToStart - Mathf.Floor(trackManager.timeToStart)));
            }
            else
            {
                _countdownRectTransform.localScale = Vector3.zero;
            }
        }
        private IEnumerator WaitForGameOver()
        {
            _finished = true;
            trackManager.StopMove();
            yield return new WaitForSeconds(2.0f);
            manager.SwitchState("GameOver");
        }
        
        private void OpenGameOverPopup()
        {
            gameOverPopup.SetActive(true);
        }

        public override string GetName()
        {
            return "Game";
        }
        public void Resume()
        {
            Time.timeScale = 1.0f;
            pauseButton.gameObject.SetActive(true);
            pauseMenu.gameObject.SetActive (false);
            wholeUI.gameObject.SetActive(true);
            if (_wasMoving)
            {
                trackManager.StartMove(false);
            }

            AudioListener.pause = false;
        }
        public void Pause(bool displayMenu = true)
        {
            //check if we aren't finished OR if we aren't already in pause (as that would mess states)
            if (_finished || AudioListener.pause == true)
                return;

            AudioListener.pause = true;
            Time.timeScale = 0.0f;

            pauseButton.gameObject.SetActive(false);
            pauseMenu.gameObject.SetActive (displayMenu);
            wholeUI.gameObject.SetActive(false);
            _wasMoving = trackManager.isMoving;
            trackManager.StopMove();
        }
        
        public void QuitToLoadout()
        {
            Time.timeScale = 1.0f;
            AudioListener.pause = false;
            trackManager.End();
            trackManager.isRerun = false;
            PlayerData.instance.Save();
            manager.SwitchState ("Loadout");
        }
    }
    
    
}