using System.IO;

namespace Discitur.QueryStack.Logic.Services
{
    public interface IImageConverter
    {
        string ToPictureString(byte[] pictureBytes);
        byte[] ToPictureBytes(string pictureString);
        string ToThumbNailString(byte[] pictureBytes);
    }
}
