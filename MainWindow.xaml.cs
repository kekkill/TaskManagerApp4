using TaskManagerApp.Resources;
using MahApps.Metro.Controls;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using TaskManagerApp.Data;
using TaskManagerApp.Models;
using TaskManagerApp.Services;

namespace TaskManagerApp
{
    public partial class MainWindow : MetroWindow
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private User? _currentUser;

        public MainWindow(User currentUser)
        {
            InitializeComponent();

            _currentUser = currentUser;
            CurrentUser.Instance = _currentUser;

            _context = new AppDbContext();
            _context.Database.Migrate();

            LoadProcesses();
            LoadProcessesComboBox();
            SetupInterfaceByRole();
        }

        private void SetupInterfaceByRole()
        {
            EditProcessButton.Visibility = _currentUser?.Role == "Admin"
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (_currentUser?.Role == "Worker")
            {
                HideAssignedToColumn();
            }
            else
            {
                ShowAssignedToColumn();
            }
        }

        private void HideAssignedToColumn()
        {
            var column = TasksDataGrid.Columns.FirstOrDefault(c =>
                c.Header?.ToString() == "Исполнитель" ||
                c.Header?.ToString() == "AssignedToUser");

            if (column != null)
                column.Visibility = Visibility.Collapsed;
        }

        private void ShowAssignedToColumn()
        {
            var column = TasksDataGrid.Columns.FirstOrDefault(c =>
                c.Header?.ToString() == "Исполнитель" ||
                c.Header?.ToString() == "AssignedToUser");

            if (column != null)
                column.Visibility = Visibility.Visible;
        }

        private void LoadProcesses()
        {
            try
            {
                var processes = _context.Processes
                    .Include(p => p.Tasks)
                    .OrderBy(p => p.Name)
                    .ToList();

                ProcessesDataGrid.ItemsSource = processes;

                if (processes.Any())
                {
                    ProcessesDataGrid.SelectedIndex = 0;
                    EditProcessButton.IsEnabled = true;
                }
                else
                {
                    EditProcessButton.IsEnabled = false;
                    TasksDataGrid.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                ShowError(Strings.MainWindow_ErrorTitle, ex.Message);
            }
        }

        private void LoadProcessesComboBox()
        {
            try
            {
                var processes = _context.Processes
                    .OrderBy(p => p.Name)
                    .ToList();

                ProcessComboBox.ItemsSource = processes;
            }
            catch (Exception ex)
            {
                ShowError(Strings.MainWindow_ErrorTitle, ex.Message);
            }
        }

        private void LoadTasksForProcess(int processId)
        {
            try
            {
                var query = _context.Tasks
                    .Include(t => t.AssignedToUser)
                    .Where(t => t.ProcessId == processId);

                if (_currentUser?.Role == "Worker")
                {
                    query = query.Where(t => t.AssignedToUserId == _currentUser.Id);
                }

                var tasks = query.ToList();

                if (_currentUser?.Role == "Worker")
                {
                    tasks = tasks.Where(t => t.AssignedToUserId == _currentUser.Id).ToList();
                    TotalTasksText.Text = string.Format(Strings.MainWindow_YourTasksCount, tasks.Count);
                }
                else
                {
                    TotalTasksText.Text = tasks.Count.ToString();
                }

                TasksDataGrid.ItemsSource = tasks;
                AddTaskButton.IsEnabled = true;
                EditTaskButton.IsEnabled = tasks.Any();
            }
            catch (Exception ex)
            {
                ShowError(Strings.MainWindow_ErrorTitle, ex.Message);
            }
        }

        private void ShowSuccess(string message)
        {
            MessageBox.Show(message, Strings.MainWindow_SuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowError(string title, string message)
        {
            MessageBox.Show(message, Strings.MainWindow_ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowWarning(string message)
        {
            MessageBox.Show(message, Strings.MainWindow_WarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            LoadProcesses();
            ShowSuccess(Strings.MainWindow_DataUpdated);
        }

        private void AddProcess_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser?.Role != "Admin")
            {
                ShowWarning(Strings.MainWindow_AdminOnlyAddProcesses);
                return;
            }

            var dialog = new InputDialog(Strings.MainWindow_AddProcessTitle, Strings.MainWindow_AddProcessLabel);
            if (dialog.ShowDialog() == true)
            {
                var processName = dialog.InputText;
                if (string.IsNullOrWhiteSpace(processName))
                {
                    ShowWarning(Strings.MainWindow_ProcessNameCannotBeEmpty);
                    return;
                }

                var newProcess = new Process
                {
                    Name = processName,
                    OwnerUserId = _currentUser.Id,
                    CreatedAt = DateTime.Now
                };

                _context.Processes.Add(newProcess);
                _context.SaveChanges();

                LoadProcesses();
                LoadProcessesComboBox();
                ShowSuccess(string.Format(Strings.MainWindow_ProcessAddedSuccess, processName));
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessComboBox.SelectedItem is not Process selectedProcess)
            {
                ShowWarning(Strings.MainWindow_SelectProcessFirst);
                return;
            }

            var dialog = new InputDialog(Strings.MainWindow_AddTaskTitle, Strings.MainWindow_AddTaskLabel, _currentUser, forTask: true);

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                var newTask = new TaskItem
                {
                    ProcessId = selectedProcess.Id,
                    TaskName = dialog.InputText,
                    AssignedToUserId = dialog.SelectedWorkerId,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                if (_currentUser?.Role == "Worker")
                {
                    newTask.AssignedToUserId = _currentUser.Id;
                }

                _context.Tasks.Add(newTask);
                _context.SaveChanges();

                LoadTasksForProcess(selectedProcess.Id);
                ShowSuccess(string.Format(Strings.MainWindow_TaskAddedSuccess, dialog.InputText));
            }
        }

        private void EditProcess_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser?.Role != "Admin")
            {
                ShowWarning(Strings.MainWindow_AdminOnlyEditProcesses);
                return;
            }

            if (ProcessesDataGrid.SelectedItem is not Process selectedProcess) return;

            var dialog = new InputDialog(Strings.MainWindow_EditProcessTitle, Strings.MainWindow_EditProcessLabel);
            dialog.InputTextBox.Text = selectedProcess.Name;

            if (dialog.ShowDialog() == true)
            {
                var newName = dialog.InputText;
                if (string.IsNullOrWhiteSpace(newName))
                {
                    ShowWarning(Strings.MainWindow_ProcessNameCannotBeEmpty);
                    return;
                }

                selectedProcess.Name = newName;
                selectedProcess.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                LoadProcesses();
                LoadProcessesComboBox();
                ShowSuccess(Strings.MainWindow_ProcessUpdatedSuccess);
            }
        }

        private void EditProcessFromGrid_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser?.Role != "Admin")
            {
                ShowWarning(Strings.MainWindow_AdminOnlyEditProcesses);
                return;
            }

            if (sender is not System.Windows.Controls.Button button ||
                button.Tag is not int processId) return;

            var process = _context.Processes.Find(processId);
            if (process == null) return;

            ProcessesDataGrid.SelectedItem = process;
            EditProcess_Click(sender, e);
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (TasksDataGrid.SelectedItem is not TaskItem selectedTask) return;

            if (_currentUser?.Role == "Worker" &&
                selectedTask.AssignedToUserId != _currentUser.Id)
            {
                ShowWarning(Strings.MainWindow_WorkerCanEditOnlyOwnTasks);
                return;
            }

            bool canChangeAssignee = (_currentUser?.Role == "Admin" || _currentUser?.Role == "Manager");
            var dialog = new InputDialog(
                Strings.MainWindow_EditTaskTitle,
                Strings.MainWindow_EditTaskLabel,
                _currentUser,
                forTask: canChangeAssignee
            );

            dialog.InputTextBox.Text = selectedTask.TaskName;

            if (dialog.ShowDialog() == true)
            {
                var newName = dialog.InputText;
                if (string.IsNullOrWhiteSpace(newName))
                {
                    ShowWarning(Strings.MainWindow_TaskNameCannotBeEmpty);
                    return;
                }

                selectedTask.TaskName = newName;
                if (canChangeAssignee)
                {
                    selectedTask.AssignedToUserId = dialog.SelectedWorkerId;
                }
                _context.SaveChanges();
                if (ProcessComboBox.SelectedItem is Process selectedProcess)
                {
                    LoadTasksForProcess(selectedProcess.Id);
                }
                ShowSuccess(Strings.MainWindow_TaskUpdatedSuccess);
            }
        }

