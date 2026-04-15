using System.Collections.ObjectModel;

namespace GameSettingsParser.Utility;

public static class EnumerableExtensions
{
    public static T? GetNext<T>(this IEnumerable<T> list, T current) where T : class
    {
        try
        {
            return list.SkipWhile(x => !x.Equals(current)).Skip(1).First();
        }
        catch
        {
            return null;
        }
    }

    public static T? GetPrevious<T>(this IEnumerable<T> list, T current) where T : class
    {
        try
        {
            return list.TakeWhile(x => !x.Equals(current)).Last();
        }
        catch
        {
            return null;
        }
    }
    
    public static int RemoveAll<T>(this ObservableCollection<T> collection, Func<T, bool> condition)
    {
        var itemsToRemove = collection.Where(condition).ToList();

        foreach (var itemToRemove in itemsToRemove)
        {
            collection.Remove(itemToRemove);
        }

        return itemsToRemove.Count;
    }
}