namespace ImageShrinker.Model
{
    public class ShrinkableImage : IShrinkableImage
    {
        public string Title { get;set; }

        public string OriginalPath { get; set; }

        public string ThumbnailTempPath { get; set; }

    }
}
