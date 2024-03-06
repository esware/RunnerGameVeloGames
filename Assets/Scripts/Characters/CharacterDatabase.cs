using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Dev.Scripts.Characters
{
    public static class CharacterDatabase
    {
        private static Dictionary<string, Character> _charactersDict;
        private static bool _loaded = false;

        public static bool Loaded => _loaded;

        public static Character GetCharacter(string type)
        {
            if (_charactersDict == null || !_charactersDict.TryGetValue(type, out var c))
            {
                return null;
            }

            return c;
        }

        public static IEnumerator LoadDatabase()
        {
            if (_charactersDict != null) yield break;
            
            _charactersDict = new Dictionary<string, Character>();

            var operation = Addressables.LoadAssetsAsync<GameObject>("characters", op =>
            {
                var character = op.GetComponent<Character>();
                if (character != null)
                {
                    _charactersDict.Add(character.characterName, character);
                }
            });

            yield return operation;

            if (operation.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError("Failed to load character database: " + operation.DebugName);
                yield break;
            }

            _loaded = true;
        }
    }

}