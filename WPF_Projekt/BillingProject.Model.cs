
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using ProjectBilling.DataAccess;

namespace WPF_Application
{
    public class Notifier : INotifyPropertyChanged
    {
    public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void NotifyPropertyChanged(
            string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public interface IProjectsModel
    {
        ObservableCollection<Project> Projects { get; set; }
        event EventHandler<ProjectEventArgs> ProjectUpdated;
        void UpdateProject(IProject updatedProject);
    }

    public class ProjectEventArgs : EventArgs
    {
        public IProject Project { get; set; }
        public ProjectEventArgs(IProject project)
        {
            Project = project;
        }
    }

    public class ProjectsModel : IProjectsModel
    {public ObservableCollection<Project> Projects { get; set; }
            public event EventHandler<ProjectEventArgs> ProjectUpdated = delegate { };

            public ProjectsModel(IDataService dataService)
            {
                Projects = new ObservableCollection<Project>();
                foreach (Project project in dataService.GetProjects())
                {
                    Projects.Add(project);
                }
            }

            public void UpdateProject(IProject updatedProject)
            {
                GetProject(updatedProject.ID).Update(updatedProject);
                ProjectUpdated(this,
                    new ProjectEventArgs(updatedProject));
            }

            private Project GetProject(int projectId)
            {
                return Projects.FirstOrDefault(
                    project => project.ID == projectId);
            }
        
    }
}