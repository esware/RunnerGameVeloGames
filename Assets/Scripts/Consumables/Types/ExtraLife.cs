using System.Collections;

namespace Dev.Scripts.Consumables.Types
{
    public class ExtraLife:Consumable
    {
        private const int MaxLives = 3;
        private const int CoinValue = 10;
        
        public override ConsumableType GetConsumableType()
        {
            return ConsumableType.Extralife;
        }
        public override string GetConsumableName()
        {
            return "Life";
        }
        public override bool CanBeUsed(CharacterControl c)
        {
            if (c.CurrentLife == MaxLives)
            {
                return false;
            }

            return true;
        }
        public override IEnumerator Started(CharacterControl c)
        {
            yield return base.Started(c);
            if (c.CurrentLife < MaxLives)
                c.CurrentLife += 1;
            else
                c.Coins += CoinValue;
        }
    }
}