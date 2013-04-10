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
    public class FilesWhichEndWithTask : Task
    {
        private ITaskItem[] _files;
        private ITaskItem[] _endWith;
        private ITaskItem[] _filesWhichEndWith;

        [Required]
        public ITaskItem[] Files
        {
            get { return _files; }
            set { _files = value; }
        }

        [Required]
        public ITaskItem[] EndWith
        {
            get { return _endWith; }
            set { _endWith = value; }
        }

        [Output]
        public ITaskItem[] FilesWhichEndWith
        {
            get
            {
                return _filesWhichEndWith;
            }
        }

        public override bool Execute()
        {
            ArrayList items = new ArrayList();

            foreach (var file in _files)
            {
                foreach (var endWith in _endWith)
                {
                    if (file.ItemSpec.EndsWith(endWith.ItemSpec))
                    {
                        items.Add(file);
                        break;
                    }
                }
            }

            // Populate the "ProjectsFilesToBuild" output items
            _filesWhichEndWith = (ITaskItem[])items
                                                    .ToArray(typeof(ITaskItem));

            // Log.HasLoggedErrors is true if the task logged any errors -- even 
            // if they were logged from a task's constructor or property setter.
            // As long as this task is written to always log an error when it
            // fails, we can reliably return HasLoggedErrors.
            return !Log.HasLoggedErrors;
        }
    }
}
