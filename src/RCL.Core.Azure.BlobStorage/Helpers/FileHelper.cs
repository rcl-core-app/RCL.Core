#nullable disable

using System.Diagnostics;

namespace RCL.Core.Azure.BlobStorage
{
    public static class FileHelper
    {
        public static string GetFileExtension(string FileName)
        {
            string ext = string.Empty;

            try
            {
                int fileExtPos = FileName.LastIndexOf(".", StringComparison.Ordinal);
                if (fileExtPos >= 0)
                {
                    ext = FileName.Substring(fileExtPos, FileName.Length - fileExtPos).ToLower();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return ext;
            }

            return ext;
        }

        public static string GetContentType(string FileExtension)
        {
            string ContentType = "application/octet-stream";

            try
            {
                if (FileExtension.ToLower() == ".png")
                {
                    ContentType = "image/png";
                }
                if (FileExtension.ToLower() == ".jpg")
                {
                    ContentType = "image/jpg";
                }
                if (FileExtension.ToLower() == ".jpeg")
                {
                    ContentType = "image/jepg";
                }
                if (FileExtension.ToLower() == ".gif")
                {
                    ContentType = "image/gif";
                }
                if (FileExtension.ToLower() == ".bmp")
                {
                    ContentType = "image/bmp";
                }
                if (FileExtension.ToLower() == ".svg")
                {
                    ContentType = "image/svg+xml";
                }
                if (FileExtension.ToLower() == ".eot")
                {
                    ContentType = "application/vnd.ms-fontobject";
                }
                if (FileExtension.ToLower() == ".ttf")
                {
                    ContentType = "application/font-sfnt";
                }
                if (FileExtension.ToLower() == ".woff")
                {
                    ContentType = "application/font-woff";
                }
                if (FileExtension.ToLower() == ".json")
                {
                    ContentType = "application/json";
                }
                if (FileExtension.ToLower() == ".txt")
                {
                    ContentType = "text/plain";
                }
                if (FileExtension.ToLower() == ".pdf")
                {
                    ContentType = "application/pdf";
                }
                if (FileExtension.ToLower() == ".mp4")
                {
                    ContentType = "video/mp4";
                }
                if (FileExtension.ToLower() == ".zip")
                {
                    ContentType = "application/zip";
                }
                if (FileExtension.ToLower() == ".xml")
                {
                    ContentType = "application/xml";
                }
                if (FileExtension.ToLower() == ".docx")
                {
                    ContentType = "application/docx";
                }
                if (FileExtension.ToLower() == ".xslx")
                {
                    ContentType = "application/xlsx";
                }
                if (FileExtension.ToLower() == ".pptx")
                {
                    ContentType = "application/pptx";
                }
                if (FileExtension.ToLower() == ".html")
                {
                    ContentType = "text/html";
                }
                if (FileExtension.ToLower() == ".css")
                {
                    ContentType = "text/css";
                }
                if (FileExtension.ToLower() == ".js")
                {
                    ContentType = "application/javascript";
                }
                if (FileExtension.ToLower() == ".ico")
                {
                    ContentType = "image/x-icon";
                }
                if (FileExtension.ToLower() == ".pfx")
                {
                    ContentType = "application/x-pkcs12";
                }
                if (FileExtension.ToLower() == ".cer")
                {
                    ContentType = "application/x-x509-ca-cert";
                }
                if (FileExtension.ToLower() == ".crt")
                {
                    ContentType = "application/x-x509-ca-cert";
                }
                if (FileExtension.ToLower() == ".der")
                {
                    ContentType = "application/x-x509-ca-cert";
                }
                if (FileExtension.ToLower() == ".pem")
                {
                    ContentType = "application/x-pem-file";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return ContentType;
            }

            return ContentType;
        }

        public static Stream GetStream(string FilePath)
        {
            Stream stream = null;

            try
            {
                stream = File.Open(FilePath, FileMode.Open);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return stream;
            }

            return stream;
        }

        public static bool IsImageFile(string FileName)
        {
            bool b = false;

            string ext = GetFileExtension(FileName);

            if (ext != ".jpg" && ext != ".JPG" && ext != ".jpeg" && ext != ".JPEG" && ext != ".gif" && ext != ".GIF" && ext != ".png" && ext != ".PNG" && ext != ".bmp" && ext != ".BMP" && ext != ".svg" && ext != ".SVG")
            {
                b = false;
            }
            else
            {
                b = true;
            }

            return b;
        }
    }
}
