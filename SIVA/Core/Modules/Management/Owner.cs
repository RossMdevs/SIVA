﻿using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SIVA.Core.Bot;
using SIVA.Core.JsonFiles;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Discord.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace SIVA.Core.Modules.Management
{
    public class Owner : SivaModule
    {
        
        public void KillProgram() => Kill(); // DO. NOT. USE. THIS. This is only for deliberately causing a StackOverflowException to stop the program.

        public void Kill() => KillProgram(); // DO. NOT. USE. THIS. This is only for deliberately causing a StackOverflowException to stop the program.
        
        [Command("Shutdown")]
        [RequireOwner]
        public async Task Shutdown()
        {
            var client = Bot.Internal.Program._client;
            var embed = Helpers.CreateEmbed(Context, Bot.Internal.Utilities.GetFormattedLocaleMsg("LoggingOutMsg", Context.User.Mention));

            await Helpers.SendMessage(Context, embed);
            await client.LogoutAsync();
            await client.StopAsync();
            KillProgram();

        }

        [Command("CreateConfigEmergency")]
        [RequireOwner]
        public async Task CreateConfigsBecauseImADumbassDotExe()
        {
            foreach (var guild in Bot.Internal.Program._client.Guilds)
            {
                if (guild.Id != 405806471578648588) { GuildConfig.CreateGuildConfig(guild.Id); }
            }

            await ReplyAsync($"Successfully created configs for {Bot.Internal.Program._client.Guilds.Count - 1} servers.");
        }

        [Command("CreateConfig")]
        [RequireOwner]
        public async Task CreateConfigIfOneDoesntExist(ulong serverId = 0)
        {
            if (serverId == 0) serverId = Context.Guild.Id;

            var g = Bot.Internal.Program._client.GetGuild(serverId);
            var embed = Helpers.CreateEmbed(Context, $"Created a config for the guild `{g.Name}! ({serverId})`");
            var targetConfig = GuildConfig.GetGuildConfig(serverId);
            
            List<ulong> serverIds = new List<ulong>();
            
            foreach (SocketGuild server in Bot.Internal.Program._client.Guilds)
            {
                serverIds.Add(server.Id);
            }
            
            if (targetConfig == null && serverIds.Contains(serverId))
            {
                GuildConfig.CreateGuildConfig(serverId);
            }
            else
            {
                embed.WithDescription($"Couldn't create a config for {serverId}. Either they already have a config, or I don't have access to that server.");
            }

            await Helpers.SendMessage(Context, embed);

        }

        [Command("NotifyBotUsers"), Alias("Nbu")]
        [RequireOwner]
        public async Task NotifyPeopleWhoUseBot([Remainder]string message)
        {
            var client = Bot.Internal.Program._client;
            var embed = new EmbedBuilder()
                .WithDescription(message)
                .WithTitle("Message from Greem (Bot Creator)")
                .WithColor(Bot.Internal.Config.bot.DefaultEmbedColour);

            foreach (SocketGuild server in client.Guilds)
            {
                var dm = await server.Owner.GetOrCreateDMChannelAsync();
                
                try 
                {
                    Thread.Sleep(1000);
                    await dm.SendMessageAsync("", false, embed);
                } 
                catch (RateLimitedException e)
                {
                    Console.WriteLine($"ratelimited. {e.Message}");
                    var ownerDm = await Helpers.GetDmChannel(Bot.Internal.Config.bot.BotOwner);
                    await ownerDm.SendMessageAsync("Ratelimited.");
                }
            }

            await ReplyAsync($"Successfully sent `{message}` to all server owners.");
        }

        [Command("SSH")]
        [RequireOwner]
        public async Task SendLinuxCommand([Remainder]string command)
        {
            var escArg = command.Replace("\"", "\\\"");

            var task = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c {escArg}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            task.Start();
            string res = task.StandardOutput.ReadToEnd();
            task.WaitForExit();

            await Helpers.SendMessage(Context, Helpers.CreateEmbed(Context, $"{res}"));

        }

        [Command("VerifyGuild"), Alias("Verify")]
        [RequireOwner]
        public async Task VerifyGuildById(ulong guildId = 0)
        {
            var id = guildId;
            if (id == 0) id = Context.Guild.Id;
            var config = GuildConfig.GetGuildConfig(id);
            config.VerifiedGuild = true;
            var embed = new EmbedBuilder()
                .WithDescription("Successfully verified this server.")
                .WithColor(Bot.Internal.Config.bot.DefaultEmbedColour)
                .WithFooter(Bot.Internal.Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            await ReplyAsync("", false, embed);
        }

        [Command("Eval")]
        [RequireOwner]
        public async Task EvaluateCSharpCode([Remainder]string code)
        {
            var result = await CSharpScript.EvaluateAsync(code, ScriptOptions.Default.AddImports("System", "System.IO", "System.Collections.Generic", "System.Threading.Tasks", "System.Threading"));
            var embed = new EmbedBuilder()
                .WithDescription($"Input: \n```cs\n{code}```\n\nOutput: `{result}`")
                .WithColor(Bot.Internal.Config.bot.DefaultEmbedColour);
            await ReplyAsync("", false, embed);
        }

        [Command("Stream")]
        [RequireOwner]
        public async Task SetBotStream(string streamer, [Remainder]string streamName)
        {
            await Bot.Internal.Program._client.SetGameAsync(streamName, $"https://twitch.tv/{streamer}", StreamType.Twitch);
            var embed = Helpers.CreateEmbed(Context, $"Set the stream name to **{streamName}**, and set the streamer to <https://twitch.tv/{streamer}>!");
            await Helpers.SendMessage(Context, embed);
        }


        [Command("Game")]
        [RequireOwner]
        public async Task SetBotGame([Remainder] string game)
        {
            var client = Bot.Internal.Program._client;

            var embed = new EmbedBuilder();
            embed.WithDescription($"Set the bot's game to {game}");
            embed.WithColor(Bot.Internal.Config.bot.DefaultEmbedColour);
            embed.WithFooter(Bot.Internal.Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            await client.SetGameAsync(game);
            await ReplyAsync("", false, embed);
        }

        [Command("Status")]
        [RequireOwner]
        public async Task SetBotStatus(string status)
        {
            var embed = new EmbedBuilder();
            embed.WithDescription($"Set the status to {status}.");
            embed.WithFooter(Bot.Internal.Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            embed.WithColor(Bot.Internal.Config.bot.DefaultEmbedColour);

            var client = Bot.Internal.Program._client;

            switch (status)
            {
                case "dnd":
                    await client.SetStatusAsync(UserStatus.DoNotDisturb);
                    break;
                case "idle":
                    await client.SetStatusAsync(UserStatus.Idle);
                    break;
                case "online":
                    await client.SetStatusAsync(UserStatus.Online);
                    break;
                case "offline":
                    await client.SetStatusAsync(UserStatus.Invisible);
                    break;
            }

            await ReplyAsync("", false, embed);
        }

        [Command("LeaveServer")]
        [RequireOwner]
        public async Task LeaveServer()
        {
            var embed = new EmbedBuilder();
            embed.WithDescription(Bot.Internal.Utilities.GetLocaleMsg("BotLeftServer"));
            embed.WithColor(Bot.Internal.Config.bot.DefaultEmbedColour);
            embed.WithFooter(Bot.Internal.Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            await ReplyAsync("", false, embed);
            await Context.Guild.LeaveAsync();
        }

        [Command("ServerCount"), Alias("Sc")]
        [RequireOwner]
        public async Task ServerCountStream()
        {
            var client = Bot.Internal.Program._client;
            var guilds = client.Guilds.Count;
            var embed = new EmbedBuilder();
            embed.WithDescription("Done.");
            embed.WithColor(Bot.Internal.Config.bot.DefaultEmbedColour);
            embed.WithFooter(Bot.Internal.Utilities.GetFormattedLocaleMsg("CommandFooter", Context.User.Username));
            await ReplyAsync("", false, embed);
            await client.SetGameAsync($"in {guilds} servers!", $"https://twitch.tv/{Bot.Internal.Config.bot.TwitchStreamer}", StreamType.Twitch);

        }
    }
}
