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
using Dev.Scripts.Characters;
using Character = Dev.Scripts.Characters.Character;

public enum TransitionParameter
{
    Start,
    Dead,
    Running,
    Jumping,
    Sliding,
    SlideEnd,
    Landing,
    Hit,
    ForceTransition,
    JumpEnd
}
public static class GameEvents
{
    public static Action PlayerDeathEvent;
    public static Action RunStartEvent;
    public static Action GameStartEvent;
}

public class CharacterControl : MonoBehaviour
{
    public CharacterInputController inputController;
    public CharacterMovement characterMovement;
    public Character character;
    public CharacterController controller;
    public TrackManager trackManager;

    public int maxLife = 3;
    
    [Header("Sound")]
    public AudioClip coinSound;
    public AudioClip powerUpUseSound;
    public AudioSource powerupSource;
    
    public new AudioSource audio { get { return _audio; } }
    public int coins { get { return _mCoins; } set { _mCoins = value; } }
    public int currentLife { get { return _mCurrentLife; } set { _mCurrentLife = value; } }
    public List<Consumable> consumables => m_ActiveConsumables;
    public List<Consumable> inventory = new List<Consumable>();

    [HideInInspector]
    public List<GameObject> magnetCoins = new List<GameObject>();

    private bool _invincible;
    private AudioSource _audio;
    
    private const float MagnetSpeed = 10f;
    private const int CoinsLayerIndex = 8;
    private const int ObstacleLayerIndex = 9;
    private const int PowerupLayerIndex = 10;
    private const float DefaultInvinsibleTime = 3f;
    
    private int _mCoins;
    private int _mCurrentLife;

    private List<Consumable> m_ActiveConsumables = new List<Consumable>();

    #region Inital Methods
    
    private void Start()
    {
        _audio = GetComponent<AudioSource>();
    }
    protected void Awake ()
    {
        Init();
    }
    private void Init()
    {
        controller = GetComponent<CharacterController>();
        _invincible = false;
        currentLife = maxLife;

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
        _invincible = invincible;
    }
    private bool IsCheatInvincible()
    {
        return _invincible;
    }
    private void CleanConsumable()
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
        trackManager.StartMove();
        if (character.animator)
        {
            characterMovement.PlayAnim(TransitionParameter.Running.ToString(),0.1f,1f);
            GameEvents.RunStartEvent?.Invoke();
        }
    }
    private void StopMoving()
    {
        trackManager.StopMove();
        if (character.animator)
        {
            characterMovement.PlayAnim(TransitionParameter.Hit.ToString(),0.1f,1f);
        }
    }
    
    #endregion
    public void SetInvincible(float timer = DefaultInvinsibleTime)
    {
        StartCoroutine(InvincibleTimer(timer));
    }
    
    private IEnumerator InvincibleTimer(float timer)
    {
        var c = GetComponentInChildren<Character>();
        var r = c.GetComponentInChildren<SkinnedMeshRenderer>();
        var originalMaterial = r.materials[0];
        var originalColor = originalMaterial.color;
        
        Material invincibleMaterial = new Material(originalMaterial);
        invincibleMaterial.color = Color.white;

        _invincible = true;
        
        float time = 0;
        float currentBlink = 1.0f;
        float lastBlink = 0.0f;
        const float blinkPeriod = 0.1f;

        while (time < timer && _invincible)
        {
            yield return null;
            time += Time.deltaTime;
            lastBlink += Time.deltaTime;

            if (blinkPeriod < lastBlink)
            {
                lastBlink = 0;
                currentBlink = 1.0f - currentBlink;
                
                r.materials[0].color = (currentBlink > 0.5f) ? originalColor : invincibleMaterial.color;
            }
        }
        r.materials[0].color = originalColor;

        _invincible = false;
    }

    
    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.layer == CoinsLayerIndex)
		{
            if (magnetCoins.Contains(c.gameObject))
				magnetCoins.Remove(c.gameObject);

            Coin.coinPool.Free(c.gameObject);
            PlayerData.Instance.coins += 1;
            coins += 1;
            _audio.PlayOneShot(coinSound);
        }
        else if(c.gameObject.layer == ObstacleLayerIndex)
        {
            if (_invincible || IsCheatInvincible())
                return;
            StopMoving();
            
            var ob = c.gameObject.GetComponent<Obstacle>() ?? c.gameObject.GetComponentInParent<Obstacle>();

            if (ob != null)
            {
                ob.Impacted();
                //c.GetComponentInChildren<Collider>().enabled = false;
            }
            else
            {
                Addressables.ReleaseInstance(c.gameObject);
            }


            currentLife -= 1;
            
            if (currentLife > 0)
			{
                _audio.PlayOneShot(character.hitSound);
                SetInvincible();
			}
            // The collision killed the player, record all data to analytics.
			else
			{
                 GameEvents.PlayerDeathEvent?.Invoke();
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
