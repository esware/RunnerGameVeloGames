using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Dev.Scripts.Characters
{
    public class CharacterDatabase
    {
        private static Dictionary<string, Character> _charactersDict;

        public static Dictionary<string, Character> dictionary => _charactersDict;

        private static bool _loaded = false;
        public static bool loaded => _loaded;

        public static Character GetCharacter(string type)
        {
            Character c;
            if (_charactersDict == null || !_charactersDict.TryGetValue(type, out c))
                return null;

            return c;
        }

        public static IEnumerator LoadDatabase()
        {
            if (_charactersDict == null)
            {
                _charactersDict = new Dictionary<string, Character>();

                yield return Addressables.LoadAssetsAsync<GameObject>("characters", op =>
                {
                    Character c = op.GetComponent<Character>();
                    if (c != null)
                    {
                        _charactersDict.Add(c.characterName, c);
                    }
                });
                _loaded = true;
            }
        }
    }
}