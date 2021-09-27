/* 
 * Human:
 * Inherits from Player. 
 * Contains human specific player actions.
 */

public class Human : Player
{
    public Human(string name, GameManager gm, BoardManager bm) : base(name, gm, bm)
    {
        // Human specific constructor. 
    }

    public override void BeginMove()
    {
        // Call on the board manager to request input. 
        bm.RequestInput(name + ", " + GetType());
    }

    // Human specific functions...
}