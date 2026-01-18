using App4.Data;
using App4.Models;
using App4.Services;
using MahApps.Metro.Controls;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace App4
{
    public partial class MainWindow : MetroWindow
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private User? _currentUser;

        public MainWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
            _context.Database.Migrate();

            _authService = new AuthService(_context);
            CheckAuthentication();
        }

        private void CheckAuthentication()
        {
            var loginWindow = new LoginWindow(_authService);
            if (loginWindow.ShowDialog() == true)
            {
                _currentUser = loginWindow.AuthenticatedUser;
                LoadProcesses();
                LoadProcessesComboBox();
            }
            else
            {
                Application.Current.Shutdown();
            }
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
                ShowError("Ошибка загрузки данных", ex.Message);
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
                ShowError("Ошибка загрузки процессов", ex.Message);
            }
        }

        private void LoadTasksForProcess(int processId)
        {
            try
            {
                var tasks = _context.Tasks
                    .Where(t => t.ProcessId == processId)
                    .OrderBy(t => t.CreatedAt)
                    .ToList();

                TasksDataGrid.ItemsSource = tasks;
                TotalTasksText.Text = tasks.Count.ToString();
                AddTaskButton.IsEnabled = true;
                EditTaskButton.IsEnabled = tasks.Any();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка загрузки задач", ex.Message);
            }
        }

        private void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowWarning(string message)
        {
            MessageBox.Show(message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            LoadProcesses();
            ShowSuccess("Данные обновлены!");
        }

        private void AddProcess_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Добавить процесс", "Введите название процесса:");
            if (dialog.ShowDialog() == true)
            {
                var processName = dialog.InputText;
                if (string.IsNullOrWhiteSpace(processName))
                {
                    ShowWarning("Название процесса не может быть пустым");
                    return;
                }

                var newProcess = new Process { Name = processName };
                _context.Processes.Add(newProcess);
                _context.SaveChanges();

                LoadProcesses();
                LoadProcessesComboBox();
                ShowSuccess($"Процесс '{processName}' успешно добавлен!");
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessComboBox.SelectedItem is not Process selectedProcess)
            {
                ShowWarning("Сначала выберите процесс для добавления задачи");
                return;
            }

            var dialog = new InputDialog("Добавить задачу", "Введите название задачи:");
            if (dialog.ShowDialog() == true)
            {
                var taskName = dialog.InputText;
                if (string.IsNullOrWhiteSpace(taskName))
                {
                    ShowWarning("Название задачи не может быть пустым");
                    return;
                }

                var newTask = new TaskItem
                {
                    ProcessId = selectedProcess.Id,
                    TaskName = taskName
                };
                _context.Tasks.Add(newTask);
                _context.SaveChanges();

                LoadTasksForProcess(selectedProcess.Id);
                ShowSuccess($"Задача '{taskName}' успешно добавлена!");
            }
        }

        private void EditProcess_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessesDataGrid.SelectedItem is not Process selectedProcess) return;

            var dialog = new InputDialog("Редактировать процесс", "Введите новое название процесса:");
            dialog.InputTextBox.Text = selectedProcess.Name;

            if (dialog.ShowDialog() == true)
            {
                var newName = dialog.InputText;
                if (string.IsNullOrWhiteSpace(newName))
                {
                    ShowWarning("Название процесса не может быть пустым");
                    return;
                }

                selectedProcess.Name = newName;
                selectedProcess.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                LoadProcesses();
                LoadProcessesComboBox();
                ShowSuccess($"Процесс успешно обновлен!");
            }
        }

        private void EditProcessFromGrid_Click(object sender, RoutedEventArgs e)
        {
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

            var dialog = new InputDialog("Редактировать задачу", "Введите новое название задачи:");
            dialog.InputTextBox.Text = selectedTask.TaskName;

            if (dialog.ShowDialog() == true)
            {
                var newName = dialog.InputText;
                if (string.IsNullOrWhiteSpace(newName))
                {
                    ShowWarning("Название задачи не может быть пустым");
                    return;
                }

                selectedTask.TaskName = newName;
                _context.SaveChanges();
                if (ProcessComboBox.SelectedItem is Process selectedProcess)
                {
                    LoadTasksForProcess(selectedProcess.Id);
                }
                ShowSuccess($"Задача успешно обновлена!");
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
            if (sender is not System.Windows.Controls.Button button ||
                button.Tag is not int processId)
                return;

            var process = _context.Processes.Include(p => p.Tasks).FirstOrDefault(p => p.Id == processId);
            if (process == null) return;

            if (MessageBox.Show($"Удалить процесс '{process.Name}' и все его задачи?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _context.Processes.Remove(process);
                _context.SaveChanges();

                LoadProcesses();
                LoadProcessesComboBox();
                ShowSuccess("Процесс и все его задачи удалены!");
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button ||
                button.Tag is not int taskId)
                return;

            var task = _context.Tasks.Find(taskId);
            if (task == null) return;

            if (MessageBox.Show($"Удалить задачу '{task.TaskName}'?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _context.Tasks.Remove(task);
                _context.SaveChanges();

                if (ProcessComboBox.SelectedItem is Process selectedProcess)
                {
                    LoadTasksForProcess(selectedProcess.Id);
                }

                ShowSuccess("Задача удалена!");
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