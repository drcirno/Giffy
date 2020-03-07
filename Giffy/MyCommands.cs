using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using DSharpPlus.VoiceNext;
using System.Diagnostics;
using VideoLibrary;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Model;
using System.Linq;

namespace Giffy
{
    public class MyCommands
    {
        [Command("ping")]
        [Description("Ping!")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");
            await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms");

        }

        [Command("navyseal")]
        [Aliases("n")]
        [Description("What the did you just say about me?")]
        public async Task NavySeal(CommandContext ctx, [Description("Member to fling this insult to.")] DiscordMember member)
        {
            await ctx.RespondAsync("Hey don't say that to me.");
        }

        [Command("hi")]
        [Description("I greet you!")]
        public async Task Hi(CommandContext ctx)
        {
            // fakes bot typing
            await ctx.TriggerTypingAsync();

            var emoji = DiscordEmoji.FromName(ctx.Client, ":wave:");
            await ctx.RespondAsync($"{emoji} Hi, {ctx.User.Mention}!");
        }


        [Command("greet")]
        [Description("I greet the specified member")]
        public async Task Greet(CommandContext ctx, [Description("Target.")] DiscordMember member)
        {
            await ctx.TriggerTypingAsync();
            var emoji = DiscordEmoji.FromName(ctx.Client, ":wave:");
            await ctx.RespondAsync($"{emoji} Hello, {member.Mention}!");
        }

        [Command("roll")]
        [Aliases("r")]
        [Description("Rolls the dice")]
        public async Task Roll(CommandContext ctx, [Description("Use d, +, or - as applicable! Example: 1d20+5, 2d10-4d6+35")] String rollstring)
        {
            await ctx.TriggerTypingAsync();
            //Separate string
            var temp = new List<String>();
            int position = 0, plus = 0, minus = 0;
            do
            {
                plus = rollstring.IndexOf('+', 0);
                minus = rollstring.IndexOf('-', 0);


                if (plus != -1 && minus != -1)
                {
                    if (plus > minus) { position = minus; }
                    if (plus < minus) { position = plus; }
                }

                else if (plus == -1 && minus == -1) { position = -1; }
                else
                {
                    if (plus == -1) position = minus;
                    if (minus == -1) position = plus;
                }

                if (position >= 0)
                {
                    temp.Add(rollstring.Substring(0, position).Trim());
                    if (position == plus) temp.Add("+");
                    else if (position == minus) temp.Add("-");
                    rollstring = rollstring.Substring(position+1);
                }

            } while (position > 0);
            temp.Add(rollstring);


            //calculation
            bool adding = false, subtracting = false;
            int d = 0, rand = 0, now = 0, total = 0, subtotal = 0, drop = 0;
            Random r = new Random();
            string display = "";
            foreach (var result in temp)
            {
                //found operators
                if (result == "+" || result == "-")
                {
                    if (result == "+" && adding == false) { adding = true; display += " + "; }
                    if (result == "-" && subtracting == false) { subtracting = true; display += " - "; }
                }
                //number
                else
                {
                    string num = result.ToLower();
                    d = result.IndexOf('d', 0);
                    drop = result.IndexOf('/', 0);

                    //if number is not a dice
                    if (d == -1) {/*Stupid-proof #1*/if (drop != -1) { await ctx.RespondAsync("Can't drop a number."); return; } now = Int32.Parse(result); display += result; }
                    else
                    {
                        //it's a dice
                        string hold = "";
                        String[] temp2 = { null, null };
                        bool dropthis = false;
                        int dropnum = 0;
                        if (drop != -1) { dropthis = true; temp2 = result.Split('/');  hold = temp2[0]; dropnum = Int32.Parse(temp2[1]); }
                        else { hold = result; }


                        int a = Int32.Parse(hold.Substring(0, d));
                        int b = Int32.Parse(hold.Substring(d + 1));

                        if (a > 500 || b > 10000)
                        {
                            await ctx.RespondAsync("You're having too much fun with this dice roll now.");
                            return;
                        }

                        if (dropthis == true)
                        {
                            if (a <= dropnum) { now = 0; display += "0"; } // if dropped number is bigger than given dice, result is just 0.
                            else
                            {
                                int[] numstore = new int[a]; string[] strstore = new string[a]; int[] droplist = new int[dropnum];
                                //gen arrays for calc
                                for (int k = 0; k < a; k++)
                                {
                                    rand = r.Next(1, b + 1);
                                    numstore[k] = rand;
                                    strstore[k] = rand.ToString();
                                }//end for

                                //drop lowest n
                                for (int k = 0; k < dropnum; k++)
                                {
                                    int lowest = numstore.Min();
                                    int index = Array.IndexOf(numstore, lowest);
                                    numstore[index] = b + 1;
                                    droplist[k] = index;
                                }//end for

                                //cross out numbers from string storage
                                foreach (int k in droplist) { strstore[k] = "~~" + strstore[k] + "~~"; }


                                display += "{";

                                //gen string
                                int cnt = 0;
                                foreach (string k in strstore) { display += k; if (cnt < a - 1) display += ", "; else display += "}";cnt++; }
                                //calc total
                                now = numstore.Sum() - (dropnum * (b + 1));


                            }//end else
                        }//endif
                        else
                        {

                            display += "{";
                            subtotal = 0;
                            for (int k = 0; k < a; k++)
                            {
                                rand = r.Next(1, b + 1);
                                subtotal += rand;
                                display += rand.ToString();

                                if (k < a - 1) display += ", ";
                                else display += "}";
                            } // end for

                            now = subtotal;
                        }
                    } // end conditionals

                    //CALCULATE TIME
                    //If there was no operation beforehand, number = total
                    if (adding == false && subtracting == false) total = now;
                    else
                    {
                        if (adding == true) { total += now; adding = false; }
                        else if (subtracting == true) { total -= now; subtracting = false; }
                    }
                    

                }// end main number if
            }// end foreach

            if (display.Length > 2000) await ctx.RespondAsync("Did you know that discord can only take up to 2000 characters per post?");
            else await ctx.RespondAsync($"Rolls: {display}\nTotal: {total}");
        }

