namespace ModelSync.Models.Internal
{
    internal class ForeignKeysResult
    {
        public int ObjectId { get; set; }
        public string ConstraintName { get; set; }
        public string ReferencedSchema { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencingSchema { get; set; }
        public string ReferencingTable { get; set; }
        public bool CascadeDelete { get; set; }
        public bool CascadeUpdate { get; set; }
    }

    internal class ForeignKeyColumnsResult
    {
        public int ObjectId { get; set; }
        public string ReferencingName { get; set; }
        public string ReferencedName { get; set; }
    }
}
