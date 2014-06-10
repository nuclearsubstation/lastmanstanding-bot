using System;
using System.IO;
using System.Threading;

namespace lastmanstanding
{
	public class Input
	{
		const string badwordlist = "badwords.txt"; // Bad words that people need to be kicked for are here
		const string opsFilename = "ops.txt"; // Allowed operators are in ops.txt, one per line
		StreamReader input = null;
		StreamWriter output = null;

		string nick;
		string channel;
		bool verbose;
		bool badwordfilter;
		string password;
		bool autovoiceservers;

		public Input (StreamReader reader, StreamWriter writer, string botNick, string botChannel, bool verboseLogging, bool badwords, string userpassword,bool minetest_autovoiceservers)
		{
			input = reader;
			output = writer;
			nick = botNick;
			channel = botChannel;
			verbose = verboseLogging;
			badwordfilter = badwords;
			password = userpassword;
			autovoiceservers = minetest_autovoiceservers;
		}

		public void Read ()
		{
			string settableFlags = "";
			bool namesearching = false;
			string nameSearchFor = "";
			string gfbotTarget;
			bool gfbotMode = false;
			output.AutoFlush = true;
			for (;;) {
				string currentInput = input.ReadLine ();

				// TODO: Add filter so that it looks like a text IRC client

				Console.WriteLine ("["+DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "]" + currentInput);

				if(verbose)
				{
					DebugOutput (currentInput, false);
				}

				string currentTranslated = currentInput.ToLower ();
				//Console.WriteLine(currentInput);

				//Console.WriteLine("Current translated is " + currentTranslated);

				if (currentInput.Contains ("PING")) {
					DebugOutput ("{STATUS} PONG");
					output.WriteLine ("PONG");
				}

				if (currentTranslated.Contains ("privmsg " + nick.ToLower () + " :hi") || currentTranslated.Contains ("privmsg " + nick.ToLower () + " :hello")) {
					char[] splits = {':', '!'};
					string[] aftersplit = currentInput.Split (splits);
					output.WriteLine ("PRIVMSG " + aftersplit [1] + " :Hello " + aftersplit [1] + ".");
					goto End;
				}

				if (currentTranslated.Contains ("hello " + nick.ToLower ()) || currentTranslated.Contains ("hi " + nick.ToLower ())) {
					char[] splits = {':', '!'};
					string[] aftersplit = currentInput.Split (splits);
					output.WriteLine ("PRIVMSG " + channel + " :Hello " + aftersplit [1] + ".");
				}

				if (currentInput.Contains ("PRIVMSG " + nick + " :chat")) {
					char[] splits = {':', '!'};
					string[] aftersplit = currentInput.Split (splits);
					output.WriteLine ("PRIVMSG " + aftersplit [1] + " :You asked for it. Loading \"War And Peace\".");
				}

//				if(currentTranslated.Contains ("^search"))
//				{
//					char[] splits = {' '};
//					string[] aftersplit = currentTranslated.Split (splits);
//					Search search = new Search(output, input,nick,channel);
//					for(int i = 0; i < aftersplit.Length; i++)
//					{
//						Console.WriteLine("{STATUS} Search requested. Printing aftersplit["+i+"]");
//						Console.WriteLine (aftersplit[i]);
//					}
//					search.Bing (aftersplit[4]);
//				}
				if (currentTranslated.Contains ("^quit")) {
					char[] splits = {':', '!'};
					string[] aftersplit = currentInput.Split (splits);

					DebugOutput ("{STATUS} User \""+aftersplit[1]+"\" requested quit. Checking op list to see if they are authorized.");
					string[] ops = GetOps (opsFilename);
					for(int i = 0; i < ops.Length; i++)
					{
						if (aftersplit [1] == ops[i]) {
							output.WriteLine ("QUIT :Bye!");
							Environment.Exit (0);
						}
					}
				}

				if (currentInput.Contains ("^join")) {
					char[] splits = {':', '!', ' '};
					string[] aftersplit = currentInput.Split (splits);
					if (UsernameIsOp (aftersplit[1])) {
						output.WriteLine ("PRIVMSG " + channel + " :Leaving channel. See you in " + aftersplit[7] +".");
						output.WriteLine ("PART " + channel + " :Bye!");
						output.WriteLine ("JOIN " + aftersplit[7]);
						channel = aftersplit[7];
					}
				}

				if(currentInput.Contains ("^op"))
				{
					char[] splits = {':', '!', ' ',};
					string[] aftersplit = currentInput.Split (splits);
					try{
						string test = aftersplit[7];
						DebugOutput ("{STATUS} User \""+aftersplit[1]+"\" requested op for user \""+aftersplit[7]+"\"");
						string[] opsList = GetOps(opsFilename);
						int i = 0;
						DebugOutput ("{STATUS} Reading op database, and looking for user that requested op.");
						for(; i < opsList.Length; i++)
						{
							if(opsList[i] == aftersplit[1])
							{
								DebugOutput ("{STATUS} Op found. Granting.");
								output.WriteLine ("MODE " + channel + " +o " +aftersplit[7]);
							}
						}

					}
					catch(Exception e)
					{
						DebugOutput("{STATUS} User \""+aftersplit[1] + "\" requested op.");
						DebugOutput("{STATUS} Reading op database.");
						string[] ops = GetOps (opsFilename);
						int i = 0;
						for(; i < ops.Length; i++)
						{
							if(aftersplit[1] == ops[i])
							{
								DebugOutput ("{STATUS} Op found. Granting.");
								output.WriteLine ("MODE " + channel + " +o " + aftersplit[1]);
								break;
							}
						}

						if(i == ops.Length)
						{
							DebugOutput ("{STATUS} Op not found. Not granting access. Offending user was \""+aftersplit[1]+"\"");
							output.WriteLine ("PRIVMSG " + channel + " :You are not authorized.");
						}
					}
				}

				if(currentInput.Contains ("^deop"))
				{
					char[] splits = {':', '!', ' ',};
					string[] aftersplit = currentInput.Split (splits);

					try
					{
						output.WriteLine ("MODE " + channel + " -o " + aftersplit[7]);
					}
					catch(Exception e)
					{
						output.WriteLine ("MODE " + channel + " -o " + aftersplit[1]);
					}

				}

				if (currentInput.Contains ("^voice")) {
					char[] splits = {':', '!', ' ',};
					string[] aftersplit = currentInput.Split (splits);
					try{
						string test = aftersplit[7];
						DebugOutput ("{STATUS} User \""+aftersplit[1]+"\" requested voice for user \""+aftersplit[7]+"\"");
						string[] opsList = GetOps(opsFilename);
						int i = 0;
						DebugOutput ("{STATUS} Reading op database, and looking for user that requested voice.");
						for(; i < opsList.Length; i++)
						{
							if(opsList[i] == aftersplit[1])
							{
								DebugOutput ("{STATUS} Op found. Granting.");
								output.WriteLine ("MODE " + channel + " +v " +aftersplit[7]);
							}
						}

					}
					catch(Exception e)
					{
						DebugOutput("{STATUS} User \""+aftersplit[1] + "\" requested voice.");
						DebugOutput("{STATUS} Reading op database.");
						string[] ops = GetOps (opsFilename);
						int i = 0;
						for(; i < ops.Length; i++)
						{
							if(aftersplit[1] == ops[i])
							{
								DebugOutput ("{STATUS} Op found. Granting.");
								output.WriteLine ("MODE " + channel + " +v\t\t " + aftersplit[1]);
								break;
							}
						}

						if(i == ops.Length)
						{
							DebugOutput ("{STATUS} Op not found. Not granting access. Offending user was \""+aftersplit[1]+"\"");
							output.WriteLine ("PRIVMSG " + channel + " :You are not authorized.");
						}
					}
				}

				if (currentInput.Contains ("^devoice")) {
					char[] splits = {':', '!', ' ',};
					string[] aftersplit = currentInput.Split (splits);

					try
					{
						output.WriteLine ("MODE " + channel + " -v " + aftersplit[7]);
					}
					catch(Exception e)
					{
						output.WriteLine ("MODE " + channel + " -v " + aftersplit[1]);
					}
				}

				if(currentTranslated.Contains (" :you're not a channel operator"))
				{
					output.WriteLine ("PRIVMSG " + channel + " :ERROR: Bot is not a channel operator. Please /op " + nick + " and try again.");
				}

				if(currentTranslated.Contains ("^version"))
				{
					char[] splits = {':', '!',};
					string[] aftersplit = currentInput.Split (splits);
					output.WriteLine ("PRIVMSG " + channel + " :" + nick + " version " + MainClass.version);
					DebugOutput ("{STATUS} User \"" + aftersplit[1] + "\" has requested version. Current version is LastManStandingBot " + MainClass.version);
				}

				if(currentTranslated.Contains ("^set"))
				{
					char[] splits = {':', '!', ' ',};
					string[] aftersplit = currentInput.Split (splits);
					if(UsernameIsOp (aftersplit[1]))
					{
						if(aftersplit[7] == "channel")
						{
							channel = aftersplit[8];
							output.WriteLine ("PRIVMSG " + channel + " :Channel set!");
						}

						if(aftersplit[7] == "nick")
						{
							nick = aftersplit[8];
							output.WriteLine ("NICK " + nick);
						}
						if (aftersplit [7] == "mode") {
							try {
								if (aftersplit [8].StartsWith ("-") || aftersplit [8].StartsWith ("+")) {
									output.WriteLine ("MODE " + channel + " " + aftersplit [8] + " " + aftersplit [1]);
									DebugOutput ("{STATUS} User " + aftersplit [1] + " requested MODE " + aftersplit [8] + " on " + channel + ".");
								} else {
									try {
										if (aftersplit [9].StartsWith ("+") || aftersplit [9].StartsWith ("-")) {
											output.WriteLine ("MODE " + channel + " " + aftersplit [9] + " " + aftersplit [8]);
											DebugOutput ("{STATUS} User " + aftersplit [1] + " requested MODE " + aftersplit [9] + " for user " + aftersplit [8] + " on " + channel + ".");
										} else {
											output.WriteLine ("PRIVMSG " + channel + " :Error, that is not a valid mode.");
											DebugOutput ("{STATUS} User " + aftersplit [1] + " requested MODE " + aftersplit [9] + " for user " + aftersplit [8] + " on " + channel + ", but that is not a valid mode.");
										}
									} catch (Exception e2) {
										output.WriteLine ("PRIVMSG " + channel + " :Not enough arguments.");
									}
								}
							} catch (Exception e) {
								output.WriteLine ("PRIVMSG " + channel + " :Not enough arguments.");
							}
						}
					}
					else
					{
						Console.WriteLine ("USER \"" + aftersplit[1] + "\" requested ^set. Checked database, name not found.");
					}
				} // Generic set mode

				if(currentTranslated.Contains ("^scanfor"))
				{
					char[] splits = {' ',};
					string[] aftersplit = currentInput.Split (splits);

					try
					{
						nameSearchFor = aftersplit[4];
					}
					catch
					{
						output.WriteLine ("PRIVMSG " + channel + " :ERROR: You didn't specify a name to search for.");
						break;
					}

					namesearching = true;
					output.WriteLine ("NAMES");
				}

				if(namesearching == true)
				{
					// This is a special sort of input, that requires flags to be set
					// So, if ^scanfor <name> is sent, then it will need to know that as the next input floods in

					char[] splits = {' ',};
					string[] aftersplit = currentInput.Split (splits);

					if(currentInput.Contains (nick + " = " + channel + " :"))
					{
						namesearching = false;
					}

					//Console.WriteLine ("---SEPARATOR----");

					for(int i = 6; i < aftersplit.Length; i++) // Offset is actually 6, but since the arrays start at 0, it's 5
					{
						// Search for each of the names in WHOIS

						settableFlags = "whois";
						output.WriteLine ("WHOIS " + aftersplit[i]);
					}
				}

				if(settableFlags == "whois")
				{
					Console.WriteLine ("---" + currentInput + "---");
				}

				// TODO: Add ^reconnect
//				if(currentInput.Contains ("^reconnect"))
//				{
//					
//				}

				// Fixes a bug because of passed arguments
				if(currentInput.Contains ("JOIN"))
				{
					char[] splits = {' ', ':', '!',};
					string[] aftersplit = currentInput.Split(splits);

					if(aftersplit[1] == nick)
					{
						channel = aftersplit[4];
					}
				}

				if (currentInput.Contains ("JOIN")) {
					char[] splits = { ' ', ':', '!', '@','~' };
					string[] aftersplit = currentInput.Split (splits);

					if (aftersplit [3] == "Minetest" && autovoiceservers) {
						output.WriteLine ("MODE " + channel + " +v " +aftersplit[1]);
					}
				} // Minetest IRC server specific option

				#region BadWords
				// Check for bad words
				if(badwordfilter)
				{
					StreamReader badwordreader = new StreamReader(badwordlist);
					string[] badwords = new string[10000];
					for(int i = 0; !badwordreader.EndOfStream; i++)
					{
						badwords[i] = badwordreader.ReadLine ();
					}

					badwordreader.Close ();

					try
					{
						for(int i = 0; badwords[i] != null; i++)
						{
							if(currentTranslated.Contains (badwords[i]))
							{
								char[] splits = {':', '!',};
								string[] aftersplit = currentInput.Split (splits);
								DebugOutput("{STATUS} User \""+aftersplit[1] + "\" is being kicked for bad language.");
								output.WriteLine ("KICK " + channel +" " + aftersplit[1] + " :No foul language!");
									break;
							}
						}
					}
					catch(Exception e)
					{

					}
				}
				#endregion
				End:
				Console.Write ("");
			}
		}

