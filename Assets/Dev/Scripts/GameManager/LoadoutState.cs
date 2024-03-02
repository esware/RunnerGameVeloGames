using System;
using System.Collections;
using Characters;
using Dev.Scripts.Camera;
using Dev.Scripts.Characters;
using Dev.Scripts.Sounds;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Dev.Scripts.GameManager
{
    public class LoadoutState:AState
    {
       
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject characterMenu;
        [SerializeField] private GameObject player;
        [SerializeField] private Text rankText;
        [SerializeField] private Text highScoreText;
        
        [Header("Character UI")] 
        [SerializeField] private SpriteRenderer characterBackground;
        [SerializeField] private Image characterIcon;
        [SerializeField] private Text charNameDisplay;
        [SerializeField] private RectTransform charSelect;
        [SerializeField] private Transform charPosition;
        [SerializeField] private Button runButton;
        [SerializeField] private AudioClip menuTheme;
        
        private GameObject _character;
        private bool _isLoadingCharacter;
        private const float CharacterRotationSpeed = 45f;
        private GameObject _canvas;
        public override void Enter(AState from)
        {
            _canvas = gameObject;
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            
            characterBackground.gameObject.SetActive(true);
            
            mainMenu.gameObject.SetActive(false);
            characterMenu.gameObject.SetActive(true);
            player.SetActive(true);
            player.transform.position = Vector3.zero;
            player.GetComponent<CharacterMovement>().cameraController.ChangeState(CameraStates.IdleCam.ToString());
            
            charNameDisplay.text = "";
            
            if (MusicPlayer.instance.GetStem(0) != menuTheme)
            {
                MusicPlayer.instance.SetStem(0, menuTheme);
                StartCoroutine(MusicPlayer.instance.RestartAllStems());
            }

            rankText.text = "Level " + PlayerData.Instance.rank;
            
            runButton.interactable = false;
            runButton.GetComponentInChildren<Text>().text = "Loading...";
            StartCoroutine(PopulateCharacters());
        }
        public override void Exit(AState to)
        {
            characterBackground.gameObject.SetActive(false);
            _canvas.gameObject.SetActive(false);
            if (_character != null) Addressables.ReleaseInstance(_character);
        }

        public override void Tick()
        {
            if (!runButton.interactable)
            {
                bool interactable = CharacterDatabase.loaded;
                if(interactable)
                {
                    runButton.interactable = true;
                    runButton.GetComponentInChildren<Text>().text = "Play";
                }
            }
            
            if(_character != null)
            {
                _character.transform.Rotate(0, CharacterRotationSpeed * Time.deltaTime, 0, Space.Self);
            }

            charSelect.gameObject.SetActive(PlayerData.Instance.characters.Count > 1);
        }

        public override string GetName()
        {
            return "Loadout";
        }
        
        public void ChangeCharacter(int dir)
        {
            PlayerData.Instance.usedCharacter += dir;
            if (PlayerData.Instance.usedCharacter >= PlayerData.Instance.characters.Count)
                PlayerData.Instance.usedCharacter = 0;
            else if(PlayerData.Instance.usedCharacter < 0)
                PlayerData.Instance.usedCharacter = PlayerData.Instance.characters.Count-1;

            StartCoroutine(PopulateCharacters());
        }
        private IEnumerator PopulateCharacters()
        {
	        yield return new WaitForSeconds(.5f);

            if (!_isLoadingCharacter)
            {
                _isLoadingCharacter = true;
                GameObject newChar = null;
                while (newChar == null)
                {
                    Characters.Character c = CharacterDatabase.GetCharacter(PlayerData.Instance.characters[PlayerData.Instance.usedCharacter]);

                    if (c != null)
                    {
                        AsyncOperationHandle op = Addressables.InstantiateAsync(c.characterName);
                        yield return op;
                        if (op.Result == null || !(op.Result is GameObject))
                        {
                            Debug.LogWarning(string.Format("Unable to load character {0}.", c.characterName));
                            yield break;
                        }
                        newChar = op.Result as GameObject;
                        newChar.transform.SetParent(charPosition, false);
                        newChar.transform.rotation = Quaternion.Euler(0,180,0);

                        foreach (var highscore in PlayerData.Instance.highscores)
                        {
                            if (highscore.name == newChar.name)
                            {
                                highScoreText.text = "HighScore :" + highscore.score;
                                Debug.Log(newChar.name+highscore.score);
                            }
                        }
                        
                        characterIcon.sprite = newChar.GetComponent<Characters.Character>().icon;
                        characterBackground.GetComponent<SpriteRenderer>().sprite = newChar.GetComponent<Characters.Character>().CharacterBg;

                        if (_character != null)
                            Addressables.ReleaseInstance(_character);

                        _character = newChar;
                        charNameDisplay.text = c.characterName;

                        _character.transform.localPosition = Vector3.right * 1000;
                        yield return new WaitForEndOfFrame();
                        _character.transform.localPosition = Vector3.zero;
                    }
                    else
                        yield return new WaitForSeconds(1.0f);
                }
                _isLoadingCharacter = false;
            }
	    }
        
        public void StartGame()
        {
            if (PlayerData.Instance.ftueLevel == 1)
            {
                PlayerData.Instance.ftueLevel = 2;
                PlayerData.Instance.Save();
            }
            manager.SwitchState("Game");
            GameEvents.GameStartEvent?.Invoke();
        }
    }
}