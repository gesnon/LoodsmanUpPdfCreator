using Ascon.Plm.Loodsman.PluginSDK;
using LoodsmanUpPdfCreator.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace LoodsmanUpPdfCreator.Infrastructure.Loodsman
{
    [LoodsmanPlugin]
    public sealed class LoodsmanPlugin : ILoodsmanNetPlugin
    {

        public void BindMenu(IMenuDefinition menu)
        {
            // ЛОЦМАН разных версий обновляет контекст команды неодинаково.
            // null в третьем параметре официально означает постоянно доступную
            // команду; фактическое выделение проверяется непосредственно при запуске.
            menu.AddMenuItem("Создать УП PDF", Execute, null);
        }

        private static void Execute(INetPluginCall context)
        {
            if (context == null || context.PluginCall == null)
            {
                System.Windows.MessageBox.Show(
                    "ЛОЦМАН не передал контекст подключённой базы данных.",
                    "Подготовка производственных файлов",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            IReadOnlyList<string> selectedIds;
            List<LoodsmanObject> selectedLoodsanObjects;

            try
            {
                selectedIds = GetSelectedVersionIds(context);
                selectedLoodsanObjects = GetSelectedLoodsmanObjects(context);
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(
                    "Не удалось получить выбранные объекты ЛОЦМАН. " + exception.Message,
                    "Подготовка производственных файлов",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            if (selectedIds.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    "Выберите в дереве ЛОЦМАН хотя бы одну версию детали.",
                    "Подготовка производственных файлов",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                return;
            }

            ObservableCollection<string> newIds = new ObservableCollection<string>();
            foreach(string str in selectedIds)
            {
                newIds.Add(str);
            }

            MainWindow window = new MainWindow(context, newIds);
            IntPtr owner = (IntPtr)context.PluginCall.ClientHandle;
            if (owner != IntPtr.Zero) new WindowInteropHelper(window).Owner = owner;
            window.ShowDialog();
        }
        private static IReadOnlyList<string> GetSelectedVersionIds(INetPluginCall context)
        {
            string rawIds = Convert.ToString(
                context.RunMethod("CGetTreeSelectedIDs", new object[0]));
            List<string> selectedIds = rawIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Where(value => value.Length > 0 && value != "0")
                .Distinct(StringComparer.Ordinal)
                .ToList();

            // В некоторых представлениях клиента метод множественного выбора
            // возвращает пустую строку, хотя PluginCall содержит активную версию.
            if (selectedIds.Count == 0 && context.PluginCall.IdVersion > 0)
            {
                selectedIds.Add(context.PluginCall.IdVersion.ToString());
            }

            return selectedIds;
        }

        private static List<LoodsmanObject> GetSelectedLoodsmanObjects(INetPluginCall context)
        {
            string rawIds = Convert.ToString(
                context.RunMethod("CGetTreeSelectedIDs", new object[0]));
            List<string> selectedIds = rawIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Where(value => value.Length > 0 && value != "0")
                .Distinct(StringComparer.Ordinal)
                .ToList();

            // В некоторых представлениях клиента метод множественного выбора
            // возвращает пустую строку, хотя PluginCall содержит активную версию.
            if (selectedIds.Count == 0 && context.PluginCall.IdVersion > 0)
            {
                selectedIds.Add(context.PluginCall.IdVersion.ToString());
            }
            List<LoodsmanObject> result = new List<LoodsmanObject>();
            if(selectedIds.Count != 0)
            {
                foreach(string id in selectedIds)
                {
                    DataTable property = context.GetDataTable("GetInfoAboutVersion", new object[] { "", "", "", id, 15 });

                    string name = property.Rows[0]["_PRODUCT"].ToString();
                    string type = property.Rows[0]["_TYPE"].ToString();

                    result.Add(new LoodsmanObject { Name = name, Type = type, Id = int.Parse(id) });

                }


            }

            return result;
        }



        public void PluginLoad() { }
        public void PluginUnload() { }
        public void OnConnectToDb(INetPluginCall call) { }
        public void OnCloseDb() { }
    }
}
