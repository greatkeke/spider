using DotnetSpider;
using System;

namespace KekeSpider
{
    class Program
    {
        static void Main(string[] args)
        {
            var spider = Spider.Create<GithubSpider>();
            spider.RunAsync();
            Console.Read();
        }
    }
}
