using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageShrinker.Model
{
    public interface IShrinkableImage
    {
        /// <summary>
        /// Name of the image file.
        /// </summary>
         string Title { get; set; }
        /// <summary>
        /// Full path to the image file.
        /// </summary>
        string OriginalPath { get; set; }
        /// <summary>
        /// Full path to the image's thumbnail file.
        /// </summary>
        string ThumbnailTempPath { get; set; }
    }
}
