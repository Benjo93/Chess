using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* 
 * Board Manager:
 * Manages the UI elements of the board and the board state. Contains functions for moving, 
 * attacking and getting the board state; which can be called from other classes like Corp/AI. 
 */

public class BoardManager : MonoBehaviour
{
    public GameManager gm;

    // Color for pieces, assigned in the inspector.
    public Color color_one;
    public Color color_two;

    public Text hover_info;
    public Text game_log;

    // Reference the game dice.
    public Dice dice;

    // Clickable Game Object assigned in unity. 
    public GameObject block;

    // Array of pieces currently in play.
    private Piece[,] pieces = new Piece[8, 8];

    // Array of blocks representing each chess board square. 
    private Block[,] blocks = new Block[8, 8];

    // UI Game Objects List
    public GameObject DelegationButton;
    public GameObject EndTurnButton;
    public GameObject ConfirmDelegationButton;
    public GameObject CancelButton;
    public GameObject RevokeButton;
    public GameObject ConfirmRevokeButton;
    public GameObject CancelRevokeButton;
    public GameObject Knight_Attack_Button;
    public GameObject Knight_Wait_Button;
    public GameObject GameOver;
    public Text gameOverText;

    private int GlobalDelegationID = 1;

    // Determines if delegation mode is enabled
    public bool delegation = false;

    // Determines if revoke mode is enabled
    public bool revoke = false;

    public bool knightMove = false;

    //Determines if knight is waiting for attack or wait to be pressed
    public bool knightReady = false;

    public int knightx = -1;
    public int knighty = -1;

    // Array of blocks representing each square of the capture box.
    private GameObject[,] whiteCaptureBox = new GameObject[16, 2];

    // Squares for capture table
    public GameObject captureSquare;

    private Vector2 resolution = new Vector2(Screen.width, Screen.height);

    private List<Piece> capturedWhite = new List<Piece>();
    private List<Piece> capturedBlack = new List<Piece>();

    private Dice diceInstance;
    // The 'Piece' component of the currently selected piece.
    private Piece selected_piece;

    // The index of the piece that is currently selected, unselected = {-1 , -1}
    private int[] selected_index = new int[] { -1, -1 };

    // The index the mouse is currently hovering over.
    private int[] hovered_index = new int[] { 0, 0 };

    // Boolean to track whether the player is making a move.
    public bool input_requested;

