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

    // Array of blocks representing each chess board square. 
    private Block[,] blocks = new Block[8, 8];

    // Array of pieces currently in play.
    private Piece[,] pieces = new Piece[8, 8];

    // The 'Piece' component of the currently selected piece.
    private Piece selected_piece;

    // The index of the piece that is currently selected, unselected = {-1 , -1}
    private int[] selected_index = new int[] { -1, -1 };

    // The index the mouse is currently hovering over.
    private int[] hovered_index = new int[] { 0, 0 };

    // Used when initializing the board before a game.
    private int[,] board_init = new int[,]
    {
        {  2,  4,  3,  6,  5,  3,  4,  2 },
        {  1,  1,  1,  1,  1,  1,  1,  1 },
        {  0,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0 },
        {  0,  0,  0,  0,  0,  0,  0,  0 },
        { -1, -1, -1, -1, -1, -1, -1, -1 },
        { -2, -4, -3, -6, -5, -3, -4, -2 },
    };

    private void Start()
    {
        // Layout blocks Grid, 8x8
        bool flip = false;
        int index = 0;
        for (int i = 0; i < blocks.GetLength(0); i++)
        {
            for (int j = 0; j < blocks.GetLength(1); j++)
            {
                // Set the blocks array to the component 'Block' from the instantiated game object.
                blocks[i, j] = Instantiate(block, new Vector3(j, blocks.GetLength(1) - i, 0f), Quaternion.identity, transform).AddComponent<Block>();
                blocks[i, j].SetPosition(i, j);
                blocks[i, j].transform.name = "Block #" + index++;

                blocks[i, j].SetColor(blocks[i, j].GetComponent<SpriteRenderer>().material.color = flip
                    ? Chess.Colors.BOARD_DARK : Chess.Colors.BOARD_DARK);
                if (index % 8 != 0) flip = !flip;
            }
        }
        InitializeBoard();
    }

    /* 
     * Initialize Board:
     * Initialize the pieces on the board according to the board_init array.
     * Each piece is contained in a static dictionary from the resources 
     * class called 'Chess', or something like that.
     */

    private void InitializeBoard()
    {
        for (int p = 0; p < board_init.GetLength(0); p++)
        {
            for (int q = 0; q < board_init.GetLength(1); q++)
            {
                switch (board_init[p, q])
                {
                    case 1: // Pawn
                        pieces[p, q] = Instantiate(Chess.PIECES["w_pawn"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("w_pawn", 1);
                        break;

                    case 2: // Rook
                        pieces[p, q] = Instantiate(Chess.PIECES["w_rook"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("w_rook", 2);
                        break;

                    case 3: // Bishop
                        pieces[p, q] = Instantiate(Chess.PIECES["w_bishop"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("w_bishop", 1);
                        break;

                    case 4: // Knight
                        pieces[p, q] = Instantiate(Chess.PIECES["w_knight"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("w_knight", 4);
                        break;

                    case 5: // Queen
                        pieces[p, q] = Instantiate(Chess.PIECES["w_queen"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("w_queen", 3);
                        break;

                    case 6: // King
                        pieces[p, q] = Instantiate(Chess.PIECES["w_king"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("w_king", 3);
                        break;

                    case -1: // Pawn
                        pieces[p, q] = Instantiate(Chess.PIECES["b_pawn"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("b_pawn", 1);
                        break;

                    case -2: // Rook
                        pieces[p, q] = Instantiate(Chess.PIECES["b_rook"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("b_rook", 2);
                        break;

                    case -3: // Bishop
                        pieces[p, q] = Instantiate(Chess.PIECES["b_bishop"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("b_bishop", 1);
                        break;

                    case -4: // Knight
                        pieces[p, q] = Instantiate(Chess.PIECES["b_knight"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("b_knight", 4);
                        break;

                    case -5: // Queen
                        pieces[p, q] = Instantiate(Chess.PIECES["b_queen"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("b_queen", 3);
                        break;

                    case -6: // King
                        pieces[p, q] = Instantiate(Chess.PIECES["b_king"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("b_king", 3);
                        break;
                }
            }
        }
    }

    // Built-in Unity function that is called every frame.
    private void Update()
    {
        // Cast a line in to where the mouse is on the screen.
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        // Check if something is being detected.
        if (hit.collider)
        {
            Block block = hit.transform.gameObject.GetComponent<Block>();

            // Check if the object is a block.
            if (block)
            {
                // Get the position of the block. 
                int[] index = block.GetPosition();

                // Hovering.
                if (index != hovered_index)
                {
                    blocks[hovered_index[0], hovered_index[1]].CurrentColor();
                    hovered_index = index;
                    blocks[index[0], index[1]].HoverColor();
                }

                // Clicking.
                if (Input.GetMouseButtonDown(0))
                {
                    if (pieces[index[0], index[1]])
                    {
                        RefreshBlocks();
                        selected_index = index;
                        selected_piece = pieces[index[0], index[1]];
                        // list of coordinates of movable blocks
                        List<int[]> availableMoves = GetMovesList(index[0], index[1], selected_piece.GetNumberOfMoves());
                        SetBlockListMovable(availableMoves);
                    }
                    // clickable if empty and is within range
                    else if (selected_index[0] >= 0 && selected_index[1] >= 0 && blocks[index[0], index[1]].IsMovable())
                    {
                        RefreshBlocks();
                        MovePiece(selected_index, block.GetPosition());
                        selected_index = new int[2] { -1, -1 };
                    }
                    else
                    {
                        RefreshBlocks();
                    }
                }
            }
        }
    }

    // Function called by the AI to get the current board state and to calculate the next move.
    public Piece[,] GetBoardState()
    {
        // Potentially validate board state first.
        return pieces;
    }

    // Function called by the AI after a decision has been made. 
    public void MovePiece(int[] from, int[] to)
    {
        // Validate move. 

        // Update the pieces game object array. 
        pieces[to[0], to[1]] = pieces[from[0], from[1]];
        pieces[from[0], from[1]] = null;

        // Slow position update.
        pieces[to[0], to[1]].MovePiece(blocks[to[0], to[1]].transform.position, to[0], to[1]);

        // Instant position update.
        //active_pieces[to[0],to[1]].transform.position = blocks[to[0],to[1]].transform.position;
    }

    public void Attack()
    {
        // Move piece and update board state / pieces.
        // Send the captured piece to the captured box.
    }

    /*
     * Get Moves List:
     * Uses Queues for breadth first search.  The queues are separated between
     * the current generation (m) and the next generation (m-1).  When the 
     * current generation is depleted, the next generation becomes the current
     * generation and a new generation is created.
     * Post-condition:
     * Returns a list of coordinates of all movable blocks given an initial
     * position and available moves. 
     */

    private List<int[]> GetMovesList(int row, int col, int m)
    {
        List<int[]> list = new List<int[]>();
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

            if (currentQueue.Count > 0)
            {
                int[] currPos = currentQueue.Dequeue();
                row = currPos[0];
                col = currPos[1];
            }
            /* if there are no spaces left but you have more moves, move count
             * becomes 0.
             */
            else
            {
                m = 0;
            }

        } while (m > 0);
        return list;
    }

    /*
     * Process Block:
     * Adds and marks the block coordinates as visited. Helper function to make
     * GetMovesList() shorter.
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
        //return board_state[row, col] == 0 && !blocks[row, col].GetComponent<Block>().IsVisited();
        return !pieces[row, col] && !blocks[row, col].GetComponent<Block>().IsVisited();
    }


    private void SetBlockListMovable(List<int[]> list)
    {
        foreach (int[] pos in list)
        {
            blocks[pos[0], pos[1]].GetComponent<Block>().SetMovable(true);
            blocks[pos[0], pos[1]].GetComponent<Block>().ChangeColor(Chess.Colors.W_SELECTED);
        }
    }

    private void RefreshBlocks()
    {
        // Deselect all blocks.
        foreach (Block block in blocks)
        {
            block.InitialColor();
            block.SetVisited(false);
            block.SetMovable(false);
        }
    }

    // Print out board state for debugging.
    private void ShowPositions()
    {
        string output = "";
        int index = 0;
        foreach (int position in board_init)
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