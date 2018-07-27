using System;
using System.Collections.Generic;
using System.Text;

namespace MobCATOrNot.Functions
{
    public class VisionIterationExports : List<VisionIterationExport>
    {

    }


    public class VisionIterationExport
    {
        public string platform { get; set; }
        public string status { get; set; }
        public string downloadUri { get; set; }
        public object flavor { get; set; }
    }


    public class VisionIterations : List<VisionIteration>
    {

    }

    public class VisionIteration
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
}