    // Boolean to keep track of game setup. Called from the Game Manager.
    public bool setup_complete;

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
        DelegationButton.gameObject.SetActive(false);
        EndTurnButton.gameObject.SetActive(false);
        RevokeButton.gameObject.SetActive(false);
        RepositionBoard();
        PrintBoardSquares();
        BuildTable();
        SpawnDice();
        RefitBoard();
    }

    // Built-in Unity function that is called every frame.
    private void Update()
    {
        if (resolution.x != Screen.width || resolution.y != Screen.height)
        {
            RepositionBoard();
            RefitBoard();

            resolution.x = Screen.width;
            resolution.y = Screen.height;
        }

        if (!setup_complete) return;

        // Check if the player is making a move.
        if (!input_requested || knightReady) return;

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

                    DisplayHoverInfo(hovered_index);
                }

                if (delegation == false && knightMove == false)
                {
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

                            // Highlight all pieces in selected piece corp.
                            ShowAllPiecesInCorp(selected_piece);

                            // Get a list of all moveable blocks.
                            List<int[]> availableMoves = GetMovesList(index[0], index[1], selected_piece.GetNumberOfMoves());
                            List<int[]> availableAttacks = GetAttackableList(index[0], index[1]);

                            // Paint the blocks that are moveable. 
                            SetBlockListMovable(availableMoves, pieces[index[0], index[1]].GetTeam());
                            SetBlockListAttackable(availableAttacks, pieces[index[0], index[1]].GetTeam());

                            // Color selected piece.
                            blocks[index[0], index[1]].ChangeColor(Color.white);
                        }
                        // Moving selected piece.
                        else if (selected_piece && blocks[index[0], index[1]].IsMovable())
                        {
                            RefreshBlocks();
                            int moves_used = MovePiece(selected_index, index);
                            EndTurn(moves_used);
                            //Autosave();
                        }
                        // Attacking 
                        else if (selected_piece && blocks[index[0], index[1]].IsAttackable())
                        {
                            RefreshBlocks();

                            StartCoroutine(DiceRollWait(selected_index, index));

                            // moving this section to DiceRollWait()
                            //int moves_used = Attack(selected_index, index);
                            //EndTurn(moves_used);

                        }
                        else
                        {
                            RefreshBlocks();
                        }
                    }
                }
                // If Revoke Mode is enabled
                else if (revoke)
                {
                    Block selectedBlock = blocks[index[0], index[1]];
                    Piece selected_piece = pieces[index[0], index[1]];
                    int corpID = selected_piece.GetCorpID();
                    int delegationID = selected_piece.GetDelegationID();
                    //Clicking.
                    if (Input.GetMouseButtonDown(0))
                    {
                        //Ensures that only one delegation is selected at a time
                        RefreshBlocks();
                        foreach (Piece piece in pieces)
                        {
                            if (piece != null)
                            {
                                //tempid is made 0 so that it doesn't break anything
                                piece.SetTempID(0);
                                if (piece.GetDelegationID() == delegationID && piece.GetDelegationID() != 0)
                                {
                                    //HIghlights every piece in the selected delegation
                                    int[] nindex = piece.position;
                                    blocks[nindex[0], nindex[1]].ChangeColor(Color.yellow);
                                    //tempid is made 1 so that each piece can later be revoked
                                    piece.SetTempID(1);
                                }
                            }

                        }
                    }
                }
                else if (knightMove == true)
                {
                    Piece selected_piece = pieces[index[0], index[1]];
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (pieces[index[0], index[1]] != null)
                        {
                            if (selected_piece.GetTempID() == 1)
                            {
                                int[] knightPosition = { knightx, knighty };
                                int moves_used = Attack(knightPosition, index);
                                EndTurn(moves_used);
                                knightMove = false;
                                foreach (Piece piece in pieces)
                                {
                                    if (piece != null)
                                    {
                                        piece.SetTempID(0);
                                    }
                                }
                                knightx = -1;
                                knighty = -1;
                            }
                        }
                    }
                }
                else if (knightReady == true)
                {
                    //This isn't needed, but I have it here for debug purposes and I may need it again one day.
                }
                // If delegation mode is enabled
                else
                {
                    Block selectedBlock = blocks[index[0], index[1]];
                    Piece selected_piece = pieces[index[0], index[1]];

                    //whenever a piece in king corp is clicked, it changes colors
                    if (Input.GetMouseButtonDown(0) && (selected_piece.GetTeam() == gm.GetTeam()) && selected_piece != null && !selected_piece.GetIsCommander() && (selected_piece.GetCorpID() == 1 || selected_piece.GetCorpID() == -1))
                    {
                        int corpID = selected_piece.GetCorpID();
                        selected_piece.IncrementTempID();
                        if (selected_piece.GetTempID() == 0)
                        {
                            selectedBlock.ChangeColor(Color.red);
                            selectedBlock.DullColor();
                        }
                        // represents left bishop corps
                        else if (selected_piece.GetTempID() == 1)
                        {
                            selectedBlock.ChangeColor(Color.blue);
                        }
                        // represents right bishop corps
                        else
                        {
                            selectedBlock.ChangeColor(Color.green);
                        }
                    }
                }
            }
        }
    }

    internal void Print(int look_count)
    {
        Debug.Log(look_count);
    }

    public void PrintBoardSquares()
    {
        // Layout blocks Grid, 8x8
        bool flip = false;
        int index = 0;
        for (int rank = 0; rank < blocks.GetLength(0); rank++)
        {
            for (int file = 0; file < blocks.GetLength(1); file++)
            {
                blocks[rank, file] = Instantiate(block, new Vector3(file, blocks.GetLength(1) - rank, 0f) * 1.2f, Quaternion.identity, transform).AddComponent<Block>();
                blocks[rank, file].SetPosition(rank, file);
                blocks[rank, file].transform.name = "Block #" + index++;

                blocks[rank, file].SetColor(blocks[rank, file].GetComponent<SpriteRenderer>().material.color = flip
                    ? Chess.Colors.BOARD_LIGHT : Chess.Colors.BOARD_DARK);
                if (index % 8 != 0) flip = !flip;
            }
        }
    }

    public Piece GeneratePiece(int pieceID, int row, int col)
    {
        Piece newPiece;
        switch (pieceID)
        {
            case 1: // Pawn
                newPiece = Instantiate(Chess.PIECES["pixel_pawn"], blocks[row, col].transform.position, Quaternion.identity).AddComponent<Piece>()
                .InitializePiece("p1 pawn", 1, "white", 1, new int[] { row, col }, Chess.Colors.PLAYER_ONE) as Piece;
                break;

            case 2: // Rook
                //Debug.Log(Chess.PIECES.Count);
                //Debug.Log(Chess.PLAYER_TWO_REF.Count);
                newPiece = Instantiate(Chess.PIECES["pixel_rook"], blocks[row, col].transform.position, Quaternion.identity).AddComponent<Piece>()
                .InitializePiece("p1 rook", 2, "white", 2, new int[] { row, col }, Chess.Colors.PLAYER_ONE) as Piece;
                break;

            case 3: // Bishop
                newPiece = Instantiate(Chess.PIECES["pixel_bishop"], blocks[row, col].transform.position, Quaternion.identity).AddComponent<Piece>()
                .InitializePiece("p1 bishop", 3, "white", 2, new int[] { row, col }, Chess.Colors.PLAYER_ONE) as Piece;
                break;

            case 4: // Knight
                newPiece = Instantiate(Chess.PIECES["pixel_knight"], blocks[row, col].transform.position, Quaternion.identity).AddComponent<Piece>()
                .InitializePiece("p1 knight", 4, "white", 4, new int[] { row, col }, Chess.Colors.PLAYER_ONE) as Piece;
                break;

            case 5: // Queen
                newPiece = Instantiate(Chess.PIECES["pixel_queen"], blocks[row, col].transform.position, Quaternion.identity).AddComponent<Piece>()
                .InitializePiece("p1 queen", 5, "white", 3, new int[] { row, col }, Chess.Colors.PLAYER_ONE) as Piece;
                break;

            case 6: // King
                newPiece = Instantiate(Chess.PIECES["pixel_king"], blocks[row, col].transform.position, Quaternion.identity).AddComponent<Piece>()
                .InitializePiece("p1 king", 6, "white", 3, new int[] { row, col }, Chess.Colors.PLAYER_ONE) as Piece;
                break;

            case -1: // Pawn
                newPiece = Instantiate(Chess.PIECES["pixel_pawn"], blocks[row, col].transform.position, Quaternion.identity).AddComponent<Piece>()
                .InitializePiece("p2 pawn", -1, "black", 1, new int[] { row, col }, Chess.Colors.PLAYER_TWO) as Piece;
                break;

            case -2: // Rook
                newPiece = Instantiate(Chess.PIECES["pixel_rook"], blocks[row, col].transform.position, Quaternion.identity).AddComponent<Piece>()
                .InitializePiece("p2 rook", -2, "black", 2, new int[] { row, col }, Chess.Colors.PLAYER_TWO) as Piece;
                break;

            case -3: // Bishop
                newPiece = Instantiate(Chess.PIECES["pixel_bishop"], blocks[row, col].transform.position, Quaternion.identity).AddComponent<Piece>()
                .InitializePiece("p2 bishop", -3, "black", 2, new int[] { row, col }, Chess.Colors.PLAYER_TWO) as Piece;
                break;

            case -4: // Knight
                newPiece = Instantiate(Chess.PIECES["pixel_knight"], blocks[row, col].transform.position, Quaternion.identity).AddComponent<Piece>()
                .InitializePiece("p2 knight", -4, "black", 4, new int[] { row, col }, Chess.Colors.PLAYER_TWO) as Piece;
                break;

            case -5: // Queen
                newPiece = Instantiate(Chess.PIECES["pixel_queen"], blocks[row, col].transform.position, Quaternion.identity).AddComponent<Piece>()
                .InitializePiece("p2 queen", -5, "black", 3, new int[] { row, col }, Chess.Colors.PLAYER_TWO) as Piece;
                break;

            case -6: // King
                newPiece = Instantiate(Chess.PIECES["pixel_king"], blocks[row, col].transform.position, Quaternion.identity).AddComponent<Piece>()
                .InitializePiece("p2 king", -6, "black", 3, new int[] { row, col }, Chess.Colors.PLAYER_TWO) as Piece;
                break;
            default:
                newPiece = Instantiate(new GameObject()).AddComponent<Piece>();
                break;
        }
        return newPiece;
    }
    public void RunRigidbody()
    {
        StartCoroutine(ApplyRigidBody());
    }
    IEnumerator ApplyRigidBody()
    {
        yield return new WaitForSeconds(1f);
        foreach (Block block in blocks)
        {
            Rigidbody2D rb = block.gameObject.AddComponent<Rigidbody2D>();
            Vector2 rand = new Vector2(UnityEngine.Random.Range(-10f, 10.0f), UnityEngine.Random.Range(0, 10.0f));
            rb.AddForce(rand, ForceMode2D.Impulse);
            rb.AddTorque(UnityEngine.Random.Range(-10f, 10.0f), ForceMode2D.Impulse);
        }
        foreach (Piece piece in pieces)
        {
            if (piece)
            {
                piece.gameObject.AddComponent<CircleCollider2D>();
                Rigidbody2D rb = piece.gameObject.AddComponent<Rigidbody2D>();
                Vector2 rand = new Vector2(UnityEngine.Random.Range(-10f, 10.0f), UnityEngine.Random.Range(0, 10.0f));
                rb.AddForce(rand, ForceMode2D.Impulse);
            }
        }
        foreach (Piece piece in capturedWhite)
        {
            if (piece)
            {
                piece.gameObject.AddComponent<CircleCollider2D>();
                Rigidbody2D rb = piece.gameObject.AddComponent<Rigidbody2D>();
                Vector2 rand = new Vector2(UnityEngine.Random.Range(-10f, 10.0f), UnityEngine.Random.Range(0, 10.0f));
                rb.AddForce(rand, ForceMode2D.Impulse);
            }
        }
        foreach (Piece piece in capturedBlack)
        {
            if (piece)
            {
                piece.gameObject.AddComponent<CircleCollider2D>();
                Rigidbody2D rb = piece.gameObject.AddComponent<Rigidbody2D>();
                Vector2 rand = new Vector2(UnityEngine.Random.Range(0, 10.0f), UnityEngine.Random.Range(0, 10.0f));
                rb.AddForce(rand, ForceMode2D.Impulse);
            }
        }
        diceInstance.gameObject.AddComponent<CircleCollider2D>();
        Rigidbody2D rbDice = diceInstance.gameObject.AddComponent<Rigidbody2D>();
        Vector2 randDice = new Vector2(UnityEngine.Random.Range(0, 10.0f), UnityEngine.Random.Range(0, 10.0f));
        rbDice.AddForce(randDice, ForceMode2D.Impulse);
        yield return new WaitForSeconds(2.5f);
    }

    /* 
     * Initialize Board:
     * Initialize the pieces on the board according to the board_init array.
     * Each piece is contained in a static dictionary from the resources 
     * class called 'Chess', or something like that.
     */
    public void InitializeBoard(int[,] pieces_state)
    {
        for (int p = 0; p < pieces_state.GetLength(0); p++)
        {
            for (int q = 0; q < pieces_state.GetLength(1); q++)
            {
                if (pieces_state[p, q] != 0)
                {
                    pieces[p, q] = GeneratePiece(pieces_state[p, q], p, q);
                }
            }
        }
    }

    /* 
     * Initialize Corps:
     * Initialize the corp membership based on a 2D array argument.
     */
    public void InitializeCorps(int[,] command_state)
    {
        int[] w_king_pos = new int[2];
        int[] w_bishop_one_pos = new int[2];
        int[] w_bishop_two_pos = new int[2];
        int[] b_king_pos = new int[2];
        int[] b_bishop_one_pos = new int[2];
        int[] b_bishop_two_pos = new int[2];
        List<Piece> w_king_memb = new List<Piece>();
        List<Piece> w_bishop_one_memb = new List<Piece>();
        List<Piece> w_bishop_two_memb = new List<Piece>();
        List<Piece> b_king_memb = new List<Piece>();
        List<Piece> b_bishop_one_memb = new List<Piece>();
        List<Piece> b_bishop_two_memb = new List<Piece>();

        for (int row = 0; row < command_state.GetLength(0); row++)
        {
            for (int col = 0; col < command_state.GetLength(1); col++)
            {
                if (pieces[row, col])
                {
                    pieces[row, col].SetCorpID(command_state[row, col]);
                }
                switch (command_state[row, col])
                {
                    case 1:
                        if (pieces[row, col].GetPName().Equals("p1 king"))
                        {
                            w_king_pos[0] = row;
                            w_king_pos[1] = col;
                        }
                        else
                            w_king_memb.Add(pieces[row, col]);
                        break;
                    case 2:
                        if (pieces[row, col].GetPName().Equals("p1 bishop"))
                        {
                            w_bishop_one_pos[0] = row;
                            w_bishop_one_pos[1] = col;
                        }
                        else
                            w_bishop_one_memb.Add(pieces[row, col]);
                        break;
                    case 3:
                        if (pieces[row, col].GetPName().Equals("p1 bishop"))
                        {
                            w_bishop_two_pos[0] = row;
                            w_bishop_two_pos[1] = col;
                        }
                        else
                            w_bishop_two_memb.Add(pieces[row, col]);
                        break;
                    case -1:
                        if (pieces[row, col].GetPName().Equals("p2 king"))
                        {
                            b_king_pos[0] = row;
                            b_king_pos[1] = col;
                        }
                        else
                            b_king_memb.Add(pieces[row, col]);
                        break;
                    case -2:
                        if (pieces[row, col].GetPName().Equals("p2 bishop"))
                        {
                            b_bishop_one_pos[0] = row;
                            b_bishop_one_pos[1] = col;
                        }
                        else
                            b_bishop_one_memb.Add(pieces[row, col]);
                        break;
                    case -3:
                        if (pieces[row, col].GetPName().Equals("p2 bishop"))
                        {
                            b_bishop_two_pos[0] = row;
                            b_bishop_two_pos[1] = col;
                        }
                        else
                            b_bishop_two_memb.Add(pieces[row, col]);
                        break;
                }
            }
        }
        Commander w_king = pieces[w_king_pos[0], w_king_pos[1]].MakeIntoCommander();
        Commander b_king = pieces[b_king_pos[0], b_king_pos[1]].MakeIntoCommander();

        w_king.king_piece = pieces[w_king_pos[0], w_king_pos[1]];
        w_king.is_king = true;
        b_king.king_piece = pieces[b_king_pos[0], b_king_pos[1]];
        b_king.is_king = true;

        Commander w_bishop_one = pieces[w_bishop_one_pos[0], w_bishop_one_pos[1]].MakeIntoCommander().SetKing(w_king);
        Commander w_bishop_two = pieces[w_bishop_two_pos[0], w_bishop_two_pos[1]].MakeIntoCommander().SetKing(w_king);
        Commander b_bishop_one = pieces[b_bishop_one_pos[0], b_bishop_one_pos[1]].MakeIntoCommander().SetKing(b_king);
        Commander b_bishop_two = pieces[b_bishop_two_pos[0], b_bishop_two_pos[1]].MakeIntoCommander().SetKing(b_king);

        b_king.SetLeft(b_bishop_one);
        b_king.SetRight(b_bishop_two);
        w_king.SetLeft(w_bishop_one);
        w_king.SetRight(w_bishop_two);

        foreach (Piece p in w_king_memb) w_king.AddPiece(p);
        foreach (Piece p in w_bishop_one_memb) w_bishop_one.AddPiece(p);
        foreach (Piece p in w_bishop_two_memb) w_bishop_two.AddPiece(p);
        foreach (Piece p in b_king_memb) b_king.AddPiece(p);
        foreach (Piece p in b_bishop_one_memb) b_bishop_one.AddPiece(p);
        foreach (Piece p in b_bishop_two_memb) b_bishop_two.AddPiece(p);
    }

    public void BuildTable()
    {
        GameObject blankBlock = new GameObject("Blank Block");
        Vector3 WhiteCapture_origin = new Vector3(9.3f, 8.15f, -15.79625f);
        for (int i = 0; i < 16; i++) // Loops for building the white table
        {
            for (int j = 0; j < 2; j++)
            {
                //whiteCaptureBox[i, j] = Instantiate(captureSquare, WhiteCapture_origin + new Vector3(j, whiteCaptureBox.GetLength(1) - i, 0f) * 0.6f, Quaternion.identity,transform);
                whiteCaptureBox[i, j] = Instantiate(blankBlock, WhiteCapture_origin + new Vector3(j, whiteCaptureBox.GetLength(1) - i, 0f) * 0.6f, Quaternion.identity, transform);
                whiteCaptureBox[i, j].transform.localScale = new Vector3(0.045f, 0.045f, 0.045f);
                //whiteCaptureBox[i, j].GetComponent<SpriteRenderer>().material.color = Chess.Colors.BOARD_DARK;
            }
        }
    }

    // Function called by the AI to get the current board state and to calculate the next move.
    public Piece[,] GetPieces()
    {
        return pieces;
    }

    // Get an integer array representing the board state (-6 to 6, 0 is empty).
    public int[,] GetBoardState()
    {
        int[,] board_state = new int[8, 8];

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                board_state[rank, file] = pieces[rank, file] ? pieces[rank, file].piece_id : 0;
            }
        }
        return board_state;
    }

    public void ShowAllPiecesInCorp(Piece selected_piece)
    {
        // Highlight all pieces in corp.
        foreach (Piece piece in selected_piece.GetCommander().GetPiecesInCorp())
        {
            //Debug.Log("A");
            int[] index = piece.position;
            blocks[index[0], index[1]].HoverColor();
            //blocks[index[0], index[1]].ChangeColor(Color.white);
        }
    }

    // Integer representation of the corps. 
    // -1 = black king, -2 = black bishop left, -3 = black bishop right
    // 1 = white king, 2 = white bishop left, 3 = white bishop right
    // 0 = empty.
    public int[,] GetCorpState()
    {
        int[,] corp_state = new int[8, 8];

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                if (pieces[rank, file])
                {
                    //corp_state[rank, file] = pieces[rank, file].commander.corp_id;
                    corp_state[rank, file] = pieces[rank, file].corp_id;
                }
            }
        }

        string result = "";
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                result += " [ " + corp_state[rank, file] + " ] ";
            }
            result += "\n";
        }

        //Debug.Log(result);

        return corp_state;
    }

    public List<Piece> GetCapturedWhite()
    {
        return capturedWhite;
    }

    public List<Piece> GetCapturedBlack()
    {
        return capturedBlack;
    }

    // Enables delegation mode
    public void EnableDelegationMode()
    {
        //Checking to make sure delegation can occur
        if (!gm.GetDidDelegate())
        {
            //makes delegation true for update function
            delegation = true;
            //buttons are created and destroyed for screen
            DelegationButton.gameObject.SetActive(false);
            EndTurnButton.gameObject.SetActive(false);
            RevokeButton.gameObject.SetActive(false);
            ConfirmDelegationButton.gameObject.SetActive(true);
            CancelButton.gameObject.SetActive(true);

            //king's corp is highlighted
            if (gm.GetTeam() == "white")
            {
                foreach (Piece piece in pieces)
                {
                    if (piece != null && piece.GetCorpID() == 1 && !piece.is_commander)
                    {
                        blocks[piece.position[0], piece.position[1]].ChangeColor(Color.red);
                    }
                    if (piece!=null && piece.GetCorpID() == 2 && piece.is_commander)
                    {
                        blocks[piece.position[0], piece.position[1]].ChangeColor(Color.blue);
                    }
                    if(piece != null && piece.GetCorpID() == 3 && piece.is_commander)
                    {
                        blocks[piece.position[0], piece.position[1]].ChangeColor(Color.green);
                    }
                }
            }
            else
            {
                foreach (Piece piece in pieces)
                {
                    if (piece != null && piece.GetCorpID() == -1 && !piece.is_commander)
                    {
                        blocks[piece.position[0], piece.position[1]].ChangeColor(Color.red);
                    }
                    if (piece != null && piece.GetCorpID() == -2 && piece.is_commander)
                    {
                        blocks[piece.position[0], piece.position[1]].ChangeColor(Color.blue);
                    }
                    if (piece != null && piece.GetCorpID() == -3 && piece.is_commander)
                    {
                        blocks[piece.position[0], piece.position[1]].ChangeColor(Color.green);
                    }
                }
            }
        }
    }

    //returns the number of member that the given corp id has
    public int TotalMembers(int id)
    {
        int total = 0;
        foreach (Piece piece in pieces)
        {
            if (piece != null)
            {
                if (piece.GetCorpID() == id)
                {
                    total++;
                }
            }
        }
        return total;
    }

    //returns the number of pieces with the given temp_id
    public int TempTotals(int id)
    {
        int total = 0;
        foreach (Piece piece in pieces)
        {
            if (piece != null)
            {
                if (piece.GetTempID() == id)
                {
                    total++;
                }
            }
        }
        return total;
    }

    //Checks to see if delegation may be ended
    private bool IsValid()
    {
        int total1 = 0;
        int total2 = 0;
        if (gm.GetTeam() == "white")
        {
            total1 = TotalMembers(2) + TempTotals(1);
            total2 = TotalMembers(3) + TempTotals(2);
        }
        else
        {
            total1 = TotalMembers(-2) + TempTotals(1);
            total2 = TotalMembers(-3) + TempTotals(2);
        }
        if (total1 > 6 || total2 > 6)
        {
            return false;
        }
        return true;
    }

    //Prevents a bug with delegations.
    public void HasCommanderMoved(Piece input, int newID)
    {
        int corpID = input.GetCorpID();
        foreach (Piece piece in pieces)
        {
            if (piece != null)
            {
                if (!piece.GetIsCommander() && newID == piece.GetCorpID())
                {
                    input.SetHasMoved(piece.GetHasMoved());
                }
            }
        }
    }

    // Confirms Delegation
    public void DisableDelegationButton()
    {
        //ensures delegation is legal
        if (!IsValid())
        {
            return;
        }
        //Delegation mode is left
        delegation = false;
        //buttons are destroyed and recreated for the game
        DelegationButton.gameObject.SetActive(true);
        EndTurnButton.gameObject.SetActive(true);
        RevokeButton.gameObject.SetActive(true);
        ConfirmDelegationButton.gameObject.SetActive(false);
        CancelButton.gameObject.SetActive(false);
        //each piece is delegated individually
        foreach (Piece piece in pieces)
        {
            if (piece != null)
            {
                int tempID = piece.GetTempID();
                int corpID = piece.GetCorpID();
                Commander king = piece.GetCommander();
                if (tempID != 0)
                {
                    if (tempID == 1)
                    {
                        king.RemovePiece(piece);
                        if (corpID > 0)
                        {
                            HasCommanderMoved(piece, 2);
                            piece.SetCorpID(2);
                            king.GetLeft().AddPiece(piece);
                        }
                        //left bishop delegation
                        else
                        {
                            HasCommanderMoved(piece, -2);
                            piece.SetCorpID(-2);
                            king.GetLeft().AddPiece(piece);
                        }
                    }
                    else if (tempID == 2)
                    {
                        king.RemovePiece(piece);
                        //right bishop delegation
                        if (corpID > 0)
                        {
                            HasCommanderMoved(piece, 3);
                            piece.SetCorpID(3);
                            king.GetRight().AddPiece(piece);
                        }
                        else
                        {
                            HasCommanderMoved(piece, -3);
                            piece.SetCorpID(-3);
                            king.GetRight().AddPiece(piece);
                        }
                    }
                    piece.SetDelegationID(GlobalDelegationID);

                    //piece.SetHasMoved(true);

                    if (piece.GetHasMoved() == true)
                    {
                        piece.ColorDim();
                    }
                }

                piece.SetTempID(0);

            }
        }
        GlobalDelegationID++;
        gm.SetDidDelegate(true);
        RefreshBlocks();
    }

    //Cancels Delegation
    public void CancelDelegationMode()
    {
        delegation = false;
        DelegationButton.gameObject.SetActive(true);
        EndTurnButton.gameObject.SetActive(true);
        RevokeButton.gameObject.SetActive(true);
        ConfirmDelegationButton.gameObject.SetActive(false);
        CancelButton.gameObject.SetActive(false);
        foreach (Piece piece in pieces)
        {
            if (piece != null)
            {
                piece.SetTempID(0);
            }
        }
        RefreshBlocks();
    }

    // Enables Revoke Mode
    public void EnableRevoke()
    {
        if (!gm.GetDidDelegate())
        {
            revoke = true;
            delegation = true;
            DelegationButton.gameObject.SetActive(false);
            EndTurnButton.gameObject.SetActive(false);
            RevokeButton.gameObject.SetActive(false);
            ConfirmRevokeButton.gameObject.SetActive(true);
            CancelRevokeButton.gameObject.SetActive(true);
            string currentTurn = gm.GetTeam();
            foreach (Piece piece in pieces)
            {
                if (piece != null)
                {
                    int id = piece.GetDelegationID();
                    if (id > 0 && ((currentTurn == "white" && piece.GetCorpID() > 0) || (currentTurn == "black" && piece.GetCorpID() < 0)))
                    {
                        //highlight piece
                        int[] index = piece.position;
                        blocks[index[0], index[1]].ChangeColor(Color.blue);
                    }
                }
            }
        }
    }

    // Confirm Revoke Mode
    public void ConfirmRevoke()
    {
        revoke = false;
        delegation = false;
        DelegationButton.gameObject.SetActive(true);
        EndTurnButton.gameObject.SetActive(true);
        RevokeButton.gameObject.SetActive(true);
        ConfirmRevokeButton.gameObject.SetActive(false);
        CancelRevokeButton.gameObject.SetActive(false);
        foreach (Piece piece in pieces)
        {
            if (piece != null && piece.GetTempID() == 1)
            {

                Commander currentCommander = piece.GetCommander();
                currentCommander.RemovePiece(piece);
                currentCommander.GetKing().AddPiece(piece);
                piece.SetTempID(0);
                if (gm.GetTeam() == "white")
                {
                    piece.SetCorpID(1);
                }
                else
                {
                    piece.SetCorpID(-1);
                }


            }
        }
        gm.SetDidDelegate(true);
        RefreshBlocks();
        Debug.Log("ConfirmRevoke AutoSave");
        gm.Autosave();
    }

    // Cancel Revoke
    public void CancelRevoke()
    {
        revoke = false;
        delegation = false;
        DelegationButton.gameObject.SetActive(true);
        EndTurnButton.gameObject.SetActive(true);
        RevokeButton.gameObject.SetActive(true);
        ConfirmRevokeButton.gameObject.SetActive(false);
        CancelRevokeButton.gameObject.SetActive(false);
        foreach (Piece piece in pieces)
        {
            if (piece != null)
            {
                piece.SetTempID(0);
            }
        }
        RefreshBlocks();
    }

    public void InitializeDelegation(int[,] delegation_state)
    {
        for (int row = 0; row < delegation_state.GetLength(0); row++)
        {
            for (int col = 0; col < delegation_state.GetLength(1); col++)
            {
                if (pieces[row, col])
                {
                    pieces[row, col].SetDelegationID(delegation_state[row, col]);
                }
            }
        }
    }

    public void InitializeHasMoved(int[,] has_moved)
    {
        for (int row = 0; row < has_moved.GetLength(0); row++)
        {
            for (int col = 0; col < has_moved.GetLength(1); col++)
            {
                if (pieces[row, col])
                {
                    bool piece_has_moved = Convert.ToBoolean(has_moved[row, col]);
                    pieces[row, col].SetHasMoved(piece_has_moved);
                    if (piece_has_moved) pieces[row, col].ColorDim();
                }
            }
        }
    }

    public void InitializeCommandNMoves(int[] numMoves)
    {
        for (int row = 0; row < pieces.GetLength(0); row++)
        {
            for (int col = 0; col < pieces.GetLength(1); col++)
            {
                if (pieces[row, col] && pieces[row, col].is_commander)
                {
                    int newNMoves = 0;
                    switch (pieces[row, col].GetCorpID())
                    {
                        case 1:
                            newNMoves = numMoves[0];
                            break;
                        case 2:
                            newNMoves = numMoves[1];
                            break;
                        case 3:
                            newNMoves = numMoves[2];
                            break;
                        case -1:
                            newNMoves = numMoves[3];
                            break;
                        case -2:
                            newNMoves = numMoves[4];
                            break;
                        case -3:
                            newNMoves = numMoves[5];
                            break;
                    }
                    pieces[row, col].SetNumberOfMoves(newNMoves);
                }
            }
        }
    }

    public void InitializeCaptureWhite(int[] capture)
    {
        for (int i = 0; i < capture.Length; i++)
        {
            if (capture[i] != 0) GenerateCaptureWhite(GeneratePiece(capture[i], 0, 0));
        }
    }

    public void InitializeCaptureBlack(int[] capture)
    {
        for (int i = 0; i < capture.Length; i++)
        {
            if (capture[i] != 0) GenerateCaptureBlack(GeneratePiece(capture[i], 0, 0));
        }
    }

    public void GenerateCaptureWhite(Piece capture)
    {
        capture.transform.position = whiteCaptureBox[capturedWhite.Count, 0].transform.position;
        capture.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
        capture.ResetPiece();
        capturedWhite.Add(capture);
    }

    public void GenerateCaptureBlack(Piece capture)
    {
        capture.transform.position = whiteCaptureBox[capturedBlack.Count, 1].transform.position;
        capture.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
        capture.ResetPiece();
        capturedBlack.Add(capture);
    }

    // Function called by human players to make a move.
    public void RequestInput(string message)
    {
        input_requested = true;
        //Debug.Log(message);
    }

    public void DelayedMove(int[] from, int[] to, float delay)
    {
        int moves_used = MovePiece(from, to);
        StartCoroutine(crMove(moves_used, delay));
    }

    public void DelayedAttack(int[] from, int[] to, float delay)
    {
        int moves_used = Attack(from, to);
        StartCoroutine(crAttack(moves_used, delay));
    }

    private IEnumerator crMove(int moves_used, float delay)
    {
        yield return new WaitForSeconds(delay);
        EndTurn(moves_used);
    }

    public IEnumerator crAttack(int moves_used, float delay)
    {
        yield return new WaitForSeconds(delay);
        EndTurn(moves_used);
    }

    public void HighlightAdjacentPieces(int[] knight)
    {
        if (!input_requested)
        {
            return;
        }
        int zero = knight[0];
        int one = knight[1];
        knightx = zero;
        knighty = one;
        for (int i = zero - 1; i <= zero + 1; i++)
        {
            for (int j = one - 1; j <= one + 1; j++)
            {
                if (i > -1 && j < 9 && i < 9 && j > -1)
                {
                    if (pieces[i, j] != null && !(zero == i && one == j) && gm.GetTeam() != pieces[i, j].GetTeam())
                    {
                        blocks[i, j].ChangeColor(Color.blue);
                        pieces[i, j].SetTempID(1);
                    }
                }

            }
        }
    }

    public bool PiecesAdjacent(int[] knight)
    {
        if (!input_requested)
        {
            return false;
        }
        int zero = knight[0];
        int one = knight[1];
        if (zero == -1 && one == -1)
        {
            return false;
        }
        for (int i = zero - 1; i <= zero + 1; i++)
        {
            for (int j = one - 1; j <= one + 1; j++)
            {
                if (i > -1 && j < 9 && i < 9 && j > -1)
                {
                    if (pieces[i, j] && !(zero == i && one == j) && gm.GetTeam() != pieces[i, j].GetTeam())
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }


    /*
     * Move Piece:
     * Takes a position 'from' and position 'to', 
     * calculates the path between the two points, 
     * calls the 'MovePiece' function from the piece class and 
     * calculates a list of vector positions for the piece to traverse (updates the transform.position component of the piece game object),
     * then notifies the game manager. 
     * 
     */
    public int MovePiece(int[] from, int[] to)
    {
        RefreshBlocks();

        Piece p = pieces[from[0], from[1]];

        // Find the best path. 
        List<int[]> path = FindPath(from, to, new List<int[]> { from }, p.GetNumberOfMoves());

        // Slow position update.
        int moves_used = pieces[from[0], from[1]].MovePiece(GetPathPositions(path), to);

        // Update the pieces game object array. 
        pieces[to[0], to[1]] = pieces[from[0], from[1]];
        pieces[from[0], from[1]] = null;

        selected_piece = null;
        //input_requested = false;

        char[] column_chars = new char[] { 'H', 'G', 'F', 'E', 'D', 'C', 'B', 'A' };

        if ((pieces[to[0], to[1]].GetPName() == "p1 knight" || pieces[to[0], to[1]].GetPName() == "p2 knight") && input_requested)
        {
            if (PiecesAdjacent(to))
            {
                Enable_Knight_Options();
                HighlightAdjacentPieces(to);
            }
        }
        input_requested = false;

        //Log move info
        game_log.text += pieces[to[0], to[1]].GetPName() + " [" + column_chars[from[1]] + (from[0] + 1) + "]";
        game_log.text += " >> [" + column_chars[to[1]] + (to[0] + 1) + "]\n";

        return moves_used;
    }

    public void EndTurn(int moves_used)
    {
        // Notify the Game Manager of the moves used. 
        gm.CompleteGameState(moves_used);
    }

    void Enable_Knight_Options()
    {
        // knight being moved AND has adjacent enemy triggers buttons to appear
        DelegationButton.gameObject.SetActive(false);
        RevokeButton.gameObject.SetActive(false);
        EndTurnButton.gameObject.SetActive(false);

        knightMove = true;

        Knight_Attack_Button.gameObject.SetActive(true);
        Knight_Wait_Button.gameObject.SetActive(true);
    }

    // Opens Knight attack options
    public void Enable_Knight_Attack()
    {
        // knight does attack: it epic fail or fat dub 
        // closes knight options
        Knight_Attack_Button.gameObject.SetActive(false);
        Knight_Wait_Button.gameObject.SetActive(false);

        DelegationButton.gameObject.SetActive(true);
        RevokeButton.gameObject.SetActive(true);
        EndTurnButton.gameObject.SetActive(true);
    }

    // Ends knight turn
    public void Enable_Knight_Wait()
    {
        Knight_Attack_Button.gameObject.SetActive(false);
        Knight_Wait_Button.gameObject.SetActive(false);

        knightMove = false;
        RefreshBlocks();

        DelegationButton.gameObject.SetActive(true);
        RevokeButton.gameObject.SetActive(true);
        EndTurnButton.gameObject.SetActive(true);
    }

    public IEnumerator DiceRollWait(int[] selected_index, int[] index)
    {
        Color originalColor = diceInstance.gameObject.GetComponent<SpriteRenderer>().color;
        diceInstance.gameObject.GetComponent<SpriteRenderer>().color = Color.gray;
        for (int i = 0; i < 10; i++)
        {
            diceInstance.RollDice();
            yield return new WaitForSeconds(.1f);
        }
        diceInstance.gameObject.GetComponent<SpriteRenderer>().color = originalColor;
        int moves_used = Attack(selected_index, index);
        EndTurn(moves_used);
        yield return null;
    }
    /* 
     * Attack:
     * Gets a random number from the dice object. 
     * Compares the roll with the table from the Chess 'roll_needed' table.
     * Calls the 'Attack' function from the piece class with parameters (path, new_position, was_successful).
     * Successful attacks -> Move the attacking piece, deactivate/remove the captured piece.
     * Unsuccessful attacks -> Only update the number of turns used (Passed back to the game manager) and handle commander stuff.
     * 
     */
    public int Attack(int[] from, int[] to)
    {
        RefreshBlocks();

        char[] column_chars = new char[] { 'H', 'G', 'F', 'E', 'D', 'C', 'B', 'A' };
        Block hovered_block = blocks[to[0], to[1]];

        int roll = diceInstance.RollDice();
        if (knightMove == true)
        {
            roll++;
        }

        // Get attacker and defender integer piece type.
        int attacker = pieces[from[0], from[1]].piece_id;
        int defender = pieces[to[0], to[1]].piece_id;

        // Lookup the attacker/defender die roll needed from the Chess class.
        int roll_needed = Chess.RollNeeded(attacker, defender);
        //Debug.Log("Roll Needed: " + roll_needed + ", Rolled: " + roll);

        // Compare roll.
        if (roll < roll_needed)
        {
            //Debug.Log("Attack Failed! " + from[0] + ", " + from[1] + " -> " + to[0] + ", " + to[1]);
            Chess.PlayAudioClip("block");

            // Null path and Null new_position, attack_successful = false
            int moves_used = pieces[from[0], from[1]].Attack(null, null, false);

            selected_piece = null;
            input_requested = false;

            game_log.text += pieces[from[0], from[1]].GetPName() + " >>> " + pieces[to[0], to[1]].GetPName() + " Failed\n";
            // Log attack info. Old method which could not track moves done by the AI
            //game_log.text += hover_info.text + "  Failed " + "\n";           
            //hover_info.text = "";

            return moves_used;
        }
        else
        {
            //Debug.Log("Attack Successful! " + from[0] + ", " + from[1] + " -> " + to[0] + ", " + to[1]);
            Chess.PlayAudioClip("capture");

            // Create a new path with only one position, was_successful = true.
            int moves_used = pieces[from[0], from[1]].Attack(new List<Vector3>() { blocks[to[0], to[1]].transform.position }, to, true);

            // King has been captured, end the game.
            if (pieces[to[0], to[1]].is_commander && pieces[to[0], to[1]].commander.is_king)
            {
                //Debug.Log(pieces[to[0], to[1]].GetPName());
                StopAllCoroutines();

                //If Black
                if (pieces[to[0], to[1]].GetPName() == "p2 king")
                {
                    //White win screen
                    //SceneManager.LoadScene("Player One Wins");
                    GameOver.gameObject.SetActive(true);
                    gameOverText.text = "Player  One  Wins";
                    RunRigidbody();
                    gm.game_in_progress = false;
                    return moves_used;
                }
                //else white
                else
                {
                    //Black win screen
                    //SceneManager.LoadScene("Player Two Wins");
                    GameOver.gameObject.SetActive(true);
                    gameOverText.text = "Player  Two  Wins";
                    RunRigidbody();
                    gm.game_in_progress = false;
                    return moves_used;
                }
                // Insert Wilhelm Scream..
            }
            // Check if piece is a commander.
            else if (pieces[to[0], to[1]].is_commander)
            {
                // If so, transfer pieces to king.
                pieces[to[0], to[1]].commander.TransferPiecesToKing();
                // Reduce max number of turns for the captured player.
                gm.LoseCommander();
            }

            string team = pieces[to[0], to[1]].GetTeam();
            bool comp = team.Equals("white", StringComparison.OrdinalIgnoreCase);

            if (comp == true) // If the given piece is white
            {
                GenerateCaptureWhite(pieces[to[0], to[1]]);
                //pieces[to[0], to[1]].transform.position = whiteCaptureBox[whiteCaptures, 0].transform.position;
                //whiteCaptures = whiteCaptures + 1;
            }
            else // If the given piece is black
            {
                GenerateCaptureBlack(pieces[to[0], to[1]]);
                //pieces[to[0], to[1]].transform.position = whiteCaptureBox[blackCaptures, 1].transform.position;
                //blackCaptures = blackCaptures + 1;
            }

            // Shift pieces array.
            pieces[to[0], to[1]] = pieces[from[0], from[1]];
            pieces[from[0], from[1]] = null;

            selected_piece = null;
            input_requested = false;

            // Log attack info. New method that also tracks moves done by the AI
            //This was placed in an if statement because an error kept coming up whenever a knight was moved and this fixed that. Make a message in Discord if you have a problem with this.
            if (pieces[from[0], from[1]] != null && pieces[to[0], to[1]] != null)
            {
                game_log.text += pieces[from[0], from[1]].GetPName() + " >>> " + pieces[to[0], to[1]].GetPName() + " Success\n";
            }

            // Log attack info. Old method which does not track moves done by the AI
            //game_log.text += hover_info.text + "  Success " + "\n";
            //hover_info.text = "";

            return moves_used;
        }
    }

    /*
    * Get Attackable List:
    * Post-condition:
    * Returns a list of coordinates of all adjacent blocks with enemy
    * pieces. Rooks have a range of 2. 
    */
    public List<int[]> GetAttackableList(int row, int col)
    {
        List<int[]> newList = new List<int[]>();
        bool isWhitePiece = pieces[row, col].GetTeam().Equals("white");
        int range = 1;
        if (pieces[row, col].GetPName().Equals("p1 rook") || pieces[row, col].GetPName().Equals("p2 rook"))
            range = pieces[row, col].GetNumberOfMoves();

        int north = Mathf.Max(0, row - range);
        int south = Mathf.Min(7, row + range);
        int west = Mathf.Max(0, col - range);
        int east = Mathf.Min(7, col + range);

        if (pieces[row, col].GetPName().Equals("p1 pawn"))
            north = row + range;
        else if (pieces[row, col].GetPName().Equals("p2 pawn"))
            south = row - range;

        for (int i = north; i <= south; i++)
        {
            for (int j = west; j <= east; j++)
            {
                if (pieces[i, j])
                {
                    if (pieces[i, j].GetTeam().Equals("white") != isWhitePiece)
                        newList.Add(new int[] { i, j });
                }
            }
        }
        return newList;
    }

    /*
     * Get Attackable Range:
     * Post-condition:
     * Returns a list of coordinates of all adjacent blocks with or without enemy pieces within range. 
     */
    public List<int[]> GetAttackableRange(int row, int col)
    {
        List<int[]> newList = new List<int[]>();
        int range =
        pieces[row, col].GetPName().Equals("p1 rook") || pieces[row, col].GetPName().Equals("p2 rook") ?
        pieces[row, col].GetNumberOfMoves() : 1;

        int north = Mathf.Max(0, row - range);
        int south = Mathf.Min(7, row + range);
        int west = Mathf.Max(0, col - range);
        int east = Mathf.Min(7, col + range);

        if (pieces[row, col].GetPName().Equals("p1 pawn"))
            north = row + range;
        else if (pieces[row, col].GetPName().Equals("p2 pawn"))
            south = row - range;

        for (int i = north; i <= south; i++)
        {
            for (int j = west; j <= east; j++)
            {
                newList.Add(new int[] { i, j });
            }
        }
        return newList;
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
    public List<int[]> GetMovesList(int row, int col, int m)
    {
        List<int[]> list = new List<int[]>();
        Queue<int[]> buildQueue = new Queue<int[]>();
        Queue<int[]> currentQueue = new Queue<int[]>();
        string selectedPiece = pieces[row, col].GetPName();
        do
        {
            /* NOTE: If you can express the validation for the adjacent blocks better
             * or more optimized, please feel free.  All adjacent blocks needs to be 
             * validated and passed to ProcessBlock() before any of them can be
             * dequeued.  Can't ProcessBlock() while Dequeueing.
             */
            if (row < 7 && !selectedPiece.Equals("p2 pawn"))
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
            if (row > 0 && !selectedPiece.Equals("p1 pawn"))
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
            if (!selectedPiece.Equals("p2 pawn") && !selectedPiece.Equals("p1 pawn"))
            {
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
            blocks[pos[0], pos[1]].ChangeColor(team == "white" ? Chess.Colors.MOVES_ONE : Chess.Colors.MOVES_TWO);
        }
    }

    private void SetBlockListAttackable(List<int[]> list, string team)
    {
        foreach (int[] pos in list)
        {
            blocks[pos[0], pos[1]].SetAttackable(true);
            blocks[pos[0], pos[1]].ChangeColor(team == "white" ? Chess.Colors.W_ATTACK : Chess.Colors.B_ATTACK);
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
            blocks[p[0], p[1]].DullColor();
        }

        // Highlight destination.
        //blocks[path[path.Count-1][0], path[path.Count-1][1]].ChangeColor(gm.GetTeam().Equals("white") ? Chess.Colors.PLAYER_ONE : Chess.Colors.PLAYER_TWO);

        return positions;
    }

    public void RefreshBlocks()
    {
        foreach (Block block in blocks)
        {
            block.InitialColor();
            block.SetVisited(false);
            block.SetMovable(false);
            block.SetAttackable(false);
        }
    }

    public void RefreshPieces()
    {
        foreach (Piece piece in pieces)
        {
            if (piece) piece.ResetPiece();
        }
    }

    public void DisplayHoverInfo(int[] h)
    {
        char[] column_chars = new char[] { 'H', 'G', 'F', 'E', 'D', 'C', 'B', 'A' };
        Block hovered_block = blocks[h[0], h[1]];

        if (selected_piece)
        {
            hover_info.text = selected_piece.GetPName() + "[" + column_chars[selected_piece.position[1]] + (selected_piece.position[0] + 1) + "]";

            if (hovered_block.IsMovable())
            {
                hover_info.text += " >> [" + column_chars[h[1]] + (h[0] + 1) + "] ";
            }

            if (hovered_block.IsAttackable())
            {
                int roll_needed = Chess.RollNeeded(selected_piece.piece_id, pieces[h[0], h[1]].piece_id);

                hover_info.text += " >> " + pieces[h[0], h[1]].GetPName() + "[ " + column_chars[h[1]] + (h[0] + 1) + " ]";
                hover_info.text += "\n Roll Needed:  " + roll_needed;
                //hover_info.text += Math.Round((7f - roll_needed) / 6f * 100f, 2) + "%";
            }
        }
    }
    public void RefreshColor()
    {
        bool flip = false;
        int index = 0;
        for (int row = 0; row < blocks.GetLength(0); row++)
        {
            for (int col = 0; col < blocks.GetLength(1); col++)
            {
                blocks[row, col].SetColor(blocks[row, col].GetComponent<SpriteRenderer>().material.color = flip
                    ? Chess.Colors.BOARD_LIGHT : Chess.Colors.BOARD_DARK);
                index++;
                if (index % 8 != 0) flip = !flip;
                if (pieces[row, col])
                {
                    if (pieces[row, col].GetTeam().Equals("white"))
                        pieces[row, col].color = Chess.Colors.PLAYER_ONE;
                    else
                        pieces[row, col].color = Chess.Colors.PLAYER_TWO;
                    pieces[row, col].GetComponent<SpriteRenderer>().material.color = pieces[row, col].color;
                    if (pieces[row, col].GetHasMoved())
                        pieces[row, col].ColorDim();
                }
            }
        }
        foreach (Piece piece in capturedWhite)
        {
            piece.color = Chess.Colors.PLAYER_ONE;
            piece.GetComponent<SpriteRenderer>().material.color = piece.color;
        }
        foreach (Piece piece in capturedBlack)
        {
            piece.color = Chess.Colors.PLAYER_TWO;
            piece.GetComponent<SpriteRenderer>().material.color = piece.color;
        }
    }
    public void RepositionBoard()
    {
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
    }
    public void RefitBoard()
    {
        float boardWidthProportion = 121 / 100f; // board's width scale
        float screenRatio = (float)Screen.width / (float)Screen.height; // screen ratio
        float spaceToFit = screenRatio * .55f; // available space's width scale
        float scaleToFitMultiplier = spaceToFit / boardWidthProportion; // multiplier for board scale to fit on space 
        transform.localScale = new Vector3(scaleToFitMultiplier, scaleToFitMultiplier, 1);
    }

    public void SpawnDice()
    {
        diceInstance = Instantiate(dice, new Vector3(-1.12f, 5.3752f, -1), Quaternion.identity, transform) as Dice;
        diceInstance.transform.localScale = new Vector3(0.07376f, 0.07376f, 0.07376f);
    }
}
