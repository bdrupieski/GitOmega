# GitOmega

This analyzes by file extension the activity of the Git repositories in a set of GitHub organizations.
It tries to give you an idea of where the most activity is happening by programming language. For
example, by pointing out that most commits and line changes by volume in your organization's git
repositories are in .py files instead of .cpp files.

Maybe it'll be more over time, but that's all it is right now.

## Why?

I was curious about technology trends and usage at my company where multiple languages and tech
stacks are used. I wanted to get an idea of how much work is actually happening on each stack to
see if it lines up with where we're trying to go.

There's a product called GitPrime targeted to engineering managers that provides metrics on the 
contributions of _people_ based on their activity in an organization's git repisitories. As far 
as I know it doesn't provide information on activity by programming language, tech stack, or the 
type of work being done. For example, it doesn't currently tell you things like which languages 
people are using, what applications they're working on, or how much test code is being written 
vs. application code.

Measuring people can be useful to engineering managers managing an engineering organization.
But, I think measuring code and technology use across an organization can be useful to senior
engineers and architects who are trying to manage the technical direction and platform choices
of an engineering organization. That's where this tool could fit in.

I named it GitOmega as a foil to GitPrime, though it would be more fitting if GitPrime was called
GitAlpha. I would've named it "Git Activity Analyzer" or something like that, but all the 
straightforward names were already token.

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

This clones all repositories in the configured GitHub organizations to your local machine as
bare repositories. It clones them as bare repositories that have no working directory in order 
to save time and space. It uses the GitHub API to do this. If they already exist locally it'll 
fetch any updates.

Once all the repositories are cloned it uses libgit2 to run a patch diff between every pair of
commits on the repository's default branch. This is likely either master or develop. It doesn't
take into account feature branches. In order to count as activity, the changes have to hit that
main default branch. 

Diffing all of the commits can take a while. If your GitHub organization has over 200 
repositories with hundreds of thousands of commits across several million lines of code, it can 
take about an hour or two to run on a June 2017 MacBook.

From each patch diff it records the number of lines added and deleted for each file extension.
These are grouped up by day across all repositories and dumped into a CSV file. In order to do
any kind of comparison or visualization of the results you'll need to work with that file in
a spreadsheet program.

It wouldn't be too much more work to break it down by individual in addition to file extension,
so that in addition to _what_ is being worked on, you can see _who_ is doing it. Though, depending
on the size of your organization that can turn into a pretty big CSV that might be too unwieldy for
a spreadsheet.