		static public string[] GetOps (string filename)
		{
			StreamReader reader = new StreamReader (filename);
			string[] ops1 = new string[1000]; // Up to 1k ops are supported TODO: add more?
			for (int i = 0; !reader.EndOfStream; i++) {
				ops1 [i] = reader.ReadLine ();
			}

			int firstNullLocation = 0;
			for (; firstNullLocation < ops1.Length; firstNullLocation++) {
				if(ops1[firstNullLocation] == null)
				{
					break;
				}
			}

			string[] ops2 = new string[firstNullLocation];
			for(int i = 0; i < ops2.Length; i++)
			{
				ops2[i] = ops1[i];
			}

			return ops2;
		}
		
		static public void DebugOutput(string text)
		{
			//File.AppendText ("debug.txt");
			File.AppendAllText ("debug.txt", "\n[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] " + text);
			Console.WriteLine ("[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] " + text);
		}

		static public void DebugOutput (string text, bool outputToScreen)
		{
			//File.AppendText ("debug.txt");
			File.AppendAllText ("debug.txt", "\n[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] " + text);
			if (outputToScreen) {
				Console.WriteLine ("[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] " + text);
			} else {

			}
		}

		static public bool UsernameIsOp (string username)
		{
			string[] ops = GetOps (opsFilename);
			for (int i = 0; i < ops.Length; i++) {
				if(ops[i] == username)
				{
					return true;
					break; // Just to be sure
				}
			}
			return false; // Won't be reached unless username isn't found
		}
	}
}

