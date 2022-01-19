using System;
using Newtonsoft.Json;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {

            string json = "{'id':768,'hash':'6485ae2e563fef29ca2805795f4f3821b49901ac4b2cd0033afe315e3ec8ff66','title':'Get Ready','artist':'Pitbull feat. Blake Shelton','mapper':'ICHDerHorst', 'duration':'03:45','bpm':'128','difficulties':['Master'],'description':'Road to 100th and Beyond DLC ', 'youtube_url':'https://youtu.be/Ejdx6_hYTiY','filename':'768-Pitbull-feat-Blake-Shelton-Get-Ready-IchDerHorst.synth'}";
            JObject response = JObject.Parse(json);
            IList<JToken> results = response.Children().ToList();
            Console.WriteLine("Hello World!");    
        }
    }
}