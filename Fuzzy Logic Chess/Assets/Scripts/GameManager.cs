using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Game Manager:
 * Manages the flow of the game and which actions should be taken 
 * at any given time.
 */

public class GameManager : MonoBehaviour
{

    private enum Active_Player { black, white }
    private Active_Player active_player;

    private enum Game_State { begin_turn, move_one, move_two, move_three, end_turn }
    private Game_State game_state = Game_State.begin_turn; 

    public void StartGame()
    {
        // Randomly select a player to go first.
        active_player = Random.Range(0, 2) == 0 ? Active_Player.black : Active_Player.white;      
    }

    private void Update()
    {
        // Space bar to move states for debugging.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MoveGameState();
        }
    }

    public void MoveGameState()
    {
        game_state++;
        if (game_state == Game_State.end_turn)
        {
            // Go to next player.
            active_player = active_player == Active_Player.black ? Active_Player.white : Active_Player.black;

            // Set the game state to begin turn.
            game_state = Game_State.begin_turn;
        }
    }

    public void ProcessGameState()
    {
        switch (game_state)
        {
            case Game_State.begin_turn:
                break;

            case Game_State.move_one:
                // Request move from Player/AI
                break;

            case Game_State.move_two:
                // Request move from Player/AI
                break;

            case Game_State.move_three:
                // Request move from Player/AI
                break;
        }
    }
}
