﻿using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace SIVA.Core.Bot.Internal
{
    internal class Program
    {
        
        public static DiscordSocketClient _client;
        public static EventHandler _handler;

        private static void Main()
        {
            Console.Title = "SIVA";
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Green;
            new Program().Start().GetAwaiter().GetResult();
        }

        private async Task Start()
        {
            if (string.IsNullOrEmpty(Config.bot.Token)) 
            {
                InteractiveSetup.Setup();
            }

            LogSeverity logSeverity;
            switch (Config.bot.LogSeverity.ToLower())
            {
                case "verbose":
                    logSeverity = LogSeverity.Verbose;
                    break;
                case "info":
                    logSeverity = LogSeverity.Info;
                    break;
                case "warning":
                    logSeverity = LogSeverity.Warning;
                    break;
                case "debug":
                    logSeverity = LogSeverity.Debug;
                    break;
                case "critical":
                    logSeverity = LogSeverity.Critical;
                    break;
                case "error":
                    logSeverity = LogSeverity.Error;
                    break;
                default:
                    logSeverity = LogSeverity.Verbose;
                    break;
            }

            if (!File.Exists("data/SIVA.db"))
            {
                File.Create("data/SIVA.db");
            }

            _client = new DiscordSocketClient(new DiscordSocketConfig {LogLevel = logSeverity} );
            await _client.LoginAsync(TokenType.Bot, Config.bot.Token);
            await _client.StartAsync();
            await _client.SetGameAsync(Config.bot.BotGameToSet, $"https://twitch.tv/{Config.bot.TwitchStreamer}", StreamType.Twitch);
            await _client.SetStatusAsync(UserStatus.Online);
            _client.Log += EventUtils.Log;
            _handler = new EventHandler();
            await _handler.InitializeAsync(_client);
            Console.WriteLine("Public SIVA: https://discordapp.com/oauth2/authorize?scope=bot&client_id=320942091049893888&permissions=8");
            Console.WriteLine("Dev SIVA: https://discordapp.com/oauth2/authorize?scope=bot&client_id=410547925597421571&permissions=8");
            await Task.Delay(-1);
            
        }
        
    }
}
