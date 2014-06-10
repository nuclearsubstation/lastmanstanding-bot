using System;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace lastmanstanding
{
	class MainClass
	{
		public static string version = "0.2.4(R4)";
		static TcpClient client;
		static StreamWriter writer;
		static StreamReader reader;
		static NetworkStream stream;
		static bool verbose = false;

		public static void Main (string[] args)
		{
			string nick = "LastManStanding";
			string host = "irc.freenode.net";
			int port = 6667;
			string channel = "#lms_default";
			string auxConfigFilename = "config.cfg";
			bool badwordfilter = true;
			string password = "<default>";


			// Read config values
			StreamReader configReader = new StreamReader (auxConfigFilename);
			string[] configArray = new string[10000];
			for (int i = 0; !configReader.EndOfStream; i++) {
				configArray [i] = configReader.ReadLine ();
			}

			ConfigValues alreadyReadConfig = Configuration.ProcessConfig (configArray);

			try {
				if (alreadyReadConfig.name != null) {
					nick = alreadyReadConfig.name;
				}
				if (alreadyReadConfig.host != null) {
					host = alreadyReadConfig.host;
				}
				if (alreadyReadConfig.channel != null) {
					channel = alreadyReadConfig.channel;
				}
				if (alreadyReadConfig.port != 0) {
					port = alreadyReadConfig.port;
				}
				if(alreadyReadConfig.version != null)
				{
					version = alreadyReadConfig.version;
				}
				badwordfilter = alreadyReadConfig.badwordfilter;
				verbose = alreadyReadConfig.verbose;
				password = alreadyReadConfig.password;
				minetest_autovoiceservers = alreadyReadConfig.minetest_autovoiceservers;

			} catch (Exception e) {

			}

			Input.DebugOutput ("{STATUS} nick = " + nick);
			Input.DebugOutput ("{STATUS} host = " + host);
			Input.DebugOutput ("{STATUS} port = " + port);
			Input.DebugOutput ("{STATUS} channel = " + channel);
			Input.DebugOutput ("{STATUS} auxConfigFilename = " + auxConfigFilename);

			try {
				client = new TcpClient (host, port);
				stream = client.GetStream ();
				reader = new StreamReader (stream);
				writer = new StreamWriter (stream);

				Input input = new Input (reader, writer, nick, channel,verbose,badwordfilter,password,minetest_autovoiceservers);

				Verbose("Input thread initialized.");

				Thread inputThread = new Thread (input.Read);
				inputThread.Start ();
				Verbose ("Input thread starting.");

				while (!inputThread.IsAlive) {
					Console.Write ("Waiting for thread to come alive.\r");
				}
				Console.WriteLine (); // Why is this here?

				Thread.Sleep (1);

				Verbose ("Async thread running simultaneously.\nWaiting 3 seconds to send USER.");
				Thread.Sleep (3000);
				Send (writer, "USER " + nick + " " + nick + " " + nick + " :Custom bot created by Microchip, coded in C#, and compiled in MonoDevelop.");
				Verbose ("USER sent.\nWaiting 3 seconds till NICK.");
				Thread.Sleep (3000);
				Send (writer, "NICK " + nick);
				Verbose ("NICK sent.");
				Console.WriteLine ("{STATUS} Waiting 10 seconds to join channel.");
				Thread.Sleep (10000);
				Send (writer, "JOIN " + channel);
				Verbose ("JOIN sent.");
				Send (writer, "PRIVMSG NickServ :identify " + password);
				Verbose ("NickServ identification sent.");
				//Send (writer, "PRIVMSG " + channel + " :Hello.");

				Console.WriteLine ("Emergency command interpreter starting.");
				for (int i = 0;; i++) {
					try
					{
						Console.Write (i + "> ");
						string command = Console.ReadLine ();
						char[] splits = {' ',};
						string[] aftersplit = command.Split (splits);
						if(aftersplit[0] == "join")
						{
							Send (writer,"PART " + channel + " :Manual disconnect.");
							channel = aftersplit[1];
							Send (writer,"JOIN " + channel);
						}

						if(aftersplit[0] == "part")
						{
							Send (writer, "PART " + aftersplit[1] + " :Manual disconnect.");
						}

						if(aftersplit[0] == "quit")
						{
							Send (writer, "PART " + channel + " :Quitting. Bye!");
							Environment.Exit (0);
						}

						if(aftersplit[0] == "say")
						{
							try{
								aftersplit[2] = aftersplit[2];
								StringBuilder builder = new StringBuilder();
								for(int j = 1; j < aftersplit.Length; j++)
								{
									builder.Append (aftersplit[j] + " ");
								}

								Send (writer, "PRIVMSG " + channel + " :" + builder.ToString ());
							}

							catch (Exception e)
							{
								Send (writer, "PRIVMSG " + channel + " :" + aftersplit[1]);
							}
						}
					}

					catch (Exception e)
					{

					}
				}
			} catch (Exception e) {
				Console.WriteLine (e.Message);
			}
		}

		static public void Send(StreamWriter streamWriter, string message)
		{
			streamWriter.WriteLine (message);
			streamWriter.Flush ();
		}

		static public void Verbose (string output)
		{
			if (verbose) {
				Console.WriteLine ("[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] {VERBOSE} " + output);
			} else {

			}
		}
	}
}
