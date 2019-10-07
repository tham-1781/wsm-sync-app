using Newtonsoft.Json;
using System.Collections.Generic;
using WSM.SynData.Models;

namespace WSM.SynData.Services
{
    public class WorkspaceService
    {
        public List<Workspace> GetWorkSpaces()
        {
            return JsonConvert.DeserializeObject<List<Workspace>>(Properties.Settings.Default.workspaces);
        }

        public void Update(List<Workspace> workspaces)
        {
            var serializedWorkspaces = JsonConvert.SerializeObject(workspaces);
            Properties.Settings.Default.workspaces = serializedWorkspaces;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        public List<Workspace> Add(List<Workspace> source)
        {
            var dataGridSource = new List<Workspace>();
            foreach (var workspace in source)
            {
                dataGridSource.Add(workspace);
            }
            dataGridSource.Add(new Workspace(Location.Danang, "", 0, MachineType.BlackNWhite));
            return dataGridSource;
        }

        public string ErrorMessagesDetails(List<Workspace> workspaces)
        {
            var errorMessages = "";
            int index = 1;
            foreach (var workspace in workspaces)
            {
                if (string.IsNullOrEmpty(workspace.ErrorMessages()))
                {
                    index++;
                    continue;
                }
                errorMessages += $"Line {index}: {workspace.ErrorMessages()}\n";
                index++;
            }
            return errorMessages;
        }
    }
}
