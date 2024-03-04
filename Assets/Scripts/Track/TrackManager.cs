using System;
using System.Collections;
using System.Collections.Generic;
using Dev.Scripts.Characters;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Dev.Scripts.Consumables;
using Dev.Scripts.Obstacles;
using Dev.Scripts.Sounds;
using Dev.Scripts.Themes;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Linq;
using System.Numerics;
using Unity.Mathematics;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif

namespace Dev.Scripts.Track
{
    
    public class TrackManager : MonoBehaviour
    {
        public static TrackManager Instance => _instance;
        private static TrackManager _instance;

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
        public Transform characterPosition;
        
        

        public System.Action<TrackSegment> NewSegmentCreated;
        public System.Action<TrackSegment> CurrentSegementChanged;
        public int trackSeed { get { return _trackSeed; } set { _trackSeed = value; } }

        public float timeToStart => _timeToStart;

        public int score => _score;

        public float CurrentSegmentDistance => _currentSegmentDistance;
        public float worldDistance => _totalWorldDistance;
        public float speed => _speed;
        public float speedRatio =>(_speed - minSpeed) / (maxSpeed - minSpeed);
        public int currentZone => _currentZone;
        
        public ThemeData currentTheme => _currentThemeData;

        public bool isMoving { get { return _isMoving; } }
        public bool isRerun { get { return _mRerun; } set { _mRerun = value; } }
        
        public bool isLoaded { get; set; }
        
        private float _timeToStart = -1.0f;
        
        private int _trackSeed = -1;

        private float _currentSegmentDistance;
        private float _totalWorldDistance;
        private bool _isMoving;
        private float _speed;

        private float _mTimeSincePowerup;

        private int _mMultiplier;

        public List<TrackSegment> _segments = new List<TrackSegment>();
        public List<TrackSegment> m_PastSegments = new List<TrackSegment>();
        private int _safeSegementLeft;

        private ThemeData _currentThemeData;
        private int _currentZone;
        private float _mCurrentZoneDistance;
        private readonly int _PreviousSegment = -1;

        private int _score;
        private float _mScoreAccum;
        private bool _mRerun;     
        
        private readonly Vector3 _offScreenSpawnPos = new Vector3(-100f, -100f, -100f);
        private const float CountdownToStartLength = 5f;
        private const float CountdownSpeed = 1.5f;
        private const int StartingSafeSegments = 1;
        private const int DesiredSegmentCount = 10;
        private const float Acceleration = 0.1f;
        private const float SegmentRemovalDistance = 60f;
        private int _spawnedSegments = 0;
        

        protected void Awake()
        {
            _mScoreAccum = 0.0f;
            _instance = this;
            _speed = minSpeed;
        }

        public void StartMove(bool isRestart = true)
        {
            _isMoving = true;
            if (isRestart)
                _speed = minSpeed;
        }
        public void StopMove()
        {
            _isMoving = false;
        }

        private IEnumerator WaitToStart()
        {
            float length = CountdownToStartLength;
            _timeToStart = length;

            while (_timeToStart >= 0)
            {
                yield return null;
                _timeToStart -= Time.deltaTime * CountdownSpeed;
            }

            _timeToStart = -1;

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
            _currentSegmentDistance = 0;
            characterController.transform.position = new Vector3(0,.2f,10);
            
            if (!_mRerun)
            {
                if (_trackSeed != -1)
                    Random.InitState(_trackSeed);
                else
                    Random.InitState((int)System.DateTime.Now.Ticks);
                _totalWorldDistance = 0f;

                characterController.gameObject.SetActive(true);
                
                var op = Addressables.InstantiateAsync(PlayerData.Instance.characters[PlayerData.Instance.usedCharacter],
                    Vector3.zero, 
                    Quaternion.identity);
                yield return op;
                if (op.Result == null || !(op.Result is GameObject))
                {
                    Debug.LogWarning(string.Format("Unable to load character {0}.", PlayerData.Instance.characters[PlayerData.Instance.usedCharacter]));
                    yield break;
                }
                Characters.Character player = op.Result.GetComponent<Characters.Character>();
                
                characterController.character = player;
                characterController.trackManager = this;
                characterController.CheatInvincible(invincible);
                
                player.transform.SetParent(characterPosition.transform, false);

                
                _currentThemeData = ThemeDatabase.GetThemeData(PlayerData.Instance.themes[PlayerData.Instance.usedTheme]);

                _currentZone = 0;
                _mCurrentZoneDistance = 2;

                gameObject.SetActive(true);
                characterController.gameObject.SetActive(true);
                characterController.coins = 0;

                _score = 0;
                _mScoreAccum = 0;

                _safeSegementLeft = StartingSafeSegments;
                Coin.coinPool = new Pooler(currentTheme.collectiblePrefab, 50);
                

#if UNITY_ANALYTICS
            AnalyticsEvent.GameStart(new Dictionary<string, object>
            {
                { "theme", m_CurrentThemeData.themeName},
                { "character", player.characterName },
            });
#endif
            }
            characterController.Begin();
            characterController.character.animator.Play(TransitionParameter.Start.ToString());
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(WaitToStart());
            isLoaded = true;
        }

