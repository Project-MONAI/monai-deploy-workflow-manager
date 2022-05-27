using Monai.Deploy.WorkflowManager.TaskManager.API.Extensions;
using Xunit;

namespace Monai.Deploy.WorkflowManager.TaskManager.API.Tests.Extensions
{
    public class StorageListExtensionsTests
    {
        [Fact]
        public void ToArtifactDictionary_ValidStorageList_GeneratesDictionary()
        {
            var storageList = new List<Messaging.Common.Storage>
            {
                new Messaging.Common.Storage
                {
                    Name = "test",
                    RelativeRootPath = "payloadid/dcm"
                },
                new Messaging.Common.Storage
                {
                    Name = "test2",
                    RelativeRootPath = "payloadid/dcm"
                }
            };

            var expected = new Dictionary<string, string>
            {
                { "test", "payloadid/dcm" },
                { "test2", "payloadid/dcm" }
            };

            var artifactDict = storageList.ToArtifactDictionary();

            Assert.Equal(expected, artifactDict);
        }

        [Fact]
        public void ToArtifactDictionary_MissingFields_ReturnsEmpty()
        {
            var storageList = new List<Messaging.Common.Storage>
            {
                new Messaging.Common.Storage
                {
                    Endpoint = "test"
                }
            };

            var expected = new Dictionary<string, string>();

            var artifactDict = storageList.ToArtifactDictionary();

            Assert.Equal(expected, artifactDict);
        }

        [Fact]
        public void ToArtifactDictionary_EmptyList_ReturnsEmpty()
        {
            var storageList = new List<Messaging.Common.Storage>();

            var expected = new Dictionary<string, string>();

            var artifactDict = storageList.ToArtifactDictionary();

            Assert.Equal(expected, artifactDict);
        }
    }
}
