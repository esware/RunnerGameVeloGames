using System;
using System.Collections;
using System.Collections.Generic;
using Dev.Scripts.Consumables;
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
        private int _currentSegmentObstacleIndex = 0;
        private TrackSegment _nextValidSegment = null;

        public override void Enter(AState from)
        {
            _countdownRectTransform = countdownText.GetComponent<RectTransform>();
            _lifeHearts = new Image[trackManager.characterController.maxLife];
            for (int i = 0; i < trackManager.characterController.maxLife; i++)
            {
                lifeRectTransform.GetChild(i).gameObject.SetActive(true);
                _lifeHearts[i] = lifeRectTransform.GetChild(i).GetComponent<Image>();
            }

            StartGame();
        }

        private void StartGame()
        {
            canvas.gameObject.SetActive(true);
            pauseMenu.gameObject.SetActive(false);
            wholeUI.gameObject.SetActive(true);
            pauseButton.gameObject.SetActive(true);
            if (!trackManager.isRerun)
            {
                trackManager.characterController.CurrentLife = trackManager.characterController.maxLife;
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
                if (characterControl.CurrentLife<=0)
                {
                    pauseButton.gameObject.SetActive(false);
                    StartCoroutine(WaitForGameOver());
                }

                List<Consumable> toRemove = new List<Consumable>();

                for (int i = 0; i < characterControl.Consumables.Count; i++)
                {
                    
                    characterControl.Consumables[i].Tick(characterControl);
                    if (!characterControl.Consumables[i].IsActive)
                    {
                        toRemove.Add(characterControl.Consumables[i]);
                    }
                    
                }

                for (int i = 0; i < toRemove.Count; i++)
                {
                    toRemove[i].Ended(trackManager.characterController);
                    characterControl.Consumables.Remove(toRemove[i]);
                }
                UpdateUI();
            }
        }
        private void UpdateUI()
        {
            coinText.text = trackManager.characterController.Coins.ToString();

            for (int i = 0; i < trackManager.characterController.maxLife; ++i)
            {

                if(trackManager.characterController.CurrentLife > i)
                {
                    _lifeHearts[i].color = Color.white;
                }
                else
                {
                    _lifeHearts[i].color = Color.black;
                }
            }

            scoreText.text = trackManager.score.ToString();

            distanceText.text = Mathf.FloorToInt(trackManager.worldDistance).ToString() + " m";

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
            PlayerData.Instance.Save();
            manager.SwitchState ("Loadout");
        }

        private void OnApplicationQuit()
        {
            PlayerData.Instance.Save();
        }
    }
    
    
}