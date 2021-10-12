/*
 * AI:
 * Inherits from Player. 
 * Contains AI specific functions. 
 */

public class AI : Player
{
    /*
     * To access the game manager, use gm:
     * gm.CompleteGameState(moves_used)
     * moves_used -> 1 if not a commander OR command authority was not used, 2 if command authority was used.
     * 
     * To access the board manager, use bm:
     * bm.GetBoardState() -> Returns an 2D integer array of the board state (-6 to 6, 0 is empty).
     * bm.GetAllPieces() -> Returns a list of all pieces to acces their attributes.
     * bm.GetCorpState() -> Returns a 2D array of where each corp is located on the board.
     * bm.Move(from, to) -> Called after AI has computed its best move, moves the piece on the board. 
     * bm.Attack(from, to) -> Called if AI has decided to attack, returns true if attack was successful, false if unsuccessful.
     * 
     */

    public AI(string name, GameManager gm, BoardManager bm) : base(name, gm, bm)
    {
        // AI specific constructor.
        // Inherits a name, game manager and board manager (Assigned in the GameManager)
    }

    public override void BeginMove()
    {
        // Call on the AI solver to get next move.

    }

    // AI specific functions...


    // returns an integer that gives a score to each side based on piece value. positive = white.
    public double EvaluateMaterial(int [,] material)
    {
        double pointTotal = 0;
        // white material
        int whiteMaterial;
        foreach (int x in material[0])
        {
            switch (x)
            {
                case 1:
                    whiteMaterial += 100;
                    break;

                case 2:
                    whiteMaterial += 600;
                    break;

                case 3:
                    whiteMaterial += 450;
                    break;

                case 4:
                    whiteMaterial += 800;
                    break;

                case 5:
                    whiteMaterial += 1000;
                    break;

                case 6:
                    whiteMaterial += 2500;
                    break;


            }

        }

        // black material
        int blackMaterial = 0;
        foreach (int x in material[1])
        {
            switch (x)
            {
                case -1:
                    blackMaterial -= 100;
                    break;

                case -2:
                    blackMaterial -= 600;
                    break;

                case -3:
                    blackMaterial -= 450;
                    break;

                case -4:
                    blackMaterial -= 800;
                    break;

                case -5: 
                    blackMaterial -= 1000;
                    break;

                case -6:
                    blackMaterial -= 2500;
                    break;

            }
        }

        return (whiteMaterial + blackMaterial);

    }
    // returns a 2 dimensional array representing the material counts of each side
    int[,]  getCurrentMaterial(int[,] board)
    {
        int[,] material = new int[2][6];

        foreach (int pieceNum in board)
        {
            
            if (pieceNum > 0)
            {
                material[0][pieceNum - 1] += 1
            }
            
            if (pieceNum < 0)
            {
                material[1][Math.abs(pieceNum) - 1] += 1
            }
        }
        return material

    }

    

    
    
}