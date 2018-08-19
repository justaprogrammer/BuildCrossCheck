using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using MSBLOC.Core.Extensions;

namespace MSBLOC.Core.Model.Builds
{
    public class ProjectDetails
    {
        public string ProjectFile { get; }
        public string CloneRoot { get; }
        public string ProjectDirectory { get; }
        public IReadOnlyDictionary<string, string> Paths => new ReadOnlyDictionary<string, string>(_paths);

        private Dictionary<string, string> _paths;

        public ProjectDetails([NotNull] string cloneRoot, [NotNull] string projectFile)
        {
            CloneRoot = cloneRoot ?? throw new ArgumentNullException(nameof(cloneRoot));
            ProjectFile = projectFile ?? throw new ArgumentNullException(nameof(projectFile));

            if (!projectFile.IsSubPathOf(cloneRoot))
            {
                throw new ProjectDetailsException($"Project file path \"{projectFile}\" is not a subpath of \"{cloneRoot}\"");
            }

            ProjectDirectory = Path.GetDirectoryName(ProjectFile)
                               ?? throw new ProjectDetailsException("Path.GetDirectoryName(projectFile) is null");

            _paths = new Dictionary<string, string>();
        }

        public void AddItems([NotNull] params string[] itemProjectPaths)
        {
            if (itemProjectPaths == null)
            {
                throw new ArgumentNullException(nameof(itemProjectPaths));
            }

            foreach (var itemProjectPath in itemProjectPaths)
            {
                AddItem(itemProjectPath);
            }
        }

        private void AddItem([NotNull] string itemProjectPath)
        {
            if (itemProjectPath == null)
            {
                throw new ArgumentNullException(nameof(itemProjectPath));
            }

            if (_paths.ContainsKey(itemProjectPath))
            {
                throw new ProjectDetailsException($"Item \"{itemProjectPath}\" already exists");
            }

            _paths[itemProjectPath] = GetClonePath(itemProjectPath);
        }

        private string GetClonePath([NotNull] string itemProjectPath)
        {
            if (itemProjectPath == null)
            {
                throw new ArgumentNullException(nameof(itemProjectPath));
            }

            return Path.Combine(ProjectDirectory, itemProjectPath)
                .Split(new[] {CloneRoot}, StringSplitOptions.RemoveEmptyEntries)
                .First()
                .Replace(@"\", "/")
                .TrimStart('\\');
        }

        public string GetPath([NotNull] string itemProjectPath)
        {
            if (itemProjectPath == null)
            {
                throw new ArgumentNullException(nameof(itemProjectPath));
            }

            if (_paths.TryGetValue(itemProjectPath, out var result))
            {
                return result;
            }

            throw new ProjectPathNotFoundException(this, itemProjectPath);
        }
    }
}