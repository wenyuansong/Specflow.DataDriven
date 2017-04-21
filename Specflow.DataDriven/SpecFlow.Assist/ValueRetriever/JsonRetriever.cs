using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Specflow.DSL.Examples.SpecFlow.Assist
{
    public class JsonRetriever : IValueRetriever
    {
        public object Retrieve(KeyValuePair<string, string> keyValuePair, Type targetType, Type propertyType)
        {
             return JsonConvert.DeserializeObject(keyValuePair.Value, propertyType, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error, NullValueHandling = NullValueHandling.Ignore});
        }

        public bool CanRetrieve(KeyValuePair<string, string> keyValuePair, Type targetType, Type propertyType)
        {
            return typeof(object).IsAssignableFrom(propertyType);
        }
    }
}
