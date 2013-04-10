using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Collections;
using System.Xml.Linq;
using System.IO;
using Microsoft.Build.BuildEngine;

namespace MSBuildTasks
{
    public class ToBuildTask : Task
    {
        private ITaskItem _solution;
        private ITaskItem[] _projectsFilesToBuild;

        [Required]
        public ITaskItem Solution
        {
            get { return _solution; }
            set { _solution = value; }
        }

        [Output]
        public ITaskItem[] ProjectsFilesToBuild
        {
            get
            {
                return _projectsFilesToBuild;
            }
        }

        public override bool Execute()
        {
            ArrayList items = new ArrayList();
            ArrayList configs = new ArrayList();
            
            // ItemSpec holds the filename or path of an Item
            string metaprojFile = _solution.ItemSpec + ".metaproj.filtered";

            var exists = false;
            
            // Check the metaproj file exists
            // otherwise warn it has to be emitted thanks to 'set MSBuildEmitSolution=1'

            if (!File.Exists(metaprojFile))
            {
                metaprojFile = _solution.ItemSpec + ".metaproj";

                if (!File.Exists(metaprojFile))
                {
                    Log.LogWarning("The metaproj file " +
                                    metaprojFile + " does not exist. You can emit it"
                                    + " by setting MSBuildEmitSolution to 1 while"
                                    + " calling MsBuild.");
                }
                else
                    exists = true;
            }
            else
                exists = true;
            
            if (exists)
            {
                try
                {
                    // Load metaproj file
                    XDocument metaproj = XDocument.Load(metaprojFile);

                    // Parse metaproj file
                    var projectsConfigurations = metaproj.Root
                        .Descendants("ProjectConfiguration"); // empty namespace

                    foreach (var projectConfiguration in projectsConfigurations)
                    {
                        if (projectConfiguration
                            .Attribute("BuildProjectInSolution").Value == "True")
                        {

                            Project project = new Project();
                            TaskItem item = new TaskItem();

                            item.ItemSpec = projectConfiguration
                                                .Attribute("AbsolutePath").Value;
                            
                            var configAndPlatform = projectConfiguration.Value;

                            if (configAndPlatform == null)
                                continue;

                            var configSplitted = configAndPlatform.Split('|');

                            if (configSplitted.Length == 0)
                                continue;

                            project.Load(item.ItemSpec);

                            item.SetMetadata("Configuration", configSplitted[0]);
                            item.SetMetadata("Platform", configSplitted[1]);

                            items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError("Error trying to load metaproj file " +
                                    metaprojFile + ". " + ex.Message);
                }
            }

            // Populate the "ProjectsFilesToBuild" output items
            _projectsFilesToBuild = (ITaskItem[])items
                                                    .ToArray(typeof(ITaskItem));

            // Log.HasLoggedErrors is true if the task logged any errors -- even 
            // if they were logged from a task's constructor or property setter.
            // As long as this task is written to always log an error when it
            // fails, we can reliably return HasLoggedErrors.
            return !Log.HasLoggedErrors;
        }
    }
}
