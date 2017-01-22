using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSourced
{
    class ContainerManager
    {
        static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Configuration.ConfigurationManager.AppSettings["storageConnCloudSourced"]);
        static CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        static Dictionary<string, CloudBlobContainer> containers = new Dictionary<string, CloudBlobContainer>();

        public static async Task<CloudBlobContainer> Get(string containerName)
        {
            CloudBlobContainer container = null;
            containerName = containerName.ToLower();

            lock (containers)
            {
                if (containers.TryGetValue(containerName, out container))
                    return container;
                container = containers[containerName] = blobClient.GetContainerReference(containerName);
            }

            await container.CreateIfNotExistsAsync();
            return container;
        }
    }
}
