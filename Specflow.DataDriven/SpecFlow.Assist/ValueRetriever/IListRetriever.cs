using Newtonsoft.Json;
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
    public class IListRetriever : IValueRetriever
    {
        public object ConvertGenericType(IEnumerable<object> list, Type genericType)
        {
            var table = new Table("Field", "Value");
            table.AddRow("", "");

            var valueRetriever = TechTalk.SpecFlow.Assist.Service.Instance
                       .GetValueRetrieverFor(table.Rows[0], typeof(string), genericType);

            list = list.Select(k =>
              Convert.ChangeType(
                  valueRetriever.Retrieve(new KeyValuePair<string, string>("field", k.ToString()),
                        typeof(string), genericType
                        ),
                  genericType));

            var list1 = typeof(System.Linq.Enumerable).GetMethod(
                                     "Cast",
                                     new[] { typeof(System.Collections.IEnumerable) })
                                     .MakeGenericMethod(genericType)
                                     .Invoke(null, new object[] { list });

            MethodInfo toListMethod = typeof(Enumerable).GetMethod("ToList")
           .MakeGenericMethod(new System.Type[] { genericType });
            return toListMethod.Invoke(null, new object[] { list1 });
        }

        public object Retrieve(KeyValuePair<string, string> keyValuePair, Type targetType, Type propertyType)
        {
            return ConvertGenericType(JsonConvert.DeserializeObject<IEnumerable<object>>(keyValuePair.Value),
                    propertyType.GetGenericArguments()[0]);
        }

        public bool CanRetrieve(KeyValuePair<string, string> keyValuePair, Type targetType, Type propertyType)
        {
            return typeof(IList).IsAssignableFrom(propertyType) && propertyType.IsGenericType;
        }
    }
}
