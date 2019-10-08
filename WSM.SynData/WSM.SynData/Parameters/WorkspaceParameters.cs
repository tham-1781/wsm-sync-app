using System;
using System.Collections.Generic;
using WSM.SynData.Models;

namespace WSM.SynData.Parameters
{
    public class WorkspaceParameters
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public IEnumerable<Workspace> SelectedWorkspaces { get; set; }
        public Workspace Workspace { get; set; }
    }
}
