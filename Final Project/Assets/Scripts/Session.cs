/*
 * Session:
 * Static class that contains game session variables. 
 * values can be assigned from GUI by creating a class that interacts with the GUI via some functions
 * and setting these static variables from those functions.
 */

public static class Session
{
    public static string[] players = new string[] { "human", "ai" };
    public static string[] names = new string[] { "Black", "White" };
}
