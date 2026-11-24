namespace chess_game
{
    // what a stored score means: alpha-beta often only proves a score is "at least" or "at most" something
    public enum BoundType : byte
    {
        Exact,      // the true score of the position
        LowerBound, // a cut-off happened: the true score is at least this (score >= beta)
        UpperBound  // no move beat alpha: the true score is at most this (score <= alpha)
    }

    // one remembered search result
    public struct TableEntry
    {
        public ulong Key;        // full hash of the position, to tell apart positions that share a slot
        public int Score;        // the score found (mate scores stored relative to this position, see Search)
        public int Depth;        // how deep the search below this position went
        public BoundType Bound;  // whether Score is exact, a lower bound or an upper bound
        public Move BestMove;    // the best move found, tried first next time
        public bool HasMove;     // false if no best move was recorded
    }

    // a fixed-size memory of positions already searched, indexed by Zobrist hash
    public class TranspositionTable
    {
        private readonly TableEntry[] entries;
        private readonly ulong indexMask; // size - 1; the size is a power of two, so "hash & mask" picks a slot

        public TranspositionTable(int sizePowerOfTwo)
        {
            entries = new TableEntry[1 << sizePowerOfTwo];
            indexMask = (ulong)entries.Length - 1;
        }

        // looks a position up; returns false if it isn't stored (empty slot, or another position is there)
        public bool TryGet(ulong key, out TableEntry entry)
        {
            entry = entries[key & indexMask];
            return entry.Key == key;
        }

        // remembers a result, replacing whatever was in that slot (simple "always replace" scheme)
        public void Store(ulong key, int depth, int score, BoundType bound, Move bestMove)
        {
            entries[key & indexMask] = new TableEntry
            {
                Key = key,
                Score = score,
                Depth = depth,
                Bound = bound,
                BestMove = bestMove,
                HasMove = true
            };
        }

        // forgets everything, e.g. for a new game
        public void Clear()
        {
            System.Array.Clear(entries, 0, entries.Length);
        }
    }
}