using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Game Manager:
 * Manages the flow of the game and which actions should be taken 
 * at any given time. Attached to the 'Game Manager' object in the unity inspector. 
 */

public class GameManager : MonoBehaviour
{
    public Player[] players = new Player[2];
    public BoardManager bm;

    private enum Team { black, white }
    private Team team;
    private bool DidDelegate = false;
    private int moves_left = 6;
    private readonly string saveFileName = "/save_state.txt";
    private Rect resetButton = new Rect(Screen.width - 150f, 100, 100, 50); // TEMPORARY PLACE HOLDER

    // Temporary GUI for Reset Button
    private void OnGUI()
    {
        if (GUI.Button(resetButton, "Reset"))
            ResetBoard();
    }

    public void StartGame()
    {
        InitializePlayer(LoadPlayer());
        InitializeName(LoadName());
        InitializeCurrentTeam(LoadCurrentTeam());
        InitializeMovesLeft(LoadMovesLeft());

        bm.InitializeBoard(LoadBoard());
        bm.InitializeCorps(LoadCorps());
        bm.InitializeDelegation(LoadDelegation());
        bm.InitializeHasMoved(LoadHasMoved());
        bm.InitializeCommandAuthorityUsed(LoadCommandAuthorityUsed());
        bm.InitializeCaptureWhite(LoadCapturedWhite());
        bm.InitializeCaptureBlack(LoadCapturedBlack());

        bm.setup_complete = true;

        // Create players with session data. 
        CreatePlayer((int)Team.white);
        CreatePlayer((int)Team.black);

        // Randomly select a player to go first.
        //team = Random.Range(0, 2) == 0 ? Team.black : Team.white;

        // Assign black to go first for demo.
        // Note: This will be moved to LoadTeam()
        // team = Team.white;

        // Initiate the first move.
        CompleteGameState(0);
    }

    private void CreatePlayer(int p)
    {
        switch (Session.players[p])
        {
            case "human":
                players[p] = new Human(Session.names[p], this, bm);
                break;

            case "ai":
                players[p] = new AI(Session.names[p], this, bm);
                break;
        }
    }

    // Called from the board manager after move has been made.
    public void CompleteGameState(int moves_used)
    {
        moves_left -= moves_used;
        if (moves_left > 0)
        {
            players[(int)team].BeginMove();
            Debug.Log("CompleteGameState Autosave");
            Autosave();
        }
        else
            EndTurn();
    }

    public void EndTurn()
    {
        // Reset all piece colors and moves. 
        bm.RefreshPieces();
        bm.RefreshBlocks();

        // Go to next player.
        team = team == Team.black ? Team.white : Team.black;

        // Reset moves to players current max_moves.
        moves_left = players[(int)team].max_moves;

        //Reset Delegation Action
        DidDelegate = false;

        CompleteGameState(0);
    }

    public void SetDidDelegate(bool answer)
    {
        Debug.Log("SetDidDelegate Autosave");
        Autosave();
        this.DidDelegate = answer;
    }

    public bool GetDidDelegate()
    {
        return DidDelegate;
    }

    public string GetTeam()
    {
        return team == Team.white ? "white" : "black";
    }

    public void LoseCommander()
    {
        players[(int) (team == Team.black ? Team.white : Team.black)].max_moves -= 2;
    }

    // Auto Save:
    // Function that currently Saves pieces positions and corp membership into a txt file
    //
    // Save Format:
    // Player Types
    // Player Names
    // Current Team Turn
    // Current Moves Left
    // Pieces Arrangement
    // Pieces Corp Membership
    // Pieces Delegation State
    // Pieces Has Moved State
    // Current Command Authority Used
    // Captured White
    // Captured Black
    public void Autosave()
    {
        string sDirectory = Application.dataPath;
        if (File.Exists(sDirectory + saveFileName))
        {
            File.Delete(sDirectory + saveFileName);
        }
        using (StreamWriter sw = new StreamWriter(sDirectory + saveFileName))
        {
            // Saves the player type of player 1 and player 2
            sw.Write(Session.players[0] + "," + Session.players[1]);
            sw.WriteLine();

            // Saves the player names of player 1 and player 2
            sw.Write(Session.names[0] + "," + Session.names[1]);
            sw.WriteLine();

            // Saves team with current turn
            sw.Write((int)team);
            sw.WriteLine();

            // Saves moves left on the current turn
            sw.Write(moves_left);
            sw.WriteLine();

            int[] w_king_pos = new int[2];
            int[] b_king_pos = new int[2];
            // Saves the each pieces' placement
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (!(row == 0 && col == 0)) sw.Write(",");
                    if (bm.GetPieces()[row, col])
                    {
                        if (bm.GetPieces()[row, col].GetPieceID() == 6)
                        {
                            w_king_pos = new int[2] { row, col };
                        }
                        else if (bm.GetPieces()[row, col].GetPieceID() == -6)
                        {
                            b_king_pos = new int[2] { row, col };
                        }
                        sw.Write(bm.GetPieces()[row, col].GetPieceID().ToString());
                    }
                    else
                    {
                        sw.Write("0");
                    }
                }
            }
            sw.WriteLine();

