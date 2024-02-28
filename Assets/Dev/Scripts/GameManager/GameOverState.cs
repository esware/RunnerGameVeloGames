using Dev.Scripts.Sounds;
using Dev.Scripts.Track;
using UnityEngine;

namespace Dev.Scripts.GameManager
{
    public class GameOverState:AState
    {
        public TrackManager trackManager;
        public Canvas canvas;
        public AudioClip gameOverTheme;
        
        public override void Enter(AState from)
        {
            canvas.gameObject.SetActive(true);
            if (MusicPlayer.instance.GetStem(0)!=gameOverTheme)
            {
                MusicPlayer.instance.SetStem(0, gameOverTheme);
                StartCoroutine(MusicPlayer.instance.RestartAllStems());
            }
        }

        public override void Exit(AState to)
        {
            canvas.gameObject.SetActive(false);
            FinishRun();
        }

        public override void Tick()
        {
            
        }

        public override string GetName()
        {
            return "GameOver";
        }
        
        public void GoToLoadout()
        {
            trackManager.isRerun = false;
            manager.SwitchState("Loadout");
        }
        
        public void RunAgain()
        {
            trackManager.isRerun = false;
            manager.SwitchState("Game");
        }

        private void FinishRun()
        {
            CharacterControl.DeathEvent de = trackManager.characterController.deathData;
            //register data to analytics
#if UNITY_ANALYTICS
        AnalyticsEvent.GameOver(null, new Dictionary<string, object> {
            { "coins", de.coins },
            { "premium", de.premium },
            { "score", de.score },
            { "distance", de.worldDistance },
            { "obstacle",  de.obstacleType },
            { "theme", de.themeUsed },
            { "character", de.character },
        });
#endif

            PlayerData.Instance.Save();

            trackManager.End();
        }
    }
}