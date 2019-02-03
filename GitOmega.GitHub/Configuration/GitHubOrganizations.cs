using System;
using System.Collections.Generic;

namespace GitOmega.GitHub.Configuration
{
    public class GitHubOrganizations
    {
        public DateTimeOffset WhenObtained { get; set; }
        public List<GitHubOrganization> Organizations { get; set; }
    }
}