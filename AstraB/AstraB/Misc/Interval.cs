public struct Interval
{
    public int Length => end - begin;
    
    public int begin, end;
    public bool includeBegin, includeEnd;

    public bool IsOverlap(Interval another)
    {
        if (this.begin <= another.begin && another.begin < this.end)
        {
            return true;
        }

        return false;
    }

    public bool IsInside(Interval smaller)
    {
        // Smaller's begin is out of bounds
        if (this.begin > smaller.begin)
            return false;
        
        // Smaller's end is out of bounds
        if (this.end > smaller.end)
            return false;

        return true;
    }
    
    public override string ToString()
    {
        return (includeBegin ? "[" : "(") + begin + ".." + end + (includeEnd ? "]" : ")");
    }

    public static Interval FromBeginAndLength(int begin, int length)
    {
        return new Interval()
        {
            begin = begin,
            end = begin + length,
            includeBegin = true,
            includeEnd = false,
        };
    }
}