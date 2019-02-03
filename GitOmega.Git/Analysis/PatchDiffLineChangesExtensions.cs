using System.Collections.Generic;
using System.Linq;

namespace GitOmega.Git.Analysis
{
    public static class PatchDiffLineChangesExtensions
    {
        public static IEnumerable<PatchDiffLineChanges> GroupByExtensionAndDate(this IEnumerable<PatchDiffLineChanges> fileExtensionChanges)
        {
            foreach (var changesForExtension in fileExtensionChanges.GroupBy(lineChange => lineChange.FileExtension))
            {
                foreach (var changesForExtensionOnDay in changesForExtension.GroupBy(lineChange => lineChange.When.UtcDateTime.Date))
                {
                    yield return new PatchDiffLineChanges
                    {
                        When = changesForExtensionOnDay.Key,
                        Added = changesForExtensionOnDay.Sum(x => x.Added),
                        Deleted = changesForExtensionOnDay.Sum(x => x.Deleted),
                        FileExtension = changesForExtension.Key,
                    };
                }
            }
        }
    }
}