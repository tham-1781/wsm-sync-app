using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using WSM.SynData.Models;
using WSM.SynData.ViewModels;

namespace WSM.SynData.Services
{
    public class WorkspaceService
    {
        public List<Workspace> GetWorkspaces()
        {
            return JsonConvert.DeserializeObject<List<Workspace>>(Properties.Settings.Default.workspaces);
        }

        public List<WorkspaceVm> WorkspaceVmList()
        {
            var workspaceVmList = GetWorkspaces().Select(wsp => wsp.ToWorkspaceVm());
            return workspaceVmList.ToList();
        }

        public void Update(List<Workspace> workspaces)
        {
            var serializedWorkspaces = JsonConvert.SerializeObject(workspaces);
            Properties.Settings.Default.workspaces = serializedWorkspaces;
            SettingService.Commit(showAlert: false);
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

        public List<Workspace> Remove(Workspace workspace, List<Workspace> source)
        {
            source.Remove(workspace);
            var dataGridSource = new List<Workspace>();
            foreach (var item in source)
            {
                dataGridSource.Add(item);
            }
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
