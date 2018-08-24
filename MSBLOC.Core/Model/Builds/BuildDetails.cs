using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MSBLOC.Core.Model.Builds
{
    public class BuildDetails
    {
        private readonly List<BuildMessage> _buildMessages;

        public BuildDetails([NotNull] SolutionDetails solutionDetails)
        {
            SolutionDetails = solutionDetails ?? throw new ArgumentNullException(nameof(solutionDetails));
            _buildMessages = new List<BuildMessage>();
        }

        public SolutionDetails SolutionDetails { get; }
        public IReadOnlyList<BuildMessage> BuildMessages => _buildMessages.ToArray();

        public void AddMessage([NotNull] BuildMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            _buildMessages.Add(message);
        }

        public void AddMessages([NotNull] IEnumerable<BuildMessage> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            _buildMessages.AddRange(messages);
        }
    }
}