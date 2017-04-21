using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Specflow.DSL.Examples.SpecFlow.Assist;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Specflow.DataDriven.DataDriven
{
    public static class DataDrivenHelper
    {
         static DataDrivenHelper()
        {
            TechTalk.SpecFlow.Assist.Service.Instance.RegisterValueRetriever(new IListRetriever());
            TechTalk.SpecFlow.Assist.Service.Instance.RegisterValueRetriever(new JsonRetriever());
            TechTalk.SpecFlow.Assist.Service.Instance.RegisterValueComparer(new JsonValueComparer());
            TechTalk.SpecFlow.Assist.Service.Instance.RegisterValueComparer(new IListValueComparer());
        }

        public static bool EqualsString(this string string1, string string2, bool ignoreCase = true)
        {
            return string.Compare(string1.Trim(), string2.Trim(), ignoreCase) == 0;
        }


        /// <summary>
        /// Assign DataDrivenObject with values from a set of key/value pairs
        /// Value Rules:
        ///  allow table header have comments in ().   e.g data(this is comment) 
        ///  1) tablecells with value="n/a" will be ignored
        ///  2) if object's property is List type, then value should be spereated by ","". 
        ///      e.g  to assign value for the following property:    public List Color {get;set;}
        ///      then the table value should be "red,green,yellow"
        ///     if object's property is List<IDataDriven> then the value should be seperated by "|"
        ///  3) if property type is boolean, then value should be "true" or "false"
        ///  4) supports int and string types
        ///  5) support class type and that class must support IDataDriven
        ///    value should be spereated by ",". the constructor of that class must have corresponding parameters:
        ///     e.g  public Color color {get;set;}
        ///          public class Color
        ///             {
        ///               public Color(string color1, string color2)
        ///             }
        ///     then the table value should be "color1=red;color2=blue;
        ///     
        ///  support "\" as escape char
        /// </summary>
        /// <param name="assignTo">The assign to.</param>
        /// <param name="properties">properties to be assigned to IDataDriven object</param>
        /// <exception cref="System.ArgumentException">Doesn't support copying value for type : + p.GetType() +  and member is  + p.Name</exception>

        public static void AssignValueTo(this Table table, object dataObject)
        {

            ForMatch(dataObject,
                    table,
                    match =>
                    {
                        var valueRetriever = TechTalk.SpecFlow.Assist.Service.Instance.GetValueRetrieverFor(match.Row, match.Obj.GetType(), match.Property.PropertyType);
                        match.Property.SetValue(dataObject, valueRetriever.Retrieve(new KeyValuePair<string, string>(match.Row[0], match.Row[1]), dataObject.GetType(), match.Property.PropertyType));
                    });
            }

        internal static void ForMatch(object dataObject, Table table, Action<MatchProperty> act)
        {
            var vTable = MyTEHelpers.GetTheProperInstanceTable(table, dataObject.GetType());

            // remove n/a
            var remoteNA = new Regex(@"(\w+)(\b*\s*):(\s*)n/a\s*(,?)");
            var filteredTable = new Table("Field", "Value");
            foreach (var row in vTable.Rows)
            {
                string propertyValue = remoteNA.Replace(row.Value().ToString(), "");

                // Ignore "n/a" value fields
                if (propertyValue.ToLower() == "n/a") continue;

                filteredTable.AddRow(row.Id(), propertyValue);
            }

            foreach (var row in filteredTable.Rows)
            {
                var propertyName = PropertyName.Parse(row.Id());
                // For fields can't find a match in test object, raise alert
                var p = MyTEHelpers.FindMatchProperty(dataObject.GetType().GetProperties(), propertyName.Name);
                if (p == null) throw new ComparisonException(string.Format("Can't find {0} field in {1}", propertyName, dataObject.GetType()));

                act.Invoke(new MatchProperty { Row = row, Property = p, Obj = dataObject });
            }
        }

        public static void VerifyData(this Table table, object dataObject)
        {
            var valueComparers = TechTalk.SpecFlow.Assist.Service.Instance.ValueComparers;
            ForMatch(dataObject,
                 table,
                 match =>
                 {
                     var expected = match.Row.Value().ToString();
                     var actual = match.Property.GetValue(match.Obj);
                     var propertyName = PropertyName.Parse(match.Row.Id());

                     // list
                     if (!string.IsNullOrEmpty(propertyName.Extra))
                     {
                         HandleExtra(match);
                     }
                     else
                     {
                         // compare equal
                         if (!valueComparers
                                 .FirstOrDefault(x => x.CanCompare(actual))
                                 .Compare(expected, actual))
                             throw new ComparisonException(
                                 string.Format(">>>{0}: Expected={1} Actual={2}",
                                         match.Row.Id(),
                                         expected, JsonConvert.SerializeObject(actual)));
                     }
                 });
        }


        private static void HandleExtra(MatchProperty match)
        {
            var expected = match.Row.Value().ToString();
            var actual = match.Property.GetValue(match.Obj);
            var valueComparers = TechTalk.SpecFlow.Assist.Service.Instance.ValueComparers;
            var propertyName = PropertyName.Parse(match.Row.Id());

            switch (propertyName.Extra.ToLower())
            {
                case ".count()":
                    Assert.AreEqual(int.Parse(expected), 
                        (actual as IList).Count, 
                       "{0} count() should be {1} but actual is {2}", propertyName.Name, expected, (actual as IList).Count);
                    break;

                case ".contain()":
                    var matcho = (actual as IList)
                     .Cast<object>()
                     .FirstOrDefault(o =>
                     {
                         try
                         {
                             return valueComparers
                             .FirstOrDefault(x => x.CanCompare(o))
                             .Compare(expected, o);
                         }
                         catch (Exception e)
                         {
                             return false;
                         }
                     });
                    Assert.IsNotNull(matcho, 
                      "{0} should contain {1}", propertyName.Name, expected);
                    break;

                case ".notcontain()":
                    var matchobj = (actual as IList)
                    .Cast<object>()
                    .FirstOrDefault(o =>
                    {
                        try
                        {
                            return valueComparers
                            .FirstOrDefault(x => x.CanCompare(o))
                            .Compare(expected, o);
                        }
                        catch (Exception e)
                        {
                            return false;
                        }
                    }
                    );
                    
                    Assert.IsNull(matchobj, 
                      "{0} should NOT contain {1}", propertyName.Name, expected);
                    break;
                default:
                    throw new NotSupportedException(propertyName.Extra + " not supported");

            }
        }
    }

internal class MatchProperty
{
    public TableRow Row { get; set; }
    public PropertyInfo Property { get; set; }
    public Object Obj { get; set; }
}
internal class PropertyName
{
    public string Name { get; set; }
    public string Extra { get; set; }

    public static PropertyName Parse(string name)
    {
        var match = new Regex("(\\w+)?([\\.|\\[]?.*)").Match(name);

        return new PropertyName
        {
            Name = match.Groups[1].Value,
            Extra = match.Groups[2].Value
        };
    }
}

internal static class MyTEHelpers
    {
        internal static Table GetTheProperInstanceTable(Table table, Type type)
        {
            return ThisIsAVerticalTable(table, type)
                       ? table
                       : FlipThisHorizontalTableToAVerticalTable(table);
        }

        private static bool ThisIsAVerticalTable(Table table, Type type)
        {
            if (TheHeaderIsTheOldFieldValuePair(table))
                return true;
            return (table.Rows.Count() != 1) || (table.Header.Count == 2 && TheFirstRowValueIsTheNameOfAProperty(table, type));
        }

        private static bool TheHeaderIsTheOldFieldValuePair(Table table)
        {
            return table.Header.Count == 2 && table.Header.First() == "Field" && table.Header.Last() == "Value";
        }

        private static bool TheFirstRowValueIsTheNameOfAProperty(Table table, Type type)
        {
            var firstRowValue = table.Rows[0][table.Header.First()];
            return FindMatchProperty(type.GetProperties(), firstRowValue) != null;
        }

        /// <summary>
        /// Finds the match property. if not found, return null
        /// </summary>
        /// <param name="propInfos">The property infos.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static PropertyInfo FindMatchProperty(PropertyInfo[] propInfos, string propertyName)
        {
            return propInfos.FirstOrDefault(pr => pr.Name.ToLower() == propertyName.ToLower());
        }

        private static Table FlipThisHorizontalTableToAVerticalTable(Table table)
        {
            return new PivotTable(table).GetInstanceTable(0);
        }
    }

    }
