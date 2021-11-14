using System;
using System.Collections;
using System.Collections.Generic;

public class VirtualBoard
{
    private VirtualBlock[,] vblocks = new VirtualBlock[8, 8];
    private VirtualPiece[,] vpieces = new VirtualPiece[8, 8];

    public Dice dice;

    public VirtualBoard(Piece[,] pieces)
    {
        for (int rank = 0; rank < vblocks.GetLength(0); rank++)
        {
            for (int file = 0; file < vblocks.GetLength(1); file++)
            {
                vblocks[rank, file] = new VirtualBlock();
                if (pieces[rank,file])
                {
                    Piece currPiece = pieces[rank, file];
                    vpieces[rank, file] = new VirtualPiece(currPiece.GetPName(), currPiece.GetPieceID(), currPiece.GetTeam(), 
                                                           currPiece.GetNumberOfMoves(), currPiece.GetCorpID(), currPiece.is_commander,
                                                           currPiece.has_moved, currPiece.GetDelegationID());
                }
            }
        }
    }

    public VirtualBoard(VirtualPiece[,] pieces)
    {
        for (int rank = 0; rank < vblocks.GetLength(0); rank++)
        {
            for (int file = 0; file < vblocks.GetLength(1); file++)
            {
                vblocks[rank, file] = new VirtualBlock();
                if (pieces[rank, file]!= null)
                {
                    VirtualPiece currPiece = pieces[rank, file];
                    vpieces[rank, file] = new VirtualPiece(currPiece.p_name, currPiece.piece_id, currPiece.team, currPiece.n_moves,
                                                           currPiece.corp_id, currPiece.is_commander, currPiece.has_moved,
                                                           currPiece.delegation_id);
                }
            }
        }
    }

    private bool VirtualIsValidBlock(int row, int col)
    {
        return row >= 0 && col >= 0 && row < 8 && col < 8 && vpieces[row, col] == null && !vblocks[row, col].visited;
    }

    private void VirtualProcessBlock(int row, int col, List<int[]> list)
    {
        list.Add(new int[] { row, col });
        vblocks[row, col].visited = true;
    }

    public List<int[]> VirtualGetMovesList(int row, int col, int m)
    {
        List<int[]> list = new List<int[]>();
        Queue<int[]> buildQueue = new Queue<int[]>();
        Queue<int[]> currentQueue = new Queue<int[]>();
        string selectedPiece = vpieces[row, col].p_name;
        do
        {
            if (row < 7 && !selectedPiece.Equals("b_pawn"))
            {
                if (VirtualIsValidBlock(row + 1, col)) // Down
                {
                    VirtualProcessBlock(row + 1, col, list);
                    buildQueue.Enqueue(new int[2] { row + 1, col });
                }
                if (col < 7 && VirtualIsValidBlock(row + 1, col + 1)) // Down Right
                {
                    VirtualProcessBlock(row + 1, col + 1, list);
                    buildQueue.Enqueue(new int[2] { row + 1, col + 1 });
                }
                if (col > 0 && VirtualIsValidBlock(row + 1, col - 1)) // Down Left
                {
                    VirtualProcessBlock(row + 1, col - 1, list);
                    buildQueue.Enqueue(new int[2] { row + 1, col - 1 });
                }
            }
            if (row > 0 && !selectedPiece.Equals("w_pawn"))
            {
                if (VirtualIsValidBlock(row - 1, col)) // Up
                {
                    VirtualProcessBlock(row - 1, col, list);
                    buildQueue.Enqueue(new int[2] { row - 1, col });
                }
                if (col < 7 && VirtualIsValidBlock(row - 1, col + 1)) // Up Right
                {
                    VirtualProcessBlock(row - 1, col + 1, list);
                    buildQueue.Enqueue(new int[2] { row - 1, col + 1 });
                }
                if (col > 0 && VirtualIsValidBlock(row - 1, col - 1)) // Up Left
                {
                    VirtualProcessBlock(row - 1, col - 1, list);
                    buildQueue.Enqueue(new int[2] { row - 1, col - 1 });
                }
            }
            if (!selectedPiece.Equals("b_pawn") && !selectedPiece.Equals("w_pawn"))
            {
                if (col > 0 && VirtualIsValidBlock(row, col - 1)) // Left
                {
                    VirtualProcessBlock(row, col - 1, list);
                    buildQueue.Enqueue(new int[2] { row, col - 1 });
                }
                if (col < 7 && VirtualIsValidBlock(row, col + 1)) // Right
                {
                    VirtualProcessBlock(row, col + 1, list);
                    buildQueue.Enqueue(new int[2] { row, col + 1 });
                }
            }
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
            else
            {
                m = 0;
            }

        } while (m > 0);
        return list;
    }

    public List<int[]> VirtualGetAttackableList(int row, int col)
    {
        List<int[]> newList = new List<int[]>();
        bool isWhitePiece = vpieces[row, col].team.Equals("white");
        int range = 1;
        if (vpieces[row, col].p_name.Equals("w_rook") || vpieces[row, col].p_name.Equals("b_rook"))
            range = vpieces[row, col].n_moves;

        int north = Math.Max(0, row - range);
        int south = Math.Min(7, row + range);
        int west = Math.Max(0, col - range);
        int east = Math.Min(7, col + range);

        if (vpieces[row, col].p_name.Equals("w_pawn"))
            north = row + range;
        else if (vpieces[row, col].p_name.Equals("b_pawn"))
            south = row - range;

        for (int i = north; i <= south; i++)
        {
            for (int j = west; j <= east; j++)
            {
                if (vpieces[i, j] != null)
                {
                    if (vpieces[i, j].team.Equals("white") != isWhitePiece)
                        newList.Add(new int[] { i, j });
                }
            }
        }
        return newList;
    }

    private void VirtualSetBlockListMovable(List<int[]> list)
    {
        foreach (int[] pos in list)
        {
            vblocks[pos[0], pos[1]].movable = true;
        }
    }

    private void VirtualSetBlockListAttackable(List<int[]> list)
    {
        foreach (int[] pos in list)
        {
            vblocks[pos[0], pos[1]].attackable = true;
        }
    }

    public void VirtualRefreshBlocks()
    {
        foreach (VirtualBlock block in vblocks)
        {
            block.movable = false;
            block.visited = false;
            block.attackable = false;
        }
    }

    public void VirtualRefreshPieces()
    {
        foreach (VirtualPiece piece in vpieces)
        {
            if (piece != null)
            {
                piece.has_moved = false;
                if(piece.is_commander)
                {
                    piece.n_moves = piece.default_moves;
                }
            }
        }
    }

    public void VirtualMovePiece(int[] from, int[] to)
    {
        VirtualRefreshBlocks();

        VirtualPiece p = vpieces[from[0], from[1]];

        vpieces[to[0], to[1]] = vpieces[from[0], from[1]];
        vpieces[from[0], from[1]] = null;

        /*
        if ((pieces[to[0], to[1]].GetPName() == "w_knight" || pieces[to[0], to[1]].GetPName() == "b_knight") && PiecesAdjacent(to))
        {
            Enable_Knight_Options();
            HighlightAdjacentPieces(to);
        }
        */
    }

}
