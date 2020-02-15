using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;

namespace ModelSync.Library.Dialects
{
    public class SqlServer : SqlDialect
    {
        public override char StartDelimiter => '[';

        public override char EndDelimiter => ']';

        public override string BatchSeparator => "\r\nGO\r\n";

        public override Dictionary<string, string> DataTypes => throw new NotImplementedException();

        public override Dictionary<IdentityType, string> IdentityTypes => throw new NotImplementedException();

        public override Dictionary<string, string> DefaultSizes => throw new NotImplementedException();
    }
}