        private void EditTaskFromGrid_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button ||
                button.Tag is not int taskId) return;

            var task = _context.Tasks.Find(taskId);
            if (task == null) return;

            TasksDataGrid.SelectedItem = task;
            EditTask_Click(sender, e);
        }

        private void DeleteProcess_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser?.Role != "Admin")
            {
                ShowWarning(Strings.MainWindow_AdminOnlyEditProcesses);
                return;
            }

            if (sender is not System.Windows.Controls.Button button ||
                button.Tag is not int processId)
                return;

            var process = _context.Processes.Include(p => p.Tasks).FirstOrDefault(p => p.Id == processId);
            if (process == null) return;

            if (MessageBox.Show(string.Format(Strings.MainWindow_DeleteProcessConfirmation, process.Name), Strings.MainWindow_ConfirmationTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _context.Processes.Remove(process);
                _context.SaveChanges();

                LoadProcesses();
                LoadProcessesComboBox();
                ShowSuccess(Strings.MainWindow_ProcessDeletedSuccess);
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button ||
                button.Tag is not int taskId)
                return;

            var task = _context.Tasks.Find(taskId);
            if (task == null) return;

            if (_currentUser?.Role == "Worker" &&
                task.AssignedToUserId != _currentUser.Id)
            {
                ShowWarning(Strings.MainWindow_WorkerCanDeleteOnlyOwnTasks);
                return;
            }

            if (MessageBox.Show(string.Format(Strings.MainWindow_DeleteTaskConfirmation, task.TaskName), Strings.MainWindow_ConfirmationTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _context.Tasks.Remove(task);
                _context.SaveChanges();

                if (ProcessComboBox.SelectedItem is Process selectedProcess)
                {
                    LoadTasksForProcess(selectedProcess.Id);
                }

                ShowSuccess(Strings.MainWindow_TaskDeletedSuccess);
            }
        }

        private void ProcessesDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProcessesDataGrid.SelectedItem is Process selectedProcess)
            {
                EditProcessButton.IsEnabled = true;
            }
            else
            {
                EditProcessButton.IsEnabled = false;
            }
        }

        private void ProcessComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProcessComboBox.SelectedItem is Process selectedProcess)
            {
                LoadTasksForProcess(selectedProcess.Id);
            }
            else
            {
                TasksDataGrid.ItemsSource = null;
                TotalTasksText.Text = "0";
                AddTaskButton.IsEnabled = false;
                EditTaskButton.IsEnabled = false;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _context.Dispose();
            base.OnClosed(e);
        }
    }
}