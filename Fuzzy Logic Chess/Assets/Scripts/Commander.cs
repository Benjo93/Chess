using System.Collections.Generic;
using UnityEngine;

public class Commander : MonoBehaviour
{
    public int corp_id;
    private List<Piece> pieces = new List<Piece>();
    public bool has_authority;
    public int default_moves;

    private Commander king;
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
    public void TransferPiecesToKing()
    {
        foreach (Piece piece in pieces)
        {
            king.AddPiece(piece);
        }

        pieces.Clear();
    }

    public void UseCommandAuthority()
    {
        foreach (Piece piece in pieces)
        {
            piece.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0.4f);
            piece.has_moved = true;
        }
    }
    public void RestrictMoves()
    {
        GetComponent<Piece>().n_moves = 1;
    }
}