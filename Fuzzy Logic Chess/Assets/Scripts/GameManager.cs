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

    private int moves_left = 6; 

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
        //team = Random.Range(0, 2) == 0 ? Team.black : Team.white;

        // Assign black to go first for demo.
        team = Team.black;

        // Initiate the first move.
        CompleteGameState(0);
    }

    // Called from the board manager after move has been made.
    public void CompleteGameState(int moves_used)
    {
        moves_left -= moves_used;

        if (moves_left > 0) players[(int)team].BeginMove();      
        else EndTurn();
    }

    public void EndTurn()
    {
        // Reset all piece colors and moves. 
        bm.RefreshPieces();
        bm.RefreshBlocks();

        // Go to next player.
        team = team == Team.black ? Team.white : Team.black;

        // Reset moves to 6 (maximum)
        moves_left = 6;

        CompleteGameState(0);
    }

    public string GetTeam()
    {
        return team == Team.white ? "white" : "black";
    }
}
