using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MSBLOC.Core.Model
{
    public class ProjectDetails
    {
        public string ProjectFile { get; }
        public string CloneRoot { get; }
        public string ProjectDirectory { get; }
        public IReadOnlyDictionary<string, string> Paths => _paths;

        private Dictionary<string, string> _paths;

        public ProjectDetails(string cloneRoot, string projectFile)
        {
            CloneRoot = cloneRoot;
            ProjectFile = projectFile;

            ProjectDirectory = Path.GetDirectoryName(projectFile)
                               ?? throw new InvalidOperationException(
                                   "Path.GetDirectoryName(startedEventArgs.ProjectFile) is null");

            _paths = new Dictionary<string, string>();
        }

        public void AddOrReplaceItems(params string[] itemProjectPaths)
        {
            foreach (var itemProjectPath in itemProjectPaths)
            {
                _paths[itemProjectPath] = GetClonePath(itemProjectPath);
            }
        }

        private string GetClonePath(string itemProjectPath)
        {
            return Path.Combine(ProjectDirectory, itemProjectPath)
                .Split(new[] {CloneRoot}, StringSplitOptions.RemoveEmptyEntries)
                .First();
        }

        public string GetPath(string itemProjectPath)
        {
            if (_paths.TryGetValue(itemProjectPath, out var result))
            {
                return result;
            }

            throw new ProjectPathNotFoundException(this, itemProjectPath);
        }
    }
}