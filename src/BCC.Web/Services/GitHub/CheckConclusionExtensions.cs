using System;
using BCC.Core.Model.CheckRunSubmission;

namespace BCC.Web.Services.GitHub
{
    public static class CheckConclusionExtensions{
        public static Octokit.CheckConclusion ToOctokit(this CheckConclusion checkConclusion)
        {
            switch (checkConclusion)
            {
                case CheckConclusion.Success:
                    return Octokit.CheckConclusion.Success;
                case CheckConclusion.Failure:
                    return Octokit.CheckConclusion.Failure;
                case CheckConclusion.Neutral:
                    return Octokit.CheckConclusion.Neutral;
                case CheckConclusion.Cancelled:
                    return Octokit.CheckConclusion.Cancelled;
                case CheckConclusion.TimedOut:
                    return Octokit.CheckConclusion.TimedOut;
                case CheckConclusion.ActionRequired:
                    return Octokit.CheckConclusion.ActionRequired;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}