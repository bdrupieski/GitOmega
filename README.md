# GitOmega

This analyzes by file extension the activity of the Git repositories in a set of GitHub organizations.

Maybe it'll be more over time, but that's all it is right now.

## Why?

I was curious about technology trends and usage at my company where multiple languages and tech
stacks are used. I wanted to get an idea of how much work is actually happening on each stack to
see if it lines up with where we're trying to go.

There's a product called GitPrime that offers similar metrics, but as far as I know doesn't provide
this kind of information. It would be more fitting if GitPrime was called GitAlpha, but that's where
the name of this repository comes from. I would've named it "Git Activity Analyzer" or something like
that, but all the straightforward names are already token.

## Usage

Pull down the project and change the hard-coded GitHub personal access token and organization names
in GitOmega.Runner.Program.cs. 

You can also pass the path of a JSON file as the first command line argument to the app if you want
to completely eliminate the risk of accidentally committing a GitHub personal access token anywhere. 
The JSON file should look like this:

```json
{
    "LocalRootDirectoryForBareGitRepositories": "path/to/some/temp/working/space",
    "GitHubPersonalAccessToken": "your GitHub personal access token",
    "GitHubOrganizationNames": ["orgname1", "orgname2"]
}
```

## How?

This clones all repositories in the configured GitHub organizations to your local machine. It clones
them as bare repositories that have no working directory in order to save space. It uses the GitHub
API to do this. If they already exist locally it'll fetch any updates.

Once all the repositories are cloned it uses libgit2 to run a patch diff between every pair of
commits on the repository's default branch. This is likely either master or develop. It doesn't
take into account feature branches. In order to count as activity, the changes have to hit that
main default branch.

From each patch diff it records the number of lines added and deleted for each file extension.
These are grouped up by day across all repositories and dumped into a CSV file. In order to do
any kind of comparison or visualization of the results you'll need to work with that file in
a spreadsheet program.