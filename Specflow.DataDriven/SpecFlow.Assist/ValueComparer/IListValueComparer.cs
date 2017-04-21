using Newtonsoft.Json;
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
    public class IListValueComparer : IValueComparer
    {
        public bool Compare(string expectedValue, object actualValue)
        {
            var expected = JsonConvert.DeserializeObject<IList>(expectedValue);

            if (expected.Count != (actualValue as IList).Count)
                throw new ComparisonException(string.Format("List lengh mismatch: expected lengh={0}, actual lengh={1}",
                                               expected.Count,
                                                (actualValue as IList).Count));

            for (int i=0; i<expected.Count; i++)
            {
                if (!IsMatch(expected[i].ToString(), (actualValue as IList)[i]))
                    return false;
            }

            return true;
        }

        private bool IsMatch(string expected, object propertyValue)
        {
            var valueComparers = TechTalk.SpecFlow.Assist.Service.Instance.ValueComparers;

            return valueComparers
                .FirstOrDefault(x => x.CanCompare(propertyValue))
                .Compare(expected, propertyValue);
        }

        public bool CanCompare(object actualValue)
        {
            return typeof(IList).IsAssignableFrom(actualValue.GetType());
        }
    }
}
