namespace BlobTriggerDemo
{
    public static class ConfigSettings
    {
        public const string GRAYSCALEIMAGES_CONTAINERNAME = "imagestoconverttograyscale";

        public const string CONVERTED_IMAGES_CONTAINERNAME = "convertedimages";

        public const string FAILED_IMAGES_CONTAINERNAME = "failedimages";

        public const string STORAGE_CONNECTIONSTRING_NAME = "AzureWebJobsStorage";

        public const string JOBS_TABLENAME = "jobs";

        public const string IMAGEJOBS_PARTITIONKEY = "ImageJobs";

        public const string JOBID_METADATA_NAME = "JobId";

    }
}
