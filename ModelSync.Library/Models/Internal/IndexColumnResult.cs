namespace ModelSync.Models.Internal
{
    internal class IndexColumnResult
    {
        public int object_id { get; set; }
        public int index_id { get; set; }
        public string name { get; set; }
        public byte key_ordinal { get; set; }
        public bool is_descending_key { get; set; }
    }

    internal class IndexKey
    {
        public int object_id { get; set; }
        public int index_id { get; set; }

        public override bool Equals(object obj)
        {
            IndexKey test = obj as IndexKey;
            if (test != null)
            {
                return test.object_id == object_id && test.index_id == index_id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return object_id.GetHashCode() + index_id.GetHashCode();
        }
    }
}
