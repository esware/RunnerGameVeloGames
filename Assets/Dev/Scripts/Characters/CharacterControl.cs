using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Dev.Scripts;
using Dev.Scripts.Character;
using Dev.Scripts.Consumables;
using Dev.Scripts.Obstacles;
using Dev.Scripts.Track;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TextCore.Text;
using Dev.Scripts.Characters;
using Character = Dev.Scripts.Characters.Character;

public enum TransitionParameter
{
    Dead,
    Running,
    Jumping,
    Sliding,
    Landing,
    Hit,
    ForceTransition
}
public static class GameEvents
{
    public static Action PlayerDeathEvent;
    public static Action RunStartEvent;
    public static Action GameStartEvent;
}

public class CharacterControl : MonoBehaviour
{
    public struct DeathEvent
    {
        public string Character;
        public string ObstacleType;
        public string ThemeUsed;
        public int Coins;
        public int Premium;
        public int Score;
        public float WorldDistance;
    }
    
    public CharacterInputController inputController;
    public CharacterMovement characterMovement;
    public Character character;
    public CharacterController controller;
    public TrackManager trackManager;

    public int maxLife = 3;
    
    [Header("Sound")]
    public AudioClip coinSound;
    public AudioClip premiumSound;
    public AudioClip powerUpUseSound;
    public AudioSource powerupSource;
    
    
    
    public DeathEvent deathData { get { return _deathData; } }
    public new AudioSource audio { get { return _audio; } }
    public int coins { get { return _mCoins; } set { _mCoins = value; } }
    public int premium { get { return _mPremium; } set { _mPremium = value; } }
    public int currentLife { get { return _mCurrentLife; } set { _mCurrentLife = value; } }
    public List<Consumable> consumables => m_ActiveConsumables;
    public List<Consumable> inventory = new List<Consumable>();

    [HideInInspector]
    public List<GameObject> magnetCoins = new List<GameObject>();

    private bool _invincible;
    private DeathEvent _deathData;
    private AudioSource _audio;
    
    private const float MagnetSpeed = 10f;
    private const int CoinsLayerIndex = 8;
    private const int ObstacleLayerIndex = 9;
    private const int PowerupLayerIndex = 10;
    private const float DefaultInvinsibleTime = 2f;
    
    private int _mCoins;
    private int _redCoins;
    private int _mPremium;
    private int _mCurrentLife;

    private List<Consumable> m_ActiveConsumables = new List<Consumable>();
    private bool _mIsInvincible;

    #region Inital Methods
    
    private void Start()
    {
        _audio = GetComponent<AudioSource>();
    }
    protected void Awake ()
    {
        Init();
        _mPremium = 0;
        _mCurrentLife = 1;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }
    private void Init()
    {
        controller = GetComponent<CharacterController>();
        _invincible = false;
        currentLife = 1;

    }
    public void Begin()
    {
        m_ActiveConsumables.Clear();
    }
    public void End()
    {
        CleanConsumable();
    }
    #endregion

    public void CheatInvincible(bool invincible)
    {
        _mIsInvincible = invincible;
    }
    public bool IsCheatInvincible()
    {
        return _mIsInvincible;
    }
    public void CleanConsumable()
    {
        for (int i = 0; i < m_ActiveConsumables.Count; ++i)
        {
            m_ActiveConsumables[i].Ended(this);
            Addressables.ReleaseInstance(m_ActiveConsumables[i].gameObject);
        }

        m_ActiveConsumables.Clear();
    }
    private void UseConsumable(Consumable c)
    {
        //audio.PlayOneShot(powerUpUseSound);

        for(int i = 0; i < m_ActiveConsumables.Count; ++i)
        {
            if(m_ActiveConsumables[i].GetType() == c.GetType())
            {
                // If we already have an active consumable of that type, we just reset the time
                m_ActiveConsumables[i].ResetTime();
                Addressables.ReleaseInstance(c.gameObject);
                return;
            }
        }

        // If we didn't had one, activate that one 
        c.transform.SetParent(transform, false);
        c.gameObject.SetActive(false);

        m_ActiveConsumables.Add(c);
        StartCoroutine(c.Started(this));
    }
    
