namespace Omega_Sudoku.Constraints;

public static class Constraints
{
    //caller function to start the constraint checking, the function will call the other functions
    //classes outside of this class don't need to know the specifics, just need to call this function
    public static Dictionary<string, string>? StartConstraints(Dictionary<string, string> GridValues, string StringKey, string DictValue
        ,Dictionary<string, IEnumerable<string>> _peers, Dictionary<string, IGrouping<string, string[]>> _units)
    {
        return assign(GridValues, StringKey, DictValue, _peers, _units);
    }
    
    
    /// <summary>Eliminate all the other GridValues (except DictValue) from GridValues[StringKey] and propagate &lt;= 2.</summary>
    public static Dictionary<string, string>? assign(Dictionary<string, string> GridValues, string StringKey, string DictValue
        ,Dictionary<string, IEnumerable<string>> _peers, Dictionary<string, IGrouping<string, string[]>> _units)
    {
        if (all(
                from digitNotStart in GridValues[StringKey]
                where digitNotStart.ToString() != DictValue
                select eliminate(GridValues, StringKey, digitNotStart.ToString(), _peers, _units)
            )
           )
            return GridValues;
        return null;
    }

    /// <summary>Eliminate DictValue from GridValues[StringKey]; propagate when GridValues or places &lt;= 2.</summary>
    public static Dictionary<string, string>? eliminate(Dictionary<string, string> GridValues, string StringKey,
        string DictValue, Dictionary<string, IEnumerable<string>> _peers, Dictionary<string, IGrouping<string, string[]>> _units)
    {
        if (!GridValues[StringKey].Contains(DictValue)) return GridValues;
        GridValues[StringKey] = GridValues[StringKey].Replace(DictValue, "");
        if (GridValues[StringKey].Length == 0)
        {
            return null; //Contradiction: removed last value
        }
        else if (GridValues[StringKey].Length == 1)
        {
            //If there is only one value (LastValue) left in cell, remove it from _peers
            var LastValue = GridValues[StringKey];
            if (!all(from peer in _peers[StringKey]
                    select eliminate(GridValues, peer, LastValue, _peers, _units)))
                return null;
        }

        //Now check the places where DictValue appears in the _units of StringKey
        foreach (var unit in _units[StringKey])
        {
            var ValuePlaces = from cell in unit
                where GridValues[cell].Contains(DictValue)
                select cell;
            if (ValuePlaces.Count() == 0)
                return null;
            else if (ValuePlaces.Count() == 1)
                // DictValue can only be in one place in unit; assign it there
                if (assign(GridValues, ValuePlaces.First(), DictValue, _peers, _units) == null)
                    return null;
        }

        return GridValues;
    }
    
    public static bool all<T>(IEnumerable<T> seq)
    {
        foreach (var e in seq)
            if (e == null)
                return false;
        return true;
    }
}