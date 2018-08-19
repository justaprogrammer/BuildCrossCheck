using System.Collections.Generic;
using JetBrains.Annotations;

namespace MSBLOC.Core.Model.Builds
{
    public class SolutionDetails: Dictionary<string, ProjectDetails>
    {
        public string CloneRoot { get; }

        public SolutionDetails([NotNull] string cloneRoot)
        {
            CloneRoot = cloneRoot ?? throw new System.ArgumentNullException(nameof(cloneRoot));
        }

        public string GetProjectItemPath([NotNull] string projectFile, [NotNull] string item)
        {
            if (projectFile == null)
            {
                throw new System.ArgumentNullException(nameof(projectFile));
            }

            if (item == null)
            {
                throw new System.ArgumentNullException(nameof(item));
            }

            var projectDetails = this[projectFile];
            return projectDetails.GetPath(item);
        }

        public void Add([NotNull] ProjectDetails projectDetails)
        {
            if (projectDetails == null)
            {
                throw new System.ArgumentNullException(nameof(projectDetails));
            }

            Add(projectDetails.ProjectFile, projectDetails);
        }
    }
}