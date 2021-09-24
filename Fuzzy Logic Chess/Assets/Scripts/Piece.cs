using System;
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
    public int n_moves;
    private float move_speed = 10f;

    private string team;

    public int[] position;

    private List<Vector3> path;
    private int path_index;
    private bool moving;

    public Commander commander;
    public bool is_commander;
    public bool has_moved;

    internal Piece InitializePiece(string p_name, string team, int n_moves, int[] position)
    {
        this.p_name = p_name;
        this.team = team;
        this.n_moves = n_moves;
        this.position = position;

        return this;
    }

    private void Update()
    {
        if (moving)
        {
            transform.position = Vector3.Lerp(transform.position, path[path_index], Time.deltaTime * move_speed);
            if ((transform.position - path[path_index]).magnitude <= 0.025f) path_index++;
            if (path_index >= path.Count) moving = false;
        }
    }

    public int MovePiece(List<Vector3> path, int[] new_position)
    {
        this.path = path;
        path_index = 0;
        position = new_position;
        moving = true;

        if (is_commander)
        {
            GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0.4f);
            has_moved = true;

            // If the commander is moving more than one space, 
            // they also use their command authority.
            if (path.Count > 2)
            {
                commander.UseCommandAuthority();
                // Uses 2 out of 6 moves.
                return 2;
            }
        }
        else // If this is not a commander. 
        {
            // Get the commander and use their authority.
            commander.UseCommandAuthority();
            // Limit commander moves to one space.
            commander.RestrictMoves();
        }

        // Uses only 1 out of 6 moves.
        return 1;
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
        // If this is a commander piece. 
        if (GetComponent<Commander>()) return GetComponent<Commander>();
        // If this is not a commander piece.
        else return commander;
    }

    public string GetPName()
    { 
        return p_name; 
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
}