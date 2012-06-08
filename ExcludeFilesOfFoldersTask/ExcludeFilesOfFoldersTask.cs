using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Collections;

namespace MSBuildTasks
{
    public class ExcludeFilesOfFoldersTask : Task
    {
        private ITaskItem[] _files;
        private ITaskItem[] _folders;
        private ITaskItem[] _excludedFiles;

        [Required]
        public ITaskItem[] Files
        {
            get { return _files; }
            set { _files = value; }
        }

        [Required]
        public ITaskItem[] Folders
        {
            get { return _folders; }
            set { _folders = value; }
        }

        [Output]
        public ITaskItem[] ExcludedFiles
        {
            get
            {
                return _excludedFiles;
            }
        }

        public override bool Execute()
        {
            ArrayList items = new ArrayList();
            bool inFolder = false;

            foreach (var file in _files)
            {
                inFolder = false;
                
                try
                {
                    // ItemSpec holds the filename or path of an Item
                    foreach (var folder in _folders)
                    {
                        if (file.ItemSpec.Contains("\\" + folder.ItemSpec + "\\")
                            || file.ItemSpec.StartsWith(folder.ItemSpec + "\\"))
                        {
                            inFolder = true;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError("Error trying to figure out if '" +
                            file.ItemSpec + "' is in the folder. " + ex.Message);
                    continue;
                }

                if (inFolder)
                {
                    items.Add(file);

                    Log.LogMessage(MessageImportance.Normal, "File \""
                        + file.ItemSpec + "\" was found in a folder provided.");
                }
                else
                {
                    Log.LogMessage(MessageImportance.Normal, "File \""
                        + file.ItemSpec + "\" is in any folder provided.");
                }
            }

            // Populate the "ExcludedFiles" output items
            _excludedFiles = (ITaskItem[])items.ToArray(typeof(ITaskItem));

            // Log.HasLoggedErrors is true if the task logged any errors -- even 
            // if they were logged from a task's constructor or property setter.
            // As long as this task is written to always log an error when it
            // fails, we can reliably return HasLoggedErrors.
            return !Log.HasLoggedErrors;
        }
    }
}
