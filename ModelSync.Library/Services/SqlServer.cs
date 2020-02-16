using ModelSync.Library.Abstract;

namespace ModelSync.Library.Services
{
    public class SqlServer : SqlDialect
    {
        public override char StartDelimiter => '[';

        public override char EndDelimiter => ']';

        public override string BatchSeparator => "\r\nGO\r\n";
    }
}
