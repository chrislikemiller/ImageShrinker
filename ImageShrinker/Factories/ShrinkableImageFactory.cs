using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageShrinker.Common;
using ImageShrinker.Model;

namespace ImageShrinker.Factories
{
    class ShrinkableImageFactory : IShrinkableImageFactory
    {
        public IShrinkableImage CreateShrinkableImage()
        {
            return new ShrinkableImage();
        }

        public IShrinkableImage CreateShrinkableImage(string path, ShrinkerService shrinkerService)
        {
            var image = new ShrinkableImage
            {
                OriginalPath = path,
                ThumbnailTempPath = shrinkerService.GetThumbnailFromTemp(path),
                Title = new FileInfo(path).Name
            };
            return image;
        }
    }
}
