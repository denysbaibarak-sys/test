using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using Newtonsoft.Json;
using System.Globalization;

public class MapService
{
    private readonly string _filePath = HostingEnvironment.MapPath("~/App_Data/users.json");
    private readonly string _geoApiKey = "API_KEY";

    public class CoordinatesDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    private List<User> LoadUsers()
    {
        if (!File.Exists(_filePath))
            return new List<User>();

        using (FileStream fs = new FileStream(_filePath, FileMode.Open))
        {
            var serializer = new DataContractJsonSerializer(typeof(List<User>));
            return (List<User>)serializer.ReadObject(fs);
        }
    }

    private void SaveUsers(List<User> users)
    {
        using (FileStream fs = new FileStream(_filePath, FileMode.Create))
        {
            var serializer = new DataContractJsonSerializer(typeof(List<User>));
            serializer.WriteObject(fs, users);
        }
    }

    public async Task<bool> SaveCoordinatesAsync(int userId, double lat, double lng)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return false;

        using (HttpClient client = new HttpClient())
        {
            //  1. Формуємо координати з крапкою замість коми 
            string latStr = lat.ToString(CultureInfo.InvariantCulture);
            string lngStr = lng.ToString(CultureInfo.InvariantCulture);

            //  2. Geoapify reverse geocode
            string geoUrl = $"https://api.geoapify.com/v1/geocode/reverse?lat={latStr}&lon={lngStr}&apiKey={_geoApiKey}";
            var geoResponse = await client.GetStringAsync(geoUrl);

            dynamic geoData = JsonConvert.DeserializeObject(geoResponse);
            string formatted = geoData.features[0].properties.formatted;

            //  3. Lingva translate
            string encoded = Uri.EscapeDataString(formatted);
            string lingvaUrl = $"https://lingva.ml/api/v1/en/uk/{encoded}";

            var lingvaResponse = await client.GetStringAsync(lingvaUrl);
            dynamic lingvaData = JsonConvert.DeserializeObject(lingvaResponse);

            string translated = lingvaData.translation;

            //  4. Записуємо в Address, при потребі прибрати переклад коментуємо Lingva translate, також беремо дані з другої дії.
            user.Address = translated;
        }

        SaveUsers(users);
        return true;
    }
}