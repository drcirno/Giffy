using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using Newtonsoft.Json;

namespace Giffy
{
    public class Program
    {

        public DiscordClient Client { get; set; }
        public CommandsNextModule Commands { get; set; }
        public VoiceNextClient voice;
        public static void Main(string[] args)
        {
            var prog = new Program();
            prog.RunBotAsync().GetAwaiter().GetResult();



        }//end Main

        public async Task RunBotAsync()
        {
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
            var cfg = new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };

            this.Client = new DiscordClient(cfg);

            //events
            this.Client.Ready +=this.Client_Ready;
            this.Client.Ready += this.Set_Status;
            this.Client.GuildAvailable += this.Client_GuildAvailable;
            this.Client.ClientErrored += this.Client_ClientError;
            this.Client.ChannelPinsUpdated += this.Client_PinsUpdated;
            this.Client.PresenceUpdated += this.Client_PresenceUpdated;

            var ccfg = new CommandsNextConfiguration
            {
                StringPrefix = cfgjson.CommandPrefix,
                EnableDms = false,
                EnableMentionPrefix = true,
                EnableDefaultHelp = true
            };

            this.Commands = this.Client.UseCommandsNext(ccfg);

            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;

            this.Commands.RegisterCommands<MyCommands>();

            voice = this.Client.UseVoiceNext();
            await Client.ConnectAsync();
            await Task.Delay(-1);

        }//end MainAsync

        private async Task Client_PresenceUpdated(PresenceUpdateEventArgs e)
        {
            ulong channel = 0;
            if (e.Member.Presence.Game != null)
            {
                if (e.Member.Presence.Game.StreamType == GameStreamType.Twitch)
                {
                    {
                        //Stream detected! Search if channel is recorded
                        var guild = e.Guild.Id.ToString();
                        var jsonData = File.ReadAllText("streamchannel.json");


                        dynamic sc = JsonConvert.DeserializeObject(jsonData);


                        bool found = false;
                        int c = sc.SChannel.Count;
                        for (int counter = 0; counter < c; counter++)
                        {
                            if (sc.SChannel[counter].guildID == guild)
                            {
                                found = true;
                                channel = Convert.ToUInt64(sc.SChannel[counter].channelID.ToString());
                            }
                        } // end for
                        if (found == true)
                        {
                            await e.Guild.GetChannel(channel).SendMessageAsync($"@everyone, {e.Member.DisplayName} is now streaming! Why don't you come over and watch?\n\nLink: {e.Member.Presence.Game.Url}");
                        }
                    }
                }
            }
           
            await Task.CompletedTask;

        }

        private async Task Client_PinsUpdated(ChannelPinsUpdateEventArgs e)
        {
            await e.Channel.SendMessageAsync("@everyone, New pin created!");
        }

        private Task Client_Ready(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "Giffy", "Client is ready to process events.", DateTime.Now);
            return Task.CompletedTask;
        }
        private async Task Set_Status(ReadyEventArgs e)
        {
            var g = new DiscordGame {Name="Online and Ready!" };
            await Client.UpdateStatusAsync(g);
        }
        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "Giffy", $"Guild available: {e.Guild.Name}", DateTime.Now);
            return Task.CompletedTask;
        }
        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "Giffy", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            return Task.CompletedTask;
        }
        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "Giffy", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "Giffy", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            // let's check if the error is a result of lack
            // of required permissions
            if (e.Exception is ChecksFailedException ex)
            {
                // yes, the user lacks required permissions, 
                // let them know

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                    // there are also some pre-defined colors available
                    // as static members of the DiscordColor struct
                };
                await e.Context.RespondAsync("", embed: embed);
            }
        }
    }

    // this structure will hold data from config.json
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }
    }
}

