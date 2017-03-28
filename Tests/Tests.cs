using System.Fabric;
using TestRunner;

namespace Tests
{
    sealed class Tests : AbstractTestRunner<Tests>
    {
        public Tests(StatefulServiceContext context)
            : base(context)
        {
        }

        protected override Tests Self => this;
    }
}