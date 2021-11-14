public class VirtualPiece
{
    public string p_name;
    public int piece_id;
    public string team;
    public int n_moves;
    public int corp_id;
    public bool is_commander;
    public bool has_moved;
    public int delegation_id;
    public int default_moves;
    //public int temp_id;
    //public Commander commander;

    public VirtualPiece(string p_name, int piece_id, string team, int n_moves, int corp_id, bool is_commander, bool has_moved, int delegation_id)
    {
        this.p_name = p_name;
        this.piece_id = piece_id;
        this.team = team;
        this.n_moves = n_moves;
        this.corp_id = corp_id;
        this.is_commander = is_commander;
        this.has_moved = has_moved;
        this.delegation_id = delegation_id;
        this.default_moves = n_moves;
        //temp_id = 0;
    }
}