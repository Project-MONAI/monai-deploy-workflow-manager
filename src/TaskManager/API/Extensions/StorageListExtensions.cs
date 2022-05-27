using Ardalis.GuardClauses;

namespace Monai.Deploy.WorkflowManager.TaskManager.API.Extensions
{
    public static class StorageListExtensions
    {
        public static Dictionary<string, string> ToArtifactDictionary(this List<Messaging.Common.Storage> storageList)
        {
            Guard.Against.Null(storageList, nameof(storageList));

            var artifactDict = new Dictionary<string, string>();

            foreach (var storage in storageList)
            {
                if (string.IsNullOrWhiteSpace(storage.Name) || string.IsNullOrWhiteSpace(storage.RelativeRootPath))
                {
                    continue;
                }

                artifactDict.Add(storage.Name, storage.RelativeRootPath);
            }

            return artifactDict;
        }
    }
}