        [Command("talk")]
        [Aliases("t")]
        [Description("Some ventriloquism")]
        public async Task Talk(CommandContext ctx, [Description("Message to fake.")] [RemainingText] String message)
        {

            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(message);
        }

        [Command("setsc")]
        [Description("Set Stream-announcement Channel. Use this command on the channel you want the announcement to go to.")]
        public async Task SetSC(CommandContext ctx)
        {
            var channel = ctx.Channel.Id.ToString();
            var guild = ctx.Guild.Id.ToString();
            var jsonData = File.ReadAllText("streamchannel.json");

            dynamic sc = JsonConvert.DeserializeObject(jsonData);
            
            
            bool rep = false; int found = 0;
            int c = sc.SChannel.Count;

            for (int counter = 0; counter < c; counter++)
            {
                if (sc.SChannel[counter].guildID == guild)
                {
                    rep = true;
                    found = counter;
                }
            }

            string output = "";
            if (rep == true)
            {                
                sc.SChannel[found].channelID = channel;
                await ctx.RespondAsync("Stream channel successfully changed!");
                output = JsonConvert.SerializeObject(sc);
            }
            else
            {
                string temp = JsonConvert.SerializeObject(sc);
                output = temp.Substring(0, temp.Length - 2) + ",{\"guildID\":\"" + guild + "\",\"channelID\":\"" + channel + "\"}]}";
                Console.WriteLine(output);

                await ctx.RespondAsync("Stream channel successfully recorded!");
            }
            

            File.WriteAllText("streamchannel.json", output);
        }

        [Command("ajoin")]
        [Description("Make the bot join Voice Channel")]
        public async Task AJoin(CommandContext ctx)
        {
            var vn = ctx.Client.GetVoiceNextClient();
            var vnc = vn.GetConnection(ctx.Guild);
            if (vnc != null) throw new InvalidOperationException("Already conneted in this guild.");
            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null) throw new InvalidOperationException("You need to be in a voice channel");


            vnc = await vn.ConnectAsync(chn);
            await ctx.RespondAsync("Voice chat joined");

        }

