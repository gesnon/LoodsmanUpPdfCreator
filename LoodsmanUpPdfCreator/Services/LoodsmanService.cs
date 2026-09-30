using Ascon.Plm.Loodsman.PluginSDK;
using Loodsman;
using LoodsmanUpPdfCreator.Domain;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using LoodsmanObject = LoodsmanUpPdfCreator.Domain.LoodsmanObject;

namespace LoodsmanUpPdfCreator.Services
{
    public class LoodsmanService
    {
        private readonly INetPluginCall _client;

        public LoodsmanService(INetPluginCall client)
        {
            if (client == null) throw new ArgumentNullException("client");
            _client = client;

        }

        public List<LoodsmanObject> GetSelectedObjects(List<string> selectedIds)
        {
            List<LoodsmanObject> result = new List<LoodsmanObject>();
            if (selectedIds.Count != 0)
            {
                foreach (string id in selectedIds)
                {
                    DataTable property = _client.GetDataTable("GetInfoAboutVersion", new object[] { "", "", "", id, 15 });

                    string name = property.Rows[0]["_PRODUCT"].ToString();
                    string type = property.Rows[0]["_TYPE"].ToString();
                    double version = double.Parse(property.Rows[0]["_VERSION"].ToString());
                    string state = property.Rows[0]["_STATE"].ToString();

                    LoodsmanObject loodsmanObject = new LoodsmanObject
                    {
                        Name = name,
                        Type = type,
                        Version = version,
                        State = state,
                        Id = int.Parse(id),
                        ErrorList = new List<string>(),
                        Documents = new List<LoodsmanObject>(),
                        Projects = new List<LoodsmanObject>(),
                        Files = new List<LoodsmanFile>(),
                        OperationStatus = OperationStatus.Waiting
                    };

                    List<LoodsmanObject> Projects = GetConnectedProjectsUP(loodsmanObject);
                    loodsmanObject.Projects.AddRange(Projects);
                    result.Add(loodsmanObject);

                }


            }

            return result;
        }


        public List<LoodsmanObject> GetVersionList(string type, string name)
        {
            List<LoodsmanObject> result = new List<LoodsmanObject>();


            DataTable allObjects = _client.GetDataTable("GetVersionList", new object[] { type, name, "", "" });

            foreach (DataRow row in allObjects.Rows)
            {
                string Name = row["_PRODUCT"].ToString();
                string Type = row["_TYPE"].ToString();
                double Version = double.Parse(row["_VERSION"].ToString());
                string State = row["_STATE"].ToString();
                string Id = row["_ID_VERSION"].ToString();


                LoodsmanObject loodsmanObject = new LoodsmanObject
                {
                    Name = Name,
                    Type = Type,
                    Version = Version,
                    State = State,
                    Id = int.Parse(Id),
                    ErrorList = new List<string>(),
                    Documents = new List<LoodsmanObject>(),
                    Projects = new List<LoodsmanObject>(),
                    Files = new List<LoodsmanFile>()
                };

                result.Add(loodsmanObject);
            }



            return result;
        }
        public List<string> FindStructureError(string type, string name)
        {
            List<string> errors = new List<string>();

            List<LoodsmanObject> allObjects = new List<LoodsmanObject>();

            DataTable allObjectsTable = _client.GetDataTable("GetVersionList", new object[] { type, name, "", "" });

            foreach (DataRow row in allObjectsTable.Rows)
            {
                string Name = row["_PRODUCT"].ToString();
                string Type = row["_TYPE"].ToString();
                double Version = double.Parse(row["_VERSION"].ToString());
                string State = row["_STATE"].ToString();
                string Id = row["_ID_VERSION"].ToString();
                int ParentId = int.Parse(row["_ID_PARENTVERSION"].ToString());
                string Owner = row["_OWNER"].ToString();


                LoodsmanObject loodsmanObject = new LoodsmanObject
                {
                    Name = Name,
                    Type = Type,
                    Version = Version,
                    State = State,
                    Id = int.Parse(Id),
                    ErrorList = new List<string>(),
                    Documents = new List<LoodsmanObject>(),
                    Projects = new List<LoodsmanObject>(),
                    Files = new List<LoodsmanFile>(),
                    ParentId = ParentId,

                };

                allObjects.Add(loodsmanObject);
            }

            //Проверка на существование нескольких объектов в стадии согласования
            if (allObjects.Where(_ => _.State == "Согласование").ToList().Count > 0)
            {
                errors.Add($"Существует объекты типа {type} {name} в состоянии согласования");
            }
            //Проверка на существование нескольких объектов в стадии проектирования
            if (allObjects.Where(_ => _.State == "Проектирование").ToList().Count > 1)
            {
                errors.Add($"Существует несколько объектов типа {type} {name} в состоянии проектирования");
            }

            //Поиск ошибок версионности
            allObjects = allObjects.OrderBy(_ => _.Version).ToList();

            int CountOfParrent = allObjects.Select(_ => _.ParentId).Distinct().Count();
            if (CountOfParrent < allObjects.Count)
            {
                errors.Add($"Есть ошибки версионности в объектах типа {type} {name}");
            }

            return errors;
        }



