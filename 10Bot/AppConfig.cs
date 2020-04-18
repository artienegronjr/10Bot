namespace _10Bot
{
	public class AppConfig
	{

		public string DiscordBotToken { get; set; }
		public string ConnectionString { get; set; }
		public ulong LobbyChannel { get; set; }
		public ulong RegisterChannel { get; set; }
		public string ChannelDisplayName { get; set; }
		public int PlayersPerTeam { get; set; }
		public bool UseRegionEU { get; set; }
		public bool UseRegionNA { get; set; }
		public bool UseGameModes { get; set; }
	}
}
