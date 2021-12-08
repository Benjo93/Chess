public class VirtualPiece
{
    public string p_name;
    public int piece_id;
    public int team;
    public int n_moves;
    public int corp_id;
    public bool is_commander;
    public bool has_moved = false;
    public int delegation_id;
    public int default_moves;
    public int[] position;
    public Piece self;
    //public int temp_id;
    //public Commander commander;

    public VirtualPiece(string p_name, int piece_id, int team, int n_moves, int corp_id, bool is_commander, bool has_moved, int delegation_id, int[] position, Piece self)
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
        this.position = position;
        this.self = self;
        //temp_id = 0;
    }
}