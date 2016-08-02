using ImageShrinker.Common;
using ImageShrinker.Model;

namespace ImageShrinker.Factories
{
    public interface IShrinkableImageFactory
    {
        IShrinkableImage CreateShrinkableImage();
        IShrinkableImage CreateShrinkableImage(string path, ShrinkerService shrinkerService);
    }
}
