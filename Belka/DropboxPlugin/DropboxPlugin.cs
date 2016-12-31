namespace BelkaCloudDownloader.Gui
{
	using System.Collections.Generic;

	/// <summary>
	/// Dropbox support.
	/// </summary>
	internal sealed class DropboxPlugin : CloudPlugin
	{
		public IList<Protocol> Protocols { get; } = new List<Protocol> { new DropboxProtocol() };

		public string CloudName { get; } = "Dropbox";
	}
}