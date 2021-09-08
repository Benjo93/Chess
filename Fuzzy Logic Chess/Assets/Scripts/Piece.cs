using UnityEngine;

/* 
 * Piece:
 * Holds Attributes about each different piece. 
 */

public class Piece : MonoBehaviour
{
    private string p_name;
    private int n_moves;

    private Vector3 destination;
    private bool moving = false;

    internal Piece InitializePiece(string p_name, int n_moves)
    {
        destination = transform.position;

        this.p_name = p_name;
        this.n_moves = n_moves;

        return this;
    }

    private void Update()
    {
        if (moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * 10f);
            if (transform.position == destination) moving = false;
        }
    }

    public void MovePiece(Vector3 new_position, int row, int col)
    {
        destination = new_position;
        moving = true;
    }

    public int GetNumberOfMoves()
    {
        return n_moves;
    }
}