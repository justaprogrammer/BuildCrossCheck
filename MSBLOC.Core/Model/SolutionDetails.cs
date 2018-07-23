using System.Collections.Generic;
using MSBLOC.Core.Services;

namespace MSBLOC.Core.Model
{
    public class SolutionDetails: Dictionary<string, ProjectDetails>
    {
        public string CloneRoot { get; }

        public SolutionDetails(string cloneRoot)
        {
            CloneRoot = cloneRoot;
        }

        public string GetProjectItemPath(string projectFile, string item)
        {
            var projectDetails = this[projectFile];
            return projectDetails.GetPath(item);
        }

        public void Add(ProjectDetails projectDetails)
        {
            Add(projectDetails.ProjectFile, projectDetails);
        }
    }
}