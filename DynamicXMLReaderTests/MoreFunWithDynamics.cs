using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DynamicXMLReaderTests
{
  [TestFixture]
  class MoreFunWithDynamics
  {
    [Test]
    public void FunWithExpando()
    {
      dynamic someExpando = new ExpandoObject();
      Func<dynamic, int> monthsOldComputation = d => d.Age*12;
      someExpando.Age = 37;
      Assert.AreEqual(37 * 12, monthsOldComputation(someExpando));
    }
  }
}
