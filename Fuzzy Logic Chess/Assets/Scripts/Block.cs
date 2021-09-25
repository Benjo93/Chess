using UnityEngine;

public class Block : MonoBehaviour
{
    private int[] position;

    public Color initial_color;
    private Color current_color;
    private bool movable = false;
    private bool visited = false;
    private bool attackable = false; 

    // Position.
    public void SetPosition(int row, int column)
    {
        position = new int[2] { row, column };
    }
    public int[] GetPosition()
    {
        return position;
    }

    // Color.
    public void SetColor(Color color)
    {
        initial_color = color;
        current_color = color;
    }
    public void ChangeColor(Color new_color)
    {        
        GetComponent<SpriteRenderer>().material.color = new_color;
        current_color = new_color;
    }
    public void HoverColor()
    {
        GetComponent<SpriteRenderer>().material.color = Color.white;
    }
    public void CurrentColor()
    {
        GetComponent<SpriteRenderer>().material.color = current_color;
    }
    public void InitialColor()
    {
        GetComponent<SpriteRenderer>().material.color = initial_color;
        current_color = initial_color;
    }
    public bool IsVisited()
    {
        return visited;
    }
    public void SetVisited(bool b)
    {
        visited = b;
    }
    public bool IsMovable()
    {
        return movable;
    }
    public void SetMovable(bool b)
    {
        movable = b;
    }
    public bool IsAttackable()
    {
        return attackable; 
    }
    public void SetAttackable(bool a)
    {
        attackable = a;
    }
}