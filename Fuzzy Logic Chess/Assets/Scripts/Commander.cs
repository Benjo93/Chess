using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commander : MonoBehaviour
{
    private List<Piece> pieces; 

    public List<Piece> GetPieces()
    {
        return pieces;
    }
    public void AddPiece(Piece piece)
    {
        pieces.Add(piece);
    }
    public void RemovePiece(Piece piece)
    {
        pieces.Remove(piece);
    }

}
