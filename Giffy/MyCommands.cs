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

        [Command("test")]
        public async Task Test(CommandContext ctx)
        {
            CelerityIO cio = new CelerityIO();
            List<CeleritySpell> celeritySpells = cio.readSpellList();
            await ctx.RespondAsync(celeritySpells[0].title);
        }

        [Group("d20celerity", CanInvokeWithoutSubcommand = true)]
        [Description("d20celerity library commands.")]
        [Aliases("d20")]
        public class D20celerity 
        {
            public async Task ExecuteGroupAsync(CommandContext ctx) 
            {
                await ctx.RespondAsync("***__Syntax:__***\n***Spells:***\n```/d20 spell Spell_Name``````/d20 spell [class Spell_Class] [level Spell_Level]```");
            }

            [Command("spell"), Aliases("s"), Description("Spells library")]
            public async Task Spell(CommandContext ctx, params string[] args)
            {
                string spellClass = "";
                int spellLevel = -1;
                bool nameSearch = true;

                //Determine if user looked for name or spell class/level
                for (int i = 0; i < args.Length; i++) {
                    args[i] = args[i].ToLower();
                    if (args[i].Equals("class")) {
                        spellClass = args[i + 1];
                        spellClass = Char.ToUpper(spellClass[0]) + spellClass.Substring(1);
                        nameSearch = false;
                    }
                    if (args[i].Equals("level")) {
                        if (!Int32.TryParse(args[i + 1], out spellLevel)) {
                            await ctx.RespondAsync("Invalid spell level!");
                            return;
                        }
                        nameSearch = false;
                    }
                } // end validation

                // Loads spell database
                CelerityIO cio = new CelerityIO();
                List<CeleritySpell> celeritySpells = cio.readSpellList();

                if (nameSearch)
                {
                    // User is searching for spell name
                    string name = String.Join(" ", args);
                    bool found = false;
                    CeleritySpell data = null;
                    foreach( CeleritySpell cs in celeritySpells){
                        if (cs.title.ToLower().Equals(name)) {
                            found = true;
                            data = cs;
                            break;
                        }
                    } // end foreach

                    if (!found)
                    {
                        await ctx.RespondAsync("Spell not found: " + name);
                        return;
                    }
                    else 
                    {
                        // Found a corresponding spell
                        Console.WriteLine("Spell Found: " + name + "\n");
                        Console.WriteLine(data.ToString());
                        DiscordEmbedBuilder embed = new DiscordEmbedBuilder { Title = data.title, Color = new DiscordColor(1621076), Url = data.link };
                        if (!data.category.Equals("null"))
                            embed.AddField("Category", data.category);
                        embed.AddField("Class Level", data.spellLevelsToString());
                        if (!data.components.Equals("null"))
                            embed.AddField("Components", data.components);
                        if (!data.castingTime.Equals("null"))
                            embed.AddField("Casting Time", data.castingTime);
                        if (!data.range.Equals("null"))
                            embed.AddField("Range", data.range);
                        if (!data.target.Equals("null"))
                            embed.AddField("Target", data.target);
                        if (!data.duration.Equals("null"))
                            embed.AddField("Duration", data.duration);
                        if (!data.effect.Equals("null"))
                            embed.AddField("Effect", data.effect);
                        if (!data.savingThrow.Equals("null"))
                            embed.AddField("Saving Throw", data.savingThrow);
                        embed.AddField("Description", data.body);

                        await ctx.RespondAsync(embed: embed);
                        return;

                    }
                }
                else 
                {
                    List<string> result = new List<string>();
                    int match = 0;
                    foreach (CeleritySpell cs in celeritySpells)
                    {
                        foreach (CeleritySpell.SpellLevel sl in cs.spellLevel) 
                        {
                            bool found = true;
                            // If spell class is to be found
                            if (!spellClass.Equals("")) 
                            {
                                if (!sl.caster.Equals(spellClass)) found = false;
                            }
                            // If spell level is to be found
                            if (spellLevel != -1) 
                            {
                                if (sl.level != spellLevel) found = false;
                            }
                            if (found) 
                            {
                                match++;
                                result.Add(cs.title);
                                break;
                            }
                        }
                    } // end foreach
                    string ret = "";
                    if (result.Count() == 0) ret = "None";
                    else ret = String.Join("\n", result);
                    Console.WriteLine("Result length: " + ret.Length.ToString());

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder { Title = "Search Result", Color = new DiscordColor(1621076) };
                    if (ret.Length <= 1024)
                        embed.AddField(match.ToString() + " results found", ret);
                    else {
                        embed.AddField(match.ToString() + " results found", ret.Substring(0, 1024));
                        int counter = 1;
                        bool stop = false;
                        while (!stop) {
                            int lineLength = 1024;
                            if (ret.Length <= 1024 * (counter + 1)) {
                                stop = true;
                                lineLength = ret.Length - (1024 * counter);
                            }
                            embed.AddField("--", ret.Substring(1024 * counter, lineLength));
                            counter++;
                        }
                    }

                    await ctx.RespondAsync(embed: embed);
                    return;

                }
            }
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
            int d = 0, rand = 0, now = 0, total = 0, subtotal = 0, drop = 0, reroll = 0;
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
                    reroll = result.IndexOf('r', 0);

                    //if number is not a dice
                    if (d == -1) {/*Stupid-proof #1*/
                        if (drop != -1) { await ctx.RespondAsync("Can't drop a number."); return; }
                        if (reroll != -1) { await ctx.RespondAsync("Can't reroll a number."); return; }
                        now = Int32.Parse(result); display += result; }
                    else
                    {
                        //it's a dice
                        string hold = "";
                        String[] temp2 = { null, null, null };
                        String[] temp3 = { null, null };
                        String[] temp4 = { null, null };
                        bool dropthis = false;
                        bool rerollthis = false;
                        int dropnum = 0;
                        int rerollnum = 0;
                        if (drop != -1 && reroll != -1)
                        {
                            dropthis = true; rerollthis = true;
                            if (drop < reroll) { temp3 = result.Split('/'); temp2[0] = temp3[0]; temp4 = temp3[1].Split('r');  temp2[1] = temp4[0]; temp2[2] = temp4[1]; }
                            if (reroll < drop) { temp3 = result.Split('r'); temp2[0] = temp3[0]; temp4 = temp3[1].Split('/');  temp2[1] = temp4[1]; temp2[2] = temp4[0]; }
                            dropnum = Int32.Parse(temp2[1]);
                            rerollnum = Int32.Parse(temp2[2]);
                            hold = temp2[0];
                            
                        }
                        else if (drop != -1) { dropthis = true; temp3 = result.Split('/'); hold = temp3[0]; dropnum = Int32.Parse(temp3[1]); }
                        else if (reroll != -1) { rerollthis = true; temp3 = result.Split('r');  hold = temp3[0]; rerollnum = Int32.Parse(temp3[1]); }
                        else { hold = result; }


                        int a = Int32.Parse(hold.Substring(0, d));
                        int b = Int32.Parse(hold.Substring(d + 1));

                        if (rerollnum > b) rerollthis = false; // disable reroll if reroll number is higher than base

                        if (a > 500 || b > 10000)
                        {
                            await ctx.RespondAsync("You're having too much fun with this dice roll now.");
                            return;
                        }

                        if (dropthis == true && a <= dropnum) { now = 0; display += "0"; } // if dropped number is bigger than given dice, result is just 0.
                        else
                        {
                            int[] numstore = new int[a]; int[] droplist = new int[dropnum];
                            List<string> strstore = new List<string>();
                            List<int> rerolllist = new List<int>(); // rerolllist
                            List<int> numindex = new List<int>(); // Index of survived numbers
                            //gen arrays for calc
                            int realindex = 0;
                            for (int k = 0; k < a; k++)
                            {
                                rand = r.Next(1, b + 1);
                                if (rerollthis == true) {
                                    while (rand == rerollnum) { strstore.Add("~~" + rand.ToString() + "~~"); rand = r.Next(1, b + 1); realindex++; 
                                    } // reroll
                                } //endif
                                numstore[k] = rand;
                                numindex.Add(realindex);
                                strstore.Add(rand.ToString());
                                realindex++;
                            }//end for
                            //drop lowest n
                            if (dropthis == true)
                            {
                                for (int k = 0; k < dropnum; k++)
                                {
                                    int lowest = numstore.Min();
                                    int index = Array.IndexOf(numstore, lowest);
                                    numstore[index] = b + 1;
                                    droplist[k] = numindex[index];
                                }//end for
                            }

                            //cross out numbers from string storage
                            foreach (int k in droplist) { strstore[k] = "~~" + strstore[k] + "~~"; }


                            display += "{";
                            display += String.Join(", ", strstore.ToArray());
                            display += "}";
                            //calc total
                            now = numstore.Sum() - (dropnum * (b + 1));


                        }//end else
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

            int hp = str + dex + cha + tal;
            if (vit == 4) hp += mag;
            hp = hp * vit + dex;
            int meleeDamage = str * 2 + dex + cha;
            int rangedDamage = dex + cha;
            int bowDamage = str + dex + cha;
            className = char.ToUpper(className[0]) + className.Substring(1);
            var embed = new DiscordEmbedBuilder { Title = "Celerity Character Information", Description = "Here is a list of data to facilitate character sheet generation of Celerity Lite.", Color = new DiscordColor(1621076) };
            embed.AddField("Class", className);
            embed.AddField("Strength", strength, true);
            embed.AddField("Dexterity", dexterity, true);
            embed.AddField("Charisma", charisma, true);
            embed.AddField("Technique", technique, true);
            embed.AddField("Magic", magic, true);
            embed.AddField("Expertise", talents, true);
            embed.AddField("Hit Points", hp.ToString());
            embed.AddField("Melee Damage Bonus", "+" + meleeDamage.ToString());
            embed.AddField("Ranged Damage Bonus", "+" + rangedDamage.ToString());
            embed.AddField("Bow Damage Bonus", "+" + bowDamage.ToString());

            await ctx.RespondAsync(embed: embed);

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
