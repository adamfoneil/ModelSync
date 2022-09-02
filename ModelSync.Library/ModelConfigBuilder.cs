using System;
using System.Collections.Generic;
using System.Text;

namespace ModelSync.Library.Services
{
    public class ModelConfigBuilder<T> where T : new()
    {
        private T CurrentModel { get; set; }
        public string IdentityColumn { get; set; }
        public int IdentitySeed { get; set; }
        public int IdentityIncrement { get; set; }

        public ModelConfigBuilder()
        {
            CurrentModel = GetObject();
        }

        protected T GetObject()
        {
            return new T();
        }
        public ModelConfigBuilder<T> AddIdentityColumn(string columnName, int seed, int increment)
        {
            IdentityColumn = columnName;
            IdentitySeed = seed;
            IdentityIncrement = increment;
            return this;
        }
        //public ModelBuilder<T> Build()
        //{
        //    return new ModelBuilder(this);
        //}
    }
}
