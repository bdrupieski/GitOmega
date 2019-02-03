using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitOmega.GitHub.Configuration;
using MoreLinq;
using Newtonsoft.Json;
using Octokit;

namespace GitOmega.GitHub
{
    public class GitHubConfigurationRetriever
    {
        private readonly string _personalAccessToken;

        public GitHubConfigurationRetriever(string personalAccessToken)
        {
            _personalAccessToken = personalAccessToken;
        }

        public async Task<GitHubOrganizations> GetRepositoriesForOrganizationsFromGitHubOrCache(string bareRepoDirectory, ICollection<string> gitHubOrganizationNames)
        {
            var configFilename = "config.json";
            var fullPath = Path.Combine(bareRepoDirectory, configFilename);

            if (File.Exists(fullPath))
                return JsonConvert.DeserializeObject<GitHubOrganizations>(File.ReadAllText(fullPath));

            var config = await GetOrganizationConfigurationFromGitHub(gitHubOrganizationNames);
            SaveConfigToDisk(config, bareRepoDirectory, configFilename);
            return config;
        }

        private async Task<GitHubOrganizations> GetOrganizationConfigurationFromGitHub(ICollection<string> gitHubOrganizationNames)
        {
            var client = BuildGitHubClient();

            var organizationConfigs = new List<GitHubOrganization>(gitHubOrganizationNames.Count + 1);

            foreach (var gitHubOrganizationName in gitHubOrganizationNames)
            {
                var repositories = await client.Repository.GetAllForOrg(gitHubOrganizationName);

                var repositoryConfigs = repositories
                    .Select(x => new GitHubRepository
                    {
                        Id = x.Id,
                        Name = x.Name,
                        FullName = x.FullName,
                        Description = x.Description,
                        DefaultBranch = x.DefaultBranch,
                        CloneUrl = x.CloneUrl,
                        IsFork = x.Fork,
                        IsPrivate = x.Private,
                        UpdatedAt = x.UpdatedAt,
                        CreatedAt = x.CreatedAt,
                        IsArchived = x.Archived,
                        Size = x.Size,
                    })
                    .ToList();

                var members = await client.Organization.Member.GetAll(gitHubOrganizationName);
                var collaborators = await client.Organization.OutsideCollaborator.GetAll(gitHubOrganizationName);

                var organizationUsers = members
                    .Concat(collaborators)
                    .DistinctBy(x => x.Id)
                    .Select(x => new GitHubUser
                    {
                        Id = x.Id,
                        Login = x.Login,
                    })
                    .ToList();

                organizationConfigs.Add(new GitHubOrganization
                {
                    Name = gitHubOrganizationName,
                    Repositories = repositoryConfigs,
                    Users = organizationUsers,
                });
            }

            return new GitHubOrganizations
            {
                WhenObtained = DateTimeOffset.UtcNow,
                Organizations = organizationConfigs,
            };
        }

        private GitHubClient BuildGitHubClient() => BuildGitHubClient(_personalAccessToken);

        public static GitHubClient BuildGitHubClient(string personalAccessToken)
        {
            var client = new GitHubClient(new ProductHeaderValue("gitomega"));
            var tokenAuth = new Credentials(personalAccessToken);
            client.Credentials = tokenAuth;

            return client;
        }

        private void SaveConfigToDisk(GitHubOrganizations gitHubOrganizations, string directory, string filename)
        {
            var json = JsonConvert.SerializeObject(gitHubOrganizations, Formatting.Indented);
            var fullPath = Path.Combine(directory, filename);
            File.WriteAllText(fullPath, json);
        }
    }
}