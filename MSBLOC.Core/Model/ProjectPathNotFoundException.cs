using System;

namespace MSBLOC.Core.Model
{
    public class ProjectPathNotFoundException : Exception
    {
        public ProjectDetails ProjectDetails { get; }
        public string ItemProjectPath { get; }

        public ProjectPathNotFoundException(ProjectDetails projectDetails, string itemProjectPath)
        {
            ProjectDetails = projectDetails;
            ItemProjectPath = itemProjectPath;
        }
    }
}