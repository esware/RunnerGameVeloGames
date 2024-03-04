using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Dev.Scripts.Themes
{
    [System.Serializable]
    public struct ThemeZone
    {
        public int length;
        public AssetReference[] prefabList;
    }
    [CreateAssetMenu(fileName = "themeData", menuName = "EWGames/Theme Data")]
    public class ThemeData : ScriptableObject
    {
        [Header("Theme Data")] 
        public string themeName;
        public Sprite themeIcon;

        [Header("Objects")] 
        public ThemeZone[] zones;
        public GameObject collectiblePrefab;

    }
}