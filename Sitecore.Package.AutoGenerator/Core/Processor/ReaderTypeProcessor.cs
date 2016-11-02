
namespace Sitecore.Package.AutoGenerator.Core.Processor
{
    using System;
    using Sitecore.Package.AutoGenerator.Core.Interface;
    using Sitecore.Package.AutoGenerator.Core.Reader;

    public class ReaderTypeProcessor
    {
        public static IReaderTypeProcessor GetTypeProcessor(string typeName)
        {
            switch (typeName.ToLower())
            {
                case "csv":
                    {
                        return Activator.CreateInstance<CSVReaderProcessor>();
                    }
            }
            return default(IReaderTypeProcessor);
        }
    }
}