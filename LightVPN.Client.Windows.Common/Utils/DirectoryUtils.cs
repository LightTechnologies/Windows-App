namespace LightVPN.Client.Windows.Common.Utils
{
    using System.IO;

    public static class DirectoryUtils
    {
        public static void DirectoryNotExistsCreate(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }
}
