using System.Collections.Generic;

namespace GitOmega.GitHub.Configuration
{
    public class GitHubOrganization
    {
        public string Name { get; set; }
        public List<GitHubRepository> Repositories { get; set; }
        public List<GitHubUser> Users { get; set; }
    }
}