using UnityEngine;

/* 
 * Piece:
 * Holds Attributes about each different piece. 
 */

public class Piece : MonoBehaviour
{
    private string p_name;
    private int n_moves;
    int[] position = new int[2]; // Do we need this? If we need this we have to change this every time the piece moves.

    private Vector3 destination;
    private bool moving = false;

    internal void InitializePiece(string p_name, int row, int col, int n_moves)
    {
        destination = transform.position;

        this.p_name = p_name;
        this.n_moves = n_moves;

        position[0] = row;
        position[1] = col;
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
        position[0] = row;
        position[1] = col;

        destination = new_position;
        moving = true;
    }

    public int GetNumberOfMoves()
    {
        return n_moves;
    }
}