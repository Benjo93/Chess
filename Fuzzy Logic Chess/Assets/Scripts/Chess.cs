using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/* 
 * Chess/Resources:
 * Stores a public list of pieces, 'Roll Needed' chart and other resources. 
 */

public class Chess : MonoBehaviour
{
    // Player colors.
    public Color player_one;
    public Color player_two;

    // Block colors. 
    public Color color_light;
    public Color color_dark;

    // Array of game objects assigned in unity.
    public GameObject[] pieces;

    // Pubic dictionary to access the chess pieces by name. 
    public static Dictionary<string, GameObject> PIECES = new Dictionary<string, GameObject>();

    public AudioSource[] sounds; 
    public static Dictionary<string, AudioSource> SOUNDS = new Dictionary<string, AudioSource>();

    // Singular audio player
    private static AudioSource AudioSourceOneshot;

    public static readonly string saveFileName = "/save_state.txt";
    public static readonly string settingFileName = "/save_setting.txt";
    public static float volume;
    public static int resolution;
    public static bool fullscreen;

    // Array of integers that correspond to the die roll needed for the column row pair. 
    private static int[,] roll_needed = new int[,]
    {
        { 4, 4, 4, 4, 5, 0 },
        { 4, 4, 4, 4, 5, 2 },
        { 5, 5, 5, 5, 5, 2 },
        { 5, 5, 5, 4, 5, 3 },
        { 4, 4, 4, 5, 5, 5 },
        { 6, 6, 6, 5, 6, 4 }
    };

    void Awake()
    {
        // Refresh Dictionaries when starting scene.
        PIECES = new Dictionary<string, GameObject>();
        SOUNDS = new Dictionary<string, AudioSource>();

        // Asign all pieces to dictionary at runtime. 
        foreach (GameObject piece in pieces) PIECES.Add(piece.transform.name, piece);
        foreach (AudioSource sound in sounds) SOUNDS.Add(sound.transform.name, sound);

        Colors.PLAYER_ONE = player_one;
        Colors.PLAYER_TWO = player_two;

        Colors.BOARD_LIGHT = color_light;
        Colors.BOARD_DARK = color_dark;

        Colors.MOVES_ONE = new Color(Colors.PLAYER_ONE.r, Colors.PLAYER_ONE.g, Colors.PLAYER_ONE.b, 0.5f);
        Colors.MOVES_TWO = new Color(Colors.PLAYER_TWO.r, Colors.PLAYER_TWO.g, Colors.PLAYER_TWO.b, 0.5f);

        AudioSourceOneshot = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        AudioSourceOneshot.playOnAwake = false;

        LoadSetting();
    }

    /* 
     * Roll Needed:
     * Determine which roll is needed to win an attack.
     * for instance, if 'roll' >= RollNeeded(6, 3) then attack is successful.
     */

    public static int RollNeeded(int attacker, int defender)
    {
        return roll_needed[6 - Math.Abs(attacker), 6 - Math.Abs(defender)];
    }

    public static void PlayAudioClip(string audioName)
    {
        AudioSourceOneshot.PlayOneShot(Chess.SOUNDS[audioName].clip, volume);
    }

    public static void SaveSetting()
    {
        if (File.Exists(Application.dataPath + Chess.settingFileName))
        {
            File.Delete(Application.dataPath + Chess.settingFileName);
        }
        using (StreamWriter sw = new StreamWriter(Application.dataPath + Chess.settingFileName))
        {
            sw.WriteLine(volume.ToString());
            sw.WriteLine(resolution.ToString());
            sw.WriteLine(fullscreen ? "1" : "0");
            sw.Close();
        }
    }

    public static void RefreshScreen()
    {
        switch(resolution)
        {
            case 0:
                Screen.SetResolution(1366, 768, fullscreen);
                break;
            case 1:
                Screen.SetResolution(1920, 1080, fullscreen);
                break;
            case 2:
                Screen.SetResolution(2560, 1440, fullscreen);
                break;
        }
    }

    public static void LoadSetting()
    {
        if (File.Exists(Application.dataPath + settingFileName))
        {
            using (StreamReader sr = new StreamReader(Application.dataPath + Chess.settingFileName))
            {
                volume = (float)Convert.ToDouble(sr.ReadLine());
                resolution = Convert.ToInt32(sr.ReadLine());
                fullscreen = sr.ReadLine().Equals("1");
                sr.Close();
            }
        }
        else
        {
            volume = 1f;
            resolution = 1;
            fullscreen = false;
            SaveSetting();
        }
    }

    // Library of colors used in the GUI and chess board.
    public static class Colors
    {

        public static Color PLAYER_ONE;
        public static Color PLAYER_TWO;

        public static Color BOARD_LIGHT;
        public static Color BOARD_DARK;

        public static Color BOARD_HOVER = Color.white;

        public static Color MOVES_ONE;
        public static Color MOVES_TWO; 

        public static Color W_SELECTED = Color.cyan;
        public static Color W_ATTACK = Color.red;

        public static Color B_SELECTED = Color.cyan;
        public static Color B_ATTACK = Color.red;
    }
}