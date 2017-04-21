using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Specflow.DSL.Examples.SpecFlow.Assist;
using Specflow.DataDriven.DataDriven;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow.Assist;

namespace Examples
{
    [Binding]
    public sealed class ExampleSteps
    {

        [When(@"fill in following data to ""(.*)"":")]
        public void WhenFillInFollowingDataTo(string p0, Table table)
        {
            Regex reg = new Regex("");
            var obj = new TestObj();
            // table.FillInstance(obj);
           table.AssignValueTo(obj);

            ScenarioContext.Current["TestObj"] = obj;
        }

        [Then(@"verify the following data in ""(.*)"" should pass:")]
        public void ThenVerifyTheFollowingDataIn(string p0, Table table)
        {
            // table.CompareToInstance(ScenarioContext.Current["TestObj"] as TestObj);
            table.VerifyData(ScenarioContext.Current["TestObj"] as TestObj);
        }

        [Then(@"verify the following data in ""(.*)"" should fail:")]
        public void ThenVerifyTheFollowingDataInFail(string p0, Table table)
        {
           try
            {
                //  table.CompareToInstance(ScenarioContext.Current["TestObj"] as TestObj);
                table.VerifyData(ScenarioContext.Current["TestObj"] as TestObj);
            }
            catch (ComparisonException e)
            {
               
            }
        }
    }
}
