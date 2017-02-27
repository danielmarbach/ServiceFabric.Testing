﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            GetValue().GetAwaiter().GetResult();
        }

        private static async Task GetValue()
        {
            var httpClient = new HttpClient();

            while (true)
            {
                var content = await httpClient.GetAsync("http://localhost:19081/TestApplication/TestRunner/Web");
                var stringContent = await content.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(stringContent))
                {
                    Console.Write(stringContent);
                }
                await Task.Delay(2000);
            }
        }
    }
}
