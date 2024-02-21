using System.Collections;
using System.Collections.Generic;
using Dev.Scripts.Characters;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Dev.Scripts.Consumables;
using Dev.Scripts.Obstacles;
using Dev.Scripts.Themes;

using Random = UnityEngine.Random;
#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif

namespace Dev.Scripts.Track
{
    
    public class TrackManager : MonoBehaviour
    {
        static public TrackManager Instance => _instance;
        static private TrackManager _instance;

        static readonly int StartHash = Animator.StringToHash("Start");

        public delegate int MultiplierModifier(int current);
        public MultiplierModifier modifyMultiply;

        [Header("Character & Movements")]
        public CharacterControl characterController;
        public CharacterInputController characterInputController;
        public float minSpeed = 5.0f;
        public float maxSpeed = 10.0f;
        public int speedStep = 4;
        public float laneOffset = 1.0f;

        public bool invincible = false;

        [Header("Objects")]
        public ConsumableDatabase consumableDatabase;
        public GameObject parentObject;
        public GameObject drone;
        public Transform characterPosition;

        [Header("Tutorial")]
        public ThemeData tutorialThemeData;
        

        public System.Action<TrackSegment> NewSegmentCreated;
        public System.Action<TrackSegment> CurrentSegementChanged;
        public int trackSeed { get { return _mTrackSeed; } set { _mTrackSeed = value; } }

        public float timeToStart { get { return m_TimeToStart; } }  // Will return -1 if already started (allow to update UI)

        public int score { get { return _mScore; } }
        public int multiplier { get { return _mMultiplier; } }
        
        public float CurrentSegmentDistance => _currentSegmentDistance;
        public float worldDistance { get { return _totalWorldDistance; } }
        public float speed { get { return _speed; } }
        public float speedRatio { get { return (_speed - minSpeed) / (maxSpeed - minSpeed); } }
        public int currentZone { get { return _mCurrentZone; } }

        public TrackSegment currentSegment { get { return m_Segments[0]; } }
        public List<TrackSegment> segments { get { return m_Segments; } }
        public ThemeData currentTheme { get { return _mCurrentThemeData; } }

        public bool isMoving { get { return _isMoving; } }
        public bool isRerun { get { return _mRerun; } set { _mRerun = value; } }

        public bool isTutorial { get { return _mIsTutorial; } set { _mIsTutorial = value; } }
        public bool isLoaded { get; set; }
        
        public bool firstObstacle { get; set; }

        private float m_TimeToStart = -1.0f;
        
        private int _mTrackSeed = -1;

        private float _currentSegmentDistance;
        private float _totalWorldDistance;
        private bool _isMoving;
        private float _speed;

        private float _mTimeSincePowerup; 
        private float _mTimeSinceLastPremium;

        private int _mMultiplier;

        public List<TrackSegment> m_Segments = new List<TrackSegment>();
        public List<TrackSegment> m_PastSegments = new List<TrackSegment>();
        private int _mSafeSegementLeft;

        private ThemeData _mCurrentThemeData;
        private int _mCurrentZone;
        private float _mCurrentZoneDistance;
        private readonly int _PreviousSegment = -1;

        private int _mScore;
        private float _mScoreAccum;
        private bool _mRerun;     

        private bool _mIsTutorial;
        private readonly Vector3 _offScreenSpawnPos = new Vector3(-100f, -100f, -100f);
        private const float KCountdownToStartLength = 5f;
        private const float KCountdownSpeed = 1.5f;
        private const int KStartingSafeSegments = 1;
        private const int KDesiredSegmentCount =5;
        private const float KAcceleration = 0.1f;


        protected void Awake()
        {
            _mScoreAccum = 0.0f;
            _instance = this;
            _speed = minSpeed;
        }

        public void StartMove(bool isRestart = true)
        {
            drone.SetActive(true);
            _isMoving = true;
            if (isRestart)
                _speed = minSpeed;
        }
        public void StopMove()
        {
            _isMoving = false;
        }

        IEnumerator WaitToStart()
        {
            float length = KCountdownToStartLength;
            m_TimeToStart = length;

            while (m_TimeToStart >= 0)
            {
                yield return null;
                m_TimeToStart -= Time.deltaTime * KCountdownSpeed;
            }

            m_TimeToStart = -1;

            if (_mRerun)
            {
                characterController.characterMovement.DesiredLane = 1;
                characterController.SetInvincible();
            }

            characterController.StartRunning();
            StartMove();
        }

