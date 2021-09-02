using UnityEngine;

public class Block : MonoBehaviour
{
    private int[] position;
    // State, color, etc..

    public Color color;
    public bool moveable; 

    public void SetPosition(int row, int column)
    {
        this.position = new int[2] { row, column };
    }

    public int[] GetPosition()
    {
        return position; 
    }
}
