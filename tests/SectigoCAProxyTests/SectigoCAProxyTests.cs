using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace SectigoCAProxyTests
{
    [TestClass]
    public class SectigoCAProxyTests
    {
        [TestMethod]
        public void TestMethod1()
        {

            Dictionary<string, string[]> syncFilter = new Dictionary<string, string[]> { ["sslTypeId"] = new string[] { "1","2","3"} };

            foreach (var s in syncFilter)
            {
                foreach (var value in s.Value)
                    Console.WriteLine($"Request with Filter: {s.Key}={value}");

            }
        }
    }
}