        //Не знаю как лучше назвать этот метод, он должен найти проекты УП, в состоянии проектирования, у которых не связи с выбранной сборкой/деталью
        public string FindFreeProjects(string type, LoodsmanObject lo)
        {
            //List<string> errors = new List<string>();

            List<LoodsmanObject> allObjects = new List<LoodsmanObject>();

            DataTable allObjectsTable = _client.GetDataTable("GetVersionList", new object[] { type, lo.Name, "", "" });

            foreach (DataRow row in allObjectsTable.Rows)
            {
                string Name = row["_PRODUCT"].ToString();
                string Type = row["_TYPE"].ToString();
                double Version = double.Parse(row["_VERSION"].ToString());
                string State = row["_STATE"].ToString();
                string Id = row["_ID_VERSION"].ToString();
                int ParentId = int.Parse(row["_ID_PARENTVERSION"].ToString());
                string Owner = row["_OWNER"].ToString();


                LoodsmanObject loodsmanObject = new LoodsmanObject
                {
                    Name = Name,
                    Type = Type,
                    Version = Version,
                    State = State,
                    Id = int.Parse(Id),
                    ErrorList = new List<string>(),
                    Documents = new List<LoodsmanObject>(),
                    Projects = new List<LoodsmanObject>(),
                    Files = new List<LoodsmanFile>(),
                    ParentId = ParentId,

                };

                allObjects.Add(loodsmanObject);
            }

            //Проверка на существование нескольких объектов в стадии согласования
            List<LoodsmanObject> freeProjects = allObjects.Where(_ => _.State == "Проектирование").ToList();

            foreach (LoodsmanObject project in freeProjects)
            {
                if (lo.Projects.FirstOrDefault(_ => _.Id == project.Id) == null)
                {
                    return $"Существует проект УП в состоянии проектирования у которого нет связи с объектом {lo.Type} {lo.Name}";
                }
            }


            return null;
        }



        public LoodsmanObject GetLastVersion(string type, string name)
        {
            List<LoodsmanObject> allObjects = GetVersionList(type, name).OrderBy(_ => _.Version).ToList();
            if (allObjects.Count != 0)
            {
                LoodsmanObject lastVersion = allObjects.Last();
                return lastVersion;
            }

            return null;


        }


        public string CreateCheckOut(LoodsmanObject lo)
        {
            var newCheckoutName = _client.RunMethod("CheckOut", new object[] { lo.Type, lo.Name, lo.Version, 0, false, false });
            //_client.RunMethod("ConnectToCheckOut", new object[] { newCheckoutName, "ElSh",  false, false });

            return newCheckoutName.ToString();
        }


        public void AddToCheckOut(LoodsmanObject lo, string checkOutName)
        {
            object obj1 = new object();
            object obj2 = new object();

            var connectToCheckOut = _client.RunMethod("ConnectToCheckOut", new object[] { checkOutName, "ElSh", false, false });
            var addToCheckOut = _client.RunMethod("AddToCheckOut", new object[] { lo.Id, false, obj1, obj2 });
        }
        public void CheckIn(string checkOutName)
        {
            _client.RunMethod("CheckIn", checkOutName, "ElSh");
        }



