﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Configuration
{
    [TestClass]
    public class ConfigurationLoaderInstanceTests
    {
        [TestMethod]
        public void CanCreateInstanceFromExistingGraphAndLoadObjectFromBlankNode()
        {
            // given
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" ;
  dnr:usingTripleCollection _:b .
_:b a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.TreeIndexedTripleCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            // when
            var configuration = new ConfigurationLoader(g);
            var collection = configuration.LoadObject<BaseTripleCollection>("a");

            // then
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection is ThreadSafeTripleCollection);
        }
        [TestMethod]
        public void CanCreateInstanceFromExistingGraphAndLoadObjectFromUri()
        {
            // given
            String graph = ConfigLookupTests.Prefixes + @"
@base <http://example.com/> .

<collection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" ;
  dnr:usingTripleCollection <indexedCollection> .
<indexedCollection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.TreeIndexedTripleCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            // when
            var configuration = new ConfigurationLoader(g);
            var collection = configuration.LoadObject<BaseTripleCollection>(new Uri("http://example.com/indexedCollection"));

            // then
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection is TreeIndexedTripleCollection);
        }
    }
}
