using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MSBLOC.Core.Model
{
    public class ProjectDetails
    {
        public string ProjectFile { get; }
        public string CloneRoot { get; }
        public string ProjectDirectory { get; }
        public IReadOnlyDictionary<string, string> Paths => new ReadOnlyDictionary<string, string>(_paths);

        private Dictionary<string, string> _paths;

        public ProjectDetails(string cloneRoot, string projectFile)
        {
            CloneRoot = cloneRoot;
            ProjectFile = projectFile;

            if (!projectFile.IsSubPathOf(cloneRoot))
            {
                throw new ProjectDetailsException($"Project file path \"{projectFile}\" is not a subpath of \"{cloneRoot}\"");
            }

            ProjectDirectory = Path.GetDirectoryName(projectFile)
                               ?? throw new ProjectDetailsException("Path.GetDirectoryName(projectFile) is null");

            _paths = new Dictionary<string, string>();
        }

        public void AddItems(params string[] itemProjectPaths)
        {
            foreach (var itemProjectPath in itemProjectPaths)
            {
                AddItem(itemProjectPath);
            }
        }

        private void AddItem(string itemProjectPath)
        {
            if (_paths.ContainsKey(itemProjectPath))
            {
                throw new ProjectDetailsException($"Item \"{itemProjectPath}\" already exists");
            }

            _paths[itemProjectPath] = GetClonePath(itemProjectPath);
        }

        private string GetClonePath(string itemProjectPath)
        {
            return Path.Combine(ProjectDirectory, itemProjectPath)
                .Split(new[] {CloneRoot}, StringSplitOptions.RemoveEmptyEntries)
                .First()
                .Replace(@"\", "/");
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