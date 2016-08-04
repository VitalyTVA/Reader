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
        const string path = @"E:\GitHub\Reader\Reader\Assets\Dictionaries\";
        const string chineseOxford = path + @"Oxford_Chinese\oxford-big5";
        [Test]
        public void ParseIfo() {
            StardictInfoParser parser = new StardictInfoParser();
            StardictInfo info = parser.parse(GetFile(chineseOxford, DictFileKind.ifo));
            Assert.AreEqual(39429, info.mWordCount);
            Assert.AreEqual("m", info.mSameTypeSequence);

        }
        [Test]
        public void ParseIdx() {
            StardictIndexParser parser = new StardictIndexParser();
            StardictIndex index = parser.parse(GetFile(chineseOxford, DictFileKind.idx));
            var entry = index.lookupWord("hello");
            Assert.IsNotNull(entry);
            Assert.AreEqual(8, entry.mLength);
            Assert.AreEqual(5770438, entry.mOffset);
        }
        [Test]
        public void testLookupWord() {
            Stardict dict = GetDict(chineseOxford);
            String meaning = dict.lookupWord("zoo");
            Assert.AreEqual(true, meaning.Contains("動物園"));
        }
        static Stardict GetDict(string fileName) {
            Stardict dict = new Stardict();
            dict.loadDictionary(GetFile(fileName, DictFileKind.ifo), GetFile(fileName, DictFileKind.idx), GetFile(fileName, DictFileKind.dict));
            return dict;
        }
        static FileStream GetFile(string fileName, DictFileKind kind) {
            return File.Open(fileName + "." + kind.ToString(), FileMode.Open, FileAccess.Read);
        }
    }
}
