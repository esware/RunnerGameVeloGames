using System;
using System.Collections;
using System.Collections.Generic;
using Dev.Scripts.Consumables;
using Dev.Scripts.Track;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace Dev.Scripts.GameManager
{
    public class GameState : AState
    {
        #region Serialized Fields

        public Canvas canvas;
        public AudioClip gameTheme;

        [Header("UI Settings")]
        [SerializeField] private Text coinText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text distanceText;
        [SerializeField] private Text countdownText;
        [SerializeField] private Text scoreMultiplierText;
        [SerializeField] private RectTransform lifeRectTransform;

        [SerializeField] private RectTransform pauseMenu;
        [SerializeField] private RectTransform wholeUI;
        [SerializeField] private Button pauseButton;

        #endregion

        #region Private Fields

        private bool _finished;
        private Image[] _lifeHearts;
        private RectTransform _countdownRectTransform;
        private bool _wasMoving;

        #endregion

        #region State Methods

        public override void Enter(AState from)
        {
            InitializeHearts();
            StartGame();
        }

        public override void Exit(AState to)
        {
            DeactivateCanvas();
        }
        
        public override void Tick()
        {
            if (trackManager.IsLoaded)
            {
                UpdateGameState();
            }
        }

        public override string GetName()
        {
            return "Game";
        }

        #endregion

        #region UI Button Methods

        public void Resume()
        {
            ResumeGame();
        }

        public void Pause(bool displayMenu = true)
        {
            PauseGame(displayMenu);
        }

        public void QuitToLoadout()
        {
            QuitGameToLoadout();
        }

        #endregion

        #region Private Methods

        #region Update UI Text

        private void UpdateCoinText()
        {
            coinText.text = trackManager.characterController.Coins.ToString();
        }

        private void UpdateMultiplierText()
        {
            scoreMultiplierText.text = trackManager.Multiplier + "x";
        }

        private void UpdateLifeHearts()
        {
            for (int i = 0; i < trackManager.characterController.maxLife; ++i)
            {
                _lifeHearts[i].color = trackManager.characterController.CurrentLife > i ? Color.red : Color.black;
            }
        }

        private void UpdateScoreText()
        {
            scoreText.text = trackManager.Score.ToString();
        }

        private void UpdateDistanceText()
        {
            distanceText.text = Mathf.FloorToInt(trackManager.WorldDistance).ToString() + " m";
        }

        private void UpdateCountdownText()
        {
            if (trackManager.TimeToStart >= 0)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = Mathf.Ceil(trackManager.TimeToStart).ToString();
                _countdownRectTransform.localScale = Vector3.one * (1.0f - (trackManager.TimeToStart - Mathf.Floor(trackManager.TimeToStart)));
            }
            else
            {
                _countdownRectTransform.localScale = Vector3.zero;
            }
        }

        #endregion
        
        private void InitializeHearts()
        {
            _countdownRectTransform = countdownText.GetComponent<RectTransform>();
            _lifeHearts = new Image[trackManager.characterController.maxLife];
            for (int i = 0; i < trackManager.characterController.maxLife; i++)
            {
                lifeRectTransform.GetChild(i).gameObject.SetActive(true);
                _lifeHearts[i] = lifeRectTransform.GetChild(i).GetComponent<Image>();

            }
        }
        private void StartGame()
        {
            ActivateGameUI();
            SetupGame();
        }
        private void ActivateGameUI()
        {
            canvas.gameObject.SetActive(true);
            pauseMenu.gameObject.SetActive(false);
            wholeUI.gameObject.SetActive(true);
            pauseButton.gameObject.SetActive(true);
        }
        private void DeactivateCanvas()
        {
            canvas.gameObject.SetActive(false);
        }
        private void SetupGame()
        {
            if (!trackManager.IsRerun)
            {
                trackManager.characterController.CurrentLife = trackManager.characterController.maxLife;
                trackManager.ClearSegments();
            }
            _finished = false;
            StartCoroutine(trackManager.Begin());
        }
        private void UpdateGameState()
        {
            CharacterControl characterControl = trackManager.characterController;
            if (characterControl.CurrentLife <= 0)
            {
                HandleGameOver();
            }
            else
            {
                UpdateConsumables();
                UpdateUI();
            }
        }
        private void HandleGameOver()
        {
            StartCoroutine(WaitForGameOver());
        }
        private void UpdateConsumables()
        {
            List<Consumable> toRemove = new List<Consumable>();

            for (int i = 0; i < trackManager.characterController.Consumables.Count; i++)
            {
                trackManager.characterController.Consumables[i].Tick(trackManager.characterController);
                if (!trackManager.characterController.Consumables[i].IsActive)
                {
                    toRemove.Add(trackManager.characterController.Consumables[i]);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                toRemove[i].Ended(trackManager.characterController);
                trackManager.characterController.Consumables.Remove(toRemove[i]);
            }
        }
        private void UpdateUI()
        {
            UpdateCoinText();
            UpdateMultiplierText();
            UpdateLifeHearts();
            UpdateScoreText();
            UpdateDistanceText();
            UpdateCountdownText();
        }
        private IEnumerator WaitForGameOver()
        {
            _finished = true;
            trackManager.StopMove();
            yield return new WaitForSeconds(2.0f);
            manager.SwitchState("GameOver");
        }
        private void PauseGame(bool displayMenu)
        {
            if (_finished || AudioListener.pause == true)
                return;

            AudioListener.pause = true;
            Time.timeScale = 0.0f;

            pauseButton.gameObject.SetActive(false);
            pauseMenu.gameObject.SetActive(displayMenu);
            wholeUI.gameObject.SetActive(false);
            _wasMoving = trackManager.IsMoving;
            trackManager.StopMove();
        }
        private void ResumeGame()
        {
            Time.timeScale = 1.0f;
            pauseButton.gameObject.SetActive(true);
            pauseMenu.gameObject.SetActive(false);
            wholeUI.gameObject.SetActive(true);
            if (_wasMoving)
            {
                trackManager.StartMove(false);
            }

            AudioListener.pause = false;
        }
        private void QuitGameToLoadout()
        {
            Time.timeScale = 1.0f;
            AudioListener.pause = false;
            trackManager.End();
            trackManager.IsRerun = false;
            PlayerData.Instance.Save();
            manager.SwitchState("Loadout");
        }

        #endregion
    }

    
    
}