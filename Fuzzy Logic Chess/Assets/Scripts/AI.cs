/*
 * AI:
 * Inherits from Player. 
 * Contains AI specific functions. 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : Player
{
    /*
     * To access the game manager, use gm:
     * gm.CompleteGameState(moves_used)
     * moves_used -> 1 if not a commander OR command authority was not used, 2 if command authority was used.
     * 
     * To access the board manager, use bm:
     * bm.GetBoardState() -> Returns an 2D integer array of the board state (-6 to 6, 0 is empty).
     * bm.GetAllPieces() -> Returns a list of all pieces to acces their attributes.
     * bm.GetCorpState() -> Returns a 2D array of where each corp is located on the board.
     * bm.Move(from, to) -> Called after AI has computed its best move, moves the piece on the board. 
     * bm.Attack(from, to) -> Called if AI has decided to attack, returns true if attack was successful, false if unsuccessful.
     * 
     */

    public int moves_examined = 0; 

    float[] material_values = new float[] { 1, 3, 5, 4, 6, 10 };

    float[,] dist_map = new float[8, 8];
    float[,] risk_map = new float[8, 8];

    float difficulty = 0.5f;

    public AI(string name, GameManager gm, BoardManager bm) : base(name, gm, bm)
    {
        // AI specific constructor.
        // Inherits a name, game manager and board manager (Assigned in the GameManager)
    }

    public override void BeginMove()
    {
        bm.input_requested = false;

        Piece[,] pieces = bm.GetPieces();

        // Attack from and to.
        int[] a_from = new int[] { -1, -1 };
        int[] a_to = new int[] { -1, -1 };

        float attack_value = 0f;
        bool attack_found = false; 

        foreach (Piece piece in pieces)
        {
            if (!piece) continue;
            if (piece.has_moved) continue;

            if (piece.GetTeam() == gm.GetTeam())
            {
                List<int[]> attacks = bm.GetAttackableList(piece.position[0], piece.position[1]);

                foreach (int[] attack in attacks)
                {
                    int defender = pieces[attack[0], attack[1]].piece_id;
                    int attacker = piece.piece_id;

                    int roll_needed = Chess.RollNeeded(attacker, defender);
                    float prob = roll_needed / 6f;

                    int material_index = Math.Abs(defender);
                    float expected_value = prob * material_values[material_index - 1];

                    moves_examined++;

                    if (expected_value > attack_value)
                    {
                        attack_found = true;
                        attack_value = expected_value;

                        a_from = piece.position;
                        a_to = attack;
                    }
                }                
            }
        }

        // RISK MAP.
        // Create a risk map to calculate the risk of each move. Essentially, an int[8, 8] that tracks the highest piece_id that can attack 
        // that position, add this value to the expected value function. 
        // prob (success) * value of success - prob (failure) * cost of failure
        // Compare the value in the risk map to the piece being moved to get the probability of success and the cost of failure.
        // Add this to the current expected value.

        // DISTANCE MAP.
        // Create a distance map to store the distance from each empty piece to the king.
        // Use this map to calculate the value of each move, avoid having to calculate distance for all possible moves.

        // Find the position of the king.
        int[] king_position = FindKing();

        // Scan the board for possible moves.
        for (int p = 0; p < 8; p++)
        {
            for (int q = 0; q < 8; q++)
            {
                // Check if the position is empty. 
                if (!pieces[p, q])
                {
                    // Calculate the difference between the king position and the empty position. 
                    float[] difference = new float[] { king_position[0] - p, king_position[1] - q };
                    // Calculate the magnitude of the difference to get the distance. 
                    float distance = Mathf.Sqrt(difference[0] * difference[0] + difference[1] * difference[1]);
                    // Normalize the distance value. distance_value is inversely proportional to the distance. 
                    float distance_value = 1f / distance;
                    // Add the distance value to the dist map.
                    float scalar = 1f;
                    dist_map[p, q] = distance_value * scalar;
                }
            }
        }

        // Loop through all moves and compare move with the dist_map values.

        float move_value = 0f;
        bool move_found = false;

        // Move from and to.
        int[] m_from = new int[] { -1, -1 };
        int[] m_to = new int[] { -1, -1 };

        foreach (Piece piece in pieces)
        {
            if (!piece) continue;
            if (piece.has_moved) continue;

            if (piece.GetTeam() == gm.GetTeam())
            {
                List<int[]> moves = bm.GetMovesList(piece.position[0], piece.position[1], piece.GetNumberOfMoves());

                foreach (int[] move in moves)
                {
                    float current_value = dist_map[move[0], move[1]];

                    if (current_value > move_value)
                    {
                        move_value = current_value;
                        move_found = true;

                        m_from = piece.position;
                        m_to = move;
                    }
                }
            }
        }

        if (move_found && move_value > attack_value)
        {
            bm.RefreshBlocks();
            bm.DelayedMove(m_from, m_to, 0.5f);
        }
        else if (attack_found)
        {
            bm.RefreshBlocks();
            bm.DelayedAttack(a_from, a_to, 0.5f);
        }
        else
        {
            bm.RefreshBlocks();
            gm.EndTurn();
        }

        bm.Print(moves_examined);
        moves_examined = 0;
    }

    public int[] FindKing()
    {
        int[] king_position = new int[2]; 
        foreach (Piece piece in bm.GetPieces())
            if (piece && piece.GetTeam() != gm.GetTeam() && piece.commander.king_piece)
                king_position = piece.commander.king_piece.position;
        //Debug.Log("King Pos: " + king_position[0] + ", " + king_position[1]);
        return king_position;
    }

    public void EvaluateMaterial(int[,] board)
    {

    }

    // AI specific functions...
}