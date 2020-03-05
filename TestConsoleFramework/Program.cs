using ModelSync.App.Helpers;
using ModelSync.Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestConsoleFramework
{
    class Program
    {
        /// <summary>
        /// added this because TestExplorer not working
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // note: AssemblyHelper is not in the source of this repo -- this test is internal only
            //Assembly.Load("System.ComponentModel.Annotations, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyHelper.LoadReflectionOnlyDependencies;
            //System.ComponentModel.Annotations, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
            var assembly = Assembly.ReflectionOnlyLoadFrom(@"C:\Users\Adam\Source\Repos\Ginseng8\Ginseng8.Models\bin\Debug\netstandard2.0\Ginseng.Models.dll");
            var dataModel = new AssemblyModelBuilder().GetDataModel(assembly);

        }
    }
}
