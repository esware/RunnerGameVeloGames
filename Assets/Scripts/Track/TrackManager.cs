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
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Dev.Scripts.Track
{
    public class TrackManager : MonoBehaviour
    {
        #region Variables

        [Header("Character & Movements")]
        public CharacterControl characterController;
        public CharacterInputController characterInputController;
        public float minSpeed = 5.0f;
        public float maxSpeed = 10.0f;
        public int speedStep = 4;
        public float laneOffset = 1.0f;

        [Header("Objects")]
        public ConsumableDatabase consumableDatabase;
        public GameObject parentObject;
        public Transform characterPosition;

        #region Public Variables
        public bool IsRerun
        {
            get => _isRerun;
            set => _isRerun = value;
        }
        public float TimeToStart => _timeToStart;
        public int Score => _score;
        public float WorldDistance => _totalWorldDistance;
        public float Speed => _speed;
        public bool IsMoving => _isMoving;
        public bool IsLoaded => _isLoaded;
        public int Multiplier => _multiplier;
        
        #endregion
        
        #region Private Variables
        
        private readonly int _previousSegment = -1;
        private readonly Vector3 _offScreenSpawnPos = new Vector3(-100f, -100f, -100f);
        
        private float SpeedRatio => (_speed - minSpeed) / (maxSpeed - minSpeed);
        private ThemeData _currentThemeData;
        private int _safeSegmentsLeft;
        private int _currentZone;
        private float _mCurrentZoneDistance;
        private int _score;
        private float _scoreAccumulator;
        private bool _isRerun;
        private float _timeToStart = -1.0f;
        private float _currentSegmentDistance;
        private float _totalWorldDistance;
        private bool _isMoving;
        private float _speed;
        private float _timeSincePowerup;
        private int _multiplier;
        private bool _isLoaded;
        private int _spawnedSegments;
        
        private const float CountdownToStartLength = 5f;
        private const float CountdownSpeed = 1.5f;
        private const int StartingSafeSegments = 1;
        private const int DesiredSegmentCount = 10;
        private const float Acceleration = 0.1f;
        private const float SegmentRemovalDistance = 60f;
        
        #endregion
        
        
        public List<TrackSegment> segments = new();
        public List<TrackSegment> pastSegments = new();

        #endregion
        
        protected void Awake()
        {
            _scoreAccumulator = 0.0f;
            _speed = minSpeed;
        }

        #region Character State Control Methods

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

        #endregion

        #region Scene State Control Methods
        
        private IEnumerator WaitToStart()
        {
            var length = CountdownToStartLength;
            _timeToStart = length;

            while (_timeToStart >= 0)
            {
                yield return null;
                _timeToStart -= Time.deltaTime * CountdownSpeed;
            }

            _timeToStart = -1;

            if (_isRerun)
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

            if (!_isRerun)
            {
                InitializeTheme();
                InitializeGame();
                yield return StartCoroutine(InitializeCharacter());
            }
    
            characterController.Begin();
            characterController.character.animator.Play(TransitionParameter.Start.ToString());
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(WaitToStart());
            _isLoaded = true;
        }

        private IEnumerator InitializeCharacter()
        {
            _totalWorldDistance = 0f;
            characterController.transform.position = new Vector3(0, .2f, 10);
            characterController.gameObject.SetActive(true);
    
            var op = Addressables.InstantiateAsync(PlayerData.Instance.characters[PlayerData.Instance.usedCharacter],
                Vector3.zero, 
                Quaternion.identity);
            yield return op;
            if (op.Result == null)
            {
                Debug.LogWarning(string.Format("Unable to load character {0}.", PlayerData.Instance.characters[PlayerData.Instance.usedCharacter]));
                yield break;
            }
            var player = op.Result.GetComponent<Characters.Character>();
    
            characterController.character = player;

            player.transform.SetParent(characterPosition.transform, false);
        }

        private void InitializeTheme()
        {
            _currentThemeData = ThemeDatabase.GetThemeData(PlayerData.Instance.themes[PlayerData.Instance.usedTheme]);
            _currentZone = 0;
            _mCurrentZoneDistance = 0;
            characterController.Coins = 0;
            _score = 0;
            _scoreAccumulator = 0;

            _safeSegmentsLeft = StartingSafeSegments;
            Coin.CoinPool = new Pooler(_currentThemeData.collectiblePrefab, 200);
        }

        private void InitializeGame()
        {
            gameObject.SetActive(true);
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
        
        #endregion
        
        #region Update Methods
        private void Update()
        {
            CheckSegmentSpawn();

            if (!_isMoving) 
                return;

            UpdateGameProgress();
            UpdateSpeed();
            RemovePastSegments();
            UpdatePlayerRank();

            MusicPlayer.Instance.UpdateVolumes(SpeedRatio);
        }
        
        private void UpdateGameProgress()
        {
            float scaledSpeed = _speed * Time.deltaTime;
            _scoreAccumulator += scaledSpeed;
            _mCurrentZoneDistance += scaledSpeed;

            int intScore = Mathf.FloorToInt(_scoreAccumulator);
            if (intScore != 0) 
            {
                AddScore(intScore);
                _scoreAccumulator -= intScore;
            }

            _totalWorldDistance += scaledSpeed;
            _currentSegmentDistance += scaledSpeed;
            _multiplier = 1 + Mathf.FloorToInt((_speed - minSpeed) / (maxSpeed - minSpeed) * speedStep);

            UpdateSegmentList();
            PowerupSpawnUpdate();
        }
        
        private void UpdateSpeed()
        {
            _speed = Mathf.Min(_speed + Acceleration * Time.deltaTime, maxSpeed);
        }
        
        private void UpdatePlayerRank()
        {
            int currentTarget = (PlayerData.Instance.rank + 1) * 300;
            if (_totalWorldDistance > currentTarget)
            {
                PlayerData.Instance.rank += 1;
                PlayerData.Instance.Save();
            }
        }
        
        private void PowerupSpawnUpdate()
        {
            _timeSincePowerup += Time.deltaTime;
        }

        private void ChangeZone()
        {
            _currentZone += 1;
            if (_currentZone >= _currentThemeData.zones.Length)
                _currentZone = 0;

            _mCurrentZoneDistance = 0;
        }
        public void AddScore(int amount)
        {
            _score += amount * _multiplier;
        }

        #endregion

        #region Segment Management Methods

        private void UpdateSegmentList()
        {
            if (segments.Count > 0)
            {
                if (_currentSegmentDistance > segments[0].WorldLength)
                {
                    _currentSegmentDistance = 0;
                    pastSegments.Add(segments[0]);
                    segments.RemoveAt(0);
                    _spawnedSegments--;
                }
            }
        }
        private void CheckSegmentSpawn()
        {
            while (_spawnedSegments < DesiredSegmentCount)
            {
                StartCoroutine(SpawnNewSegment());
                _spawnedSegments++;
            }
        }
        private IEnumerator SpawnNewSegment()
        {
            int segmentUse;
            AsyncOperationHandle segmentToUseOp;
            
            if (_currentThemeData.zones[_currentZone].length < _mCurrentZoneDistance)
                ChangeZone();
            
            segmentUse = Random.Range(0, _currentThemeData.zones[_currentZone].prefabList.Length);
            if (segmentUse == _previousSegment) segmentUse = (segmentUse + 1) % _currentThemeData.zones[_currentZone].prefabList.Length;

            segmentToUseOp = _currentThemeData.zones[_currentZone].prefabList[segmentUse].InstantiateAsync(_offScreenSpawnPos, Quaternion.identity);
            yield return segmentToUseOp;
            if (segmentToUseOp.Result == null || !(segmentToUseOp.Result is GameObject))
            {
                Debug.LogWarning(string.Format("Unable to load segment {0}.", _currentThemeData.zones[_currentZone].prefabList[segmentUse].Asset.name));
                yield break;
            }
            
                
            TrackSegment newSegment = (segmentToUseOp.Result as GameObject)?.GetComponent<TrackSegment>();

            Vector3 currentExitPoint;
            Quaternion currentExitRotation;
            
            if (segments.Count > 0)
            {
                segments[^1].GetPointAt(1.0f, out currentExitPoint, out currentExitRotation);
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


            var transform1 = newSegment.transform;
            Vector3 pos = currentExitPoint + (transform1.position - entryPoint);
            transform1.position = pos;
            newSegment.trackManager = this;
            newSegment.transform.SetParent(parentObject.transform);
            
            if (_safeSegmentsLeft <= 0)
            {
                StartCoroutine(SpawnObstacleAndCoin(newSegment));
            }
            else
                _safeSegmentsLeft -= 1;

            segments.Add(newSegment);
        }
        private void RemovePastSegments()
        {
            for (int i = 0; i < pastSegments.Count; ++i)
            {
                if ((characterController.transform.position - pastSegments[i].transform.position).z > SegmentRemovalDistance)
                {
                    pastSegments[i].Cleanup();
                    pastSegments.RemoveAt(i);
                    i--;
                }
            }
        }
        public void ClearSegments()
        {
            _safeSegmentsLeft = 1;

            foreach (var seg in segments)
            {
                Addressables.ReleaseInstance(seg.gameObject);
                _spawnedSegments--;
            }

            for (int i = 0; i < pastSegments.Count; ++i)
            {
                Addressables.ReleaseInstance(pastSegments[i].gameObject);
            }
            segments.Clear();
            pastSegments.Clear();
        }

        #endregion

        #region Coin & Obstacle Spawn Methods

        private IEnumerator SpawnObstacleAndCoin(TrackSegment segment)
        {
            if (segment.possibleObstacles.Length != 0)
            {
                for (int i = 0; i < segment.obstaclePositions.Length; ++i)
                {
                    var assetRef = segment.possibleObstacles[Random.Range(0, segment.possibleObstacles.Length)];
                    yield return StartCoroutine(SpawnFromAssetReference(assetRef, segment, i));
                }
            }
            yield return StartCoroutine(SpawnCoinAndPowerup(segment));
        }

        private IEnumerator SpawnFromAssetReference(AssetReference reference, TrackSegment segment, int posIndex)
        {
            Vector3 pos;
            Quaternion rot;
            segment.GetPointAt(segment.obstaclePositions[posIndex], out pos, out rot);
            
            AsyncOperationHandle op = Addressables.LoadAssetAsync<GameObject>(reference);
            yield return op; 
            GameObject obj = op.Result as GameObject;

            var obstacleLength = obj.GetComponent<Obstacle>().obstacleLength;
            
            if (CheckObstacleCollision(pos,Quaternion.identity, obstacleLength))
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
        private bool CheckObstacleCollision(Vector3 pos,Quaternion rot,float obstacleLength)
        {
            if (Physics.CheckBox(pos,new Vector3(laneOffset,2f,obstacleLength*2f),rot,1<<9))
            {
                return true;
            }
            return false;
        }
        
        private IEnumerator SpawnCoinAndPowerup(TrackSegment segment)
        {
            const float increment = 2f;
            float currentWorldPos = 0.0f;
            int currentLane = Random.Range(0, 3);
            float powerupChance = Mathf.Clamp01(Mathf.Floor(_timeSincePowerup) * 500f * 0.001f);

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
                        if (obstacle.coinSpawnType==ObstacleCoinSpawnType.SpawnByJumping)
                        {
                            float radius = characterController.characterMovement.jumpHeight*1.5f;
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
                                float z = obstaclePosition.z+(Mathf.Cos(angle * Mathf.Deg2Rad) * Speed/2);


                                Vector3 coinPosition = ((testedLane - 1) * laneOffset * (Vector3.right) +
                                                        new Vector3(pos.x, y , z));

                                toUse = Coin.CoinPool.Get(coinPosition, rot);
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
                        int picked = Random.Range(0, consumableDatabase.consumables.Length);
                        
                        if (consumableDatabase.consumables[picked].canBeSpawned)
                        {
                            _timeSincePowerup = 0.0f;
                            powerupChance = 0.0f;

                            AsyncOperationHandle op = Addressables.InstantiateAsync(consumableDatabase.consumables[picked].gameObject.name, pos, Quaternion.identity);
                            yield return op;
                            if (op.Result == null || !(op.Result is GameObject))
                            {
                                Debug.LogWarning(string.Format("Unable to load consumable {0}.", consumableDatabase.consumables[picked].gameObject.name));
                                yield break;
                            }
                            toUse = op.Result as GameObject;
                            toUse.transform.SetParent(segment.transform, true);
                        }
                    }
                    else
                    {
                        toUse = Coin.CoinPool.Get(pos, Quaternion.identity);
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

        #endregion
        
    }
}