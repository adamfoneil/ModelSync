using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;

namespace ModelSync.Library.Services
{
    public class SqlServer : SqlDialect
    {
        public override char StartDelimiter => '[';

        public override char EndDelimiter => ']';

        public override string BatchSeparator => "\r\nGO\r\n";

        public override Dictionary<IdentityType, Func<string, string>> IdentitySyntax => new Dictionary<IdentityType, Func<string, string>>()
        {
            { IdentityType.Int, (template) => "identity(1,1)" },
            { IdentityType.Long, (template) => "identity(1,1)" },
            { IdentityType.Guid, (template) => "DEFAULT newid()" }
        };
    }
}