        public void End()
        {
            ClearSegments();
            characterController.characterMovement.DesiredLane = 1;
            characterController.End();
            gameObject.SetActive(false);
            Addressables.ReleaseInstance(characterController.character.gameObject);
            characterController.character = null;
            characterController.gameObject.SetActive(false);

        }

        public void ClearSegments()
        {
            _safeSegementLeft = 1;

            foreach (var seg in _segments)
            {
                Addressables.ReleaseInstance(seg.gameObject);
                _spawnedSegments--;
            }

            for (int i = 0; i < m_PastSegments.Count; ++i)
            {
                Addressables.ReleaseInstance(m_PastSegments[i].gameObject);
            }
            _segments.Clear();
            m_PastSegments.Clear();
        }
        
        private void Update()
        {
            while (_spawnedSegments < DesiredSegmentCount)
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
            
            if (_segments.Count > 0)
            {
                if (_currentSegmentDistance > _segments[0].WorldLength)
                {
                    _currentSegmentDistance = 0;
                    m_PastSegments.Add(_segments[0]);
                    _segments.RemoveAt(0);
                    _spawnedSegments--;
                    if (CurrentSegementChanged != null) CurrentSegementChanged.Invoke(_segments[0]);
                }
            }

            PowerupSpawnUpdate();

            if (_speed < maxSpeed)
                _speed += Acceleration * Time.deltaTime;
            else
                _speed = maxSpeed;

            // Still move past segment until they aren't visible anymore.
            for (int i = 0; i < m_PastSegments.Count; ++i)
            {
                if ((characterController.transform.position-m_PastSegments[i].transform.position ).z > SegmentRemovalDistance)
                {
                    m_PastSegments[i].Cleanup();
                    m_PastSegments.RemoveAt(i);
                    i--;
                }
            }
            
            _mMultiplier = 1 + Mathf.FloorToInt((_speed - minSpeed) / (maxSpeed - minSpeed) * speedStep);
            

            int currentTarget = (PlayerData.Instance.rank + 1) * 300;
            if (_totalWorldDistance > currentTarget)
            {
                //"level" in our game are milestone the player have to reach : one every 300m
                PlayerData.Instance.rank += 1;
                PlayerData.Instance.Save();
            }
            MusicPlayer.instance.UpdateVolumes(speedRatio);
        }

        private void PowerupSpawnUpdate()
        {
            _mTimeSincePowerup += Time.deltaTime;
        }

        private void ChangeZone()
        {
            _currentZone += 1;
            if (_currentZone >= _currentThemeData.zones.Length)
                _currentZone = 0;

            _mCurrentZoneDistance = 0;
        }

