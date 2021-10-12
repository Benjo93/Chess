/* 
 * Player:
 * Abstract class inherited by ai and human. 
 * Contains name, team, color, etc...
 */

public abstract class Player
{
    public string name;
    protected GameManager gm;
    protected BoardManager bm;

    public int max_moves = 6; 

    public Player(string name, GameManager gm, BoardManager bm)
    {
        this.name = name;
        this.gm = gm;
        this.bm = bm; 
    }

    public abstract void BeginMove();
}
