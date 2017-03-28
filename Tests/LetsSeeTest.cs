using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using NUnit.Framework;
using TestRunner;

namespace Tests
{
    [TestFixture]
    public class LetsSeeTest : INeed<IReliableStateManager>
    {
        IReliableStateManager stateManager;

        [Test]
        public async Task LetsSee()
        {
            var primary = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("index1", TimeSpan.FromSeconds(5));
            var secondary = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("index2", TimeSpan.FromSeconds(5));

            var transaction = stateManager.CreateTransaction();
            await primary.AddOrUpdateAsync(transaction, "Key", "Value", (s, s1) => "Value");
            await secondary.AddOrUpdateAsync(transaction, "Key", "Key", (s, s1) => "Key");
            await transaction.CommitAsync();
            transaction.Dispose();

            var tcs = new TaskCompletionSource<bool>();
            var startSync = new TaskCompletionSource<bool>();

            var t1 = Task.Run(async () =>
            {
                var winningTransaction = stateManager.CreateTransaction();
                var conditional = await secondary.TryGetValueAsync(winningTransaction, "Key");
                await primary.TryGetValueAsync(winningTransaction, conditional.Value);
                await winningTransaction.CommitAsync();
                winningTransaction.Dispose();

                startSync.SetResult(true);
                await tcs.Task;

                winningTransaction = stateManager.CreateTransaction();
                var result = await primary.TryUpdateAsync(winningTransaction, "Key", "Value1", "Value");
                Console.WriteLine($"Result t1 { result }");
                await winningTransaction.CommitAsync();
                winningTransaction.Dispose();

            });

            var t2 = Task.Run(async () =>
            {
                await startSync.Task;

                var losingTransaction = stateManager.CreateTransaction();
                await primary.TryGetValueAsync(losingTransaction, "Key");
                await losingTransaction.CommitAsync();
                losingTransaction.Dispose();

                tcs.SetResult(true);
                await t1;

                losingTransaction = stateManager.CreateTransaction();
                var result = await primary.TryUpdateAsync(losingTransaction, "Key", "Value2", "Value");
                Console.WriteLine($"Result t2 {result}");
                losingTransaction.Dispose();
                Assert.IsFalse(result, "Expected to fail to update the value, but didn't.");

                transaction = stateManager.CreateTransaction();
                var conditional = await primary.TryGetValueAsync(transaction, "Key");
                transaction.Dispose();

                Assert.AreEqual("Value1", conditional.Value);
            });

            await t2;
        }

        [Test]
        [Ignore("is not supported by SF")]
        public async Task LetsSeeInterleaved()
        {
            var primary = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("index1", TimeSpan.FromSeconds(5));
            var secondary = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("index2", TimeSpan.FromSeconds(5));

            var transaction = stateManager.CreateTransaction();
            await primary.AddOrUpdateAsync(transaction, "Key", "Value", (s, s1) => "Value");
            await secondary.AddOrUpdateAsync(transaction, "Key", "Key", (s, s1) => "Key");
            await transaction.CommitAsync();
            transaction.Dispose();

            var tcs = new TaskCompletionSource<bool>();
            var startSync = new TaskCompletionSource<bool>();

            var t1 = Task.Run(async () =>
            {
                var winningTransaction = stateManager.CreateTransaction();
                var conditional = await secondary.TryGetValueAsync(winningTransaction, "Key");
                await primary.TryGetValueAsync(winningTransaction, conditional.Value);
                await winningTransaction.CommitAsync();
                winningTransaction.Dispose();

                winningTransaction = stateManager.CreateTransaction();
                var result = await primary.TryUpdateAsync(winningTransaction, "Key", "Value1", "Value");
                Console.WriteLine($"Result t1 { result }");
                startSync.SetResult(true);
                await tcs.Task;

                await winningTransaction.CommitAsync();
                winningTransaction.Dispose();

            });

            var t2 = Task.Run(async () =>
            {
                await startSync.Task;

                var losingTransaction = stateManager.CreateTransaction();
                await primary.TryGetValueAsync(losingTransaction, "Key");
                await losingTransaction.CommitAsync();
                losingTransaction.Dispose();

                losingTransaction = stateManager.CreateTransaction();
                var result = await primary.TryUpdateAsync(losingTransaction, "Key", "Value2", "Value");
                Console.WriteLine($"Result t2 {result}");
                tcs.SetResult(true);
                losingTransaction.Dispose();
                Assert.IsFalse(result, "Expected to fail to update the value, but didn't.");

                await t1;

                transaction = stateManager.CreateTransaction();
                var conditional = await primary.TryGetValueAsync(transaction, "Key");
                transaction.Dispose();

                Assert.AreEqual("Value1", conditional.Value);
            });

            await t2;
        }

