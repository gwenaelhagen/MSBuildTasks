using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.BuildEngine;
using System.Xml;

namespace MSBuildTasks
{
    public class FilterMetaprojFileTask : Task
    {
        private ITaskItem _source;
        private string _currentSolutionConfigurationContents;
        private bool _save;

        [Required]
        public ITaskItem Source
        {
            get { return _source; }
            set { _source = value; }
        }

        public bool Save
        {
            get { return _save; }
            set { _save = value; }
        }

        [Output]
        public string CurrentSolutionConfigurationContents
        {
            get { return _currentSolutionConfigurationContents; }
            set { _currentSolutionConfigurationContents = value; }
        }

        public override bool Execute()
        {
            string metaprojFile = _source.ToString();
            // Check the metaproj file exists
            // otherwise warn it has to be emitted thanks to 'set MSBuildEmitSolution=1'

            if (!File.Exists(metaprojFile))
            {
                Log.LogWarning("The metaproj file " +
                                metaprojFile + " does not exist. You can emit it"
                                + " by setting MSBuildEmitSolution to 1 while"
                                + " calling MsBuild.");
            }
            else
            {
                try
                {
                    // Load metaproj file
                    XDocument metaproj = XDocument.Load(metaprojFile);
                    XNamespace xmlns = metaproj.Root.Attribute("xmlns").Value;

                    // Parse metaproj file
                    var currentSolutionConfigurationContents = "CurrentSolutionConfigurationContents";
                    var currentSolutionConfigurationContentsElements = metaproj.Root.Descendants(xmlns
                        + currentSolutionConfigurationContents);

                    if (currentSolutionConfigurationContentsElements.Count() == 0)
                        throw new Exception();

                    var solutionConfiguration =
                        currentSolutionConfigurationContentsElements.First().FirstNode;

                    if (solutionConfiguration == null)
                        throw new Exception();
                    
                    _currentSolutionConfigurationContents =
                        solutionConfiguration.ToString(SaveOptions.OmitDuplicateNamespaces);

                    if (_save)
                    {
                        Project project = new Project();
                        project.DefaultToolsVersion = "4.0";

                        var propertyGroup = project.AddNewPropertyGroup(false);
                        propertyGroup.AddNewProperty(currentSolutionConfigurationContents,
                            _currentSolutionConfigurationContents);

                        project.Save(metaprojFile + ".filtered", System.Text.Encoding.UTF8);
                    }
                    
                }
                catch (Exception ex)
                {
                    Log.LogError("Error trying to load metaproj file " +
                                    metaprojFile + ". " + ex.Message);
                }
            }

            // Log.HasLoggedErrors is true if the task logged any errors -- even 
            // if they were logged from a task's constructor or property setter.
            // As long as this task is written to always log an error when it
            // fails, we can reliably return HasLoggedErrors.
            return !Log.HasLoggedErrors;
        }
    }
}