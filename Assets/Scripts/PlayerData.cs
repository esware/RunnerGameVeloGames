using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Dev;
using Dev.Scripts.Characters;
using Dev.Scripts.Themes;
#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerData
{
	private static PlayerData _instance;
    public static PlayerData Instance => _instance;

    #region Public Variables

    public int Coins;
    public readonly List<string> Characters = new List<string>();    
    public int UsedCharacter;
    public readonly List<string> Themes = new List<string>();                
    public int UsedTheme;
    public int Highscore;
    public float MasterVolume = float.MinValue, MusicVolume = float.MinValue, MasterSfxVolume = float.MinValue;
    public int FtueLevel = 0;
    public int Rank = 0;

    #endregion

    #region Private Variables
    
    private string _previousName = "Male";
    private string _saveFile = "";

    #endregion
    
    #region File Operations

    public static void Create()
    {
		if (_instance == null)
		{
			_instance = new PlayerData();
			
		    CoroutineHandler.Instance.StartStaticCoroutine(CharacterDatabase.LoadDatabase());
		    CoroutineHandler.Instance.StartStaticCoroutine(ThemeDatabase.LoadDatabase());
        }

        _instance._saveFile = Application.persistentDataPath + "/save.bin";

        if (File.Exists(_instance._saveFile))
        {
	        _instance.Read();
        }
        else
        {
	        NewSave();
        }
    }

	private static void NewSave()
	{
		_instance.Characters.Clear();
		_instance.Themes.Clear();
		
		_instance.Highscore = 0;
		_instance.UsedCharacter = 0;
		_instance.UsedTheme = 0;

		_instance.Coins = 0;

		_instance.Characters.Add("Male");
		_instance.Characters.Add("Female");
		_instance.Themes.Add("Day");

		_instance.FtueLevel = 0;
        _instance.Rank = 0;

        _instance.Save();
	}

	private void Read()
    {
        BinaryReader r = new BinaryReader(new FileStream(_saveFile, FileMode.Open));
        
        Coins = r.ReadInt32();
        
        Characters.Clear();
        int charCount = r.ReadInt32();
        for(int i = 0; i < charCount; ++i)
        {
            string charName = r.ReadString();

            Characters.Add(charName);
        }

        UsedCharacter = r.ReadInt32();
        
        Themes.Clear();
        int themeCount = r.ReadInt32();
        for (int i = 0; i < themeCount; ++i)
        {
            Themes.Add(r.ReadString());
        }

        UsedTheme = r.ReadInt32();
        
        
        Highscore=0;
        Highscore = r.ReadInt32();
		
        _previousName = r.ReadString();
		

        MasterVolume = r.ReadSingle ();
        MusicVolume = r.ReadSingle ();
        MasterSfxVolume = r.ReadSingle ();

        FtueLevel = r.ReadInt32();
        Rank = r.ReadInt32();
        

        r.Close();
    }

	public void Save()
    {
        BinaryWriter w = new BinaryWriter(new FileStream(_saveFile, FileMode.OpenOrCreate));
        
        w.Write(Coins);
        
        w.Write(Characters.Count);
        foreach (string c in Characters)
        {
            w.Write(c);
        }

        w.Write(UsedCharacter);
        
        w.Write(Themes.Count);
        foreach (string t in Themes)
        {
            w.Write(t);
        }

        w.Write(UsedTheme);


		w.Write(Highscore);
		
		w.Write(_previousName);
		

		w.Write (MasterVolume);
		w.Write (MusicVolume);
		w.Write (MasterSfxVolume);

        w.Write(FtueLevel);
        w.Write(Rank);
        
        w.Close();
    }

    #endregion
    
    public void InsertScore(int score)
    {
	    if (score> Highscore)
	    {
		    Highscore = score;
	    }
    }

}

#if UNITY_EDITOR
public class PlayerDataEditor : Editor
{
	[MenuItem("EWGames Debug/Clear Save")]
    static public void ClearSave()
    {
        File.Delete(Application.persistentDataPath + "/save.bin");
    } 

    [MenuItem("EWGames Debug/Give 1000000 coins and 1000 premium")]
    static public void GiveCoins()
    {
        PlayerData.Instance.Coins += 1000000;
        PlayerData.Instance.Save();
    }
    
}
#endif