        public List<LoodsmanObject> GetConnectedLoodsmanObject(LoodsmanObject lo, string connectionType)
        {
            DataTable objects = _client.GetDataTable("GetLinkedObjectsEx", new object[] { "", "", "", lo.Id, "Документы", false, false, false });

            List<LoodsmanObject> result = new List<LoodsmanObject>();

            foreach (DataRow row in objects.Rows)
            {

                string name = row["_PRODUCT"].ToString();
                string type = row["_TYPE"].ToString();
                string state = row["_STATE"].ToString();
                double version = double.Parse(row["_VERSION"].ToString());
                string isDocument = row["_DOCUMENT"].ToString();
                string id = row["_ID_VERSION"].ToString();

                result.Add(new LoodsmanObject
                {
                    Name = name,
                    Type = type,
                    Version = version,
                    State = state,
                    Id = int.Parse(id),
                    ErrorList = new List<string>(),
                    Documents = new List<LoodsmanObject>(),
                    Files = new List<LoodsmanFile>()
                });

            }

            return result;

        }

        public void CreateProjectOrVersion(LoodsmanObject lo)
        {


        }

        public LoodsmanObject CreateNewProject(LoodsmanObject lo)
        {

            string newVersion =  _client.RunMethod("NewObject", new object[] { "Проект УП", "Проектирование", lo.Name, 0 }).ToString();
            _client.RunMethod("InsertObject", new object[] { "Проект УП", lo.Name, "1.0", lo.Type, lo.Name, $"{lo.Version}.0", "Для изделий", "", false });
            return GetLoodsmanObjectById(int.Parse(newVersion));

        }
        public LoodsmanObject CreateProjectVersion(LoodsmanObject lo)
        {
            LoodsmanObject lastProjectVersion = GetLastVersion("Проект УП", lo.Name);

            string newVersion = _client.RunMethod("NewVersionEx", new object[] { "Проект УП", lo.Name, $"{lastProjectVersion.Version}.0", "Проектирование", null, 1.0, 4 }).ToString();
            _client.RunMethod("InsertObject", new object[] { "Проект УП", lo.Name, lo.Version.ToString() + ".0", lo.Type, lo.Name, $"{(lastProjectVersion.Version + 1)}.0", "Для изделий", "", false });
            return GetLoodsmanObjectById(int.Parse(newVersion));

        }

        public LoodsmanObject CreateNewArchive(LoodsmanObject project)
        {
            string newVersion =  _client.RunMethod("NewObject", new object[] { "Архив проекта УП", "Проектирование", $"{project.Name} КЭ00", 0 }).ToString();
            _client.RunMethod("InsertObject", new object[] { "Проект УП", project.Name, $"{project.Version}.0", "Архив проекта УП", $"{project.Name} КЭ00", $"1.0", "Документы", "", false });
            return GetLoodsmanObjectById(int.Parse(newVersion));

        }
        public LoodsmanObject CreateArchivetVersion(LoodsmanObject project)
        {
            LoodsmanObject lastArchiveVersion = GetLastVersion("Архив проекта УП", $"{project.Name} КЭ00");

            string newVersion = _client.RunMethod("NewVersionEx", new object[] { "Архив проекта УП", $"{project.Name} КЭ00", $"{lastArchiveVersion.Version}.0", "Проектирование", null, 1.0, 4 }).ToString();
            _client.RunMethod("InsertObject", new object[] { "Проект УП", project.Name, project.Version.ToString() + ".0", lastArchiveVersion.Type, lastArchiveVersion.Name, $"{(lastArchiveVersion.Version + 1)}.0", "Документы", "", false });

            return GetLoodsmanObjectById(int.Parse(newVersion));

        }

