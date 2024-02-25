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

public struct HighscoreEntry : System.IComparable<HighscoreEntry>
{
	public string name;
	public int score;

	public int CompareTo(HighscoreEntry other)
	{
		// We want to sort from highest to lowest, so inverse the comparison.
		return other.score.CompareTo(score);
	}
}

public class PlayerData
{
    static protected PlayerData m_Instance;
    static public PlayerData instance { get { return m_Instance; } }

    protected string saveFile = "";


    public int coins;

    public List<string> characters = new List<string>();    
    public int usedCharacter;
    public List<string> themes = new List<string>();                
    public int usedTheme;                                          
    public List<HighscoreEntry> highscores = new List<HighscoreEntry>();

    public string previousName = "Male";

    public bool licenceAccepted;
    public bool tutorialDone;

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
    
    public int GetScorePlace(int score)
	{
		HighscoreEntry entry = new HighscoreEntry();
		entry.score = score;
		entry.name = "";

		int index = highscores.BinarySearch(entry);

		return index < 0 ? (~index) : index;
	}

	public void InsertScore(int score, string name)
	{
		HighscoreEntry entry = new HighscoreEntry();
		entry.score = score;
		entry.name = name;

		highscores.Insert(GetScorePlace(score), entry);
		
        while (highscores.Count > 10)
            highscores.RemoveAt(highscores.Count - 1);
	}

    // File management

    public static void Create()
    {
		if (m_Instance == null)
		{
			m_Instance = new PlayerData();
			
		    CoroutineHandler.StartStaticCoroutine(CharacterDatabase.LoadDatabase());
		    CoroutineHandler.StartStaticCoroutine(ThemeDatabase.LoadDatabase());
        }

        m_Instance.saveFile = Application.persistentDataPath + "/save.bin";

        if (File.Exists(m_Instance.saveFile))
        {
	        m_Instance.Read();
        }
        else
        {
	        NewSave();
        }
    }

	static public void NewSave()
	{
		m_Instance.characters.Clear();
		m_Instance.themes.Clear();
		
		m_Instance.usedCharacter = 0;
		m_Instance.usedTheme = 0;

		m_Instance.coins = 0;

		m_Instance.characters.Add("Male");
		m_Instance.themes.Add("MainTheme");

        m_Instance.ftueLevel = 0;
        m_Instance.rank = 0;

        m_Instance.Save();
	}

    public void Read()
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
        

        // Added highscores.
		if(ver >= 3)
		{
			highscores.Clear();
			int count = r.ReadInt32();
			for (int i = 0; i < count; ++i)
			{
				HighscoreEntry entry = new HighscoreEntry();
				entry.name = r.ReadString();
				entry.score = r.ReadInt32();

				highscores.Add(entry);
			}
		}
		
        // Added highscore previous name used.
		if(ver >= 7)
		{
			previousName = r.ReadString();
		}

        if(ver >= 8)
        {
            licenceAccepted = r.ReadBoolean();
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

        if (ver >= 12)
        {
            tutorialDone = r.ReadBoolean();
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

        // Write highscores.
		w.Write(highscores.Count);
		for(int i = 0; i < highscores.Count; ++i)
		{
			w.Write(highscores[i].name);
			w.Write(highscores[i].score);
		}
		

		// Write name.
		w.Write(previousName);

        w.Write(licenceAccepted);

		w.Write (masterVolume);
		w.Write (musicVolume);
		w.Write (masterSFXVolume);

        w.Write(ftueLevel);
        w.Write(rank);

        w.Write(tutorialDone);

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
        PlayerData.instance.coins += 1000000;
        PlayerData.instance.Save();
    }
    
}
#endif