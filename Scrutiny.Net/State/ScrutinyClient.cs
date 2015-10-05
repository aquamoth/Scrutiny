using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.State
{
	public class TestResult
	{
		public string id { get; set; }
		public string description { get; set; }
		public string[] log { get; set; }
		public string[] suite { get; set; }
		public bool success { get; set; }
		public bool skipped { get; set; }
		public int? time { get; set; }
	}

	public class ScrutinyTestClient : WebIO.Net.Client
	{
		public string Browser { get; set; }
		public bool IsRunRequested { get; set; }
		public bool IsReady { get; set; }
		public int TotalCount { get; set; }
		public DateTime	TestsStartTime { get; set; }
		public DateTime TestsEndTime { get; set; }
		public ObservableCollection<TestResult> Results { get; private set; }

		public ScrutinyTestClient()
		{
			this.Results = new ObservableCollection<TestResult>();
		}
	}
}