        public IEnumerator Begin()
        {
            _currentSegmentDistance = 10;
            characterController.transform.position = new Vector3(0,.2f,10);
            
            if (!_mRerun)
            {
                firstObstacle = true;

                if (_mTrackSeed != -1)
                    Random.InitState(_mTrackSeed);
                else
                    Random.InitState((int)System.DateTime.Now.Ticks);
                _totalWorldDistance = 0f;

                characterController.gameObject.SetActive(true);
            
                // Spawn the player
                var op = Addressables.InstantiateAsync(PlayerData.instance.characters[PlayerData.instance.usedCharacter],
                    Vector3.zero, 
                    Quaternion.identity);
                yield return op;
                if (op.Result == null || !(op.Result is GameObject))
                {
                    Debug.LogWarning(string.Format("Unable to load character {0}.", PlayerData.instance.characters[PlayerData.instance.usedCharacter]));
                    yield break;
                }
                Characters.Character player = op.Result.GetComponent<Characters.Character>();

                //player.SetupAccesory(PlayerData.instance.usedAccessory);

                characterController.character = player;
                characterController.trackManager = this;
                characterController.CheatInvincible(invincible);
                
                player.transform.SetParent(characterPosition.transform, false);


                if (_mIsTutorial)
                    _mCurrentThemeData = tutorialThemeData;
                else
                    _mCurrentThemeData = ThemeDatabase.GetThemeData(PlayerData.instance.themes[PlayerData.instance.usedTheme]);

                _mCurrentZone = 0;
                _mCurrentZoneDistance = 2;

                gameObject.SetActive(true);
                characterController.gameObject.SetActive(true);
                characterController.coins = 0;
                characterController.premium = 0;

                _mScore = 0;
                _mScoreAccum = 0;

                _mSafeSegementLeft = _mIsTutorial ? 0 : KStartingSafeSegments;

                Coin.coinPool = new Pooler(currentTheme.collectiblePrefab, 0);
                

#if UNITY_ANALYTICS
            AnalyticsEvent.GameStart(new Dictionary<string, object>
            {
                { "theme", m_CurrentThemeData.themeName},
                { "character", player.characterName },
                { "accessory",  PlayerData.instance.usedAccessory >= 0 ? player.accessories[PlayerData.instance.usedAccessory].accessoryName : "none"}
            });
#endif
            }
            characterController.Begin();
            characterController.character.animator.Play(StartHash);
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(WaitToStart());
            isLoaded = true;
        }

        public void End()
        {
            ClearSegments();
            drone.SetActive(false);
            characterController.characterMovement.DesiredLane = 1;
            characterController.End();
            gameObject.SetActive(false);
            Addressables.ReleaseInstance(characterController.character.gameObject);
            characterController.character = null;
            characterController.gameObject.SetActive(false);

        }