        [Command("aleave")]
        [Description("Make the bot leave Voice Channel")]
        public async Task ALeave(CommandContext ctx)
        {
            var vn = ctx.Client.GetVoiceNextClient();
            var vnc = vn.GetConnection(ctx.Guild);
            if (vnc == null) throw new InvalidOperationException("Not connected in this guild.");
            vnc.Disconnect();
            await ctx.RespondAsync("Left the Voice chat.");
        }

        [Command("play")]
        [Description("Play that tune if it exists.")]
        public async Task Play(CommandContext ctx, string music)
        {
            //errcheck
            var vnext = ctx.Client.GetVoiceNextClient();
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            string file = @"audio/"+music+".mp3";
            if (!File.Exists(file)) {await ctx.RespondAsync("Tune not available!"); return; };

            //fake talking
            await vnc.SendSpeakingAsync(true);

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""{file}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var ffmpeg = Process.Start(psi);
            var ffout = ffmpeg.StandardOutput.BaseStream;

            var buff = new byte[3840];
            var br = 0;
            while ((br = ffout.Read(buff, 0, buff.Length)) > 0)
            {
                if (br < buff.Length) // not a full sample, mute the rest
                    for (var i = br; i < buff.Length; i++) buff[i] = 0;

                await vnc.SendAsync(buff, 20);
            }
            await vnc.SendSpeakingAsync(false);
        }

        [Command("youtube")]
        [Aliases("y")]
        [Description("Play YouTube music.")]
        public async Task YT(CommandContext ctx, [RemainingText] string url)
        {
            await ctx.Message.DeleteAsync();
            string ch = ctx.Channel.Id.ToString();
            ch = @"audio/" + ch + "/";
            System.IO.Directory.CreateDirectory(ch);

            await ctx.RespondAsync("Processing...");

            var youtube = YouTube.Default;
            var vid = youtube.GetVideo(url);
            var source = ch + "aaaa.mp4";

            File.WriteAllBytes(source, vid.GetBytes());
            var output = ch + "bbbb.mp3";
            var name = vid.FullName;

            await Conversion.ExtractAudio(source, output).Start();

            //errcheck
            var vnext = ctx.Client.GetVoiceNextClient();
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            if (!File.Exists(output)) throw new FileNotFoundException("File not found.");
            await ctx.RespondAsync($"Now Playing: {name}");

            //fake talking
            await vnc.SendSpeakingAsync(true);

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""{output}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var ffmpeg = Process.Start(psi);
            var ffout = ffmpeg.StandardOutput.BaseStream;

            var buff = new byte[3840];
            var br = 0;
            while ((br = ffout.Read(buff, 0, buff.Length)) > 0)
            {
                if (br < buff.Length) // not a full sample, mute the rest
                    for (var i = br; i < buff.Length; i++) buff[i] = 0;

                await vnc.SendAsync(buff, 20);
            }
            await vnc.SendSpeakingAsync(false);
            File.Delete(output);
            File.Delete(source);

        }
        [Command("chelp")]
        [Description("Displays help for celerity command")]
        public async Task CelerityHelp(CommandContext ctx)
        {
            await ctx.RespondAsync("Syntax:\n\n/celerity ClassName Str Dex Cha Tech Magic Talents");
        }

