using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcKarmaDemo.Controllers
{
    public class TestController : Controller
    {
		public ActionResult Index(string fileName)
		{
			var data = new List<string>();
			var path = Server.MapPath("~/test");
			var testScriptsFolder = new DirectoryInfo(path);
			var scripts = testScriptsFolder.GetFiles("*Spec.js", SearchOption.AllDirectories);
			foreach (var fileInfo in scripts)
			{
				if (fileInfo.DirectoryName != null &&
					(string.IsNullOrEmpty(fileName) ||
						fileInfo.Name.ToUpper().Contains(fileName.ToUpper())))
				{
					data.Add(@"/test" + fileInfo.DirectoryName.Replace(path, "")
						.Replace(@"\", @"/") + @"/" + fileInfo.Name);
				}
			}
			return View("Test", "_TestLayout", data);
		}
	}
}