using Newtonsoft.Json;

namespace SmartElectricityAPI.Utils;

public static class DeepCopyUtility
{
    public static List<T> DeepCopyList<T>(List<T> originalList)
    {
        var serializedList = JsonConvert.SerializeObject(originalList);

        var copiedList = JsonConvert.DeserializeObject<List<T>>(serializedList);

        return copiedList;
    }
    public static T DeepCopy<T>(T obj)
    {
        if (obj == null)
        {
            return default(T);
        }

        var serialized = JsonConvert.SerializeObject(obj);
        return JsonConvert.DeserializeObject<T>(serialized);
    }
}
