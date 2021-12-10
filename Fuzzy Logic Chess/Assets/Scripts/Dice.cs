using System.Collections;
using UnityEngine;

public class Dice : MonoBehaviour
{
    // Assigned in unity inspector.
    public Sprite[] dice_sides;

    public int RollDice()
    {
        // Get a random number (min inclusive, max exclusive).
        int roll = Random.Range(0, 6);

        // Swap the dice image for the image at the 'roll' index
        GetComponent<SpriteRenderer>().sprite = dice_sides[roll];

        //GetComponent<AudioSource>().Play();

        // Return number as would appear on die.
        return roll + 1;
    }
}
