using System;
using System.IO;

namespace GitOmega.GitHub.Configuration
{
    public class GitHubRepository
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public string DefaultBranch { get; set; }
        public string CloneUrl { get; set; }
        public bool IsFork { get; set; }
        public bool IsPrivate { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool IsArchived { get; set; }
        public long Size { get; set; }

        public string BuildFullLocalDirectoryPath(string rootDirectory) => Path.Combine(rootDirectory, FullName);
    }
}