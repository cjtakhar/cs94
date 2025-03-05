﻿namespace AzureBlobManagedIdentity.Settings
{
    public interface IStorageAccountSettings
    {
        /// <summary>
        /// Defines url for the container end point
        /// </summary>
        public string ContainerEndpoint { get; set; }

        /// <summary>
        /// The tenant id where the storage account resides
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Account name for the storage account
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Account key for the storage account
        /// </summary>
        public string AccountKey { get; set; }

        /// <summary>
        /// Url to the storage account
        /// </summary>
        public string Url { get; set; }
    }
}