        [Test]
        public async Task LetsSeeWithoutThreads()
        {
            var primary = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("index1", TimeSpan.FromSeconds(5));
            var secondary = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("index2", TimeSpan.FromSeconds(5));

            var transaction = stateManager.CreateTransaction();
            await primary.AddOrUpdateAsync(transaction, "Key", "Value", (s, s1) => "Value");
            await secondary.AddOrUpdateAsync(transaction, "Key", "Key", (s, s1) => "Key");
            await transaction.CommitAsync();
            transaction.Dispose();

            var winningTransaction = stateManager.CreateTransaction();
            var conditional = await secondary.TryGetValueAsync(winningTransaction, "Key");
            await primary.TryGetValueAsync(winningTransaction, conditional.Value);
            winningTransaction.Dispose();

            var losingTransaction = stateManager.CreateTransaction();
            conditional = await secondary.TryGetValueAsync(winningTransaction, "Key");
            await primary.TryGetValueAsync(losingTransaction, conditional.Value);
            losingTransaction.Dispose();

            winningTransaction = stateManager.CreateTransaction();
            var result = await primary.TryUpdateAsync(winningTransaction, "Key", "Value1", "Value");
            Console.WriteLine($"Result t1 { result }");
            await winningTransaction.CommitAsync();
            winningTransaction.Dispose();

            losingTransaction = stateManager.CreateTransaction();
            result = await primary.TryUpdateAsync(losingTransaction, "Key", "Value2", "Value");
            Console.WriteLine($"Result t2 {result}");
            await losingTransaction.CommitAsync();
            losingTransaction.Dispose();
            Assert.IsFalse(result, "Expected to fail to update the value, but didn't.");

            transaction = stateManager.CreateTransaction();
            conditional = await primary.TryGetValueAsync(transaction, "Key");
            transaction.Dispose();

            Assert.AreEqual("Value1", conditional.Value);
        }

        [Test]
        [Ignore("is not supported by SF")]
        public async Task LetsSeeWithoutThreadsInterleaved()
        {
            var primary = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("index1", TimeSpan.FromSeconds(5));
            var secondary = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("index2", TimeSpan.FromSeconds(5));

            var transaction = stateManager.CreateTransaction();
            await primary.AddOrUpdateAsync(transaction, "Key", "Value", (s, s1) => "Value");
            await secondary.AddOrUpdateAsync(transaction, "Key", "Key", (s, s1) => "Key");
            await transaction.CommitAsync();
            transaction.Dispose();

            var winningTransaction = stateManager.CreateTransaction();
            var conditional = await secondary.TryGetValueAsync(winningTransaction, "Key");
            await primary.TryGetValueAsync(winningTransaction, conditional.Value);
            winningTransaction.Dispose();

            var losingTransaction = stateManager.CreateTransaction();
            conditional = await secondary.TryGetValueAsync(winningTransaction, "Key");
            await primary.TryGetValueAsync(losingTransaction, conditional.Value);
            losingTransaction.Dispose();

            winningTransaction = stateManager.CreateTransaction();
            losingTransaction = stateManager.CreateTransaction();
            var result = await primary.TryUpdateAsync(winningTransaction, "Key", "Value1", "Value");
            Console.WriteLine($"Result t1 { result }");
            result = await primary.TryUpdateAsync(losingTransaction, "Key", "Value2", "Value");
            Console.WriteLine($"Result t2 {result}");
            await winningTransaction.CommitAsync();
            winningTransaction.Dispose();

            losingTransaction.Dispose();
            Assert.IsFalse(result, "Expected to fail to update the value, but didn't.");

            transaction = stateManager.CreateTransaction();
            conditional = await primary.TryGetValueAsync(transaction, "Key");
            transaction.Dispose();

            Assert.AreEqual("Value1", conditional.Value);
        }

        public void Need(IReliableStateManager dependency)
        {
            stateManager = dependency;
        }
    }
}