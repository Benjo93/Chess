using UnityEngine;

public class Block : MonoBehaviour
{
    private int position;
    // State, color, etc..

    public Color color;
    public bool moveable; 

    public void SetPosition(int position)
    {
        this.position = position; 
    }

    public int GetPosition()
    {
        return position; 
    }
}
