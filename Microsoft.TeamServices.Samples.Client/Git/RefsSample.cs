﻿using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.TeamServices.Samples.Client.Git
{
    [ClientSample(GitWebApiConstants.AreaName, "refs")]
    public class RefsSample : ClientSample
    {
        [ClientSampleMethod]
        public IEnumerable<GitRef> ListBranches()
        {
            VssConnection connection = this.Context.Connection;
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();

            TeamProjectReference project = ClientSampleHelpers.FindAnyProject(this.Context);
            GitRepository repo = GitSampleHelpers.FindAnyRepository(this.Context, project.Id);

            List<GitRef> refs = gitClient.GetRefsAsync(repo.Id, filter: "heads/").Result;

            Console.WriteLine("project {0}, repo {1}", project.Name, repo.Name);
            foreach(GitRef gitRef in refs)
            {
                Console.WriteLine("{0} {1} {2}", gitRef.Name, gitRef.ObjectId, gitRef.Url);
            }

            return refs;            
        }

        [ClientSampleMethod]
        public string CreateBranch()
        {
            VssConnection connection = this.Context.Connection;
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();

            // find a project, repo, and source ref to branch from
            TeamProjectReference project = ClientSampleHelpers.FindAnyProject(this.Context);
            GitRepository repo = GitSampleHelpers.FindAnyRepository(this.Context, project.Id);
            string defaultBranch = GetDefaultBranchName(repo);
            GitRef sourceRef = gitClient.GetRefsAsync(repo.Id, filter: defaultBranch).Result.First();

            // create a new branch from the source
            GitRefUpdateResult refUpdateResult = gitClient.UpdateRefsAsync(
                new GitRefUpdate[] { new GitRefUpdate() {
                    OldObjectId = new string('0', 40),
                    NewObjectId = sourceRef.ObjectId,
                    Name = $"refs/heads/vsts-api-sample/{GitSampleHelpers.ChooseRefsafeName()}",
                } },
                repositoryId: repo.Id).Result.First();

            Console.WriteLine("project {0}, repo {1}, source branch {2}", project.Name, repo.Name, sourceRef.Name);
            Console.WriteLine("new branch {0} (success={1} status={2})", refUpdateResult.Name, refUpdateResult.Success, refUpdateResult.UpdateStatus);

            return refUpdateResult.Name;
        }

        private static string GetDefaultBranchName(GitRepository repo)
        {
            if (!repo.DefaultBranch.StartsWith("refs/"))
            {
                throw new Exception("The branch name should have started with 'refs/' but it didn't.");
            }
            string defaultBranch = repo.DefaultBranch.Remove(0, "refs/".Length);
            return defaultBranch;
        }
    }
}
