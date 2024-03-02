using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Collectable
{
    public class GemCollectableRotator : MonoBehaviour
    {
        [SerializeField] private float spinDuration;
        private void LateUpdate()
        {
            var angleUnit = 360f * (Time.deltaTime / spinDuration);
            var currentEulerAngles = transform.eulerAngles;
            transform.eulerAngles = new Vector3(currentEulerAngles.x, currentEulerAngles.y + angleUnit,
                currentEulerAngles.z);
        }
    }
}