            // Integer representation of the corps. 
            // -1 = black king, -2 = black bishop left, -3 = black bishop right
            // 1 = white king, 2 = white bishop left, 3 = white bishop right
            // 0 = empty.
            int[,] corp_state = bm.GetCorpState();
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if ((bm.GetPieces()[row, col]))
                    {
                        if (!(row == 0 && col == 0))
                        {
                            sw.Write(",");
                        }
                        sw.Write(corp_state[row, col]);
                    }
                    else
                    {
                        if ((!(row == 0 && col == 0)))
                        {
                            sw.Write(",");
                        }
                        sw.Write("0");
                    }
                }
            }
            sw.WriteLine();

            // Saves each pieces' delegation state.
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (bm.GetPieces()[row, col])
                    {
                        if (!(row == 0 && col == 0)) sw.Write(",");
                        sw.Write(bm.GetPieces()[row, col].GetDelegationID());
                    }
                    else
                    {
                        if (!(row == 0 && col == 0)) sw.Write(",");
                        sw.Write("0");
                    }
                }
            }
            sw.WriteLine();

            // Saves each pieces' has_moved state.
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (bm.GetPieces()[row, col])
                    {
                        if (!(row == 0 && col == 0)) sw.Write(",");
                        sw.Write(Convert.ToInt32(bm.GetPieces()[row, col].GetHasMoved()));
                    }
                    else
                    {
                        if (!(row == 0 && col == 0)) sw.Write(",");
                        sw.Write("0");
                    }
                }
            }
            sw.WriteLine();

            // Saves the used_authority state of the white king and black king
            sw.Write(Convert.ToInt32(bm.GetPieces()[w_king_pos[0], w_king_pos[1]].commander.GetUsedAuthority()) +
                     "," + Convert.ToInt32(bm.GetPieces()[b_king_pos[0], b_king_pos[1]].commander.GetUsedAuthority()));
            sw.WriteLine();

            // Saves the piece_id of captured white pieces
            if (bm.GetCapturedWhite().Count > 0)
            {
                for (int i = 0; i < bm.GetCapturedWhite().Count; i++)
                {
                    if (i != 0) sw.Write(",");
                    sw.Write(bm.GetCapturedWhite()[i].GetPieceID());
                }
            }
            else
            {
                sw.Write("0");
            }
            sw.WriteLine();

            // Saves the piece_id of captured black pieces
            if (bm.GetCapturedBlack().Count > 0)
            {
                for (int i = 0; i < bm.GetCapturedBlack().Count; i++)
                {
                    if (i != 0) sw.Write(",");
                    sw.Write(bm.GetCapturedBlack()[i].GetPieceID());
                }
            }
            else
            {
                sw.Write("0");
            }
            sw.WriteLine();

            sw.Close();
        }
    }

    public void ResetBoard()
    {
        EraseSave();
        Chess.PIECES = new Dictionary<string, GameObject>();
        Chess.SOUNDS = new Dictionary<string, AudioSource>();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void EraseSave()
    {
        string sDirectory = Application.dataPath;
        if (File.Exists(sDirectory + saveFileName))
        {
            Debug.Log("ERASE");
            File.Delete(sDirectory + saveFileName);
        }
        else
        {
            Debug.Log("NOT ERASE");
        }
        
    }

    private void InitializePlayer(string[] players)
    {
        Session.players[0] = players[0];
        Session.players[1] = players[1];
    }

    private void InitializeName(string[] names)
    {
        Session.names[0] = names[0];
        Session.names[1] = names[1];
    }

    private void InitializeCurrentTeam(Team team)
    {
        this.team = team;
    }

    public void InitializeMovesLeft(int moves_left)
    {
        this.moves_left = moves_left;
    }

    private string[] LoadPlayer()
    {
        string[] player_type_init;
        string sDirectory = Application.dataPath;
        if (File.Exists(sDirectory + saveFileName))
        {
            player_type_init = new string[2];
            using (StreamReader sr = new StreamReader(sDirectory + saveFileName))
            {
                player_type_init = sr.ReadLine().Split(',');
                sr.Close();
            }
        }
        else
        {
            player_type_init = new string[2] { "human", "human" };
        }
        return player_type_init;
    }

    private string[] LoadName()
    {
        string[] player_name_init;
        string sDirectory = Application.dataPath;
        if (File.Exists(sDirectory + saveFileName))
        {
            player_name_init = new string[2];
            using (StreamReader sr = new StreamReader(sDirectory + saveFileName))
            {
                sr.ReadLine();
                player_name_init = sr.ReadLine().Split(',');
                sr.Close();
            }
        }
        else
        {
            player_name_init = new string[2] { "Black", "White" };
        }
        return player_name_init;
    }

    private Team LoadCurrentTeam()
    {
        Team current_team_init;
        string sDirectory = Application.dataPath;
        if (File.Exists(sDirectory + saveFileName))
        {
            int currentTeamUsedLine;
            using (StreamReader sr = new StreamReader(sDirectory + saveFileName))
            {
                sr.ReadLine();
                sr.ReadLine();
                currentTeamUsedLine = Convert.ToInt32(sr.ReadLine());
                sr.Close();
            }
            current_team_init = currentTeamUsedLine == 0 ? Team.black : Team.white;
        }
        else
        {
            current_team_init = Team.white;
        }
        return current_team_init;
    }

    private int LoadMovesLeft()
    {
        int moves_left_init;
        string sDirectory = Application.dataPath;
        if (File.Exists(sDirectory + saveFileName))
        {
            string moves_left_init_line;
            using (StreamReader sr = new StreamReader(sDirectory + saveFileName))
            {
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                moves_left_init_line = sr.ReadLine();
                sr.Close();
            }
            moves_left_init = Convert.ToInt32(moves_left_init_line);
        }
        else
        {
            moves_left_init = 6;
        }
        return moves_left_init;
    }

    private int[,] LoadBoard()
    {
        int[,] board_init = new int[8, 8];
        string sDirectory = Application.dataPath;
        if (File.Exists(sDirectory + saveFileName))
        {
            string piecesLine;
            using (StreamReader sr = new StreamReader(sDirectory + saveFileName))
            {
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                piecesLine = sr.ReadLine();
                sr.Close();
            }
            string[] piecesStringArray = piecesLine.Split(',');
            for (int row = 0, index = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++, index++)
                {
                    board_init[row, col] = Convert.ToInt32(piecesStringArray[index]);
                }
            }
        }
        else
        {
            board_init = new int[,]
            {
                {  2,  4,  3,  6,  5,  3,  4,  2 },
                {  1,  1,  1,  1,  1,  1,  1,  1 },
                {  0,  0,  0,  0,  0,  0,  0,  0 },
                {  0,  0,  0,  0,  0,  0,  0,  0 },
                {  0,  0,  0,  0,  0,  0,  0,  0 },
                {  0,  0,  0,  0,  0,  0,  0,  0 },
                { -1, -1, -1, -1, -1, -1, -1, -1 },
                { -2, -4, -3, -6, -5, -3, -4, -2 }
            };
        }
        return board_init;
    }

    private int[,] LoadCorps()
    {
        int[,] command_init = new int[8, 8];
        string sDirectory = Application.dataPath;
        if (File.Exists(sDirectory + saveFileName))
        {
            command_init = new int[8, 8];
            string commandLine;
            using (StreamReader sr = new StreamReader(sDirectory + saveFileName))
            {
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                commandLine = sr.ReadLine();
                sr.Close();
            }
            string[] commandStringArray = commandLine.Split(',');
            for (int row = 0, index = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++, index++)
                {
                    command_init[row, col] = Convert.ToInt32(commandStringArray[index]);
                }
            }
        }
        else
        {
            // Integer representation of the corps. 
            // -1 = black king corp, -2 = black bishop left corp, -3 = black bishop right corp
            // 1 = white king corp, 2 = white bishop left corp, 3 = white bishop right corp
            // 0 = empty.
            command_init = new int[,]
            {
                {  1,  2,  2,  1,  1,  3,  3,  1 },
                {  2,  2,  2,  1,  1,  3,  3,  3 },
                {  0,  0,  0,  0,  0,  0,  0,  0 },
                {  0,  0,  0,  0,  0,  0,  0,  0 },
                {  0,  0,  0,  0,  0,  0,  0,  0 },
                {  0,  0,  0,  0,  0,  0,  0,  0 },
                { -2, -2, -2, -1, -1, -3, -3, -3 },
                { -1, -2, -2, -1, -1, -3, -3, -1 },
            };
        }

        return command_init;
    }

    private int[,] LoadDelegation()
    {
        int[,] delegation_init = new int[8, 8];
        string sDirectory = Application.dataPath;
        if (File.Exists(sDirectory + saveFileName))
        {
            delegation_init = new int[8, 8];
            string delegationLine;
            using (StreamReader sr = new StreamReader(sDirectory + saveFileName))
            {
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                delegationLine = sr.ReadLine();
                sr.Close();
            }
            string[] hasMovedStringArray = delegationLine.Split(',');
            for (int row = 0, index = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++, index++)
                {
                    delegation_init[row, col] = Convert.ToInt32(hasMovedStringArray[index]);
                }
            }
        }
        else
        {
            delegation_init = new int[8, 8];
        }
        return delegation_init;
    }

    private int[,] LoadHasMoved()
    {
        int[,] has_moved_init = new int[8, 8];
        string sDirectory = Application.dataPath;
        if (File.Exists(sDirectory + saveFileName))
        {
            has_moved_init = new int[8, 8];
            string hasMovedLine;
            using (StreamReader sr = new StreamReader(sDirectory + saveFileName))
            {
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                hasMovedLine = sr.ReadLine();
                sr.Close();
            }
            string[] hasMovedStringArray = hasMovedLine.Split(',');
            for (int row = 0, index = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++, index++)
                {
                    has_moved_init[row, col] = Convert.ToInt32(hasMovedStringArray[index]);
                }
            }
        }
        else
        {
            has_moved_init = new int[8, 8];
        }
        return has_moved_init;
    }

    private bool[] LoadCommandAuthorityUsed()
    {
        bool[] command_authority_used_init;
        string sDirectory = Application.dataPath;
        if (File.Exists(sDirectory + saveFileName))
        {
            command_authority_used_init = new bool[2];
            string commandAuthorityUsedLine;
            using (StreamReader sr = new StreamReader(sDirectory + saveFileName))
            {
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                commandAuthorityUsedLine = sr.ReadLine();
                sr.Close();
            }
            string[] commandAuthorityUsedArray = commandAuthorityUsedLine.Split(',');
            for (int i = 0; i < commandAuthorityUsedArray.Length; i++)
            {
                command_authority_used_init[i] = commandAuthorityUsedArray[i].Equals("1") ? true : false;
            }
        }
        else
        {
            command_authority_used_init = new bool[2] { false, false };
        }
        return command_authority_used_init;
    }

    private int[] LoadCapturedWhite()
    {
        int[] captured_white_init;
        string sDirectory = Application.dataPath;
        if (File.Exists(sDirectory + saveFileName))
        {
            string capturedWhiteLine;
            using (StreamReader sr = new StreamReader(sDirectory + saveFileName))
            {
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                capturedWhiteLine = sr.ReadLine();
                sr.Close();
            }
            string[] capturedWhiteArray = capturedWhiteLine.Split(',');
            captured_white_init = new int[capturedWhiteArray.Length];
            for (int i = 0; i < captured_white_init.Length; i++)
            {
                captured_white_init[i] = Convert.ToInt32(capturedWhiteArray[i]);
            }
        }
        else
        {
            captured_white_init = new int[0];
        }
        return captured_white_init;
    }

    private int[] LoadCapturedBlack()
    {
        int[] captured_black_init;
        string sDirectory = Application.dataPath;
        if (File.Exists(sDirectory + saveFileName))
        {
            string capturedBlackLine;
            using (StreamReader sr = new StreamReader(sDirectory + saveFileName))
            {
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();
                capturedBlackLine = sr.ReadLine();
                sr.Close();
            }
            string[] capturedBlackArray = capturedBlackLine.Split(',');
            captured_black_init = new int[capturedBlackArray.Length];
            for (int i = 0; i < captured_black_init.Length; i++)
            {
                captured_black_init[i] = Convert.ToInt32(capturedBlackArray[i]);
            }
        }
        else
        {
            captured_black_init = new int[0];
        }
        return captured_black_init;
    }
}
