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
        
    }
}