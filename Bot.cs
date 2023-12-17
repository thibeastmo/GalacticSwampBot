using System;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using GalacticSwampBot.Library.OCR;

namespace GalacticSwampBot
{

    public class Bot
    {
        private const string Version = "1.0.0";

        private const string Token = "";//GalacticSwampBot
        // private const string Token = "";//Storage Bot

        // public static string Token = ""; //testbeastv1
        private static DiscordClient _discordClient;
        private BackgroundWorker _backgroundWorker;
        private bool _guildDownloadCompleted = false;

        public async Task RunAsync()
        {
            _discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            });

            await _discordClient.ConnectAsync();
            //Events
            _discordClient.MessageCreated += DiscordClient_MessageCreated;
            _discordClient.GuildDownloadCompleted += DiscordClient_GuildDownloadCompleted;
            _discordClient.MessageReactionAdded += DiscordClientOnMessageReactionAdded;
            _discordClient.MessageReactionRemoved += DiscordClientOnMessageReactionRemoved;

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += BackgroundWorkerOnDoWork;
            
            
            
            StartBackgroundTask();

            await Task.Delay(-1);
        }
        private void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            _ = Task.Run(async () => {
                const string dir = "./Processing";
                if (!Directory.Exists(dir)){
                    Directory.CreateDirectory(dir);
                }
                while (!_guildDownloadCompleted){
                    Thread.Sleep(500);
                }
                var guild = await _discordClient.GetGuildAsync(Constants.Guilds.GALACTIC_SWAMP);
                var rawChannel = guild.GetChannel(Constants.Channels.RAW);
                var processedChannel = guild.GetChannel(Constants.Channels.PROCESSED);
                var processedbackupChannel = guild.GetChannel(Constants.Channels.PROCESSED_BACKUP);
                var rawbackupChannel = guild.GetChannel(Constants.Channels.RAW_BACKUP);
                var channels = new []
                {
                    processedChannel,
                    rawbackupChannel,
                    processedbackupChannel
                };
                try{
                    if (!string.IsNullOrEmpty(rawChannel.Topic)){
                        rawChannel.ModifyAsync(c => c.Topic = string.Empty).Wait();
                        rawbackupChannel.ModifyAsync(c => c.Topic = string.Empty).Wait();
                    }
                }
                catch (Exception ex){
                    _discordClient.Logger.LogError("Could not modify rawchannel and/or raw backup channel.");
                }
                while (true){
                    var messages = await rawChannel.GetMessagesAsync(1);
                    if (messages.Any()){
                        var message = messages.Last();
                        if (message.Author.Id == Constants.Users.BOT || message.Author.Id == Constants.Users.BOT_STORAGE){
                            await HandleProcessing(dir, message, channels);
                            await message.DeleteAsync();
                        }
                        else{
                            await message.DeleteAsync();
                        }
                    }
                    else{
                        var totalMilliSeconds = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
                        var dtAfter = DateTime.Now.AddMilliseconds(totalMilliSeconds);
                        try{
                            rawChannel.ModifyAsync(c => c.Topic = "#raw is empty! Waiting untill " + dtAfter).Wait();
                            rawbackupChannel.ModifyAsync(c => c.Topic = "#raw is empty! Waiting untill " + dtAfter).Wait();
                        }
                        catch{
                            // ignored
                        }
                        if (!IsWinter(dtAfter)){
                            dtAfter = dtAfter.AddHours(1);
                        }
                        Thread.Sleep(totalMilliSeconds);
                        try{
                            rawChannel.ModifyAsync(c => c.Topic = string.Empty).Wait();
                            rawbackupChannel.ModifyAsync(c => c.Topic = string.Empty).Wait();
                        }
                        catch{
                            // ignored
                        }
                    }
                }
            });
        }
        private async Task HandleProcessing(string dir, DiscordMessage message, DiscordChannel[] channels)
        {
            if (!message.Attachments.Any()) return;
            var titleAttachment = message.Attachments.First(a => a.FileName.ToLower().Contains("title"));
            var playersAttachments = message.Attachments.Where(a => a.FileName.ToLower().Contains("player"));
            var starbasesAttachments = message.Attachments.Where(a => a.FileName.ToLower().Contains("starbase"));
            var levelsAttachments = message.Attachments.Where(a => a.FileName.ToLower().Contains("level"));
            try{
                foreach(var attachment in message.Attachments){
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(new Uri(attachment.Url.Split('?')[0]), dir + "/" + Path.GetFileName(attachment.Url.Split('?')[0]));
                    }
                }
            }
            catch (Exception ex){
                return;
            }
            
            //initialize
            var title = new Title(dir, Path.GetFileName(titleAttachment.Url.Split('?')[0]));
            var players = new Players(dir, playersAttachments.Select(pa => Path.GetFileName(pa.Url.Split('?')[0])).OrderBy(x => x).ToArray());
            var levels = new Levels(dir, levelsAttachments.Select(sa => Path.GetFileName(sa.Url.Split('?')[0])).OrderBy(x => x).ToArray());
            var starbases = new Starbases(dir, starbasesAttachments.Select(la => Path.GetFileName(la.Url.Split('?')[0])).OrderBy(x => x).ToArray());

            var ocrHandler = new OCRHandler(title, players, levels, starbases);
            
            //read with ocr
            var rawChannels = channels.Where(c => c.Id == Constants.Channels.RAW_BACKUP).ToList();
            rawChannels.Add(message.Channel);
            var processedResult = ocrHandler.Read(rawChannels.ToArray());
            try{
                foreach (var channel in channels){
                    if (channel.Id != Constants.Channels.RAW_BACKUP && !processedResult.AnyWorthy) continue;
                    //send json + images to processed & processed_backup
                    var dmb = new DiscordMessageBuilder
                    {
                        Content = channel.Id == Constants.Channels.RAW_BACKUP ? processedResult.All : processedResult.Worthy
                    };
                    var dict = new Dictionary<string, Stream>();
                    if (channel.Id == Constants.Channels.RAW_BACKUP){
                        var titleFile = dir + "/" + Path.GetFileName(titleAttachment.Url.Split('?')[0]);
                        dict.Add(titleFile,  new FileStream(titleFile, FileMode.Open));
                        foreach (var levelsAttachment in levelsAttachments){
                            var file = dir + "/" + Path.GetFileName(levelsAttachment.Url.Split('?')[0]);
                            dict.Add(file, new FileStream(file, FileMode.Open));
                        }
                        foreach (var starbasesAttachment in starbasesAttachments){
                            var file = dir + "/" + Path.GetFileName(starbasesAttachment.Url.Split('?')[0]);
                            dict.Add(file, new FileStream(file, FileMode.Open));
                        }
                    }
                    else{
                        var file = dir + "/" + Constants.ImageNames.WORTHY;
                        dict.Add(file, new FileStream(file, FileMode.Open));
                    }
                    for(var i = 0; i < playersAttachments.Count(); i++){
                        if (channel.Id == Constants.Channels.RAW_BACKUP || processedResult.WorthyFromRows.Contains(i)){
                            var file = dir + "/" + Path.GetFileName(playersAttachments.ElementAt(i).Url.Split('?')[0]);
                            dict.Add(file, new FileStream(file, FileMode.Open));
                        }
                    }
                    dmb.WithFiles(dict);
                    var discordMessage = await dmb.SendAsync(channel);
                    foreach (var stream in dict){
                        stream.Value.Close();
                    }
                    if (processedResult.Tag){
                        // await discordMessage.RespondAsync("<@239109910321823744> <@303920206806515713>");
                    }
                }
            }
            catch (Exception e){
                Console.WriteLine(e);
            }
        }

        private string AdaptFileName(string fileName)
        {
            if (!fileName.EndsWith(".png")){
                var splitted = fileName.Split('.');
                var sb = new StringBuilder();
                for (var j = 0; j < splitted.Length-1; j++){
                    if (j > 0){
                        sb.Append('.');
                    }
                    sb.Append(splitted[j]);
                }
                sb.Append(".png");
                fileName = sb.ToString();
            }
            return fileName;
        }

        #region Events

        private Task DiscordClient_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.Channel.Id == Constants.Channels.BATTLE_LOGS_SCREENS && e.Author.Id != Constants.Users.BOT && e.Author.Id != Constants.Users.BOT_WINNERS){
                    var content = await GetMessageContent(e.Channel, e.Message.Id);
                    if (content.StartsWith('\\')) return;
                    BattleLog bl = BattleLog.Parse(content);
                    if (bl != null){
                        int result = await bl.AddToDatabase();
                        switch (result){
                            case 1:
                                await e.Channel.SendMessageAsync($"Colony n°{bl.PlanetNumber} of {bl.PlayerName} updated.");
                                break;
                            case 2:
                                await e.Channel.SendMessageAsync($"Colony n°{bl.PlanetNumber} of {bl.PlayerName} added.");
                                break;
                            case 3:
                                await e.Channel.SendMessageAsync($"**{bl.PlayerName}** doesn't exist as a player.");
                                break;
                            case 4:
                                await e.Channel.SendMessageAsync($"Colony not found.");
                                break;
                            case 5:
                                await e.Channel.SendMessageAsync($"The coordinates are usually not negative values.");
                                break;
                            case 6:
                                await e.Channel.SendMessageAsync($"The main base coordinates will not be stored.");
                                break;
                            default:
                                await e.Channel.SendMessageAsync($"Colony didn't update. ```{bl}```");
                                break;
                        }
                    }
                    else{
                        await e.Channel.SendMessageAsync($"Please use the format of the battlelog here:\n`<player name> (<planet>: <x>, <y>)`");
                    }
                }
                else if (e.Channel.Id == Constants.Channels.ATTACK_LOGS && e.Author.Id == Constants.Users.BOT){
                    var rh = new RegenHandler(e.Guild, e.Message.Content);
                    await rh.Handle();
                }
            });
            return Task.CompletedTask;
        }
        private Task DiscordClient_GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            _discordClient.Logger.Log(LogLevel.Information, "Client( v" + Version + " ) is ready to process events.");
            _guildDownloadCompleted = true;
            _discordClient.GetGuildAsync(Constants.Guilds.GALACTIC_SWAMP).Result.GetMemberAsync(Constants.Users.THIBEASTMO).Result.SendMessageAsync("Booted up "+ Assembly.GetEntryAssembly()?.GetName().Name+" from " + Environment.MachineName);
            return Task.CompletedTask;
        }
        public static Task WriteInfoLog(string text)
        {
            _discordClient.Logger.Log(LogLevel.Information, text);
            return Task.CompletedTask;
        }
        
        private async Task DiscordClientOnMessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {
            if (e.User.Id != Constants.Users.BOT){
                var rh = new RoleHandler(sender, e.Message, e.Emoji, e.User, false);
                await rh.Handle();
            }
        }
        private async Task DiscordClientOnMessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            if (e.User.Id != Constants.Users.BOT){
                var rh = new RoleHandler(sender, e.Message, e.Emoji, e.User, true);
                await rh.Handle();
            }
        }
        #endregion
        private async Task<string> GetMessageContent(DiscordChannel discordChannel, ulong messageId)
        {
            var message = await discordChannel.GetMessageAsync(messageId);
            return message.Content;
        }

        private void StartBackgroundTask()
        {
            while (_backgroundWorker == null){
                Thread.Sleep(100);
            }
            if (!_backgroundWorker.IsBusy){
                _backgroundWorker.RunWorkerAsync();
            }
        }
        public static bool IsWinter(DateTime aDatetime)
        {
            DateTime dateTime1 = new DateTime(aDatetime.Year, 10, 1);
            List<DateTime> dateTimeList1 = new List<DateTime>();
            for (int index = 0; index < 31; ++index)
            {
                if (dateTime1.DayOfWeek == DayOfWeek.Sunday)
                    dateTimeList1.Add(dateTime1);
                dateTime1 = dateTime1.AddDays(1.0);
            }
            dateTime1 = dateTimeList1[dateTimeList1.Count - 1];
            DateTime dateTime2 = new DateTime(aDatetime.Year, 3, 1);
            List<DateTime> dateTimeList2 = new List<DateTime>();
            for (int index = 0; index < 31; ++index)
            {
                if (dateTime2.DayOfWeek == DayOfWeek.Sunday)
                    dateTimeList2.Add(dateTime2);
                dateTime2 = dateTime2.AddDays(1.0);
            }
            dateTime2 = dateTimeList2[dateTimeList2.Count - 1];
            return aDatetime >= dateTime1 || dateTime2 < aDatetime;
        }
    }
}
