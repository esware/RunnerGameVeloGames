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
    public class LoadoutState : AState
    {
        #region Serialized Fields

        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject characterMenu;
        [SerializeField] private GameObject player;
        [SerializeField] private Text rankText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Text totalCoinText;

        [Header("Character UI")]
        [SerializeField] private SpriteRenderer characterBackground;
        [SerializeField] private Image characterIcon;
        [SerializeField] private Text charNameDisplay;
        [SerializeField] private RectTransform charSelect;
        [SerializeField] private Transform charPosition;
        [SerializeField] private Button runButton;
        [SerializeField] private AudioClip menuTheme;

        #endregion

        #region Private Fields

        private GameObject _character;
        private bool _isLoadingCharacter;
        private const float CharacterRotationSpeed = 45f;
        private GameObject _canvas;

        #endregion

        #region State Methods

        public override void Enter(AState from)
        {
            ActivateCanvasAndMenus();
            SetRankText();
            LoadMainMenuTheme();
            DisableRunButtonAndStartCharacterLoading();
        }

        public override void Exit(AState to)
        {
            DeactivateBackground();
            DeactivateCanvas();
            ReleaseCharacterInstance();
        }

        public override void Tick()
        {
            CheckAndEnableRunButton();
            RotateCharacter();
            UpdateCoinText();
            ToggleCharacterSelect();
        }

        public override string GetName()
        {
            return "Loadout";
        }

        #endregion

        #region Public Methods

        public void ChangeCharacter(int dir)
        {
            UpdateUsedCharacterIndex(dir);
            StartCoroutine(PopulateCharacters());
        }

        public void StartGame()
        {
            UpdateFtueLevelAndSave();
            SwitchToGameState();
        }

        #endregion

        #region Private Methods

        private void ActivateCanvasAndMenus()
        {
            _canvas = gameObject;
            if (!_canvas.activeSelf)
            {
                _canvas.SetActive(true);
            }
            characterBackground.gameObject.SetActive(true);
            mainMenu.gameObject.SetActive(false);
            characterMenu.gameObject.SetActive(true);
            player.SetActive(true);
            player.transform.position = Vector3.zero;
            player.GetComponent<CharacterMovement>().cameraController.ChangeState(CameraStates.IdleCam.ToString());
            charNameDisplay.text = "";
        }
        
        private void SetRankText()
        {
            rankText.text = "Level " + PlayerData.Instance.Rank;
        }

        private void LoadMainMenuTheme()
        {
            if (MusicPlayer.Instance.GetStem(0) == menuTheme) return;
            
            MusicPlayer.Instance.SetStem(0, menuTheme);
            StartCoroutine(MusicPlayer.Instance.RestartAllStems());
        }

        private void DisableRunButtonAndStartCharacterLoading()
        {
            runButton.interactable = false;
            runButton.GetComponentInChildren<Text>().text = "Loading...";
            StartCoroutine(PopulateCharacters());
        }

        private void CheckAndEnableRunButton()
        {
            if (runButton.interactable) return;
            
            bool interactable = CharacterDatabase.Loaded;
            if (!interactable) return;
                
            runButton.interactable = true;
            runButton.GetComponentInChildren<Text>().text = "Play";
        }

        private void RotateCharacter()
        {
            if (_character != null)
            {
                _character.transform.Rotate(0, CharacterRotationSpeed * Time.deltaTime, 0, Space.Self);
            }
        }

        private void UpdateCoinText()
        {
            totalCoinText.text = PlayerData.Instance.Coins.ToString();
        }

        private void ToggleCharacterSelect()
        {
            charSelect.gameObject.SetActive(PlayerData.Instance.Characters.Count > 1);
        }

        private void UpdateUsedCharacterIndex(int dir)
        {
            PlayerData.Instance.UsedCharacter += dir;
            if (PlayerData.Instance.UsedCharacter >= PlayerData.Instance.Characters.Count)
                PlayerData.Instance.UsedCharacter = 0;
            else if (PlayerData.Instance.UsedCharacter < 0)
                PlayerData.Instance.UsedCharacter = PlayerData.Instance.Characters.Count - 1;
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
                    Characters.Character c = CharacterDatabase.GetCharacter(PlayerData.Instance.Characters[PlayerData.Instance.UsedCharacter]);

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
                        newChar.transform.rotation = Quaternion.Euler(0, 180, 0);

                        highScoreText.text = "HighScore " + PlayerData.Instance.Highscore;

                        characterIcon.sprite = newChar.GetComponent<Characters.Character>().icon;
                        characterBackground.GetComponent<SpriteRenderer>().sprite = newChar.GetComponent<Characters.Character>().characterBg;

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

        private void UpdateFtueLevelAndSave()
        {
            if (PlayerData.Instance.FtueLevel == 1)
            {
                PlayerData.Instance.FtueLevel = 2;
                PlayerData.Instance.Save();
            }
        }

        private void SwitchToGameState()
        {
            manager.SwitchState("Game");
        }

        private void DeactivateBackground()
        {
            characterBackground.gameObject.SetActive(false);
        }

        private void DeactivateCanvas()
        {
            _canvas.gameObject.SetActive(false);
        }

        private void ReleaseCharacterInstance()
        {
            if (_character != null) Addressables.ReleaseInstance(_character);
        }

        #endregion
    }

}