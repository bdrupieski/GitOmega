using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace GitOmega.Git
{
    public class LocalGitRepositoryUpdater
    {
        private readonly UsernamePasswordCredentials _usernamePasswordCredentials;

        public LocalGitRepositoryUpdater(string gitHubPersonalAccessToken)
        {
            _usernamePasswordCredentials = new UsernamePasswordCredentials
            {
                Username = gitHubPersonalAccessToken,
                Password = string.Empty,
            };
        }

        public LocalGitRepositoryUpdater(UsernamePasswordCredentials usernamePasswordCredentials)
        {
            _usernamePasswordCredentials = usernamePasswordCredentials;
        }

        public void Synchronize(string localRepositoryDirectory, string repositoryCloneUrl)
        {
            MakeSureDirectoryIsThereAndValid(localRepositoryDirectory);

            if (IsBareGitRepo(localRepositoryDirectory))
                Fetch(localRepositoryDirectory);
            else
                Clone(repositoryCloneUrl, localRepositoryDirectory);
        }

        private void MakeSureDirectoryIsThereAndValid(string directoryPath)
        {
            CreateDirectoryIfItDoesntExist(directoryPath);

            if (!IsBareGitRepo(directoryPath))
                DeleteAndRecreateDirectory(directoryPath);
        }

        private void CreateDirectoryIfItDoesntExist(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        private bool IsBareGitRepo(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return false;

            // It's probably a bare git repository if there's a file named "HEAD".
            var files = Directory.GetFiles(directoryPath, "HEAD", SearchOption.TopDirectoryOnly);

            return files.Any();
        }

        private void DeleteAndRecreateDirectory(string directoryPath)
        {
            Directory.Delete(directoryPath, true);
            Directory.CreateDirectory(directoryPath);
        }

        private void Fetch(string localRepoPath)
        {
            using (var repo = new Repository(localRepoPath))
            {
                var fetchOptions = new FetchOptions();
                fetchOptions.CredentialsProvider = (url, usernameFromUrl, types) => _usernamePasswordCredentials;
                fetchOptions.TagFetchMode = TagFetchMode.None;
                fetchOptions.Prune = true;

                var remote = repo.Network.Remotes["origin"];
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification).ToArray();
                Commands.Fetch(repo, remote.Name, refSpecs, fetchOptions, string.Empty);
            }
        }

        private void Clone(string gitUrl, string localRepoPath)
        {
            var cloneOptions = new CloneOptions();
            cloneOptions.IsBare = true;
            cloneOptions.CredentialsProvider = (url, fromUrl, types) => _usernamePasswordCredentials;
            cloneOptions.RecurseSubmodules = false;

            Repository.Clone(gitUrl, localRepoPath, cloneOptions);
        }
    }
}