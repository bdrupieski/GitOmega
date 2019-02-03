using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using GitOmega.Git.Analysis;
using GitOmega.GitHub.Configuration;

namespace GitOmega.Git
{
    public class FileExtensionAnalyzer
    {
        private readonly AnalysisRunConfiguration _analysisRunConfiguration;

        public FileExtensionAnalyzer(AnalysisRunConfiguration analysisRunConfiguration)
        {
            _analysisRunConfiguration = analysisRunConfiguration;
        }

        public void Run(GitHubOrganizations gitHubConfig, string outputCsvPath)
        {
            SynchronizeLocalBareRepositories(gitHubConfig);

            var sw = Stopwatch.StartNew();

            var allChangesForAllRepos = CollectLineChangesByExtensionAndDate(gitHubConfig);

            var highestActivityFileExtensions = allChangesForAllRepos
                .GroupBy(lineChanges => lineChanges.FileExtension)
                .Where(changesByExtension => changesByExtension.Sum(lineChanges => lineChanges.AddedAndDeleted) > 500)
                .SelectMany(changesByExtension => changesByExtension)
                .ToList();

            WriteToCsv(highestActivityFileExtensions, outputCsvPath);

            sw.Stop();
            Console.WriteLine($"File extension changes collected in {sw.Elapsed.TotalMinutes} minutes.");
        }

        private void WriteToCsv(IEnumerable<PatchDiffLineChanges> lineChanges, string outputCsvPath)
        {
            using (var writer = new StreamWriter(outputCsvPath))
            using (var csv = new CsvWriter(writer))
            {
                csv.Configuration.RegisterClassMap<PatchDiffLineChangesCsvClassMap>();
                csv.WriteRecords(lineChanges);
            }
        }

        private List<PatchDiffLineChanges> CollectLineChangesByExtensionAndDate(GitHubOrganizations config)
        {
            var lineChangeRetriever = new PatchDiffLineChangesRetriever();

            var repositories = config.Organizations
                .SelectMany(org => org.Repositories)
                .Where(repo => !repo.IsFork)
                .OrderBy(repo => repo.Size);

            var changesForRepos = new ConcurrentBag<List<PatchDiffLineChanges>>();

            Parallel.ForEach(repositories, repository =>
            {
                Console.WriteLine($"Starting {repository.Name}");
                var localRepoPath = repository.BuildFullLocalDirectoryPath(_analysisRunConfiguration.LocalRootDirectoryForBareGitRepositories);
                var changes = lineChangeRetriever.GetLineChangeCountsByFileExtensionAndDate(localRepoPath, repository.DefaultBranch).ToList();

                changesForRepos.Add(changes);
            });

            return changesForRepos
                .SelectMany(lineChangesForRepo => lineChangesForRepo)
                .GroupByExtensionAndDate()
                .OrderBy(lineChanges => lineChanges.FileExtension)
                .ThenByDescending(lineChanges => lineChanges.When)
                .ToList();
        }

        private void SynchronizeLocalBareRepositories(GitHubOrganizations config)
        {
            var repoUpdater = new LocalGitRepositoryUpdater(_analysisRunConfiguration.GitHubPersonalAccessToken);
            foreach (var organization in config.Organizations)
            {
                foreach (var repository in organization.Repositories)
                {
                    var localRepoPath = repository.BuildFullLocalDirectoryPath(_analysisRunConfiguration.LocalRootDirectoryForBareGitRepositories);
                    repoUpdater.Synchronize(localRepoPath, repository.CloneUrl);
                }
            }
        }
    }
}