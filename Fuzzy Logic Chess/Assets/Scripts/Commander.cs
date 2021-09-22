using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commander : MonoBehaviour
{
    private List<Piece> pieces = new List<Piece>();
    public bool has_authority;
    public int default_moves;

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