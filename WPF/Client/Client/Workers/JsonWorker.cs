using System.Text.Json;

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