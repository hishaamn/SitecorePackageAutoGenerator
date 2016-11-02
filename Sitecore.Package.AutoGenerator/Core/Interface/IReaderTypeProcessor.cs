
namespace Sitecore.Package.AutoGenerator.Core.Interface
{
    using System.Collections.Generic;
    using Sitecore.Package.AutoGenerator.Core.Entities;

    public interface IReaderTypeProcessor
    {
        List<ObjectDetails> ReadFile(string filePath);
    }
}
