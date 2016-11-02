
namespace Sitecore.Package.AutoGenerator.Core.Reader
{
    using System.IO;
    using System.Collections.Generic;
    using Sitecore.Package.AutoGenerator.Core.Entities;
    using Sitecore.Package.AutoGenerator.Core.Interface;

    public class CSVReaderProcessor : IReaderTypeProcessor
    {
        public List<ObjectDetails> ReadFile(string filePath)
        {
            var reader = new StreamReader(File.OpenRead(filePath));

            var pathList = new List<ObjectDetails>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                    
                var values = line.Split(new []{';', ','});

                var objectDetail = new ObjectDetails
                {
                    ObjectPath = values[0],
                    ObjectType = values[1],
                    IncludeSubItem = values[2]
                };

                pathList.Add(objectDetail);
            }

            return pathList;
        }
    }
}
