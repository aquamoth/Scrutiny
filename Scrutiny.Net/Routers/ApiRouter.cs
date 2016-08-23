using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Routers
{
	class ApiRouter : IRouter
	{
        const string DEFAULT_CONTROLLER_NAME_ENDING = "Controller";

        static IDictionary<string, Type> controllersMap;

        #region Static Constructor

        static ApiRouter()
        {
            var apiControllers = loadApiControllersFromConfig();
            controllersMap = createCaseInsensitiveDictionaryOf(apiControllers);
        }

        private static List<Type> loadApiControllersFromConfig()
        {
            var foundControllerTypes = new List<Type>();
            foreach (Config.PathConfigurationElement path in Config.Scrutiny.AssembliesToSearchForApisIn())
            {
                var assemblyPath = path.Name;
                var controllerTypes = findApiControllersIn(assemblyPath);
                foundControllerTypes.AddRange(controllerTypes);
            }

            return foundControllerTypes;
        }

        private static IEnumerable<Type> findApiControllersIn(string assemblyPath)
        {
            var baseType = typeof(Net.Api.ApiController);
            var assembly = System.Reflection.Assembly.LoadFrom(assemblyPath);
            var allTypes = assembly.GetTypes();
            var controllers = allTypes.Where(t => baseType.IsAssignableFrom(t));
            return controllers;
        }

        private static Dictionary<string, Type> createCaseInsensitiveDictionaryOf(IEnumerable<Type> controllers)
        {
            var dictionary = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            foreach (var type in controllers)
            {
                string name = nameOfController(type);
                dictionary.Add(name, type);
            }

            return dictionary;
        }

        private static string nameOfController(Type controllerType)
        {
            var attribute = controllerNameAttributeOf(controllerType);
            if (attribute != null)
                return attribute.Name;

            if (controllerType.Name.EndsWith(DEFAULT_CONTROLLER_NAME_ENDING))
            {
                var index = controllerType.Name.Length - DEFAULT_CONTROLLER_NAME_ENDING.Length;
                var name = controllerType.Name.Substring(0, index);
                return name;
            }

            return controllerType.Name;
        }

        private static Net.Api.ControllerNameAttribute controllerNameAttributeOf(Type controllerType)
        {
            var expectedType = typeof(Net.Api.ControllerNameAttribute);

            var attribute = Attribute.GetCustomAttribute(controllerType, expectedType);

            return attribute as Net.Api.ControllerNameAttribute;
        }

        #endregion Static Constructor

        public async Task<string> Route(ControllerActionParts parts, Net.Api.RequestType requestType)
        {
            var controllerType = controllersMap[parts.Action];
            var response = callMethod(controllerType, requestType.ToString(), parts.Value);

            return Json(response);
        }

        //if (!System.IO.Path.IsPathRooted(assemblyPath))
        //{
        //    var executingPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        //    var executingDirectory = System.IO.Path.GetDirectoryName(executingPath);
        //    assemblyPath = System.IO.Path.Combine(executingDirectory, assemblyPath);
        //}

        private static object callMethod(Type controllerType, string actionName, string[] arguments)
        {
            var controller = Activator.CreateInstance(controllerType, null);
            var action = controllerType.GetMethod(actionName);
            if (action == null)
                throw new ArgumentException("The Controller does not support the request action: " + actionName);

            var response = action.Invoke(controller, arguments);
            return response;
        }

        private static string Json(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
    }
}
