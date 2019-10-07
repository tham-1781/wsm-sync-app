using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WSM.SynData.Models;
using WSM.SynData.Services;

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
            grid.ItemsSource = _workspaceService.GetWorkSpaces();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var workspaces = (List<Workspace>)gridWorkspaces.ItemsSource;
            if (!string.IsNullOrEmpty(_workspaceService.ErrorMessagesDetails(workspaces)))
            {
                MessageBox.Show(_workspaceService.ErrorMessagesDetails(workspaces), "WSM", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                _workspaceService.Update(workspaces);
                MessageBox.Show("Workspace data successfully updated", "WSM", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddWorkspace_Click(object sender, RoutedEventArgs e)
        {
            gridWorkspaces.ItemsSource = _workspaceService.Add((List<Workspace>)gridWorkspaces.ItemsSource);
        }
    }
}
