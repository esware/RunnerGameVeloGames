using System.Collections;
using Dev;
using Dev.Scripts.Obstacles;
using Dev.Scripts.Track;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public class ObstacleManager
    {
        private static ObstacleManager _instance;
        private TrackManager _trackManager;
        private float _minimumSpacing;

        public static ObstacleManager Instance => _instance ??= new ObstacleManager();

        private ObstacleManager() 
        {
            _minimumSpacing = 5.0f;
        }

        private void SetTrackManager(TrackManager trackManager)
        {
            _trackManager = trackManager;
        }

        public IEnumerator SpawnObstacle(TrackSegment segment,TrackManager trackManager)
        {
            SetTrackManager(trackManager);
            if (segment.possibleObstacles.Length != 0)
            {
                float lastSpawnPoint = -_minimumSpacing;
                for (int i = 0; i < segment.obstaclePositions.Length; ++i)
                {
                    float obstacleSpacing = CalculateSpacing(trackManager.Speed);
                    if(segment.obstaclePositions[i] - lastSpawnPoint < obstacleSpacing)
                    {
                        continue;
                    }
                    var assetRef = segment.possibleObstacles[Random.Range(0, segment.possibleObstacles.Length)];
                    yield return CoroutineHandler.Instance.StartCoroutine(SpawnFromAssetReference(assetRef, segment, i));
                    lastSpawnPoint = segment.obstaclePositions[i];
                }
            }

            yield return CoroutineHandler.Instance.StartCoroutine(CoinManager.Instance.SpawnCoinAndPowerup(segment,trackManager));
        }

        private float CalculateSpacing(float speed)
        {
            return Mathf.Clamp(_minimumSpacing + speed / 2, _minimumSpacing, 10.0f);
        }
    
        private IEnumerator SpawnFromAssetReference(AssetReference reference, TrackSegment segment, int posIndex)
        {
            segment.GetPointAt(segment.obstaclePositions[posIndex], out var pos, out _);

            AsyncOperationHandle op = Addressables.LoadAssetAsync<GameObject>(reference);
            yield return op; 
            GameObject obj = op.Result as GameObject;
            
            if (obj != null)
            {
                Obstacle obstacle = obj.GetComponent<Obstacle>()??obj.GetComponentInParent<Obstacle>();
               
                if (IsHittingAnyObstacle(pos))
                {
                    Addressables.ReleaseInstance(obj.gameObject);
                }
                else
                {
                    if (obstacle != null)
                        yield return CoroutineHandler.Instance.StartStaticCoroutine(obstacle.Spawn(segment, segment.obstaclePositions[posIndex])); 
                }
            }
        }
        
        private bool IsHittingAnyObstacle(Vector3 pos)
        {
            var startPos = pos + (-1) * _trackManager.laneOffset * Vector3.right;
            var endPos = pos + 1 * _trackManager.laneOffset * Vector3.right;
            
            if (Physics.CheckCapsule(startPos, endPos, (_trackManager.Speed/2f)+1f,1 << 9))
            {
                return true;
            }
            
            return false;
        }
    }
}