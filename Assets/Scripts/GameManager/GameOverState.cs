using Dev.Scripts.Sounds;
using Dev.Scripts.Track;
using UnityEngine;

namespace Dev.Scripts.GameManager
{
    public class GameOverState : AState
    {
        #region Public Variables

        public Canvas gameOverCanvas;
        public AudioClip gameOverTheme;

        #endregion

        #region State Methods

        public override void Enter(AState from)
        {
            ActivateGameOverCanvas();
            CheckAndSetGameOverTheme();
        }

        public override void Exit(AState to)
        {
            DeactivateGameOverCanvas();
            FinishRun();
        }

        public override void Tick()
        {
            
        }

        public override string GetName()
        {
            return "GameOver";
        }

        #endregion

        #region Public Methods

        public void GoToLoadout()
        {
            ResetRerunFlag();
            manager.SwitchState("Loadout");
        }

        public void RunAgain()
        {
            ResetRerunFlag();
            manager.SwitchState("Game");
        }

        #endregion

        #region Private Methods

        private void ActivateGameOverCanvas()
        {
            if (gameOverCanvas != null)
            {
                gameOverCanvas.gameObject.SetActive(true);
            }
        }

        private void DeactivateGameOverCanvas()
        {
            if (gameOverCanvas != null)
            {
                gameOverCanvas.gameObject.SetActive(false);
            }
        }

        private void CheckAndSetGameOverTheme()
        {
            if (gameOverTheme != null && MusicPlayer.Instance.GetStem(0) != gameOverTheme)
            {
                MusicPlayer.Instance.SetStem(0, gameOverTheme);
                StartCoroutine(MusicPlayer.Instance.RestartAllStems());
            }
        }

        private void FinishRun()
        {
            if (trackManager != null)
            {
                PlayerData.Instance.InsertScore(trackManager.Score);
                PlayerData.Instance.Save();
                trackManager.End();
            }
        }

        private void ResetRerunFlag()
        {
            if (trackManager != null)
            {
                trackManager.IsRerun = false;
            }
        }

        #endregion
    }

}