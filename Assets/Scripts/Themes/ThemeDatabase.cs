using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Dev.Scripts.Themes
{
    public static class ThemeDatabase
    {
        private static Dictionary<string, ThemeData> themeDataList;
        public static Dictionary<string, ThemeData> Dictionary => themeDataList;

        private static bool _loaded = false;
        public static bool loaded => _loaded;

        public static ThemeData GetThemeData(string type)
        {
            ThemeData list;
            if (themeDataList == null || !themeDataList.TryGetValue(type, out list))
                return null;

            return list;
        }

        public static IEnumerator LoadDatabase()
        {
            if (themeDataList == null)
            {
                themeDataList = new Dictionary<string, ThemeData>();


                yield return Addressables.LoadAssetsAsync<ThemeData>("ThemeData", op =>
                {
                    if (op != null)
                    {
                        if(!themeDataList.ContainsKey(op.themeName))
                            themeDataList.Add(op.themeName, op);
                    }
                });

                _loaded = true;
            }

        }
    }
}