using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WSM.SynData.Models;
using WSM.SynData.Services;
using WSM.SynData.ViewModels;

namespace WSM.SynData.Pages
{
    public partial class Workspaces : Page
    {
        private readonly WorkspaceService _workspaceService;
        public Workspaces()
        {
            InitializeComponent();
            _workspaceService = new WorkspaceService();
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as DataGrid;
            grid.ItemsSource = _workspaceService.WorkspaceVmList();
            CustomWorkspaceHeader();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var workspaces = GetWorkspaces();
            if (!string.IsNullOrEmpty(_workspaceService.ErrorMessagesDetails(workspaces)))
            {
                MessageBox.Show(_workspaceService.ErrorMessagesDetails(workspaces), "WSM", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                _workspaceService.Update(workspaces);
                MessageBox.Show("Workspace data successfully updated", "WSM", MessageBoxButton.OK, MessageBoxImage.Information);
                gridWorkspaces.ItemsSource = _workspaceService.WorkspaceVmList();
            }
        }

        private void AddWorkspace_Click(object sender, RoutedEventArgs e)
        {
            var workspaces = _workspaceService.Add(GetWorkspaces());
            gridWorkspaces.ItemsSource = workspaces.Select(wsp => wsp.ToWorkspaceVm()).ToList();
        }

        private void DeleteWorkspace_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = gridWorkspaces.SelectedItem;
            if (selectedItem != null)
            {
                var workspaceVm = (WorkspaceVm)selectedItem;
                gridWorkspaces.ItemsSource = _workspaceService.Remove(workspaceVm.ToWorkspace(), GetWorkspaces());
            }
            else
            {
                MessageBox.Show("No item selected", "WSM", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CustomWorkspaceHeader()
        {
            string[] columns = new string[5] { "Location", "Machine IP", "Machine Port", "Machine Type", "Note" };
            for (int i = 0; i < columns.Length; i++)
            {
                gridWorkspaces.Columns[i].Header = columns[i];
            }
        }

        private List<Workspace> GetWorkspaces()
        {
            var workspaceVmList = (List<WorkspaceVm>)gridWorkspaces.ItemsSource;
            var workspaces = workspaceVmList.Select(item => item.ToWorkspace()).ToList();
            return workspaces;
        }
    }
}