        public void FillAttributeUP(LoodsmanObject selectedLO, LoodsmanObject project)
        {
            DataTable attribute = _client.GetDataTable("GetInfoAboutVersion", new object[] { "", "", "", selectedLO.Id, 15 });
            DataTable input = _client.GetDataTable("GetInfoAboutVersion", new object[] { "", "", "", attribute.Rows[0]["_ID_VERSION"], 2 });
            DataTable currentProject = _client.GetDataTable("GetInfoAboutVersion", new object[] { "Проект УП", attribute.Rows[0]["_PRODUCT"].ToString(), $"{project.Version}.0", "", 15 });
            string currentprojectState = currentProject.Rows[0]["_STATE"].ToString();
            string currentDccessLevel = currentProject.Rows[0]["_ACCESSLEVEL"].ToString();
            if (currentprojectState != "Согласование" && currentDccessLevel == "3")
            {
                for (int z = 0; z < input.Rows.Count; z++)
                {
                    if (input.Rows[z]["_NAME"].ToString() == "№ документа")
                    {
                        _client.RunMethod("UpAttrValue", new object[] { "Проект УП", attribute.Rows[0]["_PRODUCT"].ToString(), $"{project.Version}.0", "№ документа", input.Rows[z]["_VALUE"], "", false });

                    }
                    if (input.Rows[z]["_NAME"].ToString() == "Наименование")
                    {
                        _client.RunMethod("UpAttrValue", new object[] { "Проект УП", attribute.Rows[0]["_PRODUCT"].ToString(), $"{project.Version}.0", "Наименование", input.Rows[z]["_VALUE"], "", false });
                        _client.RunMethod("UpAttrValue", new object[] { "Проект УП", attribute.Rows[0]["_PRODUCT"].ToString(), $"{project.Version}.0", "Источник создания", "Модуль", "", false });
                        _client.RunMethod("UpAttrValue", new object[] { "Проект УП", attribute.Rows[0]["_PRODUCT"].ToString(), $"{project.Version}.0", "Описание", "Тестовое описание", "", false });
                    }
                }
            }
        }
        public void FillAttributeArchiveUP(LoodsmanObject selectedLO, LoodsmanObject archive)
        {
            DataTable attribute = _client.GetDataTable("GetInfoAboutVersion", new object[] { "", "", "", selectedLO.Id, 15 });
            DataTable input = _client.GetDataTable("GetInfoAboutVersion", new object[] { "", "", "", attribute.Rows[0]["_ID_VERSION"], 2 });
            //DataTable currentArchive = _client.GetDataTable("GetInfoAboutVersion", new object[] { "Архив проекта УП", attribute.Rows[0]["_PRODUCT"].ToString(), $"{archive.Version}.0", "", 15 });
            //string currentArchiveState = currentArchive.Rows[0]["_STATE"].ToString();
            //string currentDccessLevel = currentArchive.Rows[0]["_ACCESSLEVEL"].ToString();

            for (int z = 0; z < input.Rows.Count; z++)
            {
                if (input.Rows[z]["_NAME"].ToString() == "№ документа")
                {
                    _client.RunMethod("UpAttrValue", new object[] { "Архив проекта УП", archive.Name, $"{archive.Version}.0", "№ документа", input.Rows[z]["_VALUE"], "", false });

                }
                if (input.Rows[z]["_NAME"].ToString() == "Наименование")
                {
                    _client.RunMethod("UpAttrValue", new object[] { "Архив проекта УП", archive.Name, $"{archive.Version}.0", "Наименование", input.Rows[z]["_VALUE"], "", false });
                    _client.RunMethod("UpAttrValue", new object[] { "Архив проекта УП", archive.Name, $"{archive.Version}.0", "Источник создания", "Модуль", "", false });
                    _client.RunMethod("UpAttrValue", new object[] { "Архив проекта УП", archive.Name, $"{archive.Version}.0", "Описание", "Тестовое описание", "", false });
                }
            }
            for (int i = 0; i < attribute.Rows.Count; i++)
            {
                attribute = _client.GetDataTable("GetInfoAboutVersion", new object[] { "", "", "", selectedLO.Id, 15 });
                _client.RunMethod("UpAttrValue", new object[] { "Архив проекта УП", archive.Name, $"{archive.Version}.0", "Наименование", "Эскиз", "", false });
                _client.RunMethod("UpAttrValue", new object[] { "Архив проекта УП", archive.Name, $"{archive.Version}.0", "Код документа", "КЭ00", "", false });
            }

        }


