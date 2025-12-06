
using Xunit;
using MyPlatform.SDK.IdGenerator.Snowflake;
using MyPlatform.SDK.IdGenerator.Extensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace MyPlatform.SDK.IdGenerator.Tests
{
    public class SnowflakeTests
    {
        [Fact]
        public void NextId_ShouldGenerateUniqueIds()
        {
            var provider = new StaticWorkerIdProvider(1);
            var generator = new SnowflakeIdGenerator(provider);
            var id1 = generator.NextId();
            var id2 = generator.NextId();

            Assert.NotEqual(id1, id2);
            Assert.True(id2 > id1);
        }

        [Fact]
        public async Task NextId_ConcurrentLoadTest_ShouldBeUnique()
        {
            // Simulate high concurrency: 100 tasks, each generating 1000 IDs.
            // approx 100,000 IDs total. 
            // Snowflake technically supports 4 millions/sec (4096 * 1000ms).
            
            int taskCount = 100;
            int idsPerTask = 2000;
            var provider = new StaticWorkerIdProvider(1);
            var generator = new SnowflakeIdGenerator(provider);
            
            var allIds = new ConcurrentBag<long>();

            var tasks = Enumerable.Range(0, taskCount).Select(i => Task.Run(() =>
            {
                for (int j = 0; j < idsPerTask; j++)
                {
                    allIds.Add(generator.NextId());
                }
            }));

            await Task.WhenAll(tasks);

            var totalCount = taskCount * idsPerTask;
            Assert.Equal(totalCount, allIds.Count);

            // Check for duplicates
            var distinctCount = allIds.Distinct().Count();
            Assert.Equal(totalCount, distinctCount);
        }
    }
}
