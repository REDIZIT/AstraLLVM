namespace Astra.Compilation;

public class UniqueGenerator
{
    public HashSet<string> ids = new();

    public string Unique(string id)
    {
        string uniqueID = id;
        int i = 1;
        
        while (ids.Contains(uniqueID))
        {
            i++;
            uniqueID = id + "_" + i;
        }

        ids.Add(uniqueID);
        return uniqueID;
    }
}