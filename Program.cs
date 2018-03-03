namespace Refit_SingletonIssue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Refit;

    class Program
    {
        private static readonly Uri address = new Uri("https://google.com/");

        public static HttpClient _httpClient = new HttpClient { BaseAddress = address };
        public static ITestApi _refitClient = CreateRefitClient(_httpClient);
        private static int numberOfConnection = 100;

        static void Main(string[] args)
        {
            RunRefit();
            RunRefitStatic();
            RunRefitWithSingleHttpClient();

            Console.ReadKey();
        }

        private static void RunRefitStatic()
        {
            Console.WriteLine("Starting requests with the same static refit and HttpClient for each request.");
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < numberOfConnection; i++)
                {
                    tasks.Add(PrformRequestRefit(_refitClient, i));
                }

                Task.WaitAll(tasks.ToArray());
            }
            finally
            {
                _httpClient.Dispose();
            }

            watch.Stop();
            Console.WriteLine(
                $"Time: {watch.Elapsed.Milliseconds}");
        }

        private static void RunRefit()
        {
            Console.WriteLine("Starting requests with separate refit and HttpClient for each request.");
            var watch = new Stopwatch();
            watch.Start();

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < numberOfConnection; i++)
            {
                tasks.Add(PrformRequestRefit(CreateRefitClient(), i));
            }

            Task.WaitAll(tasks.ToArray());

            watch.Stop();
            Console.WriteLine(
                $"Time: {watch.Elapsed.Milliseconds}");
        }

        private static void RunRefitWithSingleHttpClient()
        {
            Console.WriteLine("Starting requests with separate refit and the same static HttpClient for each request.");
            var watch = new Stopwatch();
            watch.Start();

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < numberOfConnection; i++)
            {
                tasks.Add(PrformRequestRefit(CreateRefitClient(_httpClient), i));
            }

            Task.WaitAll(tasks.ToArray());

            watch.Stop();
            Console.WriteLine(
                $"Time: {watch.Elapsed.Milliseconds}");
        }

        static ITestApi CreateRefitClient(HttpClient httpClient = null)
        {
            httpClient = httpClient ?? new HttpClient { BaseAddress = address };

            return RestService.For<ITestApi>(httpClient);
        }

        static async Task PrformRequestRefit(ITestApi testApi, int i)
        {
            var response = await testApi.Test();

            Console.WriteLine($"{i}:{response.StatusCode}");
        }
    }

    public interface ITestApi
    {
        [Get("/")]
        Task<HttpResponseMessage> Test();
    }
}