using System;
using System.Collections;
using Dev.Scripts.Characters;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Dev.Scripts.GameManager
{
    public class LoadoutState:AState
    {
        [SerializeField] private Canvas mainMenu;
        [SerializeField] private Canvas characterMenu;
        [SerializeField] private GameObject player;
        
        [Header("Character UI")] 
        [SerializeField] private Text charNameDisplay;
        [SerializeField] private RectTransform charSelect;
        [SerializeField] private Transform charPosition;
        [SerializeField] private Image characterBg;
        [SerializeField] private Button runButton;
        
        private GameObject _character;
        private bool _isLoadingCharacter;
        private const float CharacterRotationSpeed = 45f;
        public override void Enter(AState from)
        {
            mainMenu.gameObject.SetActive(PlayerData.instance.tutorialDone);
            characterMenu.gameObject.SetActive(!PlayerData.instance.tutorialDone);
            player.transform.position = Vector3.zero;
            runButton.interactable = false;
            runButton.GetComponentInChildren<Text>().text = "Loading...";
            
        }
        public override void Exit(AState to)
        {
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
                }
            }
            
            if(_character != null)
            {
                _character.transform.Rotate(0, CharacterRotationSpeed * Time.deltaTime, 0, Space.Self);
            }

            charSelect.gameObject.SetActive(PlayerData.instance.characters.Count > 1);
        }

        public override string GetName()
        {
            return "Loadout";
        }
        
        public void ChangeCharacter(int dir)
        {
            PlayerData.instance.usedCharacter += dir;
            if (PlayerData.instance.usedCharacter >= PlayerData.instance.characters.Count)
                PlayerData.instance.usedCharacter = 0;
            else if(PlayerData.instance.usedCharacter < 0)
                PlayerData.instance.usedCharacter = PlayerData.instance.characters.Count-1;

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
                Characters.Character c = CharacterDatabase.GetCharacter(PlayerData.instance.characters[PlayerData.instance.usedCharacter]);

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
                    //newChar.gameObject.SetActive(false);
                    //videoPlayer.clip = newChar.GetComponent<Character>().characterVideo;
                    characterBg.sprite = newChar.GetComponent<Characters.Character>().CharacterBg;
                    //videoPlayer.Play();

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
            if (PlayerData.instance.tutorialDone)
            {
                if (PlayerData.instance.ftueLevel == 1)
                {
                    PlayerData.instance.ftueLevel = 2;
                    PlayerData.instance.Save();
                }
            }
            manager.SwitchState("Game");
            GameEvents.GameStartEvent?.Invoke();
        }
    }
}