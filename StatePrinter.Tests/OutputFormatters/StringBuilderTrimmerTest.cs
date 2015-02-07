using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using StatePrinter.OutputFormatters;

namespace StatePrinter.Tests.OutputFormatters
{
    [TestFixture]
    class StringBuilderTrimmerTest
    {
        [Test]
        public void TestTrimLast_Empty()
        {
            var sb = new StringBuilder("");
            Assert.AreEqual(0, new StringBuilderTrimmer().TrimLast(sb));
        }

        [Test]
        public void TestTrimLast_NothingToTrim()
        {
            var sb = new StringBuilder("abvc");
            Assert.AreEqual(0, new StringBuilderTrimmer().TrimLast(sb));
        }

        [Test]
        public void TestTrimLast_TrimSpaces()
        {
            var sb = new StringBuilder("abvc  ");
            Assert.AreEqual(2, new StringBuilderTrimmer().TrimLast(sb));
        }

        [Test]
        public void TestTrimLast_TrimAllSpaces()
        {
            var sb = new StringBuilder("   ");
            Assert.AreEqual(3, new StringBuilderTrimmer().TrimLast(sb));
        }
    }
}
