using NUnit.Framework;

namespace DynamicXMLReaderTests
{

    [TestFixture]
    public class DynamicXmlReaderTests
    {
        private IDynamicXmlReaderFactory _readerFactory;

        [SetUp]
        public void Given()
        {
            _readerFactory = new DynamicXmlReaderFactory();
        }

        [Test]
        public void CanReadOneLevelAttributeAsProperty()
        {
            const string someXml = "<name first=\"Aaron\" />";
            var dynamicReader = _readerFactory.Parse(someXml);
            Assert.AreEqual("Aaron", dynamicReader.Name.First);
        }

        [Test]
        public void CanReadTwoLevelElementAsProperty()
        {
            const string someXml
                = "<name first=\"Aaron\" last=\"Erickson\">" +
                  "<age>37</age>" +
                  "<book>The Nomadic Developer</book>" +
                  "<book>Professional F#</book>" +
                  "</name>";
            var dynamicReader = _readerFactory.Parse(someXml);
            var nomadicDev = dynamicReader.Name.Book[0];
            Assert.AreEqual("The Nomadic Developer", nomadicDev);
            Assert.AreEqual(37, int.Parse(dynamicReader.Name.Age));
        }

        [Test]
        public void CanReadAListOfThingsViaXPath()
        {
            const string someXml
                = "<name first=\"Aaron\" last=\"Erickson\">" +
                  "<age>37</age>" +
                  "<book>The Nomadic Developer</book>" +
                  "<book>Professional F#</book>" +
                  "<book>The Monadic Developer</book>" +
                  "<book>Hop on Pop</book>" +
                  "</name>";
            var books = _readerFactory.Parse(someXml).Using("//book");
            Assert.AreEqual("The Nomadic Developer", books[0]);
            Assert.AreEqual("Professional F#", books[1]);
            Assert.AreEqual("The Monadic Developer", books[2]);
            Assert.AreEqual("Hop on Pop", books[3]);
        }

        [Test]
        public void CanReadTheSecondBook()
        {
            const string someXml
                = "<author first=\"Aaron\" last=\"Erickson\">" +
                  "<age>37</age>" +
                  "<book>The Nomadic Developer</book>" +
                  "<book>Professional F#</book>" +
                  "</author>";
            var dynamicReader = _readerFactory.Parse(someXml);
            Assert.AreEqual("Professional F#", dynamicReader.Author.Book[1]);
        }

        [Test]
        public void CanReadTheFirstBooksSubTitleTitle()
        {
            const string someXml
                = "<name first=\"Aaron\" last=\"Erickson\">" +
                  "<age>37</age>" +
                  "<book>" +
                  "<title>The Nomadic Developer</title>" +
                  "<subtitle>Surviving in consulting</subtitle>" +
                  "</book>" +
                  "<book>Professional F#</book>" +
                  "</name>";
            var dynamicReader = _readerFactory.Parse(someXml);
            Assert.AreEqual("Surviving in consulting", dynamicReader.Name.Book[0].Subtitle);
        }

        [Test]
        public void CanReadBooksUsingXPath()
        {
            const string someXml
                = "<name first=\"Aaron\" last=\"Erickson\">" +
                  "<awesomebooks>" +
                  "<book id='Awesome'>The Big Short</book>" +
                  "</awesomebooks>" +
                  "<book>" +
                  "<title>The Nomadic Developer</title>" +
                  "<subtitle>Surviving in consulting</subtitle>" +
                  "</book>" +
                  "<book>Professional F#</book>" +
                  "</name>";
            var dynamicReader = _readerFactory.Parse(someXml).Using("//book");
            Assert.AreEqual("Awesome", dynamicReader[0].Id);
            Assert.AreEqual("The Big Short", dynamicReader[0].Value);
        }

        [Test]
        public void CanReadUsingIndexer()
        {
            const string someXml
                = "<name first=\"Aaron\" last=\"Erickson\">" +
                  "<age>37</age>" +
                  "<book>The Nomadic Developer</book>" +
                  "<book>Professional F#</book>" +
                  "</name>";
            var dynamicReader = _readerFactory.Parse(someXml);
            var result = int.Parse(dynamicReader.Name["Age"]);

            Assert.That(result, Is.EqualTo(37));
        }

        //[Test]
        //public void CanReadTccc8Rss()
        //{
        //  const string someXmlUrl
        //    = "http://search.twitter.com/search.atom?q=%23tccc8";
        //  var dynamicReader = DynamicXmlReaderDmoVersion.Load(someXmlUrl);
        //  var entries = (IEnumerable<dynamic>) dynamicReader.Feed.Entry;
        //  var entryCountForTccc8 = entries.Count(d => d.Title.Contains("#tccc8") || d.Title.Contains("#TCCC8"));
        //  Assert.AreEqual(entries.Count(), entryCountForTccc8);
        //}
    }
}