    private void Update()
    {
        if(!trackManager.isMoving)
            return;
        for(int i = 0; i < magnetCoins.Count; ++i)
        {
            if (magnetCoins[i].gameObject!=null)
            {
                magnetCoins[i].transform.position = Vector3.MoveTowards(magnetCoins[i].transform.position, transform.position, MagnetSpeed * Time.deltaTime);
            }
        }
    }
    
    #region States
    
    public void StartRunning()
    {
        if (character.animator)
        {
            GameEvents.RunStartEvent?.Invoke();
        }
    }
    public void StopMoving()
    {
        trackManager.StopMove();
        if (character.animator)
        {
            
        }
    }
    
    #endregion
    public void SetInvincibleExplicit(bool invincible)
    {
        _invincible = invincible;
    }
    public void SetInvincible(float timer = DefaultInvinsibleTime)
    {
        StartCoroutine(InvincibleTimer(timer));
    }
    
    protected IEnumerator InvincibleTimer(float timer)
    {
        _invincible = true;

        float time = 0;
        float currentBlink = 1.0f;
        float lastBlink = 0.0f; 
        const float blinkPeriod = 0.1f;

        while(time < timer && _invincible)
        {
            
            yield return null;
            time += Time.deltaTime;
            lastBlink += Time.deltaTime;

            if (blinkPeriod < lastBlink)
            {
                lastBlink = 0;
                currentBlink = 1.0f - currentBlink;
            }
        }
        _invincible = false;
    }

    private int _blueCoins=0;
    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.layer == CoinsLayerIndex)
		{
            if (magnetCoins.Contains(c.gameObject))
				magnetCoins.Remove(c.gameObject);

			if (c.GetComponent<Coin>().isPremium)
            {
				Addressables.ReleaseInstance(c.gameObject);
                premium += 1;
				_audio.PlayOneShot(premiumSound);
			}
            else if (c.GetComponent<Coin>().isNegative)
            {
                Coin.coinPool.Free(c.gameObject);
                PlayerData.instance.coins += 1;
                coins+= 1;
                _audio.PlayOneShot(coinSound);
            }
            else
            {
				Coin.coinPool.Free(c.gameObject);
                PlayerData.instance.coins += 1;
				coins += 1;
                _audio.PlayOneShot(coinSound);
            }
        }
        else if(c.gameObject.layer == ObstacleLayerIndex)
        {
            if (_invincible || IsCheatInvincible())
                return;
            StopMoving();

            if(trackManager.isTutorial)
                return;
            characterMovement.PlayAnim(TransitionParameter.Hit.ToString(), 0.1f, 1f);
            c.enabled = false;

            Obstacle ob = c.gameObject.GetComponent<Obstacle>();

			if (ob != null)
			{
				ob.Impacted();
			}
			else
			{
			    Addressables.ReleaseInstance(c.gameObject);
			}

            if (TrackManager.Instance.isTutorial)
            {
                inputController.tutorialHitObstacle = true;
            }
            else
            {
                currentLife -= 1;
            }
            
            if (currentLife > 0)
			{
                _audio.PlayOneShot(character.hitSound);
                SetInvincible();
			}
            // The collision killed the player, record all data to analytics.
			else
			{
                GameEvents.PlayerDeathEvent?.Invoke();
                _deathData.Character = character.characterName;
                _deathData.ThemeUsed = trackManager.currentTheme.themeName;
                //_deathData.ObstacleType = ob.GetType().ToString();
                _deathData.Coins = coins;
                _deathData.Premium = premium;
                _deathData.Score = trackManager.score;
                _deathData.WorldDistance = trackManager.worldDistance;

			}
        }
        else if(c.gameObject.layer == PowerupLayerIndex)
        {
            Consumable consumable = c.GetComponent<Consumable>();
            if(consumable != null)
            {
                UseConsumable(consumable);
            }
        }
    }
}
