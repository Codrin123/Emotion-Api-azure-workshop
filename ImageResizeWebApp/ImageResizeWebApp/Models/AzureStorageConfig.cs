﻿namespace ImageResizeWebApp.Models
{
    public class AzureStorageConfig
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string QueueName { get; set; }
        public string ImageContainer { get; set; }
        public string ThumbnailContainer { get; set; }
        public string EmotionApiKey { get; set; }
        public string EmotionApiEndpoint { get; set; }
    }
}
