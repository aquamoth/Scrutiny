using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcKarmaDemo.Controllers
{
    public class HelloRController : Controller
    {
        // GET: HelloR
        public ActionResult Index()
        {
            return View();
        }
    }
}