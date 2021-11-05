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
    public List<Color> playerOne;
    public List<Color> playerTwo;

    // Block colors. 
    public List<Color> colorLight;
    public List<Color> colorDark;

    private static List<Color> PLAYER_ONE_REF;
    private static List<Color> PLAYER_TWO_REF;
    private static List<Color> COLOR_LIGHT_REF;
    private static List<Color> COLOR_DARK_REF;

    private static int player1Index;
    private static int player2Index;
    private static int block1Index;
    private static int block2Index;

    private static Color moveOverlay = new Color(.75f, .75f, .75f, 1f);
    private static Color highlightOverlay = new Color(.5f, .5f, .5f, 2f);

    // Array of game objects assigned in unity.
    public GameObject[] pieces;

    // Pubic dictionary to access the chess pieces by name. 
    public static Dictionary<string, GameObject> PIECES;

    public AudioSource[] sounds; 
    public static Dictionary<string, AudioSource> SOUNDS;

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
        PLAYER_ONE_REF = new List<Color>();
        PLAYER_TWO_REF = new List<Color>();
        COLOR_LIGHT_REF = new List<Color>();
        COLOR_DARK_REF = new List<Color>();

        // Asign all pieces to dictionary at runtime. 
        foreach (GameObject piece in pieces) PIECES.Add(piece.transform.name, piece);
        foreach (AudioSource sound in sounds) SOUNDS.Add(sound.transform.name, sound);
        foreach (Color color in playerOne) PLAYER_ONE_REF.Add(color);
        foreach (Color color in playerTwo) PLAYER_TWO_REF.Add(color);
        foreach (Color color in colorLight) COLOR_LIGHT_REF.Add(color);
        foreach (Color color in colorDark) COLOR_DARK_REF.Add(color);

        LoadSetting();

        Colors.PLAYER_ONE = PLAYER_ONE_REF[player1Index];
        Colors.PLAYER_TWO = PLAYER_TWO_REF[player2Index];

        Colors.BOARD_LIGHT = COLOR_LIGHT_REF[block1Index];
        Colors.BOARD_DARK = COLOR_DARK_REF[block2Index];

        Colors.MOVES_ONE = Colors.PLAYER_ONE * moveOverlay;
        Colors.MOVES_TWO = Colors.PLAYER_TWO * moveOverlay;

        //Colors.W_SELECTED = Colors.PLAYER_ONE + (Color.cyan * highlightOverlay);
        Colors.W_ATTACK = Colors.PLAYER_ONE + (Color.red * highlightOverlay);

        //Colors.B_SELECTED = Colors.PLAYER_TWO + (Color.cyan * highlightOverlay);
        Colors.B_ATTACK = Colors.PLAYER_TWO + (Color.red * highlightOverlay);

        AudioSourceOneshot = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        AudioSourceOneshot.playOnAwake = false;
    }

    public static void IncrementPlayer1Color()
    {
        player1Index++;
        player1Index %= PLAYER_ONE_REF.Count;
        Colors.PLAYER_ONE = PLAYER_ONE_REF[player1Index];
        Colors.MOVES_ONE = Colors.PLAYER_ONE * moveOverlay;
        //Colors.W_SELECTED = Colors.PLAYER_ONE + (Color.cyan * highlightOverlay);
        Colors.W_ATTACK = Colors.PLAYER_ONE + (Color.red * highlightOverlay);
    }
    public static void IncrementPlayer2Color()
    {
        player2Index++;
        player2Index %= PLAYER_TWO_REF.Count;
        Colors.PLAYER_TWO = PLAYER_TWO_REF[player2Index];
        Colors.MOVES_TWO = Colors.PLAYER_TWO * moveOverlay;
        //Colors.B_SELECTED = Colors.PLAYER_TWO + (Color.cyan * highlightOverlay);
        Colors.B_ATTACK = Colors.PLAYER_TWO + (Color.red * highlightOverlay);
    }
    public static void IncrementBlock1Color()
    {
        block1Index++;
        block1Index %= COLOR_LIGHT_REF.Count;
        Colors.BOARD_LIGHT = COLOR_LIGHT_REF[block1Index];
    }
    public static void IncrementBlock2Color()
    {
        block2Index++;
        block2Index %= COLOR_DARK_REF.Count;
        Colors.BOARD_DARK = COLOR_DARK_REF[block2Index];
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
            sw.WriteLine(player1Index + "," + player2Index + "," + block1Index + "," + block2Index);
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
        if (File.Exists(Application.dataPath + Chess.settingFileName))
        {
            using (StreamReader sr = new StreamReader(Application.dataPath + Chess.settingFileName))
            {
                volume = (float)Convert.ToDecimal(sr.ReadLine());
                resolution = Convert.ToInt32(sr.ReadLine());
                fullscreen = sr.ReadLine().Equals("1");
                string[] colorState;
                colorState = sr.ReadLine().Split(',');
                player1Index = Convert.ToInt32(colorState[0]);
                player2Index = Convert.ToInt32(colorState[1]);
                block1Index = Convert.ToInt32(colorState[2]);
                block2Index = Convert.ToInt32(colorState[3]);
                sr.Close();
            }
        }
        else
        {
            volume = 1;
            resolution = 0;
            fullscreen = false;
            player1Index = 0;
            player2Index = 0;
            block1Index = 0;
            block2Index = 0;
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

        public static Color MOVES_ONE;
        public static Color MOVES_TWO;

        public static Color W_ATTACK;
        public static Color B_ATTACK;

        //public static Color BOARD_HOVER = Color.white; //handled by the block class
        //public static Color W_SELECTED;
        //public static Color B_SELECTED;
    }
}