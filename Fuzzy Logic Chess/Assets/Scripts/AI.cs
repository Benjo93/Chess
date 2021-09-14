/*
 * AI:
 * Inherits from Player. 
 * Contains AI specific functions. 
 */

public class AI : Player
{
    public AI(string name, GameManager gm, BoardManager bm) : base(name, gm, bm)
    {
        // AI specific constructor.
    }

    public override void Move()
    {
        // Call on the AI solver to get next move.
        bm.RequestInput(name + ", " + GetType());
    }

    // AI specific functions...
}