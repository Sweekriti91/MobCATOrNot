using System;
using System.IO;
using System.Collections.Generic;

namespace MobCATOrNot.Models
{
    public class PredictionsRequest
    {
        public Stream Image { get; set; }
    }

    public class PredictionsResult
    {
        public List<Prediction> Predictions { get; set; }
    }

    public class Prediction
    {
        public string Label { get; set; }
        public double Probability { get; set; }
    }

    public class SubmitImagesRequest
    {
        public Dictionary<string, Stream> Images { get; set; }
    }

    public enum MobCatOrNotStatus
    {
        idk,
        yes,
        no,
    }

    public class AboutItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }
}
