namespace SubclassesTracker.Models.Responses.Rows
{
    /// <summary>
    /// Interface for flattening complex data structures into flat rows.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFlattener<T>
    {
        IEnumerable<T> ToFlatRows();
    }
}
