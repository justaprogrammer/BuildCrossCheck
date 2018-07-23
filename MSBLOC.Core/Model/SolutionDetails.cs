using System.Collections.Generic;

namespace MSBLOC.Core.Services
{
    public class SolutionDetails
    {
        public string CloneRoot { get; }
        public IReadOnlyDictionary<string, ProjectDetails> Paths => _projects;

        private Dictionary<string, ProjectDetails> _projects;

        public SolutionDetails(string cloneRoot)
        {
            CloneRoot = cloneRoot;
            _projects = new Dictionary<string, ProjectDetails>();
        }

        public void AddProject(ProjectDetails projectDetails)
        {
            _projects.Add(projectDetails.ProjectFile, projectDetails);
        }

        public ProjectDetails GetProject(string projectFile)
        {
            return _projects[projectFile];
        }

        public string GetProjectItemPath(string projectFile, string item)
        {
            var projectDetails = _projects[projectFile];
            return projectDetails.GetPath(item);
        }
    }
}