namespace TestRunner
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;

    /// <summary>
    /// Orchestrates multiple communication listener.
    /// </summary>
    class CompositeCommunicationListener : ICommunicationListener
    {
        public CompositeCommunicationListener(params ICommunicationListener[] listeners)
        {
            this.listeners = listeners;
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            string result = null;
            foreach (var listener in listeners)
            {
                result = await listener.OpenAsync(cancellationToken).ConfigureAwait(false);
            }
            return result;
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            foreach (var listener in listeners)
            {
                await listener.CloseAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public void Abort()
        {
            foreach (var listener in listeners)
            {
                listener.Abort();
            }
        }

        ICommunicationListener[] listeners;
    }
}