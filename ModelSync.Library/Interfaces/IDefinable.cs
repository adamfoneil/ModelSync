namespace ModelSync.Interfaces
{
    /// <summary>
    /// used with Db objects that can be represented with a single Definition property
    /// </summary>
    public interface IDefinable
    {        
        string Name { get; set; }
        int ObjectId { get; set; }
        string Definition { get; set; }
    }
}
