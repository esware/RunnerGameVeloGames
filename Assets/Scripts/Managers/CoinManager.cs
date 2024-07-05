using System.Collections;
using Dev.Scripts;
using Dev.Scripts.Obstacles;
using Dev.Scripts.Track;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public class CoinManager
    {
        private static CoinManager _instance;
        public static CoinManager Instance => _instance ??= new CoinManager();
        public IEnumerator SpawnCoinAndPowerup(TrackSegment segment,TrackManager trackManager)
        {
            var consumableDatabase = trackManager.consumableDatabase;
            var increment = trackManager.Speed / 3f;
            var currentWorldPos = 0.0f;
            var currentLane = Random.Range(0, 3);
            var powerupChance = Mathf.Clamp01(Mathf.Floor(trackManager.TimeSincePowerup) * 5f * 0.001f);

            Vector3 pos;
            Quaternion rot;
            GameObject toUse;

            while (currentWorldPos < segment.WorldLength)
            {
                segment.GetPointAtInWorldUnit(currentWorldPos, out pos, out rot);
                pos += Vector3.up * 1.5f;
                Obstacle obstacle = null;
                var testedLane = currentLane;
                var laneValid = true;
                
                while(Physics.CheckSphere(pos + (testedLane-1) * trackManager.laneOffset * Vector3.right, 1f, 1 << 9))
                {
                    testedLane = (testedLane + 1) % 3;
                    if (currentLane == testedLane)
                    {
                        laneValid = false;
                        break;
                    }
                }
                if (Physics.Raycast(pos + ((testedLane-1) * trackManager.laneOffset * (Vector3.right)),Vector3.forward,out var hitInfo,(trackManager.Speed/2f),1<<9))
                {
                    obstacle = hitInfo.collider.GetComponent<Obstacle>() ?? hitInfo.collider.GetComponentInParent<Obstacle>();

                    if (obstacle.coinSpawnType == ObstacleCoinSpawnType.DontSpawn)
                    {
                        laneValid = false;
                    }
                }
                
                if (Physics.Raycast(pos + ((testedLane-1) * trackManager.laneOffset * (Vector3.right)),Vector3.back,(trackManager.Speed/2f)-1f,1<<9))
                {
                    laneValid = false;
                }

                currentLane = testedLane;
                
                if (laneValid)
                {
                    pos += (currentLane - 1) * trackManager.laneOffset * (rot * Vector3.right);
                    
                    if (obstacle != null)
                    {
                        if (obstacle.coinSpawnType==ObstacleCoinSpawnType.SpawnByJumping)
                        {
                            float radius = trackManager.characterController.characterMovement.jumpHeight*1.5f;
                            float circleCircumference = Mathf.PI * radius;
                            int numberOfPoints = Mathf.RoundToInt(circleCircumference / (increment));
                            
                            var obstaclePosition = obstacle.transform.position;
                            
                            for (int i = 0; i < numberOfPoints; i++)
                            {
                                float angle = (i+1) * 180f / numberOfPoints;

                                float y = obstaclePosition.y + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                                float z = obstaclePosition.z + Mathf.Cos(angle * Mathf.Deg2Rad) * trackManager.Speed/2f;


                                Vector3 coinPosition = new Vector3(pos.x, y , z);

                                toUse = Coin.CoinPool.Get(coinPosition, rot);
                                toUse.transform.SetParent(segment.collectibleTransform, true);
                                
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
                            trackManager.TimeSincePowerup = 0.0f;
                            powerupChance = 0.0f;

                            AsyncOperationHandle op = Addressables.InstantiateAsync(consumableDatabase.consumables[picked].gameObject.name, pos, Quaternion.identity);
                            yield return op;
                            if (op.Result == null || !(op.Result is GameObject))
                            {
                                Debug.LogWarning(
                                    $"Unable to load consumable {consumableDatabase.consumables[picked].gameObject.name}.");
                                yield break;
                            }
                            toUse = op.Result as GameObject;
                            if (toUse != null) toUse.transform.SetParent(segment.transform, true);
                        }
                    }
                    else
                    {
                        toUse = Coin.CoinPool.Get(pos, rot);
                        toUse.transform.SetParent(segment.collectibleTransform, true);
                    }
                }
                currentWorldPos += increment;
            }
        }
    }
}