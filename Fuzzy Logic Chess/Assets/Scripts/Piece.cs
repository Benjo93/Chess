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
    private int n_moves;
    private float move_speed = 10f;

    private string team; 

    private List<Vector3> path;
    private int path_index;
    private bool moving;

    internal Piece InitializePiece(string p_name, string team, int n_moves)
    {
        this.p_name = p_name;
        this.team = team;
        this.n_moves = n_moves;

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

    public void MovePiece(List<Vector3> path)
    {
        this.path = path;
        path_index = 0;
        moving = true;
    }

    public int GetNumberOfMoves()
    {
        return n_moves;
    }

    public string GetTeam()
    {
        return team;
    }
}