using CamStream.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CamStream.ViewModels
{
	public class ResolutionsViewModel
	{
		public ObservableCollection<ResolutionModel> GetResolutions()
		{
			string url = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["GetResolutions"]) ? ConfigurationManager.AppSettings["GetResolutions"] : string.Empty;
			WebClient client = new WebClient();
			string response = client.DownloadString(url);
			return JsonConvert.DeserializeObject<ObservableCollection<ResolutionModel>>(response);
		}
	}
}
