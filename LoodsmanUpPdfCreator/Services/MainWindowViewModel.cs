using Ascon.Plm.Loodsman.PluginSDK;
using LoodsmanObjects;
using LoodsmanUpPdfCreator.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LoodsmanUpPdfCreator.Services
{
    public class MainWindowViewModel
    {

        //Сервисы
        public LoodsmanService _loodsmanService;

        public LoodsmanService loodsmanService
        {
            get => _loodsmanService;
            set
            {
                _loodsmanService = value;
                OnPropertyChanged(nameof(selectedObjectIds));
            }
        }

        //Источники данных
        public ObservableCollection<string> _selectedObjectIds;
        public ObservableCollection<string> selectedObjectIds
        {
            get => _selectedObjectIds;
            set
            {
                _selectedObjectIds = value;
                OnPropertyChanged(nameof(selectedObjectIds));
            }
        }

        public ObservableCollection<LoodsmanObject> _selectedLoodsmanObjects;
        public ObservableCollection<LoodsmanObject> selectedLoodsmanObjects
        {
            get => _selectedLoodsmanObjects;
            set
            {
                _selectedLoodsmanObjects = value;
                OnPropertyChanged(nameof(selectedLoodsmanObjects));
            }
        }



        //Кнопки
        public ICommand PreviousButtonCommand { get; }
        private void PreviousButtonClick(object parameter)
        {

            List<LoodsmanObject> selectedObj = GetSelectedObjects();

            selectedLoodsmanObjects.Clear();

            foreach (LoodsmanObject lo in selectedObj)
            {
                selectedLoodsmanObjects.Add(lo);
            }
        }
        public ICommand NextButtonCommand { get; }
        private void NextButtonClick(object parameter)
        {


        }

        private bool CanBeClicked(object parameter)
        {
            return true; // Здесь можно задать условие доступности кнопки
        }


        public MainWindowViewModel()
        {

            selectedObjectIds = (ObservableCollection<string>)selectedObjectIds;
            selectedLoodsmanObjects = new ObservableCollection<LoodsmanObject>();


            PreviousButtonCommand = new RelayCommand(PreviousButtonClick, CanBeClicked);
            NextButtonCommand = new RelayCommand(NextButtonClick, CanBeClicked);

           
        }

        public MainWindowViewModel(INetPluginCall client, ObservableCollection<string> selectedObjectIds)
        {

            _loodsmanService = new LoodsmanService(client);
            this.selectedObjectIds = selectedObjectIds;
            selectedLoodsmanObjects = new ObservableCollection<LoodsmanObject>();


            PreviousButtonCommand = new RelayCommand(PreviousButtonClick, CanBeClicked);
            NextButtonCommand = new RelayCommand(NextButtonClick, CanBeClicked);

            
        }


        private List<LoodsmanObject> GetSelectedObjects()
        {
            try
            {
                List<LoodsmanObject> objects = _loodsmanService.GetSelectedObjects(_selectedObjectIds.ToList());

                return objects;


            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
