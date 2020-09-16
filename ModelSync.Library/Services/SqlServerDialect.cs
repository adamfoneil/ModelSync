using ModelSync.Abstract;

namespace ModelSync.Services
{
    public class SqlServerDialect : SqlDialect
    {
        public override char StartDelimiter => '[';

        public override char EndDelimiter => ']';

        public override string BatchSeparator => "\r\nGO\r\n";

        public override string CommentStart => "--";
    }
}
