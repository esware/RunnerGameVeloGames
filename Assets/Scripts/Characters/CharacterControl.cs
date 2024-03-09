using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Dev.Scripts;
using Dev.Scripts.Consumables;
using Dev.Scripts.Obstacles;
using Dev.Scripts.Track;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Dev.Scripts.Characters;
using Character = Dev.Scripts.Characters.Character;

#region Enums

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

#endregion

#region GameEvents

    public static class GameEvents
    {
        public static Action PlayerDeathEvent;
        public static Action RunStartEvent;
    }

#endregion

#region Requirements

[
    RequireComponent
    (
        typeof(CharacterInputController),
        typeof(CharacterMovement),
        typeof(CharacterController)
    )
]

#endregion
public class CharacterControl : MonoBehaviour
{
    #region Variables

    #region Public Variables

    [Header("Character Components")]
    public TrackManager trackManager;
    public CharacterInputController inputController;
    public CharacterMovement characterMovement;
    public Character character;
    public CharacterController controller;

    public int maxLife = 3;
    
    [Header("Sound")]
    public AudioClip coinSound;
    public AudioClip powerUpUseSound;
    public AudioSource powerupSource;

    #endregion
    
    #region Properties
    public new AudioSource audio => _audio;
    public int Coins
    {
        get => _coins;
        set => _coins = value;
    }

    public int CurrentLife
    {
        get => _currentLife;
        set => _currentLife = value;
    }

    public List<Consumable> Consumables => _activeConsumables;

    [HideInInspector]
    public List<GameObject> MagnetCoins = new List<GameObject>();

    #endregion
    
    #region Constants
    
    private const float MagnetSpeed = 10f;
    private const int CoinsLayerIndex = 8;
    private const int ObstacleLayerIndex = 9;
    private const int PowerUpLayer = 7;
    private const float DefaultInvincibleTime = 4f;
    
    #endregion

    #region Private Variables

    private List<Consumable> _activeConsumables = new List<Consumable>();
    private bool _invincible;
    private AudioSource _audio;
    private int _coins;
    private int _currentLife;

    #endregion

    #endregion
    
    #region Inital Methods
    private void Awake ()
    {
        Init();
    }
    private void Init()
    {
        _audio = GetComponent<AudioSource>();
        controller = GetComponent<CharacterController>();
        _invincible = false;
        _currentLife = maxLife;
    }
    public void Begin()
    {
        _activeConsumables.Clear();
    }
    public void End()
    {
        CleanConsumable();
    }
    #endregion

    #region Consumable Control
    
    public void CleanConsumable()
    {
        foreach (var t in _activeConsumables)
        {
            t.Ended(this);
            Addressables.ReleaseInstance(t.gameObject);
        }

        _activeConsumables.Clear();
    }
    private void UseConsumable(Consumable c)
    {
        if (audio != null)
            audio.PlayOneShot(powerUpUseSound);

        bool isConsumableActive = false;
        foreach (var consumable in _activeConsumables)
        {
            if (consumable.GetType() == c.GetType())
            {
                consumable.ResetTime();
                Addressables.ReleaseInstance(c.gameObject);
                isConsumableActive = true;
                break;
            }
        }

        if (!isConsumableActive)
        {
            c.transform.SetParent(transform, false);
            c.gameObject.SetActive(false);

            _activeConsumables.Add(c);
            StartCoroutine(c.Started(this));
        }
    }
    #endregion
    
    #region Invincible Control
    public void SetInvincible(float timer = DefaultInvincibleTime)
    {
        StartCoroutine(InvincibleTimer(timer));
    }
    private IEnumerator InvincibleTimer(float timer)
    {
        var c = GetComponentInChildren<Character>();
        var r = c.GetComponentInChildren<SkinnedMeshRenderer>();
        var originalMaterial = r.materials[0];
        var originalColor = originalMaterial.color;
        
        var invincibleMaterial = new Material(originalMaterial);
        invincibleMaterial.color = Color.red;

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

    #endregion
    
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
    public void StopMoving()
    {
        trackManager.StopMove();
        if (character.animator)
        {
            characterMovement.PlayAnim(TransitionParameter.Hit.ToString(),0.1f,1f);
        }
    }
    
    #endregion
    
    #region Trigger Events

    private void OnTriggerEnter(Collider other)
    {
        if (IsCoinTrigger(other))
        {
            HandleCoin(other.gameObject);
        }
        else if (IsObstacleTrigger(other))
        {
            HandleObstacle(other.gameObject);
        }
        else if (IsPowerUpTrigger(other))
        {
            HandlePowerUp(other.gameObject);
        }
    }

    #endregion

    #region Trigger Conditions

    private bool IsCoinTrigger(Collider other) => other.gameObject.layer == CoinsLayerIndex;

    private bool IsObstacleTrigger(Collider other) => other.gameObject.layer == ObstacleLayerIndex;

    private bool IsPowerUpTrigger(Collider other) => other.gameObject.layer == PowerUpLayer;

    #endregion

    #region Trigger Handlers

    private void HandleCoin(GameObject coinObject)
    {
        if (MagnetCoins.Contains(coinObject))
        {
            MagnetCoins.Remove(coinObject);
        }
        
        trackManager.AddScore(1);
        Coin.CoinPool.Free(coinObject);
        PlayerData.Instance.Coins += 1;
        Coins += 1;

        if (audio != null)
        {
            _audio.PlayOneShot(coinSound);
        }
    }

    private void HandleObstacle(GameObject obstacleObject)
    {
        if (_invincible)
            return;

        StopMoving();
    
        var obstacle = obstacleObject.GetComponent<Obstacle>() ?? obstacleObject.GetComponentInParent<Obstacle>();

        if (obstacle != null)
        {
            obstacle.Impacted();
        }
        else
        {
            Addressables.ReleaseInstance(obstacleObject);
        }

        CurrentLife -= 1;

        if (CurrentLife > 0)
        {
            if (audio != null)
                _audio.PlayOneShot(character.hitSound);

            SetInvincible();
        }
        else
        {
            GameEvents.PlayerDeathEvent?.Invoke();
        }
    }

    private void HandlePowerUp(GameObject powerUpObject)
    {
        Consumable consumable = powerUpObject.GetComponent<Consumable>();
        if (consumable != null)
        {
            UseConsumable(consumable);
        }
    }

    #endregion
    
    private void Update()
    {
        if (!trackManager.IsMoving)
            return;
        
        MagnetCoins.RemoveAll(item => item == null);
        foreach (var t in MagnetCoins)
        {
            t.transform.position = Vector3.MoveTowards(t.transform.position, transform.position + Vector3.up, MagnetSpeed * Time.deltaTime);
        }
    }

}
