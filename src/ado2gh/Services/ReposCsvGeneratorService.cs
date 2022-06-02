﻿using System;
using System.Text;
using System.Threading.Tasks;

namespace OctoshiftCLI.AdoToGithub
{
    public class ReposCsvGeneratorService
    {
        private readonly AdoInspectorServiceFactory _adoInspectorServiceFactory;
        private readonly AdoApiFactory _adoApiFactory;

        public ReposCsvGeneratorService(AdoInspectorServiceFactory adoInspectorServiceFactory, AdoApiFactory adoApiFactory)
        {
            _adoInspectorServiceFactory = adoInspectorServiceFactory;
            _adoApiFactory = adoApiFactory;
        }

        public virtual async Task<string> Generate(string adoPat)
        {
            var adoApi = _adoApiFactory.Create(adoPat);
            var inspector = _adoInspectorServiceFactory.Create(adoApi);
            var result = new StringBuilder();

            result.AppendLine("org,teamproject,repo,url,pipeline-count,pr-count");

            foreach (var org in await inspector.GetOrgs())
            {
                foreach (var teamProject in await inspector.GetTeamProjects(org))
                {
                    foreach (var repo in await inspector.GetRepos(org, teamProject))
                    {
                        var url = $"https://dev.azure.com/{Uri.EscapeDataString(org)}/{Uri.EscapeDataString(teamProject)}/_git/{Uri.EscapeDataString(repo)}";
                        var pipelineCount = await inspector.GetPipelineCount(org, teamProject, repo);
                        var prCount = await inspector.GetPullRequestCount(org, teamProject, repo);

                        result.AppendLine($"\"{org}\",\"{teamProject}\",\"{repo}\",\"{url}\",{pipelineCount},{prCount}");
                    }
                }
            }

            return result.ToString();
        }
    }
}