using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * Piece:
 * Holds Attributes about each different piece. 
 */

public class Piece : MonoBehaviour
{
    private Commander commander;
    private int number_of_moves;

    private Vector3 destination;
    private bool moving = false;

    private void Start()
    {
        destination = transform.position; 
    }

    private void Update()
    {
        if (moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * 10f);
            if (transform.position == destination) moving = false;
        }
    }

    public void MovePiece(Vector3 new_position)
    {
        destination = new_position;
        moving = true;
    }

    public void SetCommander(Commander commander)
    {
        this.commander = commander; 
    }
    public Commander GetCommander()
    {
        return commander; 
    }
    public void SetNumberOfMoves(int number_of_moves)
    {
        this.number_of_moves = number_of_moves; 
    }
    public int GetNumberOfMoves()
    {
        return number_of_moves;
    }
}
