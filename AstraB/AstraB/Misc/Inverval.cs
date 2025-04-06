public struct Inverval
{
    public int begin, end;
    public bool includeBegin, includeEnd;

    public override string ToString()
    {
        return (includeBegin ? "[" : "(") + begin + ".." + end + (includeEnd ? "]" : ")");
    }
}