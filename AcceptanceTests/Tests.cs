namespace AcceptanceTests
{
    using System.Fabric;
    using TestRunner;

    sealed class Tests : AbstractTestRunner<Tests>
    {
        public Tests(StatefulServiceContext context)
            : base(context)
        {
        }

        protected override Tests Self => this;
    }
}