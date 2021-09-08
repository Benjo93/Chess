using UnityEngine;

public class Player : MonoBehaviour
{
    public string player_name;
    private GameManager gm;
    private BoardManager bm;

    // Assigned in the inspector for now. 
    public enum Type { Human, AI }

    public bool making_move; 

    public void InitializePlayer (string player_name, GameManager game_manager, BoardManager board_manager)
    {
        this.player_name = player_name;
        gm = game_manager;
        bm = board_manager;

        // Type of player, etc..
    }

    public void Move()
    {
        // Pass move to the board manager.

        Debug.Log("Current Player: " + player_name);
        //bm.RequestUserInput();
    }
}
