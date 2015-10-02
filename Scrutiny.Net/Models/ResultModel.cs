using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrutiny.Models
{
	class ResultModel
	{
		public ResultModelItem[] Items { get; set; }

		public static ResultModel From(System.Collections.Specialized.NameValueCollection form)
		{
			var items = new List<ResultModelItem>();
			var i = 0;
			while (form.AllKeys.Contains(string.Format("args[{0}][id]", i)))
			{
				var log = form[string.Format("args[{0}][log][]", i)];
				var logs = log == null
					? new string[0]
					: log.Split(',');
				var time = form[string.Format("args[{0}][time]", i)];
				var timeInt = time == null ? null : (int?)int.Parse(time);

				var item = new ResultModelItem
				{
					id = form[string.Format("args[{0}][id]", i)],
					description = form[string.Format("args[{0}][description]", i)],
					log = logs,
					skipped = form[string.Format("args[{0}][skipped]", i)],
					success = form[string.Format("args[{0}][success]", i)],
					suite = form[string.Format("args[{0}][suite][]", i)].Split(','),
					time = timeInt
				};
				items.Add(item);
				i++;
			}
			return new ResultModel { Items = items.ToArray() };
		}
	}
	
	class ResultModelItem
	{
		public string id { get; set; }
		public string description { get; set; }
		public string[] log { get; set; }
		public string[] suite { get; set; }
		public string success{ get; set; }
		public string skipped { get; set; }
		public int? time { get; set; }
	}
}
