using System;
using System.Collections.Generic;
using UnityEngine;

/* 
 * Chess/Resources:
 * Stores a public list of pieces, 'Roll Needed' chart and other resources. 
 */

public class Chess : MonoBehaviour
{
    // Array of game objects assigned in unity.
    public GameObject[] pieces;

    // Pubic dictionary to access the chess pieces by name. 
    public static Dictionary<string, GameObject> PIECES = new Dictionary<string, GameObject>();

    public AudioSource[] sounds; 

    public static Dictionary<string, AudioSource> SOUNDS = new Dictionary<string, AudioSource>();

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

    void Start()
    {
        // Asign all pieces to dictionary at runtime. 
        foreach (GameObject piece in pieces) PIECES.Add(piece.transform.name, piece);
        foreach (AudioSource sound in sounds) SOUNDS.Add(sound.transform.name, sound);
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

    // Library of colors used in the GUI and chess board.
    public static class Colors
    {

        public static Color BOARD_LIGHT = new Color32(157, 127, 97, 255);
        public static Color BOARD_DARK = new Color32(101, 82, 62, 255);

        public static Color BOARD_HOVER = Color.white;

        public static Color W_SELECTED = Color.cyan;
        public static Color W_ATTACK = Color.red;

        public static Color B_SELECTED = Color.cyan;
        public static Color B_ATTACK = Color.red;
    }
}