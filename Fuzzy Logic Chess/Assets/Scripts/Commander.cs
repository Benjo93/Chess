using System.Collections.Generic;
using UnityEngine;

public class Commander : MonoBehaviour
{
    public int corp_id;
    private List<Piece> pieces = new List<Piece>();
    public bool has_authority;
    public int default_moves;

    private Commander king;
    public Piece king_piece; 
    private Commander left;
    private Commander right;
    public bool is_king;

    public List<Piece> GetPiecesInCorp()
    {
        return pieces;
    }

    public void AddPiece(Piece piece)
    {
        piece.commander = this;
        pieces.Add(piece);
    }

    public void RemovePiece(Piece piece)
    {
        pieces.Remove(piece);
    }

    public Commander SetKing(Commander king)
    {
        this.king = king;
        return this;
    }

    public Commander GetKing()
    {
        return king;
    }

    public void SetLeft(Commander left)
    {
        this.left = left;
    }

    public Commander GetLeft()
    {
        return left;
    }

    public Commander GetRight()
    {
        return right;
    }

    public void SetRight(Commander right)
    {
        this.right = right;
    }

    public void TransferPiecesToKing()
    {
        foreach (Piece piece in pieces)
        {
            king.AddPiece(piece);
            piece.SetCorpID(king.GetCorpID());
        }

        pieces.Clear();
    }

    //basically returns true if commander has been captured. Used to ensure captured bishops may not be delegated too
    public bool IsEmpty()
    {
        if (pieces.Count == 0)
        {
            return true;
        }
        return false;
    }


    public void UseCommandAuthority()
    {
        foreach (Piece piece in pieces)
        {
            piece.ColorDim();
            piece.has_moved = true;
        }
    }
    public void RestrictMoves()
    {
        GetComponent<Piece>().SetNumberOfMoves(1);
    }

    public void SetCorpID(int NewID)
    {
        this.corp_id = NewID;
    }

    public int GetCorpID()
    {
        return corp_id;
    }
}