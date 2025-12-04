using System.Reflection;
using NUnitLite;

namespace UnitTests
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // Run all tests in this assembly with NUnitLite
            return new AutoRun(Assembly.GetExecutingAssembly()).Execute(args);
        }
    }
}
