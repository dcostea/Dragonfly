using MLCodeGenerator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MLCodeGenerator.Helpers
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

            //var ver = Header.VersionToString(header.Version);


            /*
// Validate the header before returning. CheckDecode is used for incorrect
            // formatting.

            _host.CheckDecode(header.Signature == Header.SignatureValue,
                "This does not appear to be a binary dataview file");

            // Obviously the compatibility version can't exceed the true version of the file.
            if (header.CompatibleVersion > header.Version)
            {
                throw _host.ExceptDecode("Compatibility version {0} cannot be greater than file version {1}",
                    Header.VersionToString(header.CompatibleVersion), Header.VersionToString(header.Version));
            }

            if (header.Version < ReaderFirstVersion)
            {
                throw _host.ExceptDecode("Unexpected version {0} encountered, earliest expected here was {1}",
                    Header.VersionToString(header.Version), Header.VersionToString(ReaderFirstVersion));
            }
            // Check the versions.
            if (header.CompatibleVersion < MetadataVersion)
            {
                // This is distinct from the earlier message semantically in that the check
                // against ReaderFirstVersion is an indication of format impurity, whereas this
                // is simply a matter of software support.
                throw _host.Except("Cannot read version {0} data, earliest that can be handled is {1}",
                    Header.VersionToString(header.CompatibleVersion), Header.VersionToString(MetadataVersion));
            }
            if (header.CompatibleVersion > ReaderVersion)
            {
                throw _host.Except("Cannot read version {0} data, latest that can be handled is {1}",
                    Header.VersionToString(header.CompatibleVersion), Header.VersionToString(ReaderVersion));
            }

            _host.CheckDecode(header.RowCount >= 0, "Row count cannot be negative");
            _host.CheckDecode(header.ColumnCount >= 0, "Column count cannot be negative");
            // Check the table of contents offset, though we do not at this time have the contents themselves.
            if (header.ColumnCount != 0 && header.TableOfContentsOffset < Header.HeaderSize)
                throw _host.ExceptDecode("Table of contents offset {0} less than header size, impossible", header.TableOfContentsOffset);

            // Check the tail signature.
            if (header.TailOffset < Header.HeaderSize)
                throw _host.ExceptDecode("Tail offset {0} less than header size, impossible", header.TailOffset);
            _stream.Seek(header.TailOffset, SeekOrigin.Begin);
            ulong tailSig = _reader.ReadUInt64();
            _host.CheckDecode(tailSig == Header.TailSignatureValue, "Incorrect tail signature");             
             */





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
                //var compression = reader.ReadByte();

                reader.ReadBytes(19);
            }

            return features;
        }
    }

}
