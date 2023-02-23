using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CamStream.Models
{
	public class ResolutionModel
	{
		public int resolutionId { get; set; }
		public string resolutionName { get; set; }
		public int width { get; set; }
		public int height { get; set; }
	}
}
