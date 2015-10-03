using Scrutiny.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrutiny.Models
{
	class ResultModel
	{
		public TestResult[] Items { get; set; }

		public static ResultModel From(System.Collections.Specialized.NameValueCollection form)
		{
			var items = new List<TestResult>();
			var i = 0;
			while (form.AllKeys.Contains(string.Format("args[{0}][id]", i)))
			{
				var log = form[string.Format("args[{0}][log][]", i)];
				var logs = log == null
					? new string[0]
					: log.Split(',');
				var time = form[string.Format("args[{0}][time]", i)];
				var timeInt = time == null ? null : (int?)int.Parse(time);

				var skippedBool = bool.Parse(form[string.Format("args[{0}][skipped]", i)]);
				var successBool = bool.Parse(form[string.Format("args[{0}][success]", i)]);
				var item = new TestResult
				{
					id = form[string.Format("args[{0}][id]", i)],
					description = form[string.Format("args[{0}][description]", i)],
					log = logs,
					skipped = skippedBool,
					success = successBool,
					suite = form[string.Format("args[{0}][suite][]", i)].Split(','),
					time = timeInt
				};
				items.Add(item);
				i++;
			}
			return new ResultModel { Items = items.ToArray() };
		}
	}
}
