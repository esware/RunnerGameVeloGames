using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Dev.Scripts.Themes
{
    public static class ThemeDatabase
    {
        private static Dictionary<string, ThemeData> _themeDataList;
        public static Dictionary<string, ThemeData> Dictionary => _themeDataList;

        private static bool _loaded = false;
        public static bool Loaded => _loaded;

        public static ThemeData GetThemeData(string type)
        {
            ThemeData list;
            if (_themeDataList == null || !_themeDataList.TryGetValue(type, out list))
                return null;

            return list;
        }

        public static IEnumerator LoadDatabase()
        {
            if (_themeDataList == null)
            {
                _themeDataList = new Dictionary<string, ThemeData>();


                yield return Addressables.LoadAssetsAsync<ThemeData>("ThemeData", op =>
                {
                    if (op != null)
                    {
                        if(!_themeDataList.ContainsKey(op.themeName))
                            _themeDataList.Add(op.themeName, op);
                    }
                });

                _loaded = true;
            }

        }
    }
}