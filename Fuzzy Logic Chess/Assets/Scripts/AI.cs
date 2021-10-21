/*
 * AI:
 * Inherits from Player. 
 * Contains AI specific functions. 
 */

using System;
using System.Collections.Generic;


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

    public int look_count = 0; 

    float[] material_values = new float[] { 1, 3, 5, 4, 6, 10 };


    public AI(string name, GameManager gm, BoardManager bm) : base(name, gm, bm)
    {
        // AI specific constructor.
        // Inherits a name, game manager and board manager (Assigned in the GameManager)
    }

    public override void BeginMove()
    {
        // Call on the AI solver to get next move.
        Piece[,] pieces = bm.GetAllPieces();

        int[] best_from = new int[] { -1, -1 };
        int[] best_to = new int[] { -1, -1 };

        float highest_expected_value = 0f;

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

                    look_count++;

                    if (expected_value > highest_expected_value)
                    {
                        attack_found = true;

                        best_from = piece.position;
                        best_to = attack;

                        highest_expected_value = expected_value;
                    }
                }                
            }
        }

        if (attack_found)
        {
            bm.RefreshBlocks();
            bm.Attack(best_from, best_to);
        }

        bm.Print(look_count);
        look_count = 0;
    }

    public void EvaluateMaterial(int[,] board)
    {

    }

    // AI specific functions...
}