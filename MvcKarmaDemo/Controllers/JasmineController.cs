using System;
using System.Web.Mvc;

namespace MvcKarmaDemo.Controllers
{
    public class JasmineController : Controller
    {
        public ViewResult Run()
        {
            return View("SpecRunner");
        }
    }
}
