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
    public static class JsonWorker
    {
        //  Сериализация объекта типа User в Json строку
        public static string FilesToJson(List<FileM> files)
        {
            var settings = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(files, settings);
        }
        public static List<FileM> JsonToFiles(string jsonData)
        {
            return JsonSerializer.Deserialize<List<FileM>>(jsonData);
        }

        /*
        //  Сериализация объекта типа Message в Json строку
        public static string MessageToJson(Message message)
        {
            var settings = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(message, settings);
        }
        public static Message JsonToMessage(string jsonData)
        {
            return JsonSerializer.Deserialize<Message>(jsonData);
        }

        public static string UsersListToJson(List<User> users)
        {
            var settings = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(users, settings);
        }*/
    }
}