        [Command("celerity")]
        [Aliases("c")]
        [Description("Displays calculated Celerity Lite stats")]
        public async Task CelerityStats(CommandContext ctx, string className, string strength, string dexterity, string charisma, string technique, string magic, string talents)
        {
            className = className.ToLower();
            int str = 0, dex = 0, cha = 0, tech = 0, mag = 0, tal = 0;
            // Validates numberness
            try
            {
                str = Int32.Parse(strength);
                dex = Int32.Parse(dexterity);
                cha = Int32.Parse(charisma);
                tech = Int32.Parse(technique);
                mag = Int32.Parse(magic);
                tal = Int32.Parse(talents);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await ctx.RespondAsync("Invalid number!");
                return;
            }
            // Creates class map <name, vitality>
            var classList = new Dictionary<string, int>();
            classList.Add("berserker", 12);
            classList.Add("knight", 11);
            classList.Add("soldier", 10);
            classList.Add("monk", 9);
            classList.Add("priest", 8);
            classList.Add("dancer", 7);
            classList.Add("songstress", 7);
            classList.Add("merchant", 6);
            classList.Add("thief", 5);
            classList.Add("scholar", 4);
            int vit = 0;
            // Validates vitality
            if (!classList.TryGetValue(className, out vit)) 
            {
                await ctx.RespondAsync("Class not found: " + className);
                return;
            }
            // Validate total points
            int totalPoints = str + dex + cha + tal + tech * tech + mag;
            if (totalPoints != 13) { await ctx.RespondAsync("Points do not add up to 13 points!"); return; }

                int hp = (str + dex + cha) * vit + dex + tal;
            int meleeDamage = str * 2 + dex + cha;
            int rangedDamage = dex + cha;
            int bowDamage = str + dex + cha;
            className = char.ToUpper(className[0]) + className.Substring(1);

            string output = "Class Name: " + className + "\n" + "Str: " + strength + "    " + "Dex: " + dexterity + "\n" + "Cha: " + charisma + "    " + "Tech: " + technique + "\n";
            output = output + "Mag: " + magic + "    " + "Talents: " + talents + "\n\n";
            output = output + "HP: " + hp.ToString() + "\nMelee Damage Modifier : +" + meleeDamage.ToString() + "\nRanged Damage Modifier: +" + rangedDamage.ToString() + "\nBow Damage Modifier: +" + bowDamage.ToString();

            await ctx.RespondAsync(output);

        }

        [Command("statgenr")]
        [Aliases("sr")]
        [Description("Roll D&D stats with required number")]
        public async Task StatGen(CommandContext ctx, string threshold)
        {
            var rand = new Random();
            string output = "", flavor = "[";
            int reqnum = -1;
            if (threshold != null && !threshold.Equals(""))
            {
                try
                {
                    reqnum = Int32.Parse(threshold);
                    if (reqnum < 3 || reqnum > 18) throw new ArgumentException();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await ctx.RespondAsync("Invalid number! The number must be between 3 and 18, inclusive.");
                    return;
                }
            }
            bool metRequirement = false;
            do {
                output = "";
                flavor = "[";
                int sum = 0;
                for (int i = 0; i < 6; i++)
                {
                    if (reqnum == -1) metRequirement = true;
                    int[] dice = new int[4]; string[] dicetext = new string[4];
                    for (int k = 0; k < 4; k++) { dice[k] = rand.Next(1, 7); dicetext[k] = dice[k].ToString(); }
                    int index = Array.IndexOf(dice, dice.Min());
                    dice[index] = 0; dicetext[index] = "~~" + dicetext[index] + "~~";
                    sum = dice.Sum(); output += "{";
                    for (int k = 0; k < 4; k++) { output += dicetext[k]; if (k < 3) output += ", "; } output += "} ";
                    output = output + "[" + sum.ToString() + "]  ";
                    flavor += sum.ToString();
                    if (i < 5) flavor += ", ";
                }
                flavor += "]";
                if (sum == reqnum) metRequirement = true;
            } while(!metRequirement);

            await ctx.RespondAsync($"Here is your generation!\n\n**Roll:** {output}\n\n**Result:** {flavor}");
        }

        [Command("statgen")]
        [Aliases("s")]
        [Description("Roll D&D stats")]
        public async Task StatGen(CommandContext ctx)
        {
            var rand = new Random();
            string output = "", flavor = "[";

            for (int i = 0; i < 6; i++)
            {
                int[] dice = new int[4]; string[] dicetext = new string[4];
                for (int k = 0; k < 4; k++) { dice[k] = rand.Next(1, 7); dicetext[k] = dice[k].ToString(); }
                int index = Array.IndexOf(dice, dice.Min());
                dice[index] = 0; dicetext[index] = "~~" + dicetext[index] + "~~";
                int sum = dice.Sum(); output += "{";
                for (int k = 0; k < 4; k++) { output += dicetext[k]; if (k < 3) output += ", "; }
                output += "} ";
                output = output + "[" + sum.ToString() + "]  ";
                flavor += sum.ToString();
                if (i < 5) flavor += ", ";
            }
            flavor += "]";


            await ctx.RespondAsync($"Here is your generation!\n\n**Roll:** {output}\n\n**Result:** {flavor}");
        }



    }


}
