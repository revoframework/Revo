using System.IO;

namespace Revo.Core.IO.Resources
{
    public interface IResourceManager
    {
        Stream TryCreateReadStream(string virtualPath);
        Stream CreateReadStream(string virtualPath);
    }
}