using Rendering.Objects;

namespace Rendering.Parser;

public interface IModelParser
{
    public Model Parse(string filePath);
}