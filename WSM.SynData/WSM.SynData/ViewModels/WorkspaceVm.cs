using WSM.SynData.Models;

namespace WSM.SynData.ViewModels
{
    public class WorkspaceVm
    {
        public Location local { get; set; }
        public string attMachineIp { get; set; }

        public int attMachinePort { get; set; }
                
        public MachineType attMachineType { get; set; }
        public string Note { get; set; }

        public Workspace ToWorkspace()
        {
            return new Workspace()
            {
                local = this.local,
                attMachineIp = this.attMachineIp,
                attMachinePort = this.attMachinePort,
                attMachineType = this.attMachineType,
                Note = this.Note,
            };
        }
    }
}
