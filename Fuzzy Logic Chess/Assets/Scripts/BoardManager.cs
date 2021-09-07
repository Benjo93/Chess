using System;
using System.Collections.Generic;
using UnityEngine;

/* 
 * Board Manager:
 * Manages the UI elements of the board and the board state. Contains functions for moving, 
 * attacking and getting the board state; which can be called from other classes like Corp/AI. 
 */

public class BoardManager : MonoBehaviour
{
    // Clickable Game Object assigned in unity. 
    public GameObject block;

    // Array of blocks for player interaction and piece positioning. 
    private GameObject[,] blocks = new GameObject[8, 8];

    // All active game object pieces on the board. 
    private GameObject[,] active_pieces = new GameObject[8, 8];

    // The index of the piece that is currently selected, unselected = {-1 , -1}
    private int[] selected_index = new int[] { -1, -1 };

    // The index the mouse is currently hovering over.
    private int[] hovered_index = new int[] { 0, 0 };

    // The Piece class of the selected piece.
    private Piece selected_piece;

    /* 
     * Board State:
     * Integer array to track positions of all pieces. 
     * Positive represents white and negative represents black. 
     * Zero is an empty space. 
     * I'm not sure how we are doing the AI so this could change. 
     */

    // Alternate board state using a multidimensional array. 
    private int[,] board_state = new int[,]
    {
        {  2,  4,  3,  6,  5,  3,  4,  2 },
        {  1,  1,  1,  1,  1,  1,  1,  1 },
        {  0,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0 },
        { -1, -1, -1, -1, -1, -1, -1, -1 },
        { -2, -4, -3, -5, -6, -3, -4, -2 },
    };

    // Function called by the AI to get the current board state and to calculate the next move.
    public int[,] GetBoardState()
    {
        // Potentially validate board state first.
        return board_state;
    }

    // Function called by the AI after a decision has been made. 
    public void MovePiece(int[] from, int[] to)
    {
        // Validate move. 

        // Update the pieces game object array. 
        active_pieces[to[0], to[1]] = active_pieces[from[0], from[1]];
        active_pieces[from[0], from[1]] = null;

        // Instant position update.
        //active_pieces[to[0],to[1]].transform.position = blocks[to[0],to[1]].transform.position;

        // Slow position update.
        active_pieces[to[0], to[1]].GetComponent<Piece>().MovePiece(blocks[to[0], to[1]].transform.position, to[0], to[1]);

        // Update the board state. 
        board_state[to[0], to[1]] = board_state[from[0], from[1]];
        board_state[from[0], from[1]] = 0;
    }

    public void Attack()
    {
        // Move piece and update board state / pieces.
        // Send the captured piece to the captured box.
    }

    void Start()
    {
        // Layout blocks Grid, 8x8
        bool flip = false;
        int index = 0;
        for (int i = 0; i < blocks.GetLength(0); i++)
        {
            for (int j = 0; j < blocks.GetLength(1); j++)
            {
                GameObject theBlock = Instantiate(block, new Vector3(j, blocks.GetLength(1) - i, 0f), Quaternion.identity, transform);
                blocks[i, j] = theBlock;

                Block block_attrs = theBlock.AddComponent<Block>();
                block_attrs.SetPosition(i, j);
                theBlock.transform.name = "Block #" + index++;

                Color b_color = theBlock.GetComponent<SpriteRenderer>().material.color = flip ? new Color32(157, 127, 97, 255) : new Color32(101, 82, 62, 255);
                block_attrs.SetColor(b_color);
                if (index % 8 != 0) flip = !flip;
            }
        }
        PopulateBoard();
    }

    /* 
     * Populate Board:
     * Initialize the pieces on the board according to the board state array.
     * Each piece is contained in a static dictionary from the resources 
     * class called 'Chess', or something like that.
     */

