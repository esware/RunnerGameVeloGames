using System.Collections.Generic;
using UnityEngine;

namespace Dev.Scripts.Consumables
{
    [CreateAssetMenu(fileName="Consumables", menuName = "EWGames/Consumables Database")]
    public class ConsumableDatabase : ScriptableObject
    {
        public Consumable[] consumables;

        private static Dictionary<Consumable.ConsumableType, Consumable> _consumablesDict;

        public void Load()
        {
            if (_consumablesDict == null)
            {
                _consumablesDict = new Dictionary<Consumable.ConsumableType, Consumable>();

                foreach (var t in consumables)
                {
                    _consumablesDict.Add(t.GetConsumableType(), t);
                }
            }
        }

        public static Consumable GetConsumable(Consumable.ConsumableType type)
        {
            if (_consumablesDict == null)
            {
                Debug.LogError("Consumable database is not loaded. Call Load method before accessing consumables.");
                return null;
            }

            Consumable c;
            return _consumablesDict.TryGetValue(type, out c) ? c : null;
        }
    }

}