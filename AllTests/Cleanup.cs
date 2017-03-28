namespace Tests
{
    using System.Fabric;
    using System.Fabric.Description;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [SetUpFixture]
    public class Cleanup
    {
        [OneTimeTearDown]
        public async Task TearDown()
        {
            using (var fabric = new FabricClient())
            {
                var app = fabric.ApplicationManager;
                var applications = await fabric.QueryManager.GetApplicationListAsync().ConfigureAwait(false);
                foreach (var application in applications)
                {
                    await app.DeleteApplicationAsync(new DeleteApplicationDescription(application.ApplicationName)).ConfigureAwait(false);
                    await app.UnprovisionApplicationAsync(application.ApplicationTypeName, application.ApplicationTypeVersion).ConfigureAwait(false);

                }
            }
        }
    }
}