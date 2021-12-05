/*
 * AI:
 * Inherits from Player. 
 * Contains AI specific functions. 
 */

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

    float[] material_values = new float[] { 1, 4, 8, 8, 10, 20 };

    // Distance to king. 
    float[,] dist_map_friend = new float[8, 8];
    float[,] dist_map_enemy = new float[8, 8];

    // Risk and Reward Maps.
    int[,] r_map_friend = new int[8, 8];
    int[,] r_map_enemy = new int[8, 8];

    public AI(string name, GameManager gm, BoardManager bm) : base(name, gm, bm)
    {
        // AI specific constructor.
        // Inherits a name, game manager and board manager (Assigned in the GameManager)
    }

    public override void BeginMove()
    {
        bm.input_requested = false;

        VirtualBoard vbm = new VirtualBoard(bm.GetPieces(), gm.GetTeam());

        UpdateDistMaps(vbm);
        BuildRMaps(vbm);

        //Debug.Log("Enemy:");
        //PrintMap(dist_map_enemy);
        //Debug.Log("Friend:");
        //PrintMap(dist_map_friend);

        // Create a list of tuples that contain the piece and all of its moves/attacks.
        List<(VirtualPiece piece, List<int[]> attacks)> all_attacks = new List<(VirtualPiece, List<int[]>)>();
        List<(VirtualPiece piece, List<int[]> moves)> all_moves = new List<(VirtualPiece, List<int[]>)>();

        // Populate all attacks and moves.
        foreach (VirtualPiece piece in vbm.vpieces)
        {
            if (piece == null) continue;
            if (piece.has_moved) continue;
            if (piece.team != 1) continue;

            List<int[]> a = vbm.VirtualGetAttackableList(piece.position[0], piece.position[1]);
            if (a.Count > 0) all_attacks.Add((piece, a));
            vbm.VirtualRefreshBlocks();

            List<int[]> m = vbm.VirtualGetMovesList(piece.position[0], piece.position[1], piece.n_moves);
            if (m.Count > 0) all_moves.Add((piece, m));
            vbm.VirtualRefreshBlocks();
        }

        if (Random.Range(0f, 1f) > Chess.difficulty)
        {
            if (all_moves.Count > 0)
            {
                int pm = Random.Range(0, all_moves.Count - 1);
                VirtualPiece r_piece = all_moves[pm].piece;
                int[] r_to = all_moves[pm].moves[Random.Range(0, all_moves[pm].moves.Count - 1)];
                bm.DelayedMove(r_piece.position, r_to, 0.25f);
                return;
            }
        }

        bool _attack = false;
        bool _move = false;
        float _best = EvaluateBoard(vbm);

        Piece _piece = null;
        int[] _to = new int[] { -1, -1 };

        foreach (var (piece, moves) in all_moves)
        {
            foreach (var to in moves)
            {
                int[] from = piece.position;
                vbm.VirtualMovePiece(from, to);

                float _val = SolveBoard(vbm, 1, 1);
                if (_val > _best)
                {
                    _best = _val;
                    _piece = piece.self;
                    _to = to;
                    _move = true;
                }

                vbm.VirtualUndoMovePiece(from, to);
            }
        }

        foreach (var (piece, attacks) in all_attacks)
        {
            foreach (var to in attacks)
            {
                //int attacker = piece.piece_id;
                //int defender = vbm.vpieces[to[0], to[1]].piece_id;
                //int roll_needed = Chess.RollNeeded(attacker, defender);

                //float prob_success = (7f - roll_needed) / 6f;

                int[] from = piece.position;
                VirtualPiece captured_piece = vbm.VirtualAttackPiece(from, to);

                float _val = SolveBoard(vbm, 1, 1);
                //_val *= prob_success;

                if (_val > _best)
                {
                    _best = _val;
                    _piece = piece.self;
                    _to = to;
                    _attack = true;
                    _move = false;
                }

                vbm.VirtualUndoAttackPiece(from, to, captured_piece);
            }
        }

        Debug.Log("Best: " + _best);

        if (_move)
        {
            bm.DelayedMove(_piece.position, _to, 1.0f - Chess.turnSpeed + .25f);
        }
        if (_attack)
        {
            bm.DelayedAttack(_piece.position, _to, 1.0f - Chess.turnSpeed + .25f);
        }

        // No possible moves, end turn.
        if (!_move && !_attack) gm.CompleteGameState(6);
    }

    private float SolveBoard(VirtualBoard vbm, int min_max, int depth)
    {
        // Ending Condition.
        if (depth >= 3)
        {
            float eval = EvaluateBoard(vbm);
            return eval;
        }

        // Create a list of tuples that contain the piece and all of its moves/attacks.
        List<(VirtualPiece piece, List<int[]> attacks)> all_attacks = new List<(VirtualPiece, List<int[]>)>();
        List<(VirtualPiece piece, List<int[]> moves)> all_moves = new List<(VirtualPiece, List<int[]>)>();

        // Populate all attacks and moves.
        foreach (VirtualPiece piece in vbm.vpieces)
        {
            if (piece == null) continue;
            if (piece.has_moved) continue;
            if (piece.team != min_max) continue;

            // Add each attack to all_attacks.
            List<int[]> a = vbm.VirtualGetAttackableList(piece.position[0], piece.position[1]);
            if (a.Count > 0) all_attacks.Add((piece, a));

            // Add each move to all_moves.
            List<int[]> m = vbm.VirtualGetMovesList(piece.position[0], piece.position[1], piece.n_moves);
            if (m.Count > 0) all_moves.Add((piece, m));
        }

        // Set the intial value of the branch to the current board evaluation.
        float current_value = EvaluateBoard(vbm);

        // Check for attacks.
        foreach (var (piece, attacks) in all_attacks)
        {
            foreach (var to in attacks)
            {
                //int attacker = piece.piece_id;
                //int defender = vbm.vpieces[to[0], to[1]].piece_id;
                //int roll_needed = Chess.RollNeeded(attacker, defender);

                //float prob_success = (7f - roll_needed) / 6f; 

                int[] from = piece.position;

                VirtualPiece captured_piece = vbm.VirtualAttackPiece(from, to);

                float value_success = SolveBoard(vbm, min_max, depth + 1);

                //value_success *= prob_success; 

                // Maximize.
                if (min_max > 0) if (value_success > current_value) current_value = value_success;

                // Minimize. 
                if (min_max < 0) if (value_success < current_value) current_value = value_success;

                vbm.VirtualUndoAttackPiece(from, to, captured_piece);
            }
        }

        // Check for moves.
        foreach (var (piece, moves) in all_moves)
        {
            foreach (var to in moves)
            {
                int[] from = piece.position;

                vbm.VirtualMovePiece(from, to);

                // Branch Move.
                float value_below = SolveBoard(vbm, min_max, depth + 1);

                // Maximize.
                if (min_max > 0) if (value_below > current_value) current_value = value_below;

                // Minimize. 
                if (min_max < 0) if (value_below < current_value) current_value = value_below;

                vbm.VirtualUndoMovePiece(from, to);
            }
        }

        // No moves left in this branch.
        if (all_attacks.Count == 0 && all_moves.Count == 0)
        {
            return SolveBoard(vbm, min_max, depth + 1);
        }

        return current_value;
    }

    private float EvaluateBoard(VirtualBoard vrt_board)
    {
        UpdateDistMaps(vrt_board);

        float eval = 0f;

        float f_dist_sum = 0f;
        float e_dist_sum = 0f;

        float f_dist_count = 0f;
        float e_dist_count = 0f;

        float risk_value = 0f;
        float reward_value = 0f;

        foreach (VirtualPiece piece in vrt_board.vpieces)
        {
            if (piece == null) continue;           
            if (piece.team > 0)
            {
                eval += material_values[Mathf.Abs(piece.piece_id) - 1];

                // Evaluate Distance from enemy king to pieces.
                f_dist_sum += dist_map_friend[piece.position[0], piece.position[1]];
                f_dist_count++;

                // Evaluate Risk and Reward. 
                int rsk = r_map_enemy[piece.position[0], piece.position[1]];
                if (rsk != 0) risk_value -= (7f - Chess.RollNeeded(rsk, piece.piece_id)) / 6f * material_values[Mathf.Abs(piece.piece_id) - 1];

                //int rwd = r_map_enemy[piece.position[0], piece.position[1]];
                //if (rwd != 0) reward_value += (7f - Chess.RollNeeded(piece.piece_id, rwd)) / 6f * material_values[rwd - 1];
            }

            if (piece.team < 0)
            {
                eval -= material_values[Mathf.Abs(piece.piece_id) - 1];

                // Evaluate Distance from enemy pieces to king.
                e_dist_sum += dist_map_enemy[piece.position[0], piece.position[1]];
                e_dist_count++;

                // Evaluate Risk and Reward. 
                int rsk = r_map_friend[piece.position[0], piece.position[1]];
                if (rsk != 0) risk_value += (7f - Chess.RollNeeded(rsk, piece.piece_id)) / 6f * material_values[Mathf.Abs(piece.piece_id) - 1];           

                //int rwd = r_map_friend[piece.position[0], piece.position[1]];
                //if (rwd != 0) reward_value -= (7f - Chess.RollNeeded(piece.piece_id, rwd)) / 6f * material_values[rwd - 1];
            }
        }

        float f_avg_dist = f_dist_sum / f_dist_count;
        float e_avg_dist = e_dist_sum / e_dist_count;

        //Debug.Log("f: " + f_avg_dist + ", e: " + e_avg_dist);

        eval += f_avg_dist;
        eval -= e_avg_dist;

        eval += risk_value;
        eval += reward_value;

        return eval;
    }

    public int[] FindFriendlyKing(VirtualBoard vbm)
    {
        int[] king_position = new int[2];
        foreach (VirtualPiece piece in vbm.vpieces)
            if (piece != null && piece.team == -1 && Mathf.Abs(piece.piece_id) == 6)
                king_position = piece.position;
        return king_position;
    }

    public int[] FindEnemyKing(VirtualBoard vbm)
    {
        int[] king_position = new int[2];
        foreach (VirtualPiece piece in vbm.vpieces)
            if (piece != null && piece.team == 1 && Mathf.Abs(piece.piece_id) == 6)
                king_position = piece.position;
        return king_position;
    }

    public void UpdateDistMaps(VirtualBoard vbm) 
    {
        // Find the position of the king.
        int[] friend_king = FindFriendlyKing(vbm);
        int[] enemy_king = FindEnemyKing(vbm);

        // Scan the board for possible moves.
        for (int p = 0; p < 8; p++)
        {
            for (int q = 0; q < 8; q++)
            {
                // Calculate the difference between the king position and the empty position. 
                float[] f_diff = new float[] { friend_king[0] - p, friend_king[1] - q };
                float[] e_diff = new float[] { enemy_king[0] - p, enemy_king[1] - q };

                // Calculate the magnitude of the difference to get the distance. 
                float f_dist = Mathf.Sqrt(f_diff[0] * f_diff[0] + f_diff[1] * f_diff[1]);
                float e_dist = Mathf.Sqrt(e_diff[0] * e_diff[0] + e_diff[1] * e_diff[1]);

                // Normalize the distance value. distance_value is inversely proportional to the distance. 
                float f_dist_val = 1f / f_dist;
                float e_dist_val = 1f / e_dist;

                float scalar = 10.0f;

                // Add the distance value to the dist maps.
                dist_map_friend[p, q] = f_dist_val * scalar;
                dist_map_enemy[p, q] = e_dist_val * scalar;
            }
        }
    }

    public void BuildRMaps(VirtualBoard vbm)
    {
        r_map_enemy = new int[8, 8];

        foreach (VirtualPiece piece in vbm.vpieces)
        {
            if (piece == null) continue;

            // Look through enemy pieces.
            if (piece.team == -1)
            {
                // Get all enemy attacks.
                List<int[]> enemy_attacks = vbm.VirtualGetAttackableRange(piece.position[0], piece.position[1]);

                foreach (int[] attack in enemy_attacks)
                {
                    // Check if the piece_id is greater than the current risk value.
                    if (Mathf.Abs(piece.piece_id) > r_map_enemy[attack[0], attack[1]])
                    {
                        r_map_enemy[attack[0], attack[1]] = Mathf.Abs(piece.piece_id);
                    }
                }
            }
        }

        r_map_friend = new int[8, 8];

        foreach (VirtualPiece piece in vbm.vpieces)
        {
            if (piece == null) continue;

            // Look through enemy pieces.
            if (piece.team == 1)
            {
                // Get all enemy attacks.
                List<int[]> enemy_attacks = vbm.VirtualGetAttackableRange(piece.position[0], piece.position[1]);

                foreach (int[] attack in enemy_attacks)
                {
                    // Check if the piece_id is greater than the current risk value.
                    if (Mathf.Abs(piece.piece_id) > r_map_enemy[attack[0], attack[1]])
                    {
                        r_map_friend[attack[0], attack[1]] = Mathf.Abs(piece.piece_id);
                    }
                }
            }
        }
    }

    public void PrintMap(float[,] map)
    {
        string result = "";
        for (int p = 0; p < 8; p++)
        {
            for (int q = 0; q < 8; q++)
            {
                result += map[p, q] + " ";
            }
            result += " \n";
        }
        Debug.Log(result);
    }
}
