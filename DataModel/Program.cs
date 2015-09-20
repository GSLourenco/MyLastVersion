using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Data.SqlClient;
using System.Web;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace MvcApplication2.DataModel
{
    public static class Program
    {
        
        static string bucket = "temporary-pictograms";
        static string server = "https://s3.eu-central-1.amazonaws.com/";
        static string connectionString = "Server=a439bc53-85c1-49f7-8c5a-a46b015ffb69.sqlserver.sequelizer.com;Database=dba439bc5385c149f78c5aa46b015ffb69;User ID=svjhkfovzvikurmt;Password=Vop7sKzFtMm2gRYNnxRRjtGpzF4BPM77mTbw52thxX7SbPRmbPnx8TKAP8EUP6YP;";
        public const int ImageMinimumBytes = 512;
        public const int ImageMaximumBytes = 2000000;

        static void Main(string[] args)
        {


            using (IAmazonS3 client = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1))
            {
                ListingObjects(client);
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static bool IsImage(HttpPostedFileBase postedFile)
        {
            if (postedFile.ContentLength > ImageMaximumBytes)
                return false;
            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "image/jpg" &&
                        postedFile.ContentType.ToLower() != "image/jpeg" &&
                        postedFile.ContentType.ToLower() != "image/pjpeg" &&
                        postedFile.ContentType.ToLower() != "image/gif" &&
                        postedFile.ContentType.ToLower() != "image/x-png" &&
                        postedFile.ContentType.ToLower() != "image/png")
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (Path.GetExtension(postedFile.FileName).ToLower() != ".jpg"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".png"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".gif"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg")
            {
                return false;
            }

            //-------------------------------------------
            //  Attempt to read the file and check the first bytes
            //-------------------------------------------
            try
            {
                if (!postedFile.InputStream.CanRead)
                {
                    return false;
                }

                if (postedFile.ContentLength < ImageMinimumBytes)
                {
                    return false;
                }

                byte[] buffer = new byte[512];
                postedFile.InputStream.Read(buffer, 0, 512);
                string content = System.Text.Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            //-------------------------------------------
            //  Try to instantiate new Bitmap, if .NET will throw exception
            //  we can assume that it's not a valid image
            //-------------------------------------------

            try
            {
                using (var bitmap = new System.Drawing.Bitmap(postedFile.InputStream))
                {
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static String putObject(HttpPostedFileBase file,String name)
        {
            String filename = name + file.FileName;
            String url = server +bucket +"/"+ filename;

            using (IAmazonS3 client = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1))
            {
     
               //Check if file exists
                var s3FileInfo = new Amazon.S3.IO.S3FileInfo(client, bucket,filename);
                if (s3FileInfo.Exists)
                {
                    GetObjectMetadataRequest request = new GetObjectMetadataRequest();
                    request.BucketName = bucket;
                    request.Key = filename;

                    GetObjectMetadataResponse response = client.GetObjectMetadata(request);
                    //Get tag from Amazon file
                    String S3Tag = response.ETag.Replace("/", "").Replace("\"", "");

                    using (var md5 = MD5.Create())
                    {
                        using (var stream = file.InputStream)
                        {
                            //Get tag from local file
                            stream.Position = 0;
                            String fileTag =BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                            if (fileTag == S3Tag) return url;
                            return null;
                        }
                    }
  
                }
                else
                {
                    int size = file.ContentLength;

                    PutObjectRequest request = new PutObjectRequest();
                    request.CannedACL = S3CannedACL.PublicRead;
                    request.BucketName = bucket;
                    request.ContentType = file.ContentType;
                    request.Key = filename;
                    request.InputStream =file.InputStream;
                    client.PutObject(request);

             
                    PictogramsDb.UpdateTraffic(name, size);
                    
                }
            }

            return url;
        }

        public static Boolean validAmazonUri(String [] urls)
        {
            using (IAmazonS3 client = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1))
            {
                for (int i = 0; i < urls.Length; i++)
                {

                    String[] param = urls[i].Split('/');
                    if (param.Length != 5) return false;

                    var s3FileInfo = new Amazon.S3.IO.S3FileInfo(client, param[3], param[4]);
                    if (!s3FileInfo.Exists) return false;
                }
                return true;
            }
        }

        public static String ReplaceObject(HttpPostedFileBase file,String name)
        {
            String filename = name + file.FileName;
            String url = server + bucket + "/" + filename; ;

            using (IAmazonS3 client = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1))
            {
                 var s3FileInfo = new Amazon.S3.IO.S3FileInfo(client, bucket,filename);
                 if (s3FileInfo.Exists)
                 {
                     DeleteObjectRequest drequest = new DeleteObjectRequest();
                     drequest.BucketName = bucket;
                     drequest.Key = filename;
                     client.DeleteObject(drequest);
                 }


                int size = file.ContentLength;
                PutObjectRequest request = new PutObjectRequest();
                request.CannedACL = S3CannedACL.PublicRead;
                request.BucketName = bucket;
                request.ContentType = file.ContentType;
                request.Key = file.FileName;
                request.InputStream = file.InputStream;
                client.PutObject(request);

                PictogramsDb.UpdateTraffic(name, size);

            }

            return url;


        }
        static void ListingObjects(IAmazonS3 client)
        {
            ListObjectsRequest list = new ListObjectsRequest
            {
                BucketName = "pictograms",
                MaxKeys = 2
            };

            string name;
            string url;

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                do
                {
                    ListObjectsResponse response = client.ListObjects(list);


                    // Process response.
                    foreach (S3Object entry in response.S3Objects)
                    {
                        name = entry.Key;
                        url = server + bucket + name;
                        int index = name.IndexOf("_");

                        if (index > 0) { name = name.Substring(0, index); }

                        else
                        {
                            index = name.IndexOf(".");
                            name = name.Substring(0, index);
                        }



                        SqlCommand insertCommand = new SqlCommand("INSERT INTO Pictograms (name, url) VALUES (@0, @1)", conn);

                        insertCommand.Parameters.Add(new SqlParameter("0", name));
                        insertCommand.Parameters.Add(new SqlParameter("1", url));

                        insertCommand.ExecuteNonQuery();



                    }

                    // If response is truncated, set the marker to get the next 
                    // set of keys.
                    if (response.IsTruncated)
                    {
                        list.Marker = response.NextMarker;
                    }
                    else
                    {
                        list = null;
                    }

                } while (list != null);
            }
        }
    }


}