        public List<LoodsmanObject> GetConnectedProjectsUP(LoodsmanObject lo)
        {
            DataTable objects = _client.GetDataTable("GetLinkedObjectsEx", new object[] { "", "", "", lo.Id, "Для изделий", true, false, false });

            List<LoodsmanObject> result = new List<LoodsmanObject>();

            foreach (DataRow row in objects.Rows)
            {

                string name = row["_PRODUCT"].ToString();
                string type = row["_TYPE"].ToString();
                double version = double.Parse(row["_VERSION"].ToString());
                string state = row["_STATE"].ToString();
                string isDocument = row["_DOCUMENT"].ToString();
                string id = row["_ID_VERSION"].ToString();

                LoodsmanObject project = new LoodsmanObject
                {
                    Name = name,
                    Type = type,
                    Version = version,
                    State = state,
                    Id = int.Parse(id),
                    ErrorList = new List<string>(),
                    Documents = new List<LoodsmanObject>(),
                    Archives = new List<LoodsmanObject>(),
                    Files = new List<LoodsmanFile>()
                };

                List<LoodsmanObject> archives = GetConnectedArchiveUP(project);
                project.Archives.AddRange(archives);

                result.Add(project);

            }

            return result;

        }
        public List<LoodsmanObject> GetConnectedArchiveUP(LoodsmanObject lo)
        {
            DataTable objects = _client.GetDataTable("GetLinkedObjectsEx", new object[] { "", "", "", lo.Id, "Документы", false, false, false });

            List<LoodsmanObject> result = new List<LoodsmanObject>();

            foreach (DataRow row in objects.Rows)
            {

                string name = row["_PRODUCT"].ToString();
                string type = row["_TYPE"].ToString();
                double version = double.Parse(row["_VERSION"].ToString());
                string state = row["_STATE"].ToString();
                string isDocument = row["_DOCUMENT"].ToString();
                string id = row["_ID_VERSION"].ToString();

                result.Add(new LoodsmanObject
                {
                    Name = name,
                    Type = type,
                    Version = version,
                    State = state,
                    Id = int.Parse(id),
                    ErrorList = new List<string>(),
                    Documents = new List<LoodsmanObject>(),
                    Files = new List<LoodsmanFile>()
                });

            }

            return result;

        }

        public List<LoodsmanFile> GetFiles(LoodsmanObject lo)
        {
            DataTable files = _client.GetDataTable("GetInfoAboutVersion", new object[] { "", "", "", lo.Id, 7 });

            List<LoodsmanFile> result = new List<LoodsmanFile>();

            foreach (DataRow row in files.Rows)
            {
                string id = lo.Id.ToString();
                string name = row["_NAME"].ToString();
                string localName = row["_LOCALNAME"].ToString();

                result.Add(new LoodsmanFile { Name = name, Id = int.Parse(id), LocalName = localName });
            }

            return result;
        }
        public void ExtractFile(LoodsmanObject lo)
        {
            LoodsmanFile file = lo.Files[0];

            string path = _client.RunMethod("GetFileById", new object[] { file.Id, file.Name, file.LocalName }).ToString();
        }

        public LoodsmanObject GetLoodsmanObjectById(int id)
        {
            DataTable findObject = _client.GetDataTable("GetInfoAboutVersion", new object[] { "", "", "", id, 15 });

            if (findObject.Rows.Count != 0)
            {

                string name = findObject.Rows[0]["_PRODUCT"].ToString();
                string type = findObject.Rows[0]["_TYPE"].ToString();
                double version = double.Parse(findObject.Rows[0]["_VERSION"].ToString());
                string state = findObject.Rows[0]["_STATE"].ToString();
                string isDocument = findObject.Rows[0]["_DOCUMENT"].ToString();

                return new LoodsmanObject
                {
                    Name = name,
                    Id = id,
                    Type = type,
                    Version = version,
                    State = state
                };
            }
            return null;
        }

    }
}
