using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.BuildEngine;
using System.IO;
using System.Security;
using System.Collections;

namespace MSBuildTasks
{
    public class HaveTargetsTask : Task
    {
        private ITaskItem[] _projectFiles;
        private ITaskItem[] _targets;
        private ITaskItem[] _projectsFilesWhichHaveTargets;
        private bool _skipNonexistentProjects;

        [Required]
        public ITaskItem[] ProjectFiles
        {
            get { return _projectFiles; }
            set { _projectFiles = value; }
        }
        
        [Required]
        public ITaskItem[] Targets
        {
            get { return _targets; }
            set { _targets = value; }
        }

        public bool SkipNonexistentProjects
        {
            get { return _skipNonexistentProjects; }
            set { _skipNonexistentProjects = value; }
        }

        [Output]
        public ITaskItem[] ProjectsFilesWhichHaveTargets
        {
            get
            {
                return _projectsFilesWhichHaveTargets;
            }
        }
        
        public override bool Execute()
        {
            ArrayList items = new ArrayList();
            Project project = new Project();
            bool haveTargets = true;
            bool nonexistentProject = false;

            foreach (var projectFile in ProjectFiles)
            {
                haveTargets = false;
                nonexistentProject = false;
                
                // ItemSpec holds the filename or path of an Item
                if (projectFile.ItemSpec.Length > 0)
                {
                    try
                    {
                        project.Load(projectFile.ItemSpec);

                        haveTargets = true;

                        foreach (var targetName in Targets)
                        {
                            if (!project.Targets.Exists(targetName.ItemSpec))
                            {
                                haveTargets = false;
                                break;
                            }
                        }
                    }
                    // If a projectFile fails to be loaded, log an error, but
                    // proceed with the remaining projectsFiles
                    catch (Exception ex)
                    {
                        if (ex is InvalidProjectFileException
                            || ex is ArgumentException)
                        {
                            nonexistentProject = true;

                            if (!_skipNonexistentProjects)
                            {
                                Log.LogError("Error trying to load project file "
                                    + projectFile.ItemSpec + ". " + ex.Message);
                            }
                        }
                        else
                        {
                            Log.LogError("Error trying to load project file " +
                                projectFile.ItemSpec + ". " + ex.Message);
                        }
                        continue;
                    }
                }

                if (haveTargets)
                {
                    items.Add(projectFile);

                    Log.LogMessage(MessageImportance.Normal, "Project \""
                        + projectFile.ItemSpec + "\" contains all "
                        + "targets specified");
                }
                else if (!nonexistentProject 
                    || (nonexistentProject && !_skipNonexistentProjects))
                {
                    Log.LogMessage(MessageImportance.Normal, "Project \""
                        + projectFile.ItemSpec + "\" does not contain "
                        + "all targets specified");
                }
            }

            // Populate the "ProjectsFilesWhichHaveTargets" output items
            _projectsFilesWhichHaveTargets = (ITaskItem[])items
                                                    .ToArray(typeof(ITaskItem));

            // Log.HasLoggedErrors is true if the task logged any errors -- even 
            // if they were logged from a task's constructor or property setter.
            // As long as this task is written to always log an error when it
            // fails, we can reliably return HasLoggedErrors.
            return !Log.HasLoggedErrors;
        }
    }
}
