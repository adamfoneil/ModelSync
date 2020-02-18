using ModelSync.Library.Abstract;

namespace ModelSync.Library.Services
{
    public class SqlServerDialect : SqlDialect
    {
        public override char StartDelimiter => '[';

        public override char EndDelimiter => ']';

        public override string BatchSeparator => "\r\nGO\r\n";
    }
}
