using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Server.Models;

namespace Server.Workers
{
    public static class JsonWorker<T>
    {
        public static string ObjToJson(T files)
        {
            var settings = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(files, settings);
        }
        public static T JsonToObj(string jsonData)
        {
            return JsonSerializer.Deserialize<T>(jsonData);
        }
    }
}