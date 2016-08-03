using NUnit.Framework;
using StarDict;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests {
    [TestFixture]
    public class StarDictTests {
        const string fileName = @"E:\GitHub\Reader\Reader\Assets\Dictionaries\Oxford\oxford-big5";
        [Test]
        public void ParseIfo() {
            StardictInfoParser parser = new StardictInfoParser();
            StardictInfo info = parser.parse(GetIfo());
            Assert.AreEqual(39429, info.mWordCount);
            Assert.AreEqual("m", info.mSameTypeSequence);

        }
        [Test]
        public void ParseIdx() {
            StardictIndexParser parser = new StardictIndexParser();
            StardictIndex index = parser.parse(GetIdx());
            var entry = index.lookupWord("hello");
            Assert.IsNotNull(entry);
            Assert.AreEqual(8, entry.mLength);
            Assert.AreEqual(5770438, entry.mOffset);
            //System.out.println(entry.mOffset);
            //System.out.println(entry.mLength);
        }
        [Test]
        public void testLookupWord() {
            Stardict dict = new Stardict();
            dict.loadDictionary(GetIfo(), GetIdx(), GetDict());
            String meaning = dict.lookupWord("zoo");
            Assert.AreEqual(true, meaning.Contains("動物園"));
        }
        static FileStream GetIfo() {
            return File.Open(fileName + ".ifo", FileMode.Open, FileAccess.Read);
        }
        static FileStream GetIdx() {
            return File.Open(fileName + ".idx", FileMode.Open, FileAccess.Read);
        }
        static FileStream GetDict() {
            return File.Open(fileName + ".dict", FileMode.Open, FileAccess.ReadWrite);
        }
    }
}
