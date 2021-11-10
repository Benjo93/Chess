using System.Collections.Generic;
using UnityEngine;

/* 
 * Piece:
 * Holds Attributes about each individual piece. 
 * Attached to the piece game object when initialized.
 */

public class Piece : MonoBehaviour
{
    private string p_name;

    // Speed of piece movement
    private float move_speed = 12f;

    // Integer Representation of piece (-6 to 6)
    public int piece_id;
    public int corp_id;
    public int delegation_id;
    public int temp_id;

    // Name of the team (white/black)
    public string team;

    // Current position of the piece.
    public int[] position;

    public Color color;

    // Max number of moves.
    private int n_moves;

    // Path positions.
    private List<Vector3> path;
    private int path_index;
    private bool moving;

    public Commander commander;
    public bool is_commander;
    public bool has_moved;

    internal Piece InitializePiece(string p_name, int piece_type, string team, int n_moves, int[] position, Color color)
    {
        this.p_name = p_name;
        this.piece_id = piece_type;
        this.team = team;
        this.n_moves = n_moves;
        this.position = position;
        this.color = color;
        delegation_id = 0;
        temp_id = 0;
        GetComponent<SpriteRenderer>().material.color = color;
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
            if ((transform.position - path[path_index]).magnitude <= 0.025f)
            {
                if (path_index < path.Count - 1)
                {
                    Chess.PlayAudioClip("move");
                }
                path_index++;
            }
            // If the end of the path is reached, stop moving the piece.
            if (path_index >= path.Count)
            {
                ColorDim();
                moving = false;
            }
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
        //Chess.PlayAudioClip("move");

        this.path = path;
        path_index = 0;
        position = new_position;
        moving = true;

        if (is_commander)
        {
            // Dim piece, set to has_moved
            has_moved = true;

            // If the commander is moving more than one space, 
            // they also use their command authority.
            if (path.Count > 2)
            {
                commander.UseCommandAuthority();
                // Uses 2 out of 6 moves (They cannot move any other piece so technically their commanded pieces have used their move).
                return 2;
            }
        }
        else // If this is not a commander. 
        {
            // Get the commander and use their authority.
            commander.UseCommandAuthority();
            ColorFull();
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
            // Dim the commanders piece.
            ColorDim();
            // Authority was used. 
            commander.UseCommandAuthority();
            // Disallow commander movement.
            has_moved = true;
            // Uses 2 out of 6 moves (If a commander attacks, they cannot then command their units to attack).
            return 2;
        }
        else
        {
            // Authority was used. 
            commander.UseCommandAuthority();
            // Commander cannot move more than one space after commanding their unit to attack.
            commander.RestrictMoves();
            // The commander can still move one space. 
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
    public void SetNumberOfMoves(int n)
    {
        n_moves = n;
    }

    public string GetTeam()
    {
        return team;
    }

    public void ColorDim()
    {
        GetComponent<SpriteRenderer>().material.color = new Color(color.r, color.g, color.b, 0.5f);
    }

    public void ColorFull()
    {
        GetComponent<SpriteRenderer>().material.color = new Color(color.r, color.g, color.b, 1.0f);
    }

    internal void ResetPiece()
    {
        // Reset the color of the piece and allow movement.
        GetComponent<SpriteRenderer>().material.color = color;
        has_moved = false;

        if (is_commander)
        {
            // Reset the number of moves, in case the commander is restricted to one move.
            n_moves = commander.default_moves;
        }
    }
    public string GetPName()
    {
        return p_name;
    }

    public void SetCorpID(int corp_id)
    {
        this.corp_id = corp_id;
    }

    public int GetCorpID()
    {
        return corp_id;
    }

    public void SetDelegationID(int newID)
    {
        this.delegation_id = newID;
    }

    public int GetDelegationID()
    {
        return delegation_id;
    }

    public void SetTempID(int newID)
    {
        this.temp_id = newID;
    }

    public int GetTempID()
    {
        return temp_id;
    }

    //function to make the tempid increase for delegation.
    public void IncrementTempID()
    {
        if (temp_id == 2)
        {
            temp_id = 0;
        }
        else
        {
            temp_id++;
        }
        //tempid is increased an additional time if the bishop it would end up under is alreay dead
        if (temp_id == 1 && commander.GetLeft().IsEmpty())
        {
            temp_id = 2;
        }
        if (temp_id == 2 && commander.GetRight().IsEmpty())
        {
            temp_id = 0;
        }
    }
    public bool GetHasMoved()
    {
        return has_moved;
    }

    public void SetHasMoved(bool has_moved)
    {
        this.has_moved = has_moved;
    }

    public int GetPieceID()
    {
        return piece_id;
    }

    public bool GetIsCommander()
    {
        return is_commander;
    }
}
