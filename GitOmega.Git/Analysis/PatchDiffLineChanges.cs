using System;

namespace GitOmega.Git.Analysis
{
    public class PatchDiffLineChanges
    {
        public string FileExtension { get; set; }
        public int Added { get; set; }
        public int Deleted { get; set; }
        public int AddedAndDeleted => Added + Deleted;
        public DateTimeOffset When { get; set; }
    }
}