        private IEnumerator SpawnNewSegment()
        {
            int segmentUse;
            AsyncOperationHandle segmentToUseOp;
            
            if (_currentThemeData.zones[_currentZone].length < _mCurrentZoneDistance)
                ChangeZone();
            
            segmentUse = Random.Range(0, _currentThemeData.zones[_currentZone].prefabList.Length);
            if (segmentUse == _PreviousSegment) segmentUse = (segmentUse + 1) % _currentThemeData.zones[_currentZone].prefabList.Length;

            segmentToUseOp = _currentThemeData.zones[_currentZone].prefabList[segmentUse].InstantiateAsync(_offScreenSpawnPos, Quaternion.identity);
            yield return segmentToUseOp;
            if (segmentToUseOp.Result == null || !(segmentToUseOp.Result is GameObject))
            {
                Debug.LogWarning(string.Format("Unable to load segment {0}.", _currentThemeData.zones[_currentZone].prefabList[segmentUse].Asset.name));
                yield break;
            }
            
                
            TrackSegment newSegment = (segmentToUseOp.Result as GameObject).GetComponent<TrackSegment>();

            Vector3 currentExitPoint;
            Quaternion currentExitRotation;
            
            if (_segments.Count > 0)
            {
                _segments[_segments.Count - 1].GetPointAt(1.0f, out currentExitPoint, out currentExitRotation);
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
            
            if (_safeSegementLeft <= 0)
            {
                StartCoroutine(SpawnObstacleAndCoin(newSegment));
            }
            else
                _safeSegementLeft -= 1;

            _segments.Add(newSegment);

            if (NewSegmentCreated != null) NewSegmentCreated.Invoke(newSegment);
        }
        
        private IEnumerator SpawnObstacleAndCoin(TrackSegment segment)
        {
            if (segment.possibleObstacles.Length != 0)
            {
                for (int i = 0; i < segment.obstaclePositions.Length; ++i)
                {
                    var assetRef = segment.possibleObstacles[Random.Range(0, segment.possibleObstacles.Length)];
                    yield return StartCoroutine(SpawnFromAssetReference(assetRef, segment, i));
                    yield return null;
                }
            }
            
            yield return StartCoroutine(SpawnCoinAndPowerup(segment));
            
        }

        private IEnumerator SpawnFromAssetReference(AssetReference reference, TrackSegment segment, int posIndex)
        {
            Vector3 position;
            Quaternion rotation;
            segment.GetPointAt(segment.obstaclePositions[posIndex], out position, out rotation);
            
            AsyncOperationHandle op = Addressables.LoadAssetAsync<GameObject>(reference);
            yield return op; 
            GameObject obj = op.Result as GameObject;
            
            if (CheckCollision(obj,posIndex))
            {
                Addressables.ReleaseInstance(obj.gameObject);
                yield break;
            }

            
            if (obj != null)
            {
                Obstacle obstacle = obj.GetComponent<Obstacle>();
                if (obstacle != null)
                    yield return obstacle.Spawn(segment, segment.obstaclePositions[posIndex]);
            }
        }
        private bool CheckCollision(GameObject obstacle, int currentIndex)
        {
            Bounds obstacleBounds = obstacle.GetComponentInChildren<Renderer>().bounds;

            for (int i = 0; i < currentIndex; i++)
            {
                if (obstacle == null)
                {
                    Debug.LogWarning("Failed to load existing obstacle prefab.");
                    continue;
                }

                Bounds existingBounds = obstacle.GetComponentInChildren<Renderer>().bounds;

                if (obstacleBounds.Intersects(existingBounds))
                {
                    return true;
                }
            }

            return false;
        }
        
        private IEnumerator SpawnCoinAndPowerup(TrackSegment segment)
        {
            const float increment = 2f;
            float currentWorldPos = 0.0f;
            int currentLane = Random.Range(0, 3);
            float powerupChance = Mathf.Clamp01(Mathf.Floor(_mTimeSincePowerup) * 500f * 0.001f);

            Vector3 pos;
            Quaternion rot;
            GameObject toUse;
            
            while (currentWorldPos < segment.WorldLength*0.7f)
            {
                segment.GetPointAtInWorldUnit(currentWorldPos, out pos, out rot);
                pos += Vector3.up;
                int testedLane = currentLane;
                bool laneValid = true;
                Obstacle obstacle = null;
                
                while (Physics.CheckSphere(pos + ((testedLane - 1) * laneOffset * (Vector3.right)), 1f, 1 << 9))
                {
                    
                   Collider[] colliders = new Collider[10];

                    int colliderCount = Physics.OverlapSphereNonAlloc(pos + (testedLane - 1) * laneOffset * Vector3.right, 1f, colliders, 1 << 9);
                
                    for (int i = 0; i < colliderCount; i++)
                    {
                        Collider c = colliders[i];
                        if (c.GetComponent<Obstacle>() || c.GetComponentInParent<Obstacle>())
                        {
                            obstacle = c.GetComponent<Obstacle>() ?? c.GetComponentInParent<Obstacle>();
                            break;
                        }
                    }

                    if (obstacle != null)
                    {
                        if (obstacle.coinSpawnType == ObstacleCoinSpawnType.SpawnByJumping)
                        {
                            break;
                        }
                    }
                    
                    testedLane = (testedLane + 1) % 3;
                    if (currentLane == testedLane)
                    {
                        laneValid = false;
                        break;
                    }
                    
                }
                
                currentLane = testedLane;
                
                if (laneValid)
                {
                    segment.GetPointAtInWorldUnit(currentWorldPos, out pos, out rot);
                    pos += Vector3.up;
                    pos += (currentLane - 1) * laneOffset * (Vector3.right);
                    
                    if (obstacle != null)
                    {
                        //pos += (currentLane - 1) * laneOffset * (Vector3.right);
                        //var newPos = pos + (Vector3.forward*(obstacle.obstacleLength+speed/2f));
                    
                        if (obstacle.coinSpawnType==ObstacleCoinSpawnType.SpawnByJumping)
                        {
                            float radius = characterController.characterMovement.jumpHeight*2f;
                            float circleCircumference = Mathf.PI * radius;
                            int numberOfPoints = Mathf.RoundToInt(circleCircumference / increment);
                            
                            var obstaclePosition =obstacle.transform.position;
                            for (int i = 0; i < numberOfPoints; i++)
                            {
                                segment.GetPointAtInWorldUnit(currentWorldPos, out pos, out rot);
                    
                                float angle = i * 180f / numberOfPoints;
                                segment.GetPointAtInWorldUnit(currentWorldPos,out pos,out rot);
                                pos+= Vector3.up;

                                float y = obstaclePosition.y+(Mathf.Sin(angle * Mathf.Deg2Rad) * radius);
                                float z = obstaclePosition.z+(Mathf.Cos(angle * Mathf.Deg2Rad) * speed/2);


                                Vector3 coinPosition = ((testedLane - 1) * laneOffset * (Vector3.right) +
                                                        new Vector3(pos.x, y , z));

                                toUse = Coin.coinPool.Get(coinPosition, rot);
                                toUse.transform.SetParent(segment.collectibleTransform, true);
                    
                                if (toUse != null)
                                {
                                    Vector3 oldPos = toUse.transform.position;
                                    toUse.transform.position += Vector3.back*3;
                                    toUse.transform.position = oldPos;
                                }
                                currentWorldPos += increment;
                            }
                        }
                        continue;
                    }
                    
                    if (Random.value < powerupChance)
                    {
                        int picked = Random.Range(0, consumableDatabase.consumbales.Length);
                        
                        if (consumableDatabase.consumbales[picked].canBeSpawned)
                        {
                            _mTimeSincePowerup = 0.0f;
                            powerupChance = 0.0f;

                            AsyncOperationHandle op = Addressables.InstantiateAsync(consumableDatabase.consumbales[picked].gameObject.name, pos, Quaternion.identity);
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
                        toUse = Coin.coinPool.Get(pos, Quaternion.identity);
                        toUse.transform.SetParent(segment.collectibleTransform, true);
            
                        if (toUse != null)
                        {
                            Vector3 oldPos = toUse.transform.position;
                            toUse.transform.position +=  Vector3.back*3;;
                            toUse.transform.position = oldPos;
                        }
                    }
                }
                
                currentWorldPos += increment;
            }
        }
        
        private void AddScore(int amount)
        {
            int finalAmount = amount;
            _score += finalAmount * _mMultiplier;
        }
    }
}