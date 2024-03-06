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

    private string saveFile = "";


    public int coins;

    public List<string> characters = new List<string>();    
    public int usedCharacter;
    public List<string> themes = new List<string>();                
    public int usedTheme;
    public int highscore;

    public string previousName = "Male";
    
    
	public float masterVolume = float.MinValue, musicVolume = float.MinValue, masterSFXVolume = float.MinValue;
	
    public int ftueLevel = 0;
 
    public int rank = 0;
    
    static int s_Version = 12;
    
    public void AddCharacter(string name)
    {
        characters.Add(name);
    }

    public void AddTheme(string theme)
    {
        themes.Add(theme);
    }

    public void InsertScore(int score)
    {
	    if (score> highscore)
	    {
		    highscore = score;
	    }
    }
    
	public static void Create()
    {
		if (_instance == null)
		{
			_instance = new PlayerData();
			
		    CoroutineHandler.Instance.StartStaticCoroutine(CharacterDatabase.LoadDatabase());
		    CoroutineHandler.Instance.StartStaticCoroutine(ThemeDatabase.LoadDatabase());
        }

        _instance.saveFile = Application.persistentDataPath + "/save.bin";

        if (File.Exists(_instance.saveFile))
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
		_instance.characters.Clear();
		_instance.themes.Clear();
		
		_instance.highscore = 0;
		_instance.usedCharacter = 0;
		_instance.usedTheme = 0;

		_instance.coins = 0;

		_instance.characters.Add("Male");
		_instance.characters.Add("Female");
		_instance.themes.Add("Day");

		_instance.ftueLevel = 0;
        _instance.rank = 0;

        _instance.Save();
	}

     private void Read()
    {
        BinaryReader r = new BinaryReader(new FileStream(saveFile, FileMode.Open));

        int ver = r.ReadInt32();

		if(ver < 6)
		{
			r.Close();

			NewSave();
			r = new BinaryReader(new FileStream(saveFile, FileMode.Open));
			ver = r.ReadInt32();
		}

        coins = r.ReadInt32();
        

        // Read character.
        characters.Clear();
        int charCount = r.ReadInt32();
        for(int i = 0; i < charCount; ++i)
        {
            string charName = r.ReadString();

            characters.Add(charName);
        }

        usedCharacter = r.ReadInt32();

        // Read Themes.
        themes.Clear();
        int themeCount = r.ReadInt32();
        for (int i = 0; i < themeCount; ++i)
        {
            themes.Add(r.ReadString());
        }

        usedTheme = r.ReadInt32();
        
        
		if(ver >= 3)
		{
			highscore=0;
			highscore = r.ReadInt32();
		}
		
		if(ver >= 7)
		{
			previousName = r.ReadString();
		}
		

		if (ver >= 9) 
		{
			masterVolume = r.ReadSingle ();
			musicVolume = r.ReadSingle ();
			masterSFXVolume = r.ReadSingle ();
		}

        if(ver >= 10)
        {
            ftueLevel = r.ReadInt32();
            rank = r.ReadInt32();
        }
        

        r.Close();
    }

     public void Save()
    {
        BinaryWriter w = new BinaryWriter(new FileStream(saveFile, FileMode.OpenOrCreate));

        w.Write(s_Version);
        w.Write(coins);
        

        // Write characters.
        w.Write(characters.Count);
        foreach (string c in characters)
        {
            w.Write(c);
        }

        w.Write(usedCharacter);
        

        // Write themes.
        w.Write(themes.Count);
        foreach (string t in themes)
        {
            w.Write(t);
        }

        w.Write(usedTheme);


		w.Write(highscore);
		

		// Write name.
		w.Write(previousName);
		

		w.Write (masterVolume);
		w.Write (musicVolume);
		w.Write (masterSFXVolume);

        w.Write(ftueLevel);
        w.Write(rank);
        
        w.Close();
    }


}

// Helper class to cheat in the editor for test purpose
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
        PlayerData.Instance.coins += 1000000;
        PlayerData.Instance.Save();
    }
    
}
#endif