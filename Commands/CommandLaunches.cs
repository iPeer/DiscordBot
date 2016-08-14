﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Threading;
using Newtonsoft.Json.Linq;
using DiscordBot.Utilities;
using DiscordBot.Extensions;

namespace DiscordBot.Commands
{
    public class CommandLaunches : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.NORMAL_USER;
        }

        public void customInvoke(MessageEventArgs e, bool pub, bool past = false)
        {
            Thread t = new Thread(new ThreadStart(() => runThread(e, pub, past)));
            t.Name = "LaunchBot Launches thread";
            t.IsBackground = true;
            t.Start();
        }

        public override void invoke(MessageEventArgs e, bool pub)
        {
            e.Channel.SendIsTyping();
            customInvoke(e, pub);
        }

        private void runThread(MessageEventArgs e, bool pub, bool past) // I was orignally going to hack this into CommandNextLaunch but decided against it.
        {

            int limit = 5;

            // Should we allow this?
            try
            {
                limit = Convert.ToInt32(e.Message.Text.ToLower().Split(' ')[1]);
            }
            catch { }

            JObject _json = JObject.Parse(Utils.getWebPage("https://ipeer.auron.co.uk/launchschedule/api/1/launches?limit="+limit+"&omitapidata=1" + (past ? "&history=1" : "")));
            JToken launches = _json["launches"];
            int maxCount = launches.Count();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormattedLine("{0} next upcoming launches:", maxCount);

            foreach (JToken l in launches)
            {
                string vehicle = l["vehicle"].ToString();
                string payload = l["payload"].ToString();

                DateTime launch = DateTime.Parse(l["launchtime"].ToString())/*.ToUniversalTime()*/;
                string date;
                string timeStamp = LaunchUtils.getFormattedTime(l, out date);
                string final = String.Format("{0}/{1} — {2} {3}", vehicle, payload, date, timeStamp);

                sb.AppendFormattedLine("\t{0}", final);
            }

            e.Channel.SendMessage(sb.ToString());

        }


    }
}