using System;

namespace lastmanstanding
{
	public class Configuration
	{
		public Configuration ()
		{

		}

		static public ConfigValues ProcessConfig (string[] configArray)
		{
			ConfigValues returnableConfig = new ConfigValues();
			char[] splits = {'=',};
			string[] aftersplit;
			for (int i = 0; i < configArray.Length; i++) {
				try
				{
				aftersplit = configArray[i].Split (splits);
				}
				catch
				{
					break;
				}
				if(aftersplit[0] == "name")
				{
					returnableConfig.name = aftersplit[1];
				}

				if(aftersplit[0] == "host")
				{
					returnableConfig.host = aftersplit[1];
				}

				if(aftersplit[0] == "port")
				{
					returnableConfig.port = Int32.Parse (aftersplit[1]);
				}

				if(aftersplit[0] == "channel")
				{
					returnableConfig.channel = aftersplit[1];
				}

				if(aftersplit[0] == "verbose")
				{
					if(aftersplit[1] == "true")
					{
						returnableConfig.verbose = true;
					}
					else
					{
						returnableConfig.verbose = false;
					}
				}
				if(aftersplit[0] == "version")
				{
					returnableConfig.version = aftersplit[1];
				}
				if(aftersplit[0] == "badwordfilter")
				{
					if(aftersplit[1] == "true")
					{
						returnableConfig.badwordfilter = true;
					}
					else
					{
						returnableConfig.badwordfilter = false;
					}
				}
				if (aftersplit [0] == "password") {
					returnableConfig.password = aftersplit [1];
				}
				if (aftersplit [0] == "minetest.autovoiceservers") {
					if (aftersplit [1] == "true") {
						returnableConfig.minetest_autovoiceservers = true;
					} else if (aftersplit [1] == "false") {
						returnableConfig.minetest_autovoiceservers = false;
					}
				}
			}

			return returnableConfig;
		}
	}

	public class ConfigValues
	{
		public string name;
		public string host;
		public int port;
		public string channel;
		public bool verbose = false;
		public string version;
		public bool badwordfilter;
		public string password;
		public bool minetest_autovoiceservers = false;
	}
}

