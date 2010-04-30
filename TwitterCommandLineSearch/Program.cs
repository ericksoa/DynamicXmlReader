using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using DynamicXMLReader;

namespace TwitterCommandLineSearch
{
  class Program
  {
    static void Main(string[] args)
    {
      dynamic config = new ExpandoObject();
      if (args.Length > 0)
        config.Search = args[0];
      else
      {
        Console.WriteLine("What would you like to search on Twitter?");
        config.Search = Console.ReadLine();
      }
      string someXmlUrl = "http://search.twitter.com/search.atom?q=" + config.Search;
      var dynamicReader = DynamicXmlReaderDmoVersion.Load(someXmlUrl);

      //TODO: fix strange binding issue with *iterating* through enumerables
      Console.WriteLine(dynamicReader.Feed.Entry[0].Author.Name + " wrote " + dynamicReader.Feed.Entry[0].Title);

      Console.WriteLine("Find the any key and press it");
      Console.ReadKey();
    }
  }
}
