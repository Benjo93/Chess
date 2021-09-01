using System;
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
    private GameObject[] blocks = new GameObject[64];

    // All active game object pieces on the board. 
    private GameObject[] active_pieces = new GameObject[64];

    // The index of the piece that is currently selected, unselected = -1
    private int selected_index; 

    /* 
     * Board State:
     * Integer array to track positions of all pieces. 
     * Positive represents white and negative represents black. 
     * Zero is an empty space. 
     * I'm not sure how we are doing the AI so this could change. 
     */ 
    private int[] board_state = new int[]
    {
         2,  4,  3,  6,  5,  3,  4,  2,
         1,  1,  1,  1,  0,  1,  1,  1,
         0,  0,  0,  0,  0,  0,  0,  0,
         0,  0,  0,  0,  0,  0,  0,  0,
         0,  0,  0,  0,  1,  0,  0,  0,
         0,  0,  0,  0,  0,  0,  0,  0,
        -1, -1, -1, -1, -1, -1, -1, -1,
        -2, -4, -3, -5, -6, -3, -4, -2
    };

    // Alternate board state using a multidimensional array. 
    private int[,] board_state_alt = new int[,]
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

    // Function called by the AI to get the current board state and to calculate the next move.
    public int[] GetBoardState()
    {
        // Potentially validate board state first.
        return board_state; 
    }

    // Function called by the AI after a decision has been made. 
    public void MovePiece (int from, int to)
    {
        // Validate move. 
       
        // Update the pieces game object array. 
        active_pieces[to] = active_pieces[from];
        active_pieces[from] = null;

        // Update the position of the piece.
        active_pieces[to].transform.position = blocks[to].transform.position;

        // Update the board state. 
        board_state[to] = board_state[from];
        board_state[from] = 0;
    }

    public void Attack()
    {
        // Move piece and update board state / pieces.
        // Send the captured piece to the captured box.
    }

    void Start()
    {
        // Layout blocks Grid, 8x8
        bool flip = true;
        int index = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject theBlock = Instantiate(block, new Vector3(j, i, 0f), Quaternion.identity, transform);
                blocks[index] = theBlock;

                Block block_attrs = theBlock.AddComponent<Block>();
                block_attrs.SetPosition(index); 
                theBlock.transform.name = "Block #" + index++;

                Color b_color = theBlock.GetComponent<SpriteRenderer>().material.color = flip ? new Color32(157, 127, 97, 255) : new Color32(101, 82, 62, 255);
                block_attrs.color = b_color;
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
        for (int p = 0; p < board_state.Length; p++)
        {
            switch (board_state[p])
            {
                case 1: // Pawn
                    active_pieces[p] = Instantiate(Chess.PIECES["w_pawn"], blocks[p].transform.position, Quaternion.identity);
                    break;

                case 2: // Rook
                    active_pieces[p] = Instantiate(Chess.PIECES["w_rook"], blocks[p].transform.position, Quaternion.identity);         
                    break;

                case 3: // Bishop
                    active_pieces[p] = Instantiate(Chess.PIECES["w_bishop"], blocks[p].transform.position, Quaternion.identity);
                    break;

                case 4: // Knight
                    active_pieces[p] = Instantiate(Chess.PIECES["w_knight"], blocks[p].transform.position, Quaternion.identity);
                    break;

                case 5: // Queen
                    active_pieces[p] = Instantiate(Chess.PIECES["w_queen"], blocks[p].transform.position, Quaternion.identity);
                    break;

                case 6: // King
                    active_pieces[p] = Instantiate(Chess.PIECES["w_king"], blocks[p].transform.position, Quaternion.identity);
                    break;

                case -1: // Pawn
                    active_pieces[p] = Instantiate(Chess.PIECES["b_pawn"], blocks[p].transform.position, Quaternion.identity);
                    break;

                case -2: // Rook
                    active_pieces[p] = Instantiate(Chess.PIECES["b_rook"], blocks[p].transform.position, Quaternion.identity);
                    break;

                case -3: // Bishop
                    active_pieces[p] = Instantiate(Chess.PIECES["b_bishop"], blocks[p].transform.position, Quaternion.identity);
                    break;

                case -4: // Knight
                    active_pieces[p] = Instantiate(Chess.PIECES["b_knight"], blocks[p].transform.position, Quaternion.identity);
                    break;

                case -5: // Queen
                    active_pieces[p] = Instantiate(Chess.PIECES["b_queen"], blocks[p].transform.position, Quaternion.identity);
                    break;

                case -6: // King
                    active_pieces[p] = Instantiate(Chess.PIECES["b_king"], blocks[p].transform.position, Quaternion.identity);
                    break;
            }
        }
    }

    private void Update()
    {
        /* 
         * This will proabably all end up in the Game Manager class, which would handle 
         * requesting a move from the player or AI. 
         * For this, I am just storing the index of the clicked block and calling
         * the move function with the two indexes.       
         */

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider != null)
            {
                Block block = hit.transform.gameObject.GetComponent<Block>();

                if (block)
                {
                    int index = block.GetPosition();

                    if (active_pieces[index])
                    {
                        selected_index = index;
                    }
                    else if (selected_index >= 0)
                    {
                        MovePiece(selected_index, block.GetPosition());
                        selected_index = -1;
                    }
                    ShowPositions();
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
    
    private void CalculateMoves(int position)
    {
        // Deselect all blocks.
        foreach (GameObject d_block in blocks)
        {
            d_block.GetComponent<Block>().moveable = false;
            d_block.GetComponent<SpriteRenderer>().material.color = d_block.GetComponent<Block>().color;
        }

        try
        {
            // Right
            if (board_state[position + 1] == 0)
            {
                blocks[position + 1].GetComponent<SpriteRenderer>().material.color = new Color(0f, 1f, 0f, 0.25f);
                blocks[position + 1].GetComponent<Block>().moveable = true;

            }
            // Left 
            if (board_state[position - 1] == 0)
            {
                blocks[position - 1].GetComponent<SpriteRenderer>().material.color = new Color(0f, 1f, 0f, 0.25f);
                blocks[position - 1].GetComponent<Block>().moveable = true;
            }
            // Up
            if (board_state[position + 8] == 0)
            {
                blocks[position + 8].GetComponent<SpriteRenderer>().material.color = new Color(0f, 1f, 0f, 0.25f);
                blocks[position + 8].GetComponent<Block>().moveable = true;
            }
            // Down
            if (board_state[position - 8] == 0)
            {
                blocks[position - 8].GetComponent<SpriteRenderer>().material.color = new Color(0f, 1f, 0f, 0.25f);
                blocks[position - 8].GetComponent<Block>().moveable = true;
            }
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.Log("Position off of the board");
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
