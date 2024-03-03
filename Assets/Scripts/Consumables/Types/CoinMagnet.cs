using UnityEngine;

namespace Dev.Scripts.Consumables.Types
{
    public class CoinMagnet:Consumable
    {
        private readonly Vector3 _halfExtendsBox = new Vector3(20f, 5.0f, 1.0f);
        private const int _layerMask = 1 << 8;
        private Collider[] _returnColliders = new Collider[20];
        public override ConsumableType GetConsumableType()
        {
            return ConsumableType.COIN_MAG;
        }

        public override string GetConsumableName()
        {
            return "Magnet";
        }
        
        void OnDrawGizmos() {
            Vector3 center = transform.position;
            Vector3 size = new Vector3(20, 5, 1); 
            Quaternion rotation = Quaternion.identity; 
            int layerMask = 1 << 8;
            
            Collider[] colliders = new Collider[20];
            int hitCount = Physics.OverlapBoxNonAlloc(center, size, colliders, rotation, layerMask);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }


        public override void Tick(CharacterControl c)
        {
            base.Tick(c);
            
            int nb = Physics.OverlapBoxNonAlloc(c.transform.position, 
                _halfExtendsBox, _returnColliders,
                c.transform.rotation, _layerMask);
            
            for (int i = 0; i < nb; ++i)
            {
                Coin returnCoin = _returnColliders[i].GetComponent<Coin>();
                if (returnCoin != null && !c.magnetCoins.Contains(returnCoin.gameObject))
                {
                    _returnColliders[i].transform.SetParent(c.transform);
                    c.magnetCoins.Add(_returnColliders[i].gameObject);
                }
            }
        }

        public override void Ended(CharacterControl c)
        {
            base.Ended(c);
        }
    }
}