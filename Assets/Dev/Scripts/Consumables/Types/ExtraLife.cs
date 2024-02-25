using System.Collections;

namespace Dev.Scripts.Consumables.Types
{
    public class ExtraLife:Consumable
    {
        private const int _maxLives = 3;
        private const int _coinValue = 10;
        public override ConsumableType GetConsumableType()
        {
            return ConsumableType.EXTRALIFE;
        }

        public override string GetConsumableName()
        {
            return "Life";
        }

        public override int GetPrice()
        {
            return 2000;
        }

        public override int GetPremiumCost()
        {
            return 0;
        }

        public override bool CanBeUsed(CharacterControl c)
        {
            if (c.currentLife == _maxLives)
            {
                return false;
            }

            return true;
        }

        public override IEnumerator Started(CharacterControl c)
        {
            yield return base.Started(c);
            if (c.currentLife < _maxLives)
                c.currentLife += 1;
            else
                c.coins += _coinValue;
        }
    }
}