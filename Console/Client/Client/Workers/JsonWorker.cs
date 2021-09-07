using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Client.Models;

namespace Client.Workers
{
    public static class JsonWorker<T>
    {
        //  Сериализация объекта типа User в Json строку
        public static string ObjToJson(T obj)
        {
            var settings = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(obj, settings);
        }
        public static T JsonToObj(string jsonData)
        {
            return JsonSerializer.Deserialize<T>(jsonData);
        }
        
    }
}