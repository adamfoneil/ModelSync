using ModelSync.Library.Abstract;
using System.Collections.Generic;

namespace ModelSync.Library.Services
{
    public class SqlServer : SqlDialect
    {
        public override char StartDelimiter => '[';

        public override char EndDelimiter => ']';

        public override string BatchSeparator => "\r\nGO\r\n";

        public override Dictionary<IdentityType, string> IdentitySyntax => new Dictionary<IdentityType, string>()
        {
            { IdentityType.Int, "identity(1,1)" },
            { IdentityType.Long, "identity(1,1)" },
            { IdentityType.Guid, "newid" }
        };
    }
}
