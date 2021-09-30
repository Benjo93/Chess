using System.Collections.Generic;
using UnityEngine;

/* 
 * Piece:
 * Holds Attributes about each individual piece. 
 * Attached to the piece game object when initialized.
 */

public class Piece : MonoBehaviour
{
    private int corp_id;
    private string p_name;

    // Max number of moves.
    public int n_moves;

    // Speed of piece movement
    private float move_speed = 10f;

    // Integer Representation of piece (-6 to 6)
    public int piece_type;

    // Name of the team (white/black)
    private string team;

    // Current position of the piece.
    public int[] position; 

    private List<Vector3> path;
    private int path_index;
    private bool moving;

    public Commander commander;
    public bool is_commander; 
    public bool has_moved; 

    internal Piece InitializePiece(string p_name, int piece_type, string team, int n_moves, int[] position)
    {
        this.p_name = p_name;
        this.piece_type = piece_type;
        this.team = team;
        this.n_moves = n_moves;
        this.position = position; 
        return this;
    }

    /*
     * Update: Built-in unity function, called once every frame.
     * If 'moving', the position value of the piece gameobjects 'transform' component is Lerped (Linearly Interpolated) to the position 
     * of the path at the current index.
     */

    private void Update()
    {
        // Moving is set to true when the 'MovePiece' function is called.
        if (moving)
        {
            // Reference the 'transform' component of this piece to modify its position.
            transform.position = Vector3.Lerp(transform.position, path[path_index], Time.deltaTime * move_speed);
            // If the magnitude of the difference between the current vector position and the path vector position is below some threashold (0.025), increment path_index.
            if ((transform.position - path[path_index]).magnitude <= 0.025f) path_index++;    
            // If the end of the path is reached, stop moving the piece.
            if (path_index >= path.Count) moving = false;
        }
    }

    /* 
     * MovePiece:
     * Takes a list of vector positions and sets 'moving' to true. 
     * Then the update function handles iterating through each vector position in the path.
     * 
     */

    public int MovePiece(List<Vector3> path, int[] new_position)
    {
        this.path = path;
        path_index = 0;
        position = new_position;
        moving = true;

        if (is_commander)
        {
            // Dim piece, set to has_moved
            GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0.4f);
            has_moved = true;

            // If the commander is moving more than one space, 
            // they also use their command authority.
            if (path.Count > 2)
            {
                commander.UseCommandAuthority();

                // Uses 2 out of 6 moves (They cannot move any other piece).
                return 2; 
            }
        }
        else // If this is not a commander. 
        {
            // Get the commander and use their authority.
            commander.UseCommandAuthority();
            // Limit commander moves to one space (They can move, but not attack).
            commander.RestrictMoves();
        }

        // Uses only 1 out of 6 moves.
        return 1; 
    }

    public int Attack(List<Vector3> path, int[] new_position, bool was_successful)
    {
        if (was_successful)
        {
            this.path = path;
            path_index = 0;
            position = new_position;
            moving = true;
        }

        if (is_commander)
        {
            GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0.4f);
            has_moved = true;
            return 2;
        }
        else
        {
            commander.UseCommandAuthority();
            commander.RestrictMoves();
            return 1;
        }
    }

    public Commander MakeIntoCommander()
    {
        is_commander = true;
        commander = gameObject.AddComponent<Commander>();
        commander.default_moves = n_moves; 
        return commander; 
    }

    public Commander GetCommander()
    {
        return commander;
    }

    public int GetNumberOfMoves()
    {
        return n_moves;
    }

    public string GetTeam()
    {
        return team;
    }

    internal void ResetPiece()
    {
        GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 1f);
        has_moved = false;
        if (is_commander) n_moves = commander.default_moves; 
    }

    public string GetPName()
    {
        return p_name; 
    }
    public int GetCorpID()
    {
        return corp_id;
    }
    public void SetCorpID(int id)
    {
        corp_id = id;
    }
}