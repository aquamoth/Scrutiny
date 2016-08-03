using Scrutiny.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrutiny.Models
{
	public class SocketIORouterModels
	{
		public class RegisterModel
		{
			public string name { get; set; }

			internal static RegisterModel From(System.Collections.Specialized.NameValueCollection form)
			{
				return new RegisterModel
				{
					name = form["args[name]"]
				};
			}
		}

		public class RegisterResponse
		{
			public bool isReady { get; set; }
			public string name { get; set; }
		}

		public class StartModel
		{
			public int total { get; set; }

			public static StartModel From(System.Collections.Specialized.NameValueCollection form)
			{
				return new StartModel { total = int.Parse(form["args[total]"]) };
			}
		}

		public class CompleteModel
		{
			public object coverage { get; set; }

			public static CompleteModel From(System.Collections.Specialized.NameValueCollection form)
			{
				return new CompleteModel { coverage = form["args[coverage]"] };
			}
		}

		public class InfoModel
		{
			public string Log { get; set; }
			public string type { get; set; }

			public static InfoModel From(System.Collections.Specialized.NameValueCollection form)
			{
                var log = form["args[Log]"];
                if (!log.StartsWith("'") && log.EndsWith("'"))
                    throw new ArgumentException("Expected Log argument to start and end with '.");

                return new InfoModel
                {
                    Log = log.Substring(1, log.Length - 2),
                    type = form["args[type]"]
                };
			}
		}

		public class ResultModel
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
}
