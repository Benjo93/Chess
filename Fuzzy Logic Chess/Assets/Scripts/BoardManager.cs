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
    public GameManager gm; 

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

    // Boolean to track whether the player is making a move.
    private bool input_requested;

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
                    ? Chess.Colors.BOARD_LIGHT : Chess.Colors.BOARD_DARK);
                if (index % 8 != 0) flip = !flip;
            }
        }
        InitializeBoard();
        InitializeCorps();
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
                        .InitializePiece("w_pawn", "white", 1, new int[] { p, q });
                        break;

                    case 2: // Rook
                        pieces[p, q] = Instantiate(Chess.PIECES["w_rook"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("w_rook", "white", 2, new int[] { p, q });
                        break;

                    case 3: // Bishop
                        pieces[p, q] = Instantiate(Chess.PIECES["w_bishop"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("w_bishop", "white", 2, new int[] { p, q });
                        break;

                    case 4: // Knight
                        pieces[p, q] = Instantiate(Chess.PIECES["w_knight"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("w_knight", "white", 4, new int[] { p, q });
                        break;

                    case 5: // Queen
                        pieces[p, q] = Instantiate(Chess.PIECES["w_queen"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("w_queen", "white", 3, new int[] { p, q });
                        break;

                    case 6: // King
                        pieces[p, q] = Instantiate(Chess.PIECES["w_king"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("w_king", "white", 3, new int[] { p, q });
                        break;

                    case -1: // Pawn
                        pieces[p, q] = Instantiate(Chess.PIECES["b_pawn"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("b_pawn", "black", 1, new int[] { p, q });
                        break;

                    case -2: // Rook
                        pieces[p, q] = Instantiate(Chess.PIECES["b_rook"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("b_rook", "black", 2, new int[] { p, q });
                        break;

                    case -3: // Bishop
                        pieces[p, q] = Instantiate(Chess.PIECES["b_bishop"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("b_bishop", "black", 2, new int[] { p, q });
                        break;

                    case -4: // Knight
                        pieces[p, q] = Instantiate(Chess.PIECES["b_knight"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("b_knight", "black", 4, new int[] { p, q });
                        break;

                    case -5: // Queen
                        pieces[p, q] = Instantiate(Chess.PIECES["b_queen"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("b_queen", "black", 3, new int[] { p, q });
                        break;

                    case -6: // King
                        pieces[p, q] = Instantiate(Chess.PIECES["b_king"], blocks[p, q].transform.position, Quaternion.identity).AddComponent<Piece>()
                        .InitializePiece("b_king", "black", 3, new int[] { p, q });
                        break;
                }
            }
        }
    }

    public void InitializeCorps()
    {
        Commander w_bishop_one = pieces[0, 2].MakeIntoCommander();
        // Left Three pawns.
        w_bishop_one.AddPiece(pieces[1, 0]);
        w_bishop_one.AddPiece(pieces[1, 1]);
        w_bishop_one.AddPiece(pieces[1, 2]);
        // Left-side knight.
        w_bishop_one.AddPiece(pieces[0, 1]);

        Commander w_bishop_two = pieces[0, 5].MakeIntoCommander();
        // Right Three pawns.
        w_bishop_two.AddPiece(pieces[1, 5]);
        w_bishop_two.AddPiece(pieces[1, 6]);
        w_bishop_two.AddPiece(pieces[1, 7]);
        // Right-side knight.
        w_bishop_two.AddPiece(pieces[0, 6]);

        Commander w_king = pieces[0, 3].MakeIntoCommander();
        // Middle pawns.
        w_king.AddPiece(pieces[1, 3]);
        w_king.AddPiece(pieces[1, 4]);
        // Two-outer rooks.
        w_king.AddPiece(pieces[0, 0]);
        w_king.AddPiece(pieces[0, 7]);
        // Queen
        w_king.AddPiece(pieces[0, 4]);


        Commander b_bishop_one = pieces[7, 2].MakeIntoCommander();
        // Left Three pawns.
        b_bishop_one.AddPiece(pieces[6, 0]);
        b_bishop_one.AddPiece(pieces[6, 1]);
        b_bishop_one.AddPiece(pieces[6, 2]);
        // Left-side knight.
        b_bishop_one.AddPiece(pieces[7, 1]);

        Commander b_bishop_two = pieces[7, 5].MakeIntoCommander();
        // Right Three pawns.
        b_bishop_two.AddPiece(pieces[6, 5]);
        b_bishop_two.AddPiece(pieces[6, 6]);
        b_bishop_two.AddPiece(pieces[6, 7]);
        // Right-side knight.
        b_bishop_two.AddPiece(pieces[7, 6]);

        Commander b_king = pieces[7, 3].MakeIntoCommander();
        // Middle pawns.
        b_king.AddPiece(pieces[6, 3]);
        b_king.AddPiece(pieces[6, 4]);
        // Two-outer rooks.
        b_king.AddPiece(pieces[7, 0]);
        b_king.AddPiece(pieces[7, 7]);
        // Queen
        b_king.AddPiece(pieces[7, 4]);
    }

    // Built-in Unity function that is called every frame.
    private void Update()
    {
        // Check if the player is making a move.
        if (!input_requested) return;

        // Cast a line in to where the mouse is on the screen.
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        // Check if something is being detected.
        if (hit.collider)
        {
            // Get the 'Block' class of whatever was detected
            Block block = hit.transform.gameObject.GetComponent<Block>();

            // Check if the detected object has a block class.
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
                    // Check if there is a piece at the selected blocks index.
                    if (pieces[index[0], index[1]] && !pieces[index[0], index[1]].has_moved && pieces[index[0], index[1]].GetTeam() == gm.GetTeam())
                    {
                        RefreshBlocks();

                        // Select the piece.
                        selected_index = index;
                        selected_piece = pieces[index[0], index[1]];

                        GetAllPiecesInCorp(selected_piece);

                        // Get a list of all moveable blocks.
                        List<int[]> availableMoves = GetMovesList(index[0], index[1], selected_piece.GetNumberOfMoves());

                        // Paint the blocks that are moveable. 
                        SetBlockListMovable(availableMoves, pieces[index[0], index[1]].GetTeam());

                        // Color selected piece.
                        blocks[index[0], index[1]].ChangeColor(Color.magenta);
                    }
                    // Check if piece is already selected, then if selected block is moveable.
                    else if (selected_piece && blocks[index[0], index[1]].IsMovable())
                    {
                        RefreshBlocks();

                        // Move the piece.
                        MovePiece(selected_index, block.GetPosition(), selected_piece.GetNumberOfMoves());
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

    // Function called by human players to make a move.
    public void RequestInput(string message)
    {
        input_requested = true;
        Debug.Log(message);
    }

    // Function called by the AI after a decision has been made. 
    public void MovePiece(int[] from, int[] to, int max_moves)
    {
        // Find the best path. 
        List<int[]> path = FindPath(from, to, new List<int[]> { from }, max_moves);

        // Slow position update.
        int moves_used = pieces[from[0], from[1]].MovePiece(GetPathPositions(path), to);

        // Update the pieces game object array. 
        pieces[to[0], to[1]] = pieces[from[0], from[1]];
        pieces[from[0], from[1]] = null;
   
        selected_piece = null;
        input_requested = false;

        // Notify the Game Manager of the moves used. 
        gm.CompleteGameState(moves_used);
    }

    public void Attack()
    {
        // Move piece and update board state / pieces.
        // Send the captured piece to the captured box.
    }

    public void GetAllPiecesInCorp(Piece selected_piece)
    {
        // Highlight all pieces in corp.
        foreach (Piece piece in selected_piece.GetCommander().GetPiecesInCorp())
        {
            int[] index = piece.position;
            blocks[index[0], index[1]].ChangeColor(Color.cyan);
        }    

        // Possibly return pieces for AI. 
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
        blocks[row, col].SetVisited(true);
    }

    /*
     * Is Valid Block:
     * Post-condition:
     * Validates if the block, based on the coordinates given, is empty and has
     * not been visited yet.
     */
    private bool IsValidBlock(int row, int col)
    {
        bool within_board = row >= 0 && col >= 0 && row < 8 && col < 8;
        return within_board && !pieces[row, col] && !blocks[row, col].IsVisited();

    }

    private void SetBlockListMovable(List<int[]> list, string team)
    {
        foreach (int[] pos in list)
        {
            blocks[pos[0], pos[1]].SetMovable(true);
            blocks[pos[0], pos[1]].ChangeColor(team == "white" ? Chess.Colors.W_SELECTED : Chess.Colors.B_SELECTED);
        }
    }

    // List of all directions to simplify things.
    private int[,] dir = new int[,] { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 }, { -1, 1 }, { 1, -1 }, { 1, 1 }, { -1, -1 } };

    /*
     * FindPath:
     * Path-finding function that recursivley finds the closest block to the destination. 
     * The closest node is added to a list until the destination is reached.
     */
    private List<int[]> FindPath(int[] current, int[] destination, List<int[]> path, int max_moves)
    {
        // Destination has been reached.
        if (current[0] == destination[0] && current[1] == destination[1]) return path;

        // Initialize cost and current node.
        float cost = Mathf.Infinity;
        int[] closest = current;

        // Iterate through all directions.
        for (int d = 0; d < 8; d++)
        {
            // Reference each node in every direction.
            int[] node = new int[] { current[0] + dir[d, 0], current[1] + dir[d, 1] };

            // If the path was longer than max moves, reset the path. The redundant nodes are now all visited.
            if (max_moves < 0) return FindPath(path[0], destination, new List<int[]> { path[0] }, path.Count - 1);

            // Check if node at direction d is valid.
            if (!IsValidBlock(node[0], node[1])) continue;

            // Calculate the difference between the current node and the destination. 
            int[] diff = new int[] { destination[0] - node[0], destination[1] - node[1] };

            // Calculate the cost of each possible move (magnitude of the distance to the destination).
            float n_cost = Mathf.Sqrt(diff[0] * diff[0] + diff[1] * diff[1]);

            // Check if current node has the lowest cost.
            if (n_cost < cost)
            {
                cost = n_cost;
                closest = node;
            }
        }
        // Visit the node and add it to the path.
        path.Add(closest);
        blocks[closest[0], closest[1]].SetVisited(true);
        return FindPath(closest, destination, path, max_moves - 1);
    }

    /*
     * GetPathPositions:
     * Returns a list of vector positions used to move the piece along the path. 
     */
    private List<Vector3> GetPathPositions(List<int[]> path)
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (int[] p in path)
        {
            positions.Add(blocks[p[0], p[1]].transform.position);

            // Highlight path.
            blocks[p[0], p[1]].ChangeColor(Color.grey);
        }

        // Highlight destination.
        blocks[path[path.Count-1][0], path[path.Count-1][1]].ChangeColor(Color.cyan);

        return positions;
    }

    public void RefreshBlocks()
    {
        foreach (Block block in blocks)
        {
            block.InitialColor();
            block.SetVisited(false);
            block.SetMovable(false);
        }
    }

    public void RefreshPieces()
    {
        foreach (Piece piece in pieces)
        {
            if (piece) piece.ResetPiece();        
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