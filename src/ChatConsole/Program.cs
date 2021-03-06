﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatConsole
{
    class Program
    {
        //useful: https://github.com/aspnet/SignalR-samples
        //https://docs.microsoft.com/en-us/aspnet/core/signalr/dotnet-client?view=aspnetcore-5.0&tabs=visual-studio
        private static IConfigurationRoot Configuration { get; set; }
        static async Task Main(string[] args)
        {
            // Display title as the C# SignalR Chat
            Console.WriteLine("SignalR Chatter\r");
            Console.WriteLine("------------------------\n");

            //configure the default host for local (current work IP)
            string host = "http://10.15.38.39:5000/chatHub"; //http://localhost:5000/chatHub

            //Ask the user if they want to configure a different host
            Console.WriteLine("Enter host (i.e. http://localhost:5000/chatHub - or leave blank and use default!)");
            var inputHost = Console.ReadLine();
            //Check if there was anything provided - otherwise we use the default
            if (!string.IsNullOrWhiteSpace(inputHost))
                host = inputHost;

            Console.WriteLine($"Using host: {host}");

            Console.WriteLine("Enter a UserName");
            string name = Console.ReadLine();

            try
            {
                var connection = new HubConnectionBuilder().WithUrl(host).Build();

                await connection.StartAsync();
                Console.WriteLine("Starting connection. Press Ctrl-C to close.");

                //Handle cancellation/closure events
                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += async (sender, a) =>
                {
                    a.Cancel = true;
                    cts.Cancel();
                    await connection.InvokeAsync("sendMessage", "ConsoleClient", $"{name} has left");
                    Environment.Exit(0);
                };

                //Listen for incoming messages from signalR hub
                connection.On("broadcastMessage", (string userName, string message) =>
                {
                    if (userName == name)
                        Console.WriteLine($"You said: {message}");
                    else
                        Console.WriteLine($"{userName} says: {message}");
                });

                await connection.InvokeAsync("sendMessage", "ConsoleClient", $"{name} has connected");

                Console.WriteLine("Please write into chat below!");
                while (true)
                {
                    // wait for user to write something into the chat
                    string content = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(content))
                        await connection.InvokeAsync("sendMessage", $"{name}:", content);
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"Caught exception {e.Message}");
                throw;
            }
            finally
            {
                Console.WriteLine("Closing App");
            }
        }

        /// <summary>
        /// Test method to handle Yes/No selection on console apps
        /// </summary>
        /// <param name="title">The text to display on the read message</param>
        /// <returns></returns>
        public static bool Confirm(string title)
        {
            ConsoleKey response;
            do
            {
                Console.Write($"{ title } [y/n] ");
                response = Console.ReadKey(false).Key;
                if (response != ConsoleKey.Enter)
                {
                    Console.WriteLine();
                }
            } while (response != ConsoleKey.Y && response != ConsoleKey.N);

            return (response == ConsoleKey.Y);
        }
    }
}
