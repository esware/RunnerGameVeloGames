using UnityEngine;

namespace Dev.Scripts.Consumables.Types
{
    public class CoinMagnet : Consumable
    {
        private const int LayerMask = 1 << 8;
        private readonly Vector3 _magnetAreaSize = new Vector3(20f, 5.0f, 1.0f);
        private readonly Collider[] _overlappingColliders = new Collider[20];

        public override ConsumableType GetConsumableType()
        {
            return ConsumableType.CoinMag;
        }

        public override string GetConsumableName()
        {
            return "Magnet";
        }

        public override void Tick(CharacterControl c)
        {
            base.Tick(c);

            var cTransform = c.transform;
            int numOverlapping = Physics.OverlapBoxNonAlloc(cTransform.position, 
                _magnetAreaSize, _overlappingColliders,
                cTransform.rotation, LayerMask);
        
            for (int i = 0; i < numOverlapping; ++i)
            {
                Coin coin = _overlappingColliders[i].GetComponent<Coin>();
                if (coin != null && !c.MagnetCoins.Contains(coin.gameObject))
                {
                    _overlappingColliders[i].transform.SetParent(c.transform);
                    c.MagnetCoins.Add(_overlappingColliders[i].gameObject);
                }
            }
        }
    }

}