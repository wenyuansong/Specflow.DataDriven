using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Specflow.DataDriven.DataDriven;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Specflow.DSL.Examples.SpecFlow.Assist
{
    public class JsonValueComparer : IValueComparer
    {
        public bool Compare(string expectedValue, object actualValue)
        {
            //if (expectedValue)
            dynamic obj = JsonConvert.DeserializeObject(expectedValue);

            foreach (Newtonsoft.Json.Linq.JProperty item in obj)
            {
                if (!IsMatch(item.Value.ToString(), actualValue, item.Name))
                    throw new ComparisonException(string.Format(">>>{0} in type {1} : Expected={2} Actual={3}", item.Name, actualValue.GetType(), item.Value, GetPropertyValue(actualValue, item.Name)));
            }

            return true;
        }

        private object GetPropertyValue(object obj, string name)
        {
            return obj.GetType()
                .GetProperties()
                .FirstOrDefault(p => p.Name.Equals(name))
                .GetValue(obj);
        }

        private bool IsMatch(string expected, object obj, string key)
        {
            if (obj.GetType().GetProperties().FirstOrDefault(pr => pr.Name.ToLower() == key.ToLower()) == null)
                throw new ComparisonException(string.Format("Can't find property {0} in {1}", key, obj.GetType()));

            var propertyValue = GetPropertyValue(obj, key);

            var valueComparers = TechTalk.SpecFlow.Assist.Service.Instance.ValueComparers;

            return valueComparers
                .FirstOrDefault(x => x.CanCompare(propertyValue))
                .Compare(expected, propertyValue);
        }

        public bool CanCompare(object actualValue)
        {
            return !actualValue.GetType().IsPrimitive
                && !actualValue.GetType().IsValueType
                && !(actualValue is string)
                && !(actualValue.GetType().IsGenericType);
        }
    }

}
