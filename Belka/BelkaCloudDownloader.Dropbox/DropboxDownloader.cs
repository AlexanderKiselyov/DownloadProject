using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Users;

namespace BelkaCloudDownloader.Dropbox
{
	using Utils;

	public sealed class DropboxDownloader : AbstractDownloader
	{

		public DropboxDownloader(CancellationToken cancellationToken)
		: base(cancellationToken, 10)
		{
		}

		public static async Task<string> Authenticate(CancellationToken cancellationTolen, IIoLogger log)
		{
			AuthenticationForm authentification = new AuthenticationForm();
			authentification.ShowDialog();
			log.LogMessage("Authorizing manually for Dropbox as Dropbox Sync");
			try
			{
				DropboxClient dropboxClient = new DropboxClient(authentification.ReturnToken());
				log.LogMessage("Successfully authenticated");
				return authentification.ReturnToken();
			}
			catch (Exception e)
			{
				if (e is OperationCanceledException)
				{
					throw (OperationCanceledException) e;
				}
				log.LogError("Authentication error", e);
				throw;
			}
		}

		public async Task<MetaInformation> Download(DriveDownloaderSettings settings, CancellationToken cancellationToken, ProgressBar currentOperationProgressBar)
		{
			this.Log.LogMessage($"Started Dropbox downloading, Refresh Token = {settings.RefreshToken},"
				   + $" Target Directory = {settings.TargetDirectory}.");
			this.Status = Status.Authenticating;
			this.Log.LogMessage("Authenticating...");
			try
			{
				DropboxClient dropboxClient = new DropboxClient(settings.RefreshToken);
				var list = await dropboxClient.Files.ListFolderAsync(string.Empty, recursive: true);
				int filesNumber = 0;
				foreach (var item in list.Entries)
				{
					if (item.IsFile)
					{
						filesNumber++;
					}
				}
				currentOperationProgressBar.Minimum = 0;
				currentOperationProgressBar.Step = 1;
				currentOperationProgressBar.Maximum = filesNumber;
				var account = new FullAccount();
				var metaInformation = new MetaInformation(account, list);

				if (settings.DoDownloadFiles)
				{
					this.Status = Status.DownloadingFiles;
					this.CancellationToken.ThrowIfCancellationRequested();

					this.Log.LogMessage("Downloading files...");
				}
				else
				{
					metaInformation.DownloadedFiles = new List<DownloadedFile>();
				}
				foreach (var item in list.Entries)
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (item.IsFolder == true)
					{
						Directory.CreateDirectory(settings.TargetDirectory + item.PathDisplay);
					}
					else
					{
						currentOperationProgressBar.PerformStep();
						var response = await dropboxClient.Files.DownloadAsync(item.PathDisplay);
						using (var fileStream = File.Create(settings.TargetDirectory + item.PathDisplay))
						{
							(await response.GetContentAsStreamAsync()).CopyTo(fileStream);
						}
					}
				}

				this.Done();
				return metaInformation;
			}
			catch (Exception)
			{
				this.Error();
				return new MetaInformation(null, new ListFolderResult());
			}
		}
	}
}