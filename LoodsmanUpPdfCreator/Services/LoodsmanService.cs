using Ascon.Plm.Loodsman.PluginSDK;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

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

        public List<Domain.LoodsmanObject> GetSelectedObjects(List<string> selectedIds)
        {
            List<Domain.LoodsmanObject> result = new List<Domain.LoodsmanObject>();
            if (selectedIds.Count != 0)
            {
                foreach (string id in selectedIds)
                {
                    DataTable property = _client.GetDataTable("GetInfoAboutVersion", new object[] { "", "", "", id, 15 });

                    string name = property.Rows[0]["_PRODUCT"].ToString();
                    string type = property.Rows[0]["_TYPE"].ToString();

                    result.Add(new Domain.LoodsmanObject { Name = name, Type = type, Id = int.Parse(id) });

                }


            }

            return result;
        }
    }
}
