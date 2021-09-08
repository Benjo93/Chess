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
                        // list of coordinates of movable blocks
                        List<int[]> availableMoves = GetMovesList(index[0], index[1], selected_piece.GetNumberOfMoves());
                        SetBlockListMovable(availableMoves);
                    }
                    // clickable if empty and is within range
                    else if (selected_index[0] >= 0 && selected_index[1] >= 0 && blocks[index[0], index[1]].GetComponent<Block>().IsMovable())
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
     * Get Moves List:
     * Post-condition:
     * Returns a list of coordinates of all movable blocks given an initial
     * position and available moves. 
     */

    private List<int[]> GetMovesList(int row, int col, int moves)
    {
        List<int[]> movesList = new List<int[]>();
        CalculateMovesFIFO(row, col, moves, movesList);
        return movesList;
    }

    /*
     * Calculate Moves FIFO
     * Uses Queues for breadth first search.  The queues are separated between
     * the current generation (m) and the next generation (m-1).  When the 
     * current generation is depleted, the next generation becomes the current
     * generation and a new generation is created.
     */

    private void CalculateMovesFIFO(int row, int col, int m, List<int[]> list)
    {
        Queue<int[]> buildQueue = new Queue<int[]>();
        Queue<int[]> currentQueue = new Queue<int[]>();
        do
        {

            /* NOTE: If you can express the validation for the adjacent blocks better
             * or more optimized, please feel free.  All adjacent blocks needs to be 
             * validated and passed to ProcessBlock() before any of them can be
             * dequeued.  Can't ProcessBlock() while Dequeueing.
             */
            if (row < 7)
            {
                if (IsValidBlock(row + 1, col)) // Down
                {
                    ProcessBlock(row + 1, col, list);
                    buildQueue.Enqueue(new int[2] { row + 1, col });
                }
                if (col < 7 && IsValidBlock(row + 1, col + 1)) // Down Right
                {
                    ProcessBlock(row + 1, col + 1, list);
                    buildQueue.Enqueue(new int[2] { row + 1, col + 1 });
                }
                if (col > 0 && IsValidBlock(row + 1, col - 1)) // Down Left
                {
                    ProcessBlock(row + 1, col - 1, list);
                    buildQueue.Enqueue(new int[2] { row + 1, col - 1 });
                }
            }
            if (row > 0)
            {
                if (IsValidBlock(row - 1, col)) // Up
                {
                    ProcessBlock(row - 1, col, list);
                    buildQueue.Enqueue(new int[2] { row - 1, col });
                }
                if (col < 7 && IsValidBlock(row - 1, col + 1)) // Up Right
                {
                    ProcessBlock(row - 1, col + 1, list);
                    buildQueue.Enqueue(new int[2] { row - 1, col + 1 });
                }
                if (col > 0 && IsValidBlock(row - 1, col - 1)) // Up Left
                {
                    ProcessBlock(row - 1, col - 1, list);
                    buildQueue.Enqueue(new int[2] { row - 1, col - 1 });
                }
            }
            if (col > 0 && IsValidBlock(row, col - 1)) // Left
            {
                ProcessBlock(row, col - 1, list);
                buildQueue.Enqueue(new int[2] { row, col - 1 });
            }
            if (col < 7 && IsValidBlock(row, col + 1)) // Right
            {
                ProcessBlock(row, col + 1, list);
                buildQueue.Enqueue(new int[2] { row, col + 1 });
            }

            /* switches to the next generation if the current generation
             * has been depleted.
             */

            if (currentQueue.Count <= 0)
            {
                currentQueue = buildQueue;
                buildQueue = new Queue<int[]>();
                m--;
            }

            int[] currPos = currentQueue.Dequeue();
            row = currPos[0];
            col = currPos[1];

        } while (m > 0);
    }

    /*
     * Process Block:
     * Adds and marks the block coordinates as visited. Helper function to make
     * CalculateMovesFIFO() shorter.
     * Pre-condition:
     * The block coordinate given is valid.
     * Post-condition:
     * The coordinates are added into the given list and its block's visited
     * and movable attributes are now true.
     */
    private void ProcessBlock(int row, int col, List<int[]> list)
    {
        list.Add(new int[] { row, col });
        blocks[row, col].GetComponent<Block>().SetVisited(true);
    }

    /*
     * Is Valid Block:
     * Post-condition:
     * Validates if the block, based on the coordinates given, is empty and has
     * not been visited yet.
     */
    private bool IsValidBlock(int row, int col)
    {
        return board_state[row, col] == 0 && !blocks[row, col].GetComponent<Block>().IsVisited();
    }


    private void SetBlockListMovable(List<int[]> list)
    {
        foreach (int[] pos in list)
        {
            blocks[pos[0], pos[1]].GetComponent<Block>().SetMovable(true);
            blocks[pos[0], pos[1]].GetComponent<Block>().ChangeColor(Chess.Colors.W_MOVE);
        }
    }

    private void RefreshBlocks()
    {
        // Deselect all blocks.
        foreach (GameObject d_block in blocks)
        {
            d_block.GetComponent<Block>().InitialColor();
            d_block.GetComponent<Block>().SetVisited(false);
            d_block.GetComponent<Block>().SetMovable(false);
        }

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