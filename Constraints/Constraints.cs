namespace Omega_Sudoku.Constraints;

public static class Constraints
{
    //caller function to start the constraint checking, the function will call the other functions
    //classes outside of this class don't need to know the specifics, just need to call this function
    public static Dictionary<string, string>? StartConstraints(Dictionary<string, string> gridValues, string stringKey,
        string dictValue
        , Dictionary<string, IEnumerable<string>>? peers, Dictionary<string, IGrouping<string, string[]>>? units)
    {
        return Assign(gridValues, stringKey, dictValue, peers, units);
    }


    /// <summary>Eliminate all the other GridValues (except DictValue) from GridValues[StringKey] and propagate &lt;= 2.</summary>
    ///The assign function is a function that eliminates all other possible values for a given cell in the input
    /// GridValues dictionary except for the input value DictValue, and propagates the changes to other cells in
    /// the puzzle. It first calls the eliminate function for all other possible values of the given cell and returns
    /// the input GridValues dictionary if all calls are successful. If any call returns null, it returns null as well,
    /// indicating a contradiction.
    public static Dictionary<string, string>? Assign(Dictionary<string, string> gridValues, string stringKey,
        string dictValue
        , Dictionary<string, IEnumerable<string>>? peers, Dictionary<string, IGrouping<string, string[]>>? units)
    {
        if (All(
                from digitNotStart in gridValues[stringKey]
                where digitNotStart.ToString() != dictValue
                select Eliminate(gridValues, stringKey, digitNotStart.ToString(), peers, units)
            )
           )
            return gridValues;
        return null;
    }

    /// <summary>Eliminate DictValue from GridValues[StringKey]; propagate when GridValues or places &lt;= 2.</summary>
    /// The eliminate function is a function that eliminates a given value DictValue from the possible values of a cell
    /// in the input GridValues dictionary, specified by the input StringKey. It first checks if the cell already contains
    /// the value and proceeds to remove it from the cell's possible values. If this results in no possible values left
    /// for the cell, it returns null indicating a contradiction. If there is only one possible value left for the cell,
    /// it calls the eliminate function for that value on all the peers of the cell. Lastly, it iterates through all units
    /// the cell belongs to and checks if the value can only appear in one cell in the unit and assigns it to that cell if
    /// so. If any of the previous steps returns null, it also returns null indicating a contradiction.
    public static Dictionary<string, string>? Eliminate(Dictionary<string, string> gridValues, string stringKey,
        string dictValue, Dictionary<string, IEnumerable<string>>? peers,
        Dictionary<string, IGrouping<string, string[]>>? units)
    {
        if (!gridValues[stringKey].Contains(dictValue)) return gridValues;
        gridValues[stringKey] = gridValues[stringKey].Replace(dictValue, "");
        if (gridValues[stringKey].Length == 0)
        {
            return null; //Contradiction: removed last value
        }
        else if (gridValues[stringKey].Length == 1)
        {
            //If there is only one value (LastValue) left in cell, remove it from _peers
            var lastValue = gridValues[stringKey];
            if (!All(from peer in peers[stringKey]
                    select Eliminate(gridValues, peer, lastValue, peers, units)))
                return null;
        }

        //Now check the places where DictValue appears in the _units of StringKey
        foreach (var unit in units[stringKey])
        {
            var valuePlaces = from cell in unit
                where gridValues[cell].Contains(dictValue)
                select cell;
            if (valuePlaces.Count() == 0)
                return null;
            else if (valuePlaces.Count() == 1)
                // DictValue can only be in one place in unit; assign it there
                if (Assign(gridValues, valuePlaces.First(), dictValue, peers, units) == null)
                    return null;
        }

        return gridValues;
    }

    /// <summary>
    /// The all function is a helper function that takes an enumerable input of type T and returns a Boolean
    /// indicating whether all elements in the sequence are non-null. This function is used in both assign
    /// and eliminate functions to check if all calls to eliminate are successful.
    /// </summary>
    /// <param name="seq"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool All<T>(IEnumerable<T> seq)
    {
        foreach (var e in seq)
            if (e == null)
                return false;
        return true;
    }
}