        public void ClearSegments()
        {
            _mSafeSegementLeft = 1;

            foreach (var seg in m_Segments)
            {
                Addressables.ReleaseInstance(seg.gameObject);
                _spawnedSegments--;
            }

            for (int i = 0; i < m_PastSegments.Count; ++i)
            {
                Addressables.ReleaseInstance(m_PastSegments[i].gameObject);
            }
            m_Segments.Clear();
            m_PastSegments.Clear();
        }
        private int _spawnedSegments = 0;
        void Update()
        {
            while (_spawnedSegments < (_mIsTutorial ? 3 : KDesiredSegmentCount))
            {
                StartCoroutine(SpawnNewSegment());
                _spawnedSegments++;
            }
            
            if (!_isMoving) return;
            
            float scaledSpeed = _speed * Time.deltaTime;
            _mScoreAccum += scaledSpeed;
            _mCurrentZoneDistance += scaledSpeed;

            int intScore = Mathf.FloorToInt(_mScoreAccum);
            if (intScore != 0) AddScore(intScore);
            _mScoreAccum -= intScore;

            _totalWorldDistance += scaledSpeed;
            _currentSegmentDistance += scaledSpeed;
            
            if (m_Segments.Count > 0)
            {
                if (_currentSegmentDistance > m_Segments[0].WorldLength)
                {
                    _currentSegmentDistance = 0;
                    // m_PastSegments are segment we already passed, we keep them to move them and destroy them later 
                    // but they aren't part of the game anymore 
                    m_PastSegments.Add(m_Segments[0]);
                    m_Segments.RemoveAt(0);
                    _spawnedSegments--;
                    if (CurrentSegementChanged != null) CurrentSegementChanged.Invoke(m_Segments[0]);
                }
            }

            PowerupSpawnUpdate();

            if (!_mIsTutorial)
            {
                if (_speed < maxSpeed)
                    _speed += KAcceleration * Time.deltaTime;
                else
                    _speed = maxSpeed;
            }

            _mMultiplier = 1 + Mathf.FloorToInt((_speed - minSpeed) / (maxSpeed - minSpeed) * speedStep);

            if (modifyMultiply != null)
            {
                foreach (MultiplierModifier part in modifyMultiply.GetInvocationList())
                {
                    _mMultiplier = part(_mMultiplier);
                }
            }

            if (!_mIsTutorial)
            {
                //check for next rank achieved
                int currentTarget = (PlayerData.instance.rank + 1) * 300;
                if (_totalWorldDistance > currentTarget)
                {
                    PlayerData.instance.rank += 1;
                    PlayerData.instance.Save();
#if UNITY_ANALYTICS
//"level" in our game are milestone the player have to reach : one every 300m
            AnalyticsEvent.LevelUp(PlayerData.instance.rank);
#endif
                }
            }

            //MusicPlayer.instance.UpdateVolumes(speedRatio);
        }

        private void PowerupSpawnUpdate()
        {
            _mTimeSincePowerup += Time.deltaTime;
            _mTimeSinceLastPremium += Time.deltaTime;
        }

        public void ChangeZone()
        {
            _mCurrentZone += 1;
            if (_mCurrentZone >= _mCurrentThemeData.zones.Length)
                _mCurrentZone = 0;

            _mCurrentZoneDistance = -35;
        }

        private IEnumerator SpawnNewSegment()
        {
            int segmentUse;
            AsyncOperationHandle segmentToUseOp;
            if (!_mIsTutorial)
            {
                if (_mCurrentThemeData.zones[_mCurrentZone].length < _mCurrentZoneDistance)
                    ChangeZone();
            }
            
            segmentUse = Random.Range(0, _mCurrentThemeData.zones[_mCurrentZone].prefabList.Length);
            if (segmentUse == _PreviousSegment) segmentUse = (segmentUse + 1) % _mCurrentThemeData.zones[_mCurrentZone].prefabList.Length;

            segmentToUseOp = _mCurrentThemeData.zones[_mCurrentZone].prefabList[segmentUse].InstantiateAsync(_offScreenSpawnPos, Quaternion.identity);
            yield return segmentToUseOp;
            if (segmentToUseOp.Result == null || !(segmentToUseOp.Result is GameObject))
            {
                Debug.LogWarning(string.Format("Unable to load segment {0}.", _mCurrentThemeData.zones[_mCurrentZone].prefabList[segmentUse].Asset.name));
                yield break;
            }
            
                
            TrackSegment newSegment = (segmentToUseOp.Result as GameObject).GetComponent<TrackSegment>();

            Vector3 currentExitPoint;
            Quaternion currentExitRotation;
            
            if (m_Segments.Count > 0)
            {
                m_Segments[m_Segments.Count - 1].GetPointAt(1.0f, out currentExitPoint, out currentExitRotation);
            }
            else
            {
                currentExitPoint = Vector3.zero;
                currentExitRotation = Quaternion.identity;
            }

            newSegment.transform.rotation = currentExitRotation;

            Vector3 entryPoint;
            Quaternion entryRotation;
            newSegment.GetPointAt(0.0f, out entryPoint, out entryRotation);


            Vector3 pos = currentExitPoint + (newSegment.transform.position - entryPoint);
            newSegment.transform.position = pos;
            newSegment.manager = this;
            newSegment.transform.SetParent(parentObject.transform);
            
            if (_mSafeSegementLeft <= 0)
            {
                SpawnObstacle(newSegment);
            }
            else
                _mSafeSegementLeft -= 1;

            m_Segments.Add(newSegment);

            if (NewSegmentCreated != null) NewSegmentCreated.Invoke(newSegment);
        }
        private void SpawnObstacle(TrackSegment segment)
        {
            if (segment.possibleObstacles.Length != 0)
            {
                for (int i = 0; i < segment.obstaclePositions.Length; ++i)
                {
                    var assetRef = segment.possibleObstacles[Random.Range(0, segment.possibleObstacles.Length)];
                    StartCoroutine(SpawnFromAssetReference(assetRef, segment, i));
                }
            }

            StartCoroutine(SpawnCoinAndPowerup(segment));
        } 
        private IEnumerator SpawnFromAssetReference(AssetReference reference, TrackSegment segment, int posIndex)
        {
            
            AsyncOperationHandle op = Addressables.LoadAssetAsync<GameObject>(reference);
        yield return op; 
        GameObject obj = op.Result as GameObject;
        
        if (obj != null)
        {
            Obstacle obstacle = obj.GetComponent<Obstacle>();
            if (obstacle != null)
                yield return obstacle.Spawn(segment, segment.obstaclePositions[posIndex]);
        }
        }

