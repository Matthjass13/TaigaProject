
//using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using MVC.Services;

namespace MVC.Services
{
    public class Services //: IServices
    {

        private readonly HttpClient _client;
        private readonly string _baseUrl = "https://localhost:7003/api/Digitec";

        public Services(HttpClient client)
        {
            _client = client;
        }


    }

}
