using UnityEngine;

/*
 * Game Manager:
 * Manages the flow of the game and which actions should be taken 
 * at any given time. Attached to the 'Game Manager' object in the unity inspector. 
 */

public class GameManager : MonoBehaviour
{
    public Player[] players = new Player[2];
    public BoardManager bm;

    private enum Team { black, white }
    private Team team;

    private enum Game_State { move_one, move_two, move_three, end_turn }
    private Game_State game_state = Game_State.move_one;

    private void Start()
    {
        // Create players with session data. 
        CreatePlayer((int)Team.white);
        CreatePlayer((int)Team.black);

        // Start the game.
        StartGame();
    }

    private void CreatePlayer(int p)
    {
        switch (Session.players[p])
        {
            case "human":
                players[p] = new Human(Session.names[p], this, bm);
                break;

            case "ai":
                players[p] = new AI(Session.names[p], this, bm);
                break;
        }
    }

    public void StartGame()
    {
        // Randomly select a player to go first.
        team = Random.Range(0, 2) == 0 ? Team.black : Team.white;

        // Initiate the first move.
        CompleteGameState();
    }

    // Called from the board manager after move has been made.
    public void CompleteGameState()
    {
        // Process the current state. 
        switch (game_state)
        {
            case Game_State.move_one:
                Debug.Log("Move One");
                players[(int)team].Move();
                break;

            case Game_State.move_two:
                Debug.Log("Move Two");
                players[(int)team].Move();
                break;

            case Game_State.move_three:
                Debug.Log("Move Three");
                players[(int)team].Move();
                break;
        }

        // Move to the next game state.
        game_state++;
        if (game_state == Game_State.end_turn)
        {
            // Go to next player.
            team = team == Team.black ? Team.white : Team.black;

            // Set the game state to begin turn.
            game_state = Game_State.move_one;
        }
    }

    public string GetTeam()
    {
        return team == Team.white ? "white" : "black";
    }
}
