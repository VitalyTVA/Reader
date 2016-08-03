using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StarDict {
    public class StardictInfo {
        public int mWordCount;
        public string mSameTypeSequence;

        public bool hasSameTypeSequence() {
            return mSameTypeSequence != null && mSameTypeSequence.Length > 0;
        }
    }

    public class StardictInfoParser {
        public StardictInfoParser() {
        }

        private static readonly String KEY_WORDCOUNT = "wordcount";
        private static readonly String KEY_SAME_TYPE_SEQ = "sametypesequence";

        /**
         * Example info file
         * StarDict's dict ifo file
         * version=2.4.2
         * wordcount=57508
         * idxfilesize=1033658
         * bookname=CDICT5Φï▒µ╝óΦ╛¡σà╕
         * description=τÄïτàÆΦ╜ëµÅ¢σê░stardict1.33+∩╝îΦâíµ¡úσåìσ░çσà╢Φ╜ëµÅ¢σê░stardict2πÇé
         * date=2003.05.13
         * sametypesequence=tm
         *
         * @param line
         * @param info
         */
        private void processLine(String line, StardictInfo info) {
            String[] tokens = line.Split('=');
            if(tokens.Length == 2) {
                String key = tokens[0];
                String value = tokens[1];
                if(KEY_WORDCOUNT.Equals(key)) {
                    info.mWordCount = int.Parse(value);
                } else if(KEY_SAME_TYPE_SEQ.Equals(key)) {
                    info.mSameTypeSequence = value;
                }
            }
        }

        public StardictInfo parse(Stream infoFile) {
            var reader = new StreamReader(infoFile);
            StardictInfo info = new StardictInfo();
            try {
                String line;
                while((line = reader.ReadLine()) != null) {
                    processLine(line, info);
                }
            } finally {
                reader.Dispose();
            }
            return info;
        }
    }

    public class StardictIndex {

        private Dictionary<char, List<StarDictIndexEntry>> mMap;

        public StardictIndex() {
            mMap = new Dictionary<char, List<StarDictIndexEntry>>();
            for(char c = 'a'; c <= 'z'; c++) {
                mMap.Add(c, new List<StarDictIndexEntry>());
            }
        }

        public void addToIndex(String word, int offset, int length) {
            word = word.ToLower();
            var firstCharacter = word[0];
            if(char.IsLetter(firstCharacter)) {
                var list = mMap[firstCharacter];
                StarDictIndexEntry entry = new StarDictIndexEntry(word, offset,
                        length);
                list.Add(entry);
            }
        }

        public StarDictIndexEntry lookupWord(String word) {
            word = word.ToLower();
            var needle = new StarDictIndexEntry(word, 0, 0);
            var firstCharacter = word[0];
            var list = mMap[firstCharacter];
            int index = list.BinarySearch(needle);
            if(index == -1) {
                return null;
            }
            return list[index];
        }

        public sealed class StarDictIndexEntry : IComparable<StarDictIndexEntry> {

            public StarDictIndexEntry(String word, int offset, int length) {
                this.mWord = word;
                this.mOffset = offset;
                this.mLength = length;
            }

            public String mWord;
            public int mOffset;
            public int mLength;


            int IComparable<StarDictIndexEntry>.CompareTo(StarDictIndexEntry o) {
                return this.mWord.CompareTo(o.mWord);
            }
        }
    }

    public class StardictIndexParser {
        public StardictIndex parse(Stream indexFile) {
            StardictIndex index = new StardictIndex();
            var @in = new BinaryReader(indexFile);
            byte byteRead;
            var bos = new List<byte>();
            bool eof = false;
            while(!eof) {
                try {
                    byteRead = @in.ReadByte();
                    if(byteRead == 0) {
                        var bytes = bos.ToArray();
                        String word = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                        uint offset = ReadUInt32(@in);
                        uint length = ReadUInt32(@in);

                        index.addToIndex(word, (int)offset, (int)length);
                        bos = new List<byte>();
                    } else {
                        bos.Add(byteRead);
                    }
                } catch(Exception) {
                    eof = true;
                }
            }
            @in.Dispose();
            return index;
        }
        static byte[] ReadBigEndianBytes(BinaryReader reader, int count) {
            byte[] bytes = new byte[count];
            for(int i = count - 1; i >= 0; i--)
                bytes[i] = reader.ReadByte();

            return bytes;
        }
        static uint ReadUInt32(BinaryReader reader) {
            return BitConverter.ToUInt32(ReadBigEndianBytes(reader, 4), 0);
        }
    }


    public class Stardict {
        private StardictInfo mInfo;
        private StardictIndex mIndex;
        private Stream mDictFile;

        public void loadDictionary(Stream infoFile, Stream indexFile,
                Stream mDictFile) {
            StardictInfoParser infoParser = new StardictInfoParser();
            mInfo = infoParser.parse(infoFile);
            StardictIndexParser indexParser = new StardictIndexParser();
            mIndex = indexParser.parse(indexFile);
            this.mDictFile = mDictFile;
        }

        public String lookupWord(String word) {
            var entry = mIndex.lookupWord(word);
            return getMeaning(entry.mOffset, entry.mLength);
        }


        private String getMeaning(int offset, int length) {
            long unsignedOffset = offset & 0xFFFFFFFF;
            long unsignedLength = length & 0xFFFFFFFF;

            mDictFile.Seek(unsignedOffset, SeekOrigin.Begin);

            int count = 0;

            if(mInfo.mSameTypeSequence.Equals("tm")) {
                while(mDictFile.ReadByte() != 0) {
                    count++;
                }
            }

            byte[] buffer = new byte[(int)unsignedLength - count];
            mDictFile.Read(buffer, 0, buffer.Length);

            return System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }
    }
}
