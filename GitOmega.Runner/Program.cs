using System.IO;
using System.Threading.Tasks;
using GitOmega.Git;
using GitOmega.GitHub;
using Newtonsoft.Json;

namespace GitOmega.Runner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var runConfiguration = GetConfig(args);

            var gitHubConfigRetriever = new GitHubConfigurationRetriever(runConfiguration.GitHubPersonalAccessToken);

            var gitHubConfig = await gitHubConfigRetriever.GetRepositoriesForOrganizationsFromGitHubOrCache(
                runConfiguration.LocalRootDirectoryForBareGitRepositories,
                runConfiguration.GitHubOrganizationNames);

            var outputFilePath = Path.Combine(runConfiguration.LocalRootDirectoryForBareGitRepositories, "file_extensions.csv");

            var fileExtensionAnalyzer = new FileExtensionAnalyzer(runConfiguration);
            fileExtensionAnalyzer.Run(gitHubConfig, outputFilePath);
        }

        private static AnalysisRunConfiguration GetConfig(string[] args)
        {
            AnalysisRunConfiguration runConfiguration;

            if (args.Length > 0)
            {
                var json = File.ReadAllText(args[0]);
                runConfiguration = JsonConvert.DeserializeObject<AnalysisRunConfiguration>(json);
            }
            else
            {
                runConfiguration = new AnalysisRunConfiguration
                {
                    LocalRootDirectoryForBareGitRepositories = @"C:\bare",
                    GitHubPersonalAccessToken = "",
                    GitHubOrganizationNames = new[] { "" },
                };
            }

            return runConfiguration;
        }
    }
}
