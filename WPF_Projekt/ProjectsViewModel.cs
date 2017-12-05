using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using ProjectBilling.DataAccess;


namespace WPF_Application
{
    
    public interface IProjectsViewModel : INotifyPropertyChanged
    {
        IProjectViewModel SelectedProject { get; set; }
        void UpdateProject();
    }
    public enum Status { None, Good, Bad }
    public class ProjectsViewModel : Notifier, IProjectsViewModel
    {
        public const string SELECTED_PROJECT_PROPERRTY_NAME
            = "SelectedProject";

        private readonly IProjectsModel _model;
        private IProjectViewModel _selectedProject;
        private Status _detailsEstimateStatus
            = Status.None;
        private bool _detailsEnabled;
        private readonly ICommand _updateCommand;


        public ObservableCollection<Project>
            Projects { get { return _model.Projects; } }

        public int? SelectedValue {
            set {
                if (value == null)
                    return;
                Project project = GetProject((int)value);
                if (SelectedProject == null)
                {
                    SelectedProject
                        = new ProjectViewModel(project);
                }
                else
                {
                    SelectedProject.Update(project);
                }
                DetailsEstimateStatus =
                    SelectedProject.EstimateStatus;
            }
        }

        public IProjectViewModel SelectedProject {
            get { return _selectedProject; }
            set {
                if (value == null)
                {
                    _selectedProject = value;
                    DetailsEnabled = false;
                }
                else
                {
                    if (_selectedProject == null)
                    {
                        _selectedProject =
                            new ProjectViewModel(value);
                    }
                    _selectedProject.Update(value);
                    DetailsEstimateStatus =
                        _selectedProject.EstimateStatus;
                    DetailsEnabled = true;
                    NotifyPropertyChanged(
                        SELECTED_PROJECT_PROPERRTY_NAME);
                }
            }
        }

        public Status DetailsEstimateStatus {
            get { return _detailsEstimateStatus; }
            set {
                _detailsEstimateStatus = value;
                NotifyPropertyChanged("DetailsEstimateStatus");
            }
        }

        public bool DetailsEnabled {
            get { return _detailsEnabled; }
            set {
                _detailsEnabled = value;
                NotifyPropertyChanged("DetailsEnabled");
            }
        }

        public ICommand UpdateCommand {
            get { return _updateCommand; }
        }

        public ProjectsViewModel(IProjectsModel projectModel)
        {
            _model = projectModel;
            _model.ProjectUpdated +=
                model_ProjectUpdated;
            _updateCommand = new UpdateCommand(this);
        }

        public void UpdateProject()
        {
            DetailsEstimateStatus =
                SelectedProject.EstimateStatus;
            _model.UpdateProject(SelectedProject);
        }

        private void model_ProjectUpdated(object sender,
                                          ProjectEventArgs e)
        {
            GetProject(e.Project.ID).Update(e.Project);
            if (SelectedProject != null
                && e.Project.ID == SelectedProject.ID)
            {
                SelectedProject.Update(e.Project);
                DetailsEstimateStatus =
                    SelectedProject.EstimateStatus;
            }
        }

        private Project GetProject(int projectId)
        {
            return (from p in Projects
                    where p.ID == projectId
                    select p).FirstOrDefault();
        }
    }
    internal class UpdateCommand : ICommand
    {
        private const int ARE_EQUAL = 0;
        private const int NONE_SELECTED = -1;
        private IProjectsViewModel _vm;

        public UpdateCommand(IProjectsViewModel viewModel)
        {
            _vm = viewModel;
            _vm.PropertyChanged += vm_PropertyChanged;
        }

        private void vm_PropertyChanged(object sender,
            PropertyChangedEventArgs e)
        {
            if (string.Compare(e.PropertyName,
                               ProjectsViewModel.
                               SELECTED_PROJECT_PROPERRTY_NAME)
                == ARE_EQUAL)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }

        public bool CanExecute(object parameter)
        {
            if (_vm.SelectedProject == null)
                return false;
            return ((ProjectViewModel)_vm.SelectedProject).ID
                   > NONE_SELECTED;
        }

        public event EventHandler CanExecuteChanged
            = delegate { };

        public void Execute(object parameter)
        {
            _vm.UpdateProject();
        }
    }
}
