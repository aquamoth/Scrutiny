using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcKarmaDemo.Tests.WebApi
{
    public class ImageFolderTest : Scrutiny.Net.Api.ApiController
    {

    }

    [Scrutiny.Net.Api.ControllerName("DefinedName")]
    public class ImageFolderShoot : Scrutiny.Net.Api.ApiController
    {

    }

    public class ImageFolderController : Scrutiny.Net.Api.ApiController
    {
        public object GET(string id)
        {
            return new {
                folderA = new {
                    folderAA = id,
                    folderAB = new { }
                },
                folderB = new { },
                folderC = new { }
            };
//            return @"{ 
//    folderA: {
//        folderAA: {},
//        folderAB: {}
//    },
//    folderB: {} 
//}";
        }
    }
}
