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

    /// <summary>
    /// This is an asset which contains all the data for a theme.
    /// As an asset it live in the project folder, and get built into an asset bundle.
    /// </summary>
    [CreateAssetMenu(fileName = "themeData", menuName = "EWGames/Theme Data")]
    public class ThemeData : ScriptableObject
    {
        [Header("Theme Data")] 
        public string themeName;
        public int cost;
        public Sprite themeIcon;

        [Header("Objects")] 
        public ThemeZone[] zones;
        public GameObject collectiblePrefab;

    }
}