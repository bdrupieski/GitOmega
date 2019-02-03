using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using MoreLinq;

namespace GitOmega.Git.Analysis
{
    public class PatchDiffLineChangesRetriever
    {
        public IEnumerable<PatchDiffLineChanges> GetLineChangeCountsByFileExtensionAndDate(string localRepoPath, string branch)
        {
            var lineChanges = GetLineChangeCounts(localRepoPath, branch);

            return lineChanges.GroupByExtensionAndDate();
        }

        private IEnumerable<PatchDiffLineChanges> GetLineChangeCounts(string localRepoPath, string branch)
        {
            using (var repo = new Repository(localRepoPath))
            {
                if (!repo.Branches.Any())
                    yield break;

                var master = repo.Branches.First(x => x.CanonicalName.Contains(branch));

                var pairs = master.Commits.Pairwise((commit, parentCommit) => (Commit: commit, ParentCommit: parentCommit));

                foreach (var (commit, parentCommit) in pairs)
                {
                    Patch patch;

                    try
                    {
                        patch = repo.Diff.Compare<Patch>(parentCommit.Tree, commit.Tree);
                    }
                    catch (ArgumentException e) when (e.Message.StartsWith("An item with the same key has already been added"))
                    {
                        // Weird. Either a bug with libgit2sharp or a corrupt repository.
                        yield break;
                    }

                    foreach (var patchEntryChanges in patch)
                    {
                        var fileChange = new PatchDiffLineChanges
                        {
                            FileExtension = GetExtension(patchEntryChanges.Path),
                            Added = patchEntryChanges.LinesAdded,
                            Deleted = patchEntryChanges.LinesDeleted,
                            When = commit.Committer.When,
                        };

                        yield return fileChange;
                    }
                }
            }
        }

        private string GetExtension(string path)
        {
            try
            {
                var extension = Path.GetExtension(path);

                return !string.IsNullOrWhiteSpace(extension)
                    ? extension
                    : "unknown";
            }
            catch (ArgumentException e) when (e.Message == "Illegal characters in path.")
            {
                var periodIndex = path.LastIndexOf('.');
                return periodIndex >= 0
                    ? path.Substring(periodIndex)
                    : "unknown";
            }
        }
    }
}