using ImageResizer;
using System;
using System.IO;

namespace Discitur.QueryStack.Logic.Services
{
    public class ImageConverter : IImageConverter
    {
        public string ToPictureString(byte[] pictureBytes)
        {
            Stream stPictureSource = new MemoryStream(pictureBytes);
            // Resize for Picture
            MemoryStream stPictureDest = new MemoryStream();
            var pictureSettings = new ResizeSettings
            {
                MaxWidth = Constants.USER_PICTURE_MAXWIDTH,
                MaxHeight = Constants.USER_PICTURE_MAXHEIGHT,
                Format = Constants.USER_PICTURE_FORMAT,
                Mode = FitMode.Crop
            };
            ImageBuilder.Current.Build(stPictureSource, stPictureDest, pictureSettings);
            return Convert.ToBase64String(stPictureDest.ToArray());
        }

        public byte[] ToPictureBytes(string pictureString)
        {
            byte[] bytes = new byte[pictureString.Length * sizeof(char)];
            System.Buffer.BlockCopy(pictureString.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public string ToThumbNailString(byte[] pictureBytes)
        {
            MemoryStream stThumbDest = new MemoryStream();
            Stream stThumbSource = new MemoryStream(pictureBytes);
            var thumbSettings = new ResizeSettings
            {
                MaxWidth = Constants.USER_THUMB_MAXWIDTH,
                MaxHeight = Constants.USER_THUMB_MAXHEIGHT,
                Format = Constants.USER_THUMB_FORMAT,
                Mode = FitMode.Crop
            };
            ImageBuilder.Current.Build(stThumbSource, stThumbDest, thumbSettings);
            return Convert.ToBase64String(stThumbDest.ToArray());
        }

    }
}
