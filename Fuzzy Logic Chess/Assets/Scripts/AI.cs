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

    float[] material_values = new float[] { 1, 5, 6, 8, 10, 20 };

    float[,] dist_map = new float[8, 8];
    int[,] risk_map_enemy = new int[8, 8];
    int[,] risk_map_friend = new int[8, 8];

    public AI(string name, GameManager gm, BoardManager bm) : base(name, gm, bm)
    {
        // AI specific constructor.
        // Inherits a name, game manager and board manager (Assigned in the GameManager)
    }

    public override void BeginMove()
    {
        bm.input_requested = false;

        VirtualBoard vbm = new VirtualBoard(bm.GetPieces(), gm.GetTeam());

        BuildDistMap(vbm);
        BuildRiskMap(vbm);

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
                int[] from = piece.position;
                VirtualPiece captured_piece = vbm.VirtualAttackPiece(from, to);

                float _val = SolveBoard(vbm, 1, 1);
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
                //float prob_success 

                int[] from = piece.position;

                VirtualPiece captured_piece = vbm.VirtualAttackPiece(from, to);

                float value_success = SolveBoard(vbm, min_max, depth + 1);

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
        float eval = 0;
        float dist_sum = 0f;
        float dist_count = 0;

        float risk_value = 0;

        foreach (VirtualPiece piece in vrt_board.vpieces)
        {
            if (piece == null) continue;           
            if (piece.team > 0)
            {
                eval += material_values[Mathf.Abs(piece.piece_id) - 1];
                dist_count++;
                dist_sum += dist_map[piece.position[0], piece.position[1]];

                int r = risk_map_enemy[piece.position[0], piece.position[1]];
                if (r != 0) risk_value -= (7f - Chess.RollNeeded(r, piece.piece_id)) / 6f * material_values[Mathf.Abs(piece.piece_id) - 1];
            }

            if (piece.team < 0)
            {
                eval -= material_values[Mathf.Abs(piece.piece_id) - 1];

                int r = risk_map_friend[piece.position[0], piece.position[1]];
                if (r != 0) risk_value += (7f - Chess.RollNeeded(r, piece.piece_id)) / 6f * material_values[Mathf.Abs(piece.piece_id) - 1];
            }
        }

        float avg_dist = dist_sum / dist_count;

        eval += avg_dist;
        eval += risk_value;

        return eval;
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

    public int[] FindVirtualKing(VirtualBoard vbm)
    {
        int[] king_position = new int[2];
        foreach (VirtualPiece piece in vbm.vpieces)
            if (piece != null && piece.team == -1 && Mathf.Abs(piece.piece_id) == 6)
                king_position = piece.position;
        return king_position;
    }

    public void BuildDistMap(VirtualBoard vbm) 
    {
        // Find the position of the king.
        int[] king_position = FindVirtualKing(vbm);

        // Scan the board for possible moves.
        for (int p = 0; p < 8; p++)
        {
            for (int q = 0; q < 8; q++)
            {
                // Calculate the difference between the king position and the empty position. 
                float[] difference = new float[] { king_position[0] - p, king_position[1] - q };
                // Calculate the magnitude of the difference to get the distance. 
                float distance = Mathf.Sqrt(difference[0] * difference[0] + difference[1] * difference[1]);
                // Normalize the distance value. distance_value is inversely proportional to the distance. 
                float distance_value = 1f / distance;
                // Add the distance value to the dist map.
                float scalar = 1.0f;
                dist_map[p, q] = distance_value * scalar;
            }
        }
    }

    public void BuildRiskMap(VirtualBoard vbm)
    {
        risk_map_enemy = new int[8, 8];

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
                    if (Mathf.Abs(piece.piece_id) > risk_map_enemy[attack[0], attack[1]])
                    {
                        risk_map_enemy[attack[0], attack[1]] = Mathf.Abs(piece.piece_id);
                    }
                }
            }
        }

        risk_map_friend = new int[8, 8];

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
                    if (Mathf.Abs(piece.piece_id) > risk_map_enemy[attack[0], attack[1]])
                    {
                        risk_map_friend[attack[0], attack[1]] = Mathf.Abs(piece.piece_id);
                    }
                }
            }
        }
    }
}
