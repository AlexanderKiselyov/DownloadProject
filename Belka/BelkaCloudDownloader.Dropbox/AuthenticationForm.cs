using System;
using System.IO;
using System.Drawing;
using System.Threading;
using Dropbox.Api;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BelkaCloudDownloader.Dropbox
{
	public partial class AuthenticationForm : Form
	{
		CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
		/// <summary>
		/// Client app key.
		/// </summary>
		public const string appKey = "2807c3t2c1pd564";
		public string token = "";
		private string URL = "";

		public AuthenticationForm()
		{
			InitializeComponent();
			MaximizeBox = false;
			FormBorderStyle = FormBorderStyle.Fixed3D;
			Uri url = new Uri("https://www.dropbox.com/oauth2/authorize?client_id=" + appKey + "&response_type=token&redirect_uri=http://localhost:50080/authorize/");
			browser.Navigate(url);
			browser.ProgressChanged += Browser_ProgressChanged;
		}

		private void Browser_ProgressChanged(object sender, EventArgs e)
		{
			if (browser.Url != null)
			{
				URL = browser.Url.ToString();
				if ((URL != "https://www.dropbox.com/oauth2/authorize?client_id=" + appKey + "&response_type=token&redirect_uri=http://localhost:50080/authorize/") && (URL.Contains("access_token")))
				{
					if (token == "")
					{
						int fragment = URL.IndexOf("access_token");
						int i = 0;
						while (URL[fragment + 13 + i] != '&')
						{
							token += URL[fragment + 13 + i];
							i++;
						}
					}
					//Hide();
					Close();
				}
			}
		}

		public string ReturnToken()
		{
			return token;
		}
	}
}
