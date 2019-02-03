namespace GitOmega.Git
{
    public class AnalysisRunConfiguration
    {
        public string GitHubPersonalAccessToken { get; set; }
        public string[] GitHubOrganizationNames { get; set; }

        public string LocalRootDirectoryForBareGitRepositories { get; set; }
    }
}