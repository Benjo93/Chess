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
    private bool DidDelegate = false;

    private int moves_left = 6; 

    public void StartGame()
    {
        // Load the pieces and assign the commanders. 
        //bm.InitializeBoard(bm.LoadPieces());
        //bm.InitializeCorps(bm.LoadCommand());
        bm.setup_complete = true;

        // Create players with session data. 
        CreatePlayer((int)Team.white);
        CreatePlayer((int)Team.black);

        // Randomly select a player to go first.
        //team = Random.Range(0, 2) == 0 ? Team.black : Team.white;

        // Assign black to go first for demo.
        team = Team.black;

        // Initiate the first move.
        CompleteGameState(0);
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

        // Reset moves to players current max_moves.
        moves_left = players[(int)team].max_moves;

        //Reset Delegation Action
        DidDelegate = false;

        CompleteGameState(0);
    }

    public void SetDidDelegate(bool answer)
    {
        this.DidDelegate = answer;
    }

    public bool GetDidDelegate()
    {
        return DidDelegate;
    }

    public string GetTeam()
    {
        return team == Team.white ? "white" : "black";
    }

    public void LooseCommander()
    {
        players[(int) (team == Team.black ? Team.white : Team.black)].max_moves -= 2;
    }
}
