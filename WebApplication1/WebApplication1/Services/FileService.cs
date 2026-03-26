using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Web.Hosting;

public class FileService
{
    public List<Restaurant> LoadRestaurants()
    {
        string path = HostingEnvironment.MapPath("~/App_Data/restaurants.json");

        if (!File.Exists(path))
            return new List<Restaurant>();

        var serializer = new DataContractJsonSerializer(typeof(List<Restaurant>));

        using (FileStream fs = new FileStream(path, FileMode.Open))
        {
            return (List<Restaurant>)serializer.ReadObject(fs);
        }
    }

    public void SaveRestaurants(List<Restaurant> restaurants)
    {
        string path = HostingEnvironment.MapPath("~/App_Data/restaurants.json");

        var serializer = new DataContractJsonSerializer(typeof(List<Restaurant>));

        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            serializer.WriteObject(fs, restaurants);
        }
    }
}