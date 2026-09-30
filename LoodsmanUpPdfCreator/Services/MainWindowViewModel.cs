using Ascon.Plm.Loodsman.PluginSDK;
using LoodsmanObjects;
using LoodsmanUpPdfCreator.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LoodsmanUpPdfCreator.Services
{
    public class MainWindowViewModel
    {

        //Сервисы
        public static LoodsmanService _loodsmanService;

        public LoodsmanService loodsmanService
        {
            get => _loodsmanService;
            set
            {
                _loodsmanService = value;
                OnPropertyChanged(nameof(selectedObjectIds));
            }
        }

        public LoodsmanObject _selectedObject;
        public LoodsmanObject selectedObject
        {
            get => _selectedObject;
            set
            {
                _selectedObject = value;
                OnPropertyChanged(nameof(selectedObject));
                getSelectedObjectErrors();
            }
        }

        //Источники данных
        //public static ObservableCollection<string> _selectedObjectIds = new ObservableCollection<string>();


        public ObservableCollection<Operation> _operationList;
        public ObservableCollection<Operation> operationList
        {
            get => _operationList;
            set
            {
                _operationList = value;
                OnPropertyChanged(nameof(operationList));
            }
        }

        public ObservableCollection<string> _objectErrorsList;
        public ObservableCollection<string> objectErrorsList
        {
            get => _objectErrorsList;
            set
            {
                _objectErrorsList = value;
                OnPropertyChanged(nameof(objectErrorsList));
            }
        }
        public static ObservableCollection<string> _selectedObjectIds;
        public ObservableCollection<string> selectedObjectIds
        {
            get => _selectedObjectIds;
            set
            {
                _selectedObjectIds = value;
                OnPropertyChanged(nameof(selectedObjectIds));
            }

        }

        //public ObservableCollection<LoodsmanObject> _selectedLoodsmanObjects = new ObservableCollection<LoodsmanObject>();
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

        public void updateList()
        {
            selectedLoodsmanObjects.Add(new LoodsmanObject { Name = "TestName", Type = "TestType", Version = 1.0, OperationStatus = OperationStatus.Unsuccess });
        }

        //Кнопки
        public AsyncRelayCommand PreviousButtonCommand { get; }
        private async Task PreviousButtonClick()
        {

            //для каждого выделенного объекта без ошибок
            foreach (LoodsmanObject loodsmanObject in selectedLoodsmanObjects.Where(_ => _.ErrorList.Count == 0))
            {
                //Сначала нужно понять что делать с проектом УП (Создать версию или изменить существующий)


                loodsmanObject.OperationStatus = await Task.Run(() => MainMethod(loodsmanObject));


                continue;


                //Сценарий изменения существующего проекта УП включается если последнии версия Проекта УП имеет связь с выбранной деталью и находится в состоянии проектирования
                LoodsmanObject lastProject = _loodsmanService.GetLastVersion("Проект УП", loodsmanObject.Name);

                if (lastProject == null)
                {
                    //Сценарий создания нового проекта УП и архива проекта УП
                    string NewCheckOut = _loodsmanService.CreateCheckOut(loodsmanObject);
                    _loodsmanService.AddToCheckOut(loodsmanObject, NewCheckOut);
                    LoodsmanObject newProject = _loodsmanService.CreateNewProject(loodsmanObject);

                    _loodsmanService.FillAttributeUP(loodsmanObject, newProject);
                    LoodsmanObject lastArchive = _loodsmanService.GetLastVersion("Архив проекта УП", $"{loodsmanObject.Name} КЭ00");

                    if (lastArchive != null && lastArchive.State == "Утвержден")
                    {
                        LoodsmanObject newArchive = _loodsmanService.CreateArchivetVersion(newProject);
                        _loodsmanService.FillAttributeArchiveUP(loodsmanObject, newArchive);

                    }
                    if (lastArchive == null)
                    {
                        LoodsmanObject newArchive = _loodsmanService.CreateNewArchive(newProject);
                        _loodsmanService.FillAttributeArchiveUP(loodsmanObject, newArchive);

                    }
                    _loodsmanService.CheckIn(NewCheckOut);
                    loodsmanObject.OperationStatus = OperationStatus.Success;
                    continue;
                }

                //Сценарий создания версии проекта УП
                if (lastProject.State == "Утвержден")
                {
                    string NewCheckOut = _loodsmanService.CreateCheckOut(loodsmanObject);
                    _loodsmanService.AddToCheckOut(loodsmanObject, NewCheckOut);
                    LoodsmanObject newProject = _loodsmanService.CreateProjectVersion(loodsmanObject);
                    _loodsmanService.FillAttributeUP(loodsmanObject, newProject);
                    LoodsmanObject lastArchive = _loodsmanService.GetLastVersion("Архив проекта УП", $"{loodsmanObject.Name} КЭ00");

                    if (lastArchive != null && lastArchive.State == "Утвержден")
                    {
                        LoodsmanObject newArchive = _loodsmanService.CreateArchivetVersion(newProject);
                        _loodsmanService.FillAttributeArchiveUP(loodsmanObject, newArchive);

                    }
                    if (lastArchive == null)
                    {
                        LoodsmanObject newArchive = _loodsmanService.CreateNewArchive(newProject);
                        _loodsmanService.FillAttributeArchiveUP(loodsmanObject, newArchive);

                    }

                    _loodsmanService.CheckIn(NewCheckOut);
                    loodsmanObject.OperationStatus = OperationStatus.Success;
                    continue;
                }


                //Сценарий обновления проекта УП
                LoodsmanObject lastprojectInSelectedLO = loodsmanObject.Projects.FirstOrDefault(_ => _.Id == lastProject.Id);

                if (lastprojectInSelectedLO != null && (lastProject.State == "Проектирование"))
                {
                    string NewCheckOut = _loodsmanService.CreateCheckOut(loodsmanObject);
                    _loodsmanService.AddToCheckOut(loodsmanObject, NewCheckOut);
                    _loodsmanService.AddToCheckOut(lastProject, NewCheckOut);
                    _loodsmanService.FillAttributeUP(loodsmanObject, lastProject);

                    LoodsmanObject lastArchive = _loodsmanService.GetLastVersion("Архив проекта УП", $"{loodsmanObject.Name} КЭ00");

                    if (lastArchive != null && lastArchive.State == "Утвержден")
                    {
                        LoodsmanObject newArchive = _loodsmanService.CreateArchivetVersion(lastProject);
                        _loodsmanService.FillAttributeArchiveUP(loodsmanObject, newArchive);

                    }
                    if (lastprojectInSelectedLO.Archives.FirstOrDefault(_ => _.Id == lastArchive.Id) != null && lastArchive.State == "Проектирование")
                    {
                        _loodsmanService.AddToCheckOut(lastArchive, NewCheckOut);
                        _loodsmanService.FillAttributeArchiveUP(loodsmanObject, lastArchive);
                    }
                    if (lastArchive == null)
                    {
                        LoodsmanObject newArchive = _loodsmanService.CreateNewArchive(lastProject);
                        _loodsmanService.FillAttributeArchiveUP(loodsmanObject, newArchive);

                    }

                    _loodsmanService.CheckIn(NewCheckOut);
                    loodsmanObject.OperationStatus = OperationStatus.Success;
                    continue;

                }


            }


        }

        public async Task<OperationStatus> MainMethod(LoodsmanObject loodsmanObject)
        {

            //Thread.Sleep(2000);

            OperationStatus resultTask = OperationStatus.Unsuccess;

            //return Task.FromResult(OperationStatus.Success);



            //Сценарий изменения существующего проекта УП включается если последнии версия Проекта УП имеет связь с выбранной деталью и находится в состоянии проектирования
            LoodsmanObject lastProject = _loodsmanService.GetLastVersion("Проект УП", loodsmanObject.Name);

            if (lastProject == null)
            {
                //Сценарий создания нового проекта УП и архива проекта УП
                string NewCheckOut = _loodsmanService.CreateCheckOut(loodsmanObject);
                _loodsmanService.AddToCheckOut(loodsmanObject, NewCheckOut);
                LoodsmanObject newProject = _loodsmanService.CreateNewProject(loodsmanObject);

                _loodsmanService.FillAttributeUP(loodsmanObject, newProject);
                LoodsmanObject lastArchive = _loodsmanService.GetLastVersion("Архив проекта УП", $"{loodsmanObject.Name} КЭ00");

                if (lastArchive != null && lastArchive.State == "Утвержден")
                {
                    LoodsmanObject newArchive = _loodsmanService.CreateArchivetVersion(newProject);
                    _loodsmanService.FillAttributeArchiveUP(loodsmanObject, newArchive);

                }
                if (lastArchive == null)
                {
                    LoodsmanObject newArchive = _loodsmanService.CreateNewArchive(newProject);
                    _loodsmanService.FillAttributeArchiveUP(loodsmanObject, newArchive);

                }
                _loodsmanService.CheckIn(NewCheckOut);
                //loodsmanObject.OperationStatus = OperationStatus.Success;

                resultTask = OperationStatus.Success;
                //return resultTask;
            }

            //Сценарий создания версии проекта УП
            if (lastProject.State == "Утвержден")
            {
                string NewCheckOut = _loodsmanService.CreateCheckOut(loodsmanObject);
                _loodsmanService.AddToCheckOut(loodsmanObject, NewCheckOut);
                LoodsmanObject newProject = _loodsmanService.CreateProjectVersion(loodsmanObject);
                _loodsmanService.FillAttributeUP(loodsmanObject, newProject);
                LoodsmanObject lastArchive = _loodsmanService.GetLastVersion("Архив проекта УП", $"{loodsmanObject.Name} КЭ00");

                if (lastArchive != null && lastArchive.State == "Утвержден")
                {
                    LoodsmanObject newArchive = _loodsmanService.CreateArchivetVersion(newProject);
                    _loodsmanService.FillAttributeArchiveUP(loodsmanObject, newArchive);

                }
                if (lastArchive == null)
                {
                    LoodsmanObject newArchive = _loodsmanService.CreateNewArchive(newProject);
                    _loodsmanService.FillAttributeArchiveUP(loodsmanObject, newArchive);

                }

                _loodsmanService.CheckIn(NewCheckOut);
                //loodsmanObject.OperationStatus = OperationStatus.Success;
                resultTask = OperationStatus.Success;
                //return resultTask;
            }


            //Сценарий обновления проекта УП
            LoodsmanObject lastprojectInSelectedLO = loodsmanObject.Projects.FirstOrDefault(_ => _.Id == lastProject.Id);

            if (lastprojectInSelectedLO != null && (lastProject.State == "Проектирование"))
            {
                string NewCheckOut = _loodsmanService.CreateCheckOut(loodsmanObject);
                _loodsmanService.AddToCheckOut(loodsmanObject, NewCheckOut);
                _loodsmanService.AddToCheckOut(lastProject, NewCheckOut);
                _loodsmanService.FillAttributeUP(loodsmanObject, lastProject);

                LoodsmanObject lastArchive = _loodsmanService.GetLastVersion("Архив проекта УП", $"{loodsmanObject.Name} КЭ00");

                if (lastArchive != null && lastArchive.State == "Утвержден")
                {
                    LoodsmanObject newArchive = _loodsmanService.CreateArchivetVersion(lastProject);
                    _loodsmanService.FillAttributeArchiveUP(loodsmanObject, newArchive);

                }
                if (lastprojectInSelectedLO.Archives.FirstOrDefault(_ => _.Id == lastArchive.Id) != null && lastArchive.State == "Проектирование")
                {
                    _loodsmanService.AddToCheckOut(lastArchive, NewCheckOut);
                    _loodsmanService.FillAttributeArchiveUP(loodsmanObject, lastArchive);
                }
                if (lastArchive == null)
                {
                    LoodsmanObject newArchive = _loodsmanService.CreateNewArchive(lastProject);
                    _loodsmanService.FillAttributeArchiveUP(loodsmanObject, newArchive);

                }

                _loodsmanService.CheckIn(NewCheckOut);
                //loodsmanObject.OperationStatus = OperationStatus.Success;
                resultTask = OperationStatus.Success;
                // return resultTask;
            }           


            return resultTask;
        }
        public ICommand NextButtonCommand { get; }
        private void NextButtonClick(object parameter)
        {


        }

        public void selectedLoodsmanObjectListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private bool CanBeClicked(object parameter)
        {
            return true; // Здесь можно задать условие доступности кнопки
        }
        private void getSelectedObjectErrors()
        {
            objectErrorsList.Clear();
            foreach (string error in selectedObject.ErrorList)
            {
                objectErrorsList.Add(error);
            }

        }

        public MainWindowViewModel(INetPluginCall client, ObservableCollection<string> selectedObjectsIds)
        {

            loodsmanService = new LoodsmanService(client);
            selectedObjectIds = selectedObjectsIds;
            selectedLoodsmanObjects = new ObservableCollection<LoodsmanObject>();

            operationList = new ObservableCollection<Operation> {
                new Operation("Поиск модели", OperationStatus.Waiting),
                new Operation("Выгрузка модели", OperationStatus.Waiting),
                new Operation("Запуск компаса", OperationStatus.Waiting),
                new Operation("Открытие модели", OperationStatus.Waiting),
                new Operation("Очистка мусора", OperationStatus.Waiting),
                new Operation("Копирование изображений", OperationStatus.Waiting)

            };
            objectErrorsList = new ObservableCollection<string>();

            //PreviousButtonCommand => new RelayCommand(async ()=> await PreviousButtonClick, CanBeClicked);
            PreviousButtonCommand = new AsyncRelayCommand(PreviousButtonClick, null);
            NextButtonCommand = new RelayCommand(NextButtonClick);

            FillSelectedOnjectsSource();

            string NewCheckOut = "";
            foreach (LoodsmanObject loodsmanObject in selectedLoodsmanObjects)
            {
                List<LoodsmanObject> documents = _loodsmanService.GetConnectedLoodsmanObject(loodsmanObject, "Документы");

                if (documents.Count != 0)
                {
                    loodsmanObject.Documents.AddRange(documents);

                    LoodsmanObject plan = loodsmanObject.Documents.FirstOrDefault(_ => _.Type == "Сборочный чертеж");
                    if (plan != null)
                    {
                        List<LoodsmanFile> files = _loodsmanService.GetFiles(plan);
                        if (files.Count != 0)
                        {
                            plan.Files.AddRange(files);
                            if (files.Count == 1)
                            {
                                //_loodsmanService.ExtractFile(plan);
                            }
                        }


                    }
                    else
                    {
                        throw new Exception($"Не найден чертеж для {loodsmanObject.Name}");
                    }


                    if (string.IsNullOrEmpty(NewCheckOut))
                    {
                        // NewCheckOut = _loodsmanService.CreateCheckOut(loodsmanObject);

                    }
                    else
                    {
                        //_loodsmanService.AddToCheckOut(loodsmanObject, NewCheckOut);
                    }

                    //Теперь нужно определить состояние Проекта УП и в зависимости от этого действовать дальше



                    List<string> errors = _loodsmanService.FindStructureError("Проект УП", loodsmanObject.Name);

                    LoodsmanObject lastProjectVersion = _loodsmanService.GetLastVersion("Проект УП", loodsmanObject.Name);

                    string freeError = _loodsmanService.FindFreeProjects("Проект УП", loodsmanObject);

                    if (!string.IsNullOrEmpty(freeError))
                    {
                        errors.Add(freeError);
                    }

                    if (errors.Count != 0)
                    {
                        loodsmanObject.ErrorList.AddRange(errors);
                        loodsmanObject.OperationStatus = OperationStatus.Unsuccess;

                        continue;
                    }

                    else
                    {

                    }


                }


            }

        }


        private List<LoodsmanObject> GetSelectedObjects()
        {
            try
            {
                List<LoodsmanObject> objects = _loodsmanService.GetSelectedObjects(selectedObjectIds.ToList());

                return objects;


            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public void FillSelectedOnjectsSource()
        {

            List<LoodsmanObject> selectedObj = GetSelectedObjects();

            foreach (LoodsmanObject lo in selectedObj)
            {
                selectedLoodsmanObjects.Add(lo);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
