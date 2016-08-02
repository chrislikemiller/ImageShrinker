using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using ImageShrinker.Common;
using ImageShrinker.Model;

public class ShrinkerService
{
    // Private fields
    private DateTime _creationTime;
    private Dictionary<string, ImageCodecInfo> _encoders;
    private static readonly string _tempFolder =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\ImageResizer\thumbs";
    private volatile bool _canShrink;

    // Events
    public readonly Event<int> ReportProgressEvent = new Event<int>();
    public readonly Event<string> ReportCurrentImageEvent = new Event<string>();
    public readonly Event CancelledEvent = new Event();

    // Methods
    public async Task ShrinkAllImagesAsync(ObservableCollection<IShrinkableImage> images, string shrinkDestinationPath, int quality)
    {
        _canShrink = true;
        for (int i = 0; i < images.Count; i++)
        {
            if (!_canShrink)
            {
                CancelledEvent.Raise();
                return;
            }
            ReportCurrentImageEvent.Raise(images[i].Title);
            await ShrinkAsync(images[i].OriginalPath, shrinkDestinationPath, quality);
            ReportProgressEvent.Raise(i + 1);
        }
    }

    private async Task ShrinkAsync(string filepath, string destinationPath, int quality)
    {
        try
        {
            if (!File.Exists(filepath)) throw new FileNotFoundException();
            using (var img = Image.FromFile(filepath))
            {
                _creationTime = File.GetLastAccessTime(filepath);
                string filename = Path.GetFileName(filepath);
                CreateFolder(_tempFolder);
				CreateFolder(destinationPath);
				string path = Path.Combine(destinationPath, filename);
				await SaveImageAsync(path, img, quality, img.PropertyItems);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(@"Exception handled!\n\n" + ex.Message);
        }
    }

    public void StopShrinking()
    {
        _canShrink = false;
    }



    private async Task SaveImageAsync(string path, Image image, int quality, PropertyItem[] properties)
    {
        var qualityParam = new EncoderParameter(Encoder.Quality, quality);
        var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = qualityParam;

        ImageCodecInfo imgCodec;
        if (path.Substring(path.LastIndexOf('.') + 1).ToLower() == "jpg" ||
            path.Substring(path.LastIndexOf('.') + 1).ToLower() == "jpeg")
            imgCodec = GetEncoderInfo("image/jpeg");
        else if (path.Substring(path.LastIndexOf('.') + 1).ToLower() == "bmp")
            imgCodec = GetEncoderInfo("image/bmp");
        else imgCodec = GetEncoderInfo("image/png");

        for (int i = 0; i < properties.Length; i++)
        {
            image.PropertyItems[i] = properties[i];
        }

        await Task.Run(() => image.Save(path, imgCodec, encoderParams));
        File.SetCreationTime(path, _creationTime);
    }

    private ImageCodecInfo GetEncoderInfo(string mimeType)
    {
        string lookupKey = mimeType.ToLower();
        ImageCodecInfo foundCodec = null;
        var encoders = GetEncoders();
        if (encoders.ContainsKey(lookupKey))
        {
            foundCodec = encoders[lookupKey];
        }
        return foundCodec;
    }


    private Dictionary<string, ImageCodecInfo> GetEncoders()
    {
        if (_encoders == null)
        {
            _encoders = new Dictionary<string, ImageCodecInfo>();
        }
        if (_encoders.Count != 0)
            return _encoders;
        foreach (var codec in ImageCodecInfo.GetImageEncoders())
        {
            _encoders.Add(codec.MimeType.ToLower(), codec);
        }
        return _encoders;
    }


    public string GetThumbnailFromTemp(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path");
        var filename = Path.GetFileName(path);
        CreateFolder(_tempFolder);
        var fullPath = _tempFolder + Path.DirectorySeparatorChar + filename;

        using (var bitmap = new Bitmap(Image.FromFile(path).GetThumbnailImage(100, 100, () => false, IntPtr.Zero)))
        {
            if (File.Exists(fullPath))
            {
                var finfo = new FileInfo(fullPath);
                fullPath = finfo.DirectoryName + finfo.Name + new Random().Next(5000) + finfo.Extension;
            }
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Jpeg);
                File.WriteAllBytes(fullPath, stream.ToArray());
            }
        }
        return fullPath;
    }

    private void CreateFolder(string folder)
    {
        if (Directory.Exists(folder))
			return;
        Directory.CreateDirectory(folder);
        SetDirectoryAccess(folder);
    }

    public void CleanupTempFolder()
    {
        if (!Directory.Exists(_tempFolder)) return;
        foreach (var file in new DirectoryInfo(_tempFolder).GetFiles())
        {
            file.Delete();
        }
    }

    private void SetDirectoryAccess(string folder)
    {
        var dInfo = new DirectoryInfo(folder);

        var dSecurity = dInfo.GetAccessControl();
        dSecurity.AddAccessRule(new FileSystemAccessRule(
            new SecurityIdentifier(
                WellKnownSidType.BuiltinUsersSid,
                null), FileSystemRights.DeleteSubdirectoriesAndFiles, //FileSystemRights.FullControl,
            AccessControlType.Allow));

        dInfo.SetAccessControl(dSecurity);
    }
}


