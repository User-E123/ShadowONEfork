using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ShadowONE.Models;

namespace ShadowONE.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _searchText = string.Empty;
        private string _currentFilePath = string.Empty;
        private ObservableCollection<FileEntry> _allFiles = new ObservableCollection<FileEntry>();
        private ObservableCollection<FileEntry> _filteredFiles = new ObservableCollection<FileEntry>();

        public ObservableCollection<FileEntry> FilteredFiles
        {
            get => _filteredFiles;
            set
            {
                _filteredFiles = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterFiles();
            }
        }

        public string CurrentFilePath
        {
            get => _currentFilePath;
            set
            {
                _currentFilePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasFileOpen));
            }
        }

        public bool HasFileOpen => !string.IsNullOrEmpty(_currentFilePath);

        public void LoadFiles(ObservableCollection<FileEntry> files)
        {
            _allFiles = new ObservableCollection<FileEntry>(files);
            FilteredFiles = new ObservableCollection<FileEntry>(files);
            OnPropertyChanged(nameof(HasFileOpen));
            OnPropertyChanged(nameof(FilteredFiles));
        }

        private void FilterFiles()
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                FilteredFiles = new ObservableCollection<FileEntry>(_allFiles);
            }
            else
            {
            var filtered = _allFiles.Where(f => 
                f.FileName.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
                FilteredFiles = new ObservableCollection<FileEntry>(filtered);
            }
            OnPropertyChanged(nameof(HasFileOpen));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
