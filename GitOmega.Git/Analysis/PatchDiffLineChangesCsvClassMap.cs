using CsvHelper.Configuration;

namespace GitOmega.Git.Analysis
{
    public sealed class PatchDiffLineChangesCsvClassMap : ClassMap<PatchDiffLineChanges>
    {
        public PatchDiffLineChangesCsvClassMap()
        {
            Map(m => m.FileExtension);
            Map(m => m.Added);
            Map(m => m.Deleted);
            Map(m => m.AddedAndDeleted);
            Map(m => m.When).ConvertUsing(x => x.When.UtcDateTime.ToShortDateString());
        }
    }
}