        public IEnumerator SpawnCoinAndPowerup(TrackSegment segment)
        {
            if (!_mIsTutorial)
            {
                const float increment = 3f;
                float currentWorldPos = 0.0f;
                int currentLane = Random.Range(0, 3);

                float powerupChance = Mathf.Clamp01(Mathf.Floor(_mTimeSincePowerup) * 0.5f * 0.001f);
                float premiumChance = Mathf.Clamp01(Mathf.Floor(_mTimeSinceLastPremium) * 0.5f * 0.0001f);
                while (currentWorldPos < segment.WorldLength/2)
                {
                    Vector3 pos;
                    Quaternion rot;
                    segment.GetPointAtInWorldUnit(currentWorldPos, out pos, out rot);
                    pos += new Vector3(0, 1, 0);

                    bool laneValid = true;
                    int testedLane = currentLane;
                    while (Physics.CheckSphere(pos + ((testedLane - 1) * laneOffset * (rot * Vector3.right)), 0.4f, 1 << 9))
                    {
                        testedLane = (testedLane + 1) % 3;
                        if (currentLane == testedLane)
                        {
                            // Couldn't find a valid lane.
                            laneValid = false;
                            break;
                        }
                    }

                    currentLane = testedLane;

                    if (laneValid)
                    {
                        pos = pos + ((currentLane - 1) * laneOffset * (rot * Vector3.right));

                        GameObject toUse = null;
                        if (Random.value < powerupChance)
                        {
                            int picked = Random.Range(0, consumableDatabase.consumbales.Length);

                            //if the powerup can't be spawned, we don't reset the time since powerup to continue to have a high chance of picking one next track segment
                            if (consumableDatabase.consumbales[picked].canBeSpawned)
                            {
                                // Spawn a powerup instead.
                                _mTimeSincePowerup = 0.0f;
                                powerupChance = 0.0f;

                                AsyncOperationHandle op = Addressables.InstantiateAsync(consumableDatabase.consumbales[picked].gameObject.name, pos, rot);
                                yield return op;
                                if (op.Result == null || !(op.Result is GameObject))
                                {
                                    Debug.LogWarning(string.Format("Unable to load consumable {0}.", consumableDatabase.consumbales[picked].gameObject.name));
                                    yield break;
                                }
                                toUse = op.Result as GameObject;
                                toUse.transform.SetParent(segment.transform, true);
                            }
                        }
                        else
                        {
                            toUse = Coin.coinPool.Get(pos, rot,false);
                            toUse.transform.SetParent(segment.collectibleTransform, true);
                        }
                        if (toUse != null)
                        {
                            //TODO : remove that hack related to #issue7
                            Vector3 oldPos = toUse.transform.position;
                            toUse.transform.position += Vector3.back;
                            toUse.transform.position = oldPos;
                        }
                    }

                    currentWorldPos += increment;
                }
            }
        }

        public void AddScore(int amount)
        {
            int finalAmount = amount;
            _mScore += finalAmount * _mMultiplier;
        }
    }
}