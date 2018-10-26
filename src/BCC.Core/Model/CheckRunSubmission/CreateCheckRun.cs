using System;

namespace BCC.Core.Model.CheckRunSubmission
{
    public class CreateCheckRun
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public bool Success { get; set; }
        public Annotation[] Annotations { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset CompletedAt { get; set; }
    }
}