    private void PopulateBoard()
    {
        for (int p = 0; p < board_state.GetLength(0); p++)
        {
            for (int q = 0; q < board_state.GetLength(1); q++)
            {
                switch (board_state[p, q])
                {
                    case 1: // Pawn
                        active_pieces[p, q] = Instantiate(Chess.PIECES["w_pawn"], blocks[p, q].transform.position, Quaternion.identity);
                        active_pieces[p, q].AddComponent<Piece>().InitializePiece("w_pawn", p, q, 1);
                        break;

                    case 2: // Rook
                        active_pieces[p, q] = Instantiate(Chess.PIECES["w_rook"], blocks[p, q].transform.position, Quaternion.identity);
                        active_pieces[p, q].AddComponent<Piece>().InitializePiece("w_rook", p, q, 2);
                        break;

                    case 3: // Bishop
                        active_pieces[p, q] = Instantiate(Chess.PIECES["w_bishop"], blocks[p, q].transform.position, Quaternion.identity);
                        active_pieces[p, q].AddComponent<Piece>().InitializePiece("w_bishop", p, q, 1);
                        break;

                    case 4: // Knight
                        active_pieces[p, q] = Instantiate(Chess.PIECES["w_knight"], blocks[p, q].transform.position, Quaternion.identity);
                        active_pieces[p, q].AddComponent<Piece>().InitializePiece("w_knight", p, q, 4);
                        break;

                    case 5: // Queen
                        active_pieces[p, q] = Instantiate(Chess.PIECES["w_queen"], blocks[p, q].transform.position, Quaternion.identity);
                        active_pieces[p, q].AddComponent<Piece>().InitializePiece("w_queen", p, q, 3);
                        break;

                    case 6: // King
                        active_pieces[p, q] = Instantiate(Chess.PIECES["w_king"], blocks[p, q].transform.position, Quaternion.identity);
                        active_pieces[p, q].AddComponent<Piece>().InitializePiece("w_king", p, q, 3);
                        break;

                    case -1: // Pawn
                        active_pieces[p, q] = Instantiate(Chess.PIECES["b_pawn"], blocks[p, q].transform.position, Quaternion.identity);
                        active_pieces[p, q].AddComponent<Piece>().InitializePiece("b_pawn", p, q, 1);
                        break;

                    case -2: // Rook
                        active_pieces[p, q] = Instantiate(Chess.PIECES["b_rook"], blocks[p, q].transform.position, Quaternion.identity);
                        active_pieces[p, q].AddComponent<Piece>().InitializePiece("b_rook", p, q, 2);
                        break;

                    case -3: // Bishop
                        active_pieces[p, q] = Instantiate(Chess.PIECES["b_bishop"], blocks[p, q].transform.position, Quaternion.identity);
                        active_pieces[p, q].AddComponent<Piece>().InitializePiece("b_bishop", p, q, 1);
                        break;

                    case -4: // Knight
                        active_pieces[p, q] = Instantiate(Chess.PIECES["b_knight"], blocks[p, q].transform.position, Quaternion.identity);
                        active_pieces[p, q].AddComponent<Piece>().InitializePiece("b_knight", p, q, 4);
                        break;

                    case -5: // Queen
                        active_pieces[p, q] = Instantiate(Chess.PIECES["b_queen"], blocks[p, q].transform.position, Quaternion.identity);
                        active_pieces[p, q].AddComponent<Piece>().InitializePiece("b_queen", p, q, 3);
                        break;

                    case -6: // King
                        active_pieces[p, q] = Instantiate(Chess.PIECES["b_king"], blocks[p, q].transform.position, Quaternion.identity);
                        active_pieces[p, q].AddComponent<Piece>().InitializePiece("b_king", p, q, 3);
                        break;
                }
            }
        }
    }

    private void Update()
    {

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider)
        {
            Block block = hit.transform.gameObject.GetComponent<Block>();

            if (block)
            {
                int[] index = block.GetPosition();

                // Hovering.
                if (index != hovered_index)
                {
                    blocks[hovered_index[0], hovered_index[1]].GetComponent<Block>().CurrentColor();
                    hovered_index = index;
                    blocks[index[0], index[1]].GetComponent<Block>().HoverColor();
                }

                // Clicking.
                if (Input.GetMouseButtonDown(0))
                {
                    if (active_pieces[index[0], index[1]])
                    {
                        RefreshBlocks();
                        selected_index = index;
                        selected_piece = active_pieces[index[0], index[1]].GetComponent<Piece>();
                        CalculateMoves(index[0], index[1], selected_piece.GetNumberOfMoves()); // Number of moves depends on the type of piece.
                    }
                    else if (selected_index[0] >= 0 && selected_index[1] >= 0)
                    {
                        RefreshBlocks();
                        MovePiece(selected_index, block.GetPosition());
                        selected_index = new int[2] { -1, -1 };
                    }
                }
            }
        }
    }

    /*
     * Calculate Moves:
     * Will probably be handled outside of this class by the AI components, could instead be 
     * a method that asks the AI to return all possible moves to update the UI, only if we 
     * want to display all possible moves in game. 
     */

    private void CalculateMoves(int col, int row, int m)
    {
        CM_Recursive(col + 1, row, m); // Up
        CM_Recursive(col - 1, row, m); // Down
        CM_Recursive(col, row + 1, m); // Left
        CM_Recursive(col, row - 1, m); // Right
        CM_Recursive(col + 1, row + 1, m); // Up and Right
        CM_Recursive(col + 1, row - 1, m); // Up and Left
        CM_Recursive(col - 1, row + 1, m); // Down and Right
        CM_Recursive(col - 1, row - 1, m); // Down and Left
    }

    private void CM_Recursive(int col, int row, int m)
    {
        if (m <= 0 || col >= 8 || col < 0 || row >= 8 || row < 0) return;

        // Check if the current block is empty. 
        if (board_state[col, row] == 0)
        {
            blocks[col, row].GetComponent<Block>().ChangeColor(Chess.Colors.W_MOVE);
            CalculateMoves(col, row, m - 1);
        }
    }

    private void RefreshBlocks()
    {
        // Deselect all blocks.
        foreach (GameObject d_block in blocks)
            d_block.GetComponent<Block>().InitialColor();
    }

    // Print out board state for debugging.
    private void ShowPositions()
    {
        string output = "";
        int index = 0;
        foreach (int position in board_state)
        {
            output += position + "   ";
            if (index++ >= 7)
            {
                output += "\n";
                index = 0;
            }
        }
        Debug.Log(output);
    }
}