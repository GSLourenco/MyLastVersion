using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Security.Principal;
using System.Web.Script.Serialization;
using System.IO;


namespace MvcApplication2.DataModel
{
    public class Utils
    {
        public static int getDaysOfWeekInt(IEnumerable<bool> daysOfWeek)
        {
            int res = 0;
            for (int i = 0; i < daysOfWeek.Count(); i++)
            {
                if (daysOfWeek.ElementAt(i))
                    res += (int)Math.Pow(2, i);
            }
            return res;
        }

        public static string GenerateSaltedSHA1(string plainTextString)
        {
            HashAlgorithm algorithm = new SHA1Managed();
            //var saltBytes = GenerateSalt(4);
            var plainTextBytes = Encoding.ASCII.GetBytes(plainTextString);

            //var plainTextWithSaltBytes = AppendByteArray(plainTextBytes, saltBytes);
            var saltedSHA1Bytes = algorithm.ComputeHash(plainTextBytes);
            //var saltedSHA1WithAppendedSaltBytes = AppendByteArray(saltedSHA1Bytes, saltBytes);

            return Convert.ToBase64String(saltedSHA1Bytes);
        }

        public static string [] GenerateSHA1WithNonRandomSalt(string plainTextString, string salt)
        {
            HashAlgorithm algorithm = new SHA1Managed();
            var saltBytes = Encoding.ASCII.GetBytes(salt); 
            var plainTextBytes = Encoding.ASCII.GetBytes(plainTextString);

            var plainTextWithSaltBytes = AppendByteArray(plainTextBytes, saltBytes);
            var saltedSHA1Bytes = algorithm.ComputeHash(plainTextWithSaltBytes);
            //var saltedSHA1WithAppendedSaltBytes = AppendByteArray(saltedSHA1Bytes, saltBytes);

            return new String[] { Convert.ToBase64String(saltedSHA1Bytes), Convert.ToBase64String(saltBytes) };
        }

        public static System.Drawing.Bitmap DoResize(System.Drawing.Bitmap originalImg, int widthInPixels, int heightInPixels)
        {
            System.Drawing.Bitmap bitmap;
            
                bitmap = new System.Drawing.Bitmap(widthInPixels, heightInPixels);
                using (System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap))
                {
                    // Quality properties
                    graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphic.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                    graphic.DrawImage(originalImg, 0, 0, widthInPixels, heightInPixels);
                    return bitmap;
                }
            
            
        }

        public static Stream ImageToArray(System.Drawing.Bitmap image,String path)
        {
                MemoryStream mem = new MemoryStream();
                mem.Position = 0;
                switch (path)
                {
                    case ".gif":
                        image.Save(mem, System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                    case ".png":
                        image.Save(mem, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    default:
                        image.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                }
                return mem;
        }

        private static byte[] GenerateSalt(int saltSize)
        {
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[saltSize];
            rng.GetBytes(buff);
            return buff;
        }

        private static byte[] AppendByteArray(byte[] byteArray1, byte[] byteArray2)
        {
            var byteArrayResult =
                    new byte[byteArray1.Length + byteArray2.Length];

            for (var i = 0; i < byteArray1.Length; i++)
                byteArrayResult[i] = byteArray1[i];
            for (var i = 0; i < byteArray2.Length; i++)
                byteArrayResult[byteArray1.Length + i] = byteArray2[i];

            return byteArrayResult;
        }

        internal static Boolean checkUri(string urls)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            String [] u = js.Deserialize<String []>(urls);
            if (u.Length > 5) return false;

            for (int i = 0; i < u.Length; i++)
            {
                Uri uriResult;
                String uri = u[i];
                bool result = Uri.TryCreate(uri, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp
                  || uriResult.Scheme == Uri.UriSchemeHttps) ;
                if (!result) return false;
            }
            return Program.validAmazonUri(u);
        }

        
    }

    class ICustomPrincipal : IPrincipal
    {

        public String EmailId { get; set; }
       
        public IIdentity Identity { get; private set; }
        public bool IsInRole(string role) { return false; }

        public ICustomPrincipal(){
         this.Identity = new GenericIdentity("");   
        }

        public ICustomPrincipal(string email)
        {
        this.Identity = new GenericIdentity(email);
        }
    }
}