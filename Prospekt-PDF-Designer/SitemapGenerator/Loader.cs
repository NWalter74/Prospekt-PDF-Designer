using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SitemapGenerator.Sitemap
{
    public class Loader : ILoader
    {
        private static HttpClient _client;
        public Loader()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(300);

        }
        public async Task<string> Get(string url)
        {
            HttpResponseMessage resp = await _client.GetAsync(url);
            return await resp.Content.ReadAsStringAsync();
        }
    }
}
