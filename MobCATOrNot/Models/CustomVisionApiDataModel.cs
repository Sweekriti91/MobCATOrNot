using System;
using System.Collections.Generic;

namespace MobCATOrNot.Models
{
    public class CustomVisionErrorResponse
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class CustomVisionIteration
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool isDefault { get; set; }
        public string status { get; set; }
        public DateTime created { get; set; }
        public DateTime lastModified { get; set; }
        public DateTime trainedAt { get; set; }
        public string projectId { get; set; }
        public bool exportable { get; set; }
        public string domainId { get; set; }
    }

    public enum CustomVisionModelType
    {
        CoreML,
        TensorFlow,
    }

    public enum CustomVisionIterationStatus
    {
        Training,
        Completed,
    }

    public enum CustomVisionIterationExportStatus
    {
        Done,
    }

    public enum CustomVisionUploadImageStatus
    {
        OK,
    }


    public class CustomVisionTag
    {
        public string id { get; set; }
        public string name { get; set; }
        public DateTime created { get; set; }
        public int? imageCount { get; set; }
    }

    public class CustomVisionImage
    {
        public string id { get; set; }
        public DateTime created { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string imageUri { get; set; }
        public string thumbnailUri { get; set; }
        public List<CustomVisionTag> tags { get; set; }
    }

    public class CustomVisionUploadedImage
    {
        public string sourceUrl { get; set; }
        public string status { get; set; }
        public CustomVisionImage image { get; set; }
    }

    public class CustomVisionUploadImagesResult
    {
        public bool isBatchSuccessful { get; set; }
        public List<CustomVisionUploadedImage> images { get; set; }
    }

    public class CustomVisionIterationExport
    {
        public string platform { get; set; }
        public string status { get; set; }
        public string downloadUri { get; set; }
        public object flavor { get; set; }
    }
}