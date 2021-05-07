using Dragonfly.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Dragonfly.Helpers
{
    public static class StreamHelper
    {
        public static Stream GetZipFileStream(string zipPath)
        {
            var zip = ZipFile.OpenRead(zipPath)
                .Entries.Where(x => x.Name.Equals("Schema", StringComparison.InvariantCulture))
                .FirstOrDefault()
                .Open();

            return zip;
        }

        public static IEnumerable<Feature> ExtractFeatures(BinaryReader reader)
        {
            var features = new List<Feature>();

            var index = 0;
            var header = new Header
            {
                Signature = reader.ReadUInt64(),
                Version = reader.ReadUInt64(),
                CompatibleVersion = reader.ReadUInt64(),
                TableOfContentsOffset = reader.ReadInt64(),
                TailOffset = reader.ReadInt64(),
                RowCount = reader.ReadInt64(),
                ColumnCount = reader.ReadInt32()
            };

            var paddingSize = (int)header.TableOfContentsOffset - sizeof(ulong) * 3 - sizeof(long) * 3 - sizeof(int);
            reader.ReadBytes(paddingSize);

            for (int i = 0; i < header.ColumnCount; i++)
            {
                var name = reader.ReadString();
                var type = reader.ReadString();
                features.Add(new Feature
                {
                    Name = name,
                    Type = type,
                    Index = index++
                });

                reader.ReadBytes(19); // skip bytes to the next column
            }

            return features;
        }
    }
}
