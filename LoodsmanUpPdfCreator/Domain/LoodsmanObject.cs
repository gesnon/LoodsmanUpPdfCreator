using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoodsmanUpPdfCreator.Domain
{
    public sealed class LoodsmanObject : INotifyPropertyChanged
    {

        public LoodsmanObject()
        {

        }

        public string _name { get; set; }
        public string _type { get; set; }
        public double _version { get; set; }
        public string _state { get; set; }
        public int _id { get; set; }
        public List<string> _errorList { get; set; }
        public List<LoodsmanObject> _documents { get; set; }
        public List<LoodsmanObject> _projects { get; set; }
        public List<LoodsmanObject> _archives { get; set; }
        public List<LoodsmanFile> _files { get; set; }

        public string _owner { get; set; }
        public int _parentId { get; set; }

        public OperationStatus _operationStatus { get; set; }
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;

                _name = value;

                OnPropertyChanged(nameof(Name));
            }
        }
        public string Type
        {
            get => _type;
            set
            {
                if (_type == value)
                    return;

                _type = value;

                OnPropertyChanged(nameof(Type));
            }
        }
        public double Version
        {
            get => _version;
            set
            {
                if (_version == value)
                    return;

                _version = value;

                OnPropertyChanged(nameof(Version));
            }
        }
        public string State
        {
            get => _state;
            set
            {
                if (_state == value)
                    return;

                _state = value;

                OnPropertyChanged(nameof(State));
            }
        }
        public int Id
        {
            get => _id;
            set
            {
                if (_id == value)
                    return;

                _id = value;

                OnPropertyChanged(nameof(Id));
            }
        }
        public List<string> ErrorList
        {
            get => _errorList;
            set
            {
                if (_errorList == value)
                    return;

                _errorList = value;

                OnPropertyChanged(nameof(ErrorList));
            }
        }

        public List<LoodsmanObject> Documents
        {
            get => _documents;
            set
            {
                if (_documents == value)
                    return;

                _documents = value;

                OnPropertyChanged(nameof(Documents));
            }
        }
        public List<LoodsmanObject> Projects
        {
            get => _projects;
            set
            {
                if (_projects == value)
                    return;

                _projects = value;

                OnPropertyChanged(nameof(Projects));
            }
        }
        public List<LoodsmanObject> Archives
        {
            get => _archives;
            set
            {
                if (_archives == value)
                    return;

                _archives = value;

                OnPropertyChanged(nameof(Archives));
            }
        }
        public List<LoodsmanFile> Files
        {
            get => _files;
            set
            {
                if (_files == value)
                    return;

                _files = value;

                OnPropertyChanged(nameof(Files));
            }
        }


        public string Owner
        {
            get => _owner;
            set
            {
                if (_owner == value)
                    return;

                _owner = value;

                OnPropertyChanged(nameof(Owner));
            }
        }
        public int ParentId
        {
            get => _parentId;
            set
            {
                if (_parentId == value)
                    return;

                _parentId = value;

                OnPropertyChanged(nameof(ParentId));
            }
        }
        public OperationStatus OperationStatus
        {
            get => _operationStatus;
            set
            {
                if (_operationStatus == value)
                    return;

                _operationStatus = value;

                OnPropertyChanged(nameof(OperationStatus));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
             this,
             new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{Type} {Name} {Version}";
        }
    }
}
