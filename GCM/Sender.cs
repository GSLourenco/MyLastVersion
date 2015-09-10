using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Cache;
using MvcApplication2.GCM;

namespace MvcApplication2.GCM
{
    public class Sender
    {
        protected static readonly String UTF8 = "UTF-8";
        private readonly String key;

        /**
        * Default constructor.
         *
         * @param key API key obtained through the Google API Console.
         */
        public Sender(String key)
        {
            this.key = key;
        }



        public Result sendNoRetry(Message message, String registrationId)
        {
            StringBuilder body = newBody(Constants.PARAM_REGISTRATION_ID, registrationId);
            Boolean? delayWhileIdle = message.isDelayWhileIdle();
            if (delayWhileIdle != null)
            {
                addParameter(body, Constants.PARAM_DELAY_WHILE_IDLE, (bool)delayWhileIdle ? "1" : "0");
            }
            Boolean? dryRun = message.isDryRun();
            if (dryRun != null)
            {
                addParameter(body, Constants.PARAM_DRY_RUN, (bool)dryRun ? "1" : "0");
            }
            String collapseKey = message.getCollapseKey();
            if (collapseKey != null)
            {
                addParameter(body, Constants.PARAM_COLLAPSE_KEY, collapseKey);
            }
            String restrictedPackageName = message.getRestrictedPackageName();
            if (restrictedPackageName != null)
            {
                addParameter(body, Constants.PARAM_RESTRICTED_PACKAGE_NAME, restrictedPackageName);
            }
            int? timeToLive = message.getTimeToLive();
            if (timeToLive != null)
            {
                addParameter(body, Constants.PARAM_TIME_TO_LIVE, timeToLive.ToString());
            }

            foreach (KeyValuePair<string, string> pair in message.getData())
            {
                if (pair.Key != null && pair.Value != null)
                {
                    String prefixedKey = Constants.PARAM_PLAINTEXT_PAYLOAD_PREFIX + pair.Key;
                    addParameter(body, prefixedKey, HttpUtility.UrlEncode(pair.Value, Encoding.UTF8));
                }
            }

            String requestBody = body.ToString();
            HttpWebRequest conn;
            HttpWebResponse response;
            int status;


            conn = post(Constants.GCM_SEND_ENDPOINT, requestBody);
            response = (HttpWebResponse)conn.GetResponse();
            status = (int)response.StatusCode;


            String responseBody;
            if (status != 200)
            {
                responseBody = getAndClose(response.GetResponseStream());
                throw new IOException(status + " " + responseBody);
            }
            else{ responseBody = getAndClose(response.GetResponseStream());}

            char[] delimiterChars = { '\n' };
            String[] lines = responseBody.Split(delimiterChars);
            if (lines.Length == 0 || lines[0].Equals(""))
            {
                throw new IOException("Received empty response from GCM service.");
            }

            String firstLine = lines[0];
            String[] responseParts = split(firstLine);
            String token = responseParts[0];
            String value = responseParts[1];

            if (token.Equals(Constants.TOKEN_MESSAGE_ID))
            {
                Result.Builder builder = new Result.Builder().messageId(value);
                // check for canonical registration id
                if (lines.Length > 1)
                {
                    String secondLine = lines[1];
                    responseParts = split(secondLine);
                    token = responseParts[0];
                    value = responseParts[1];
                    if (token.Equals(Constants.TOKEN_CANONICAL_REG_ID))
                    {
                        builder.canonicalRegistrationId(value);
                    }
                    else
                    {
                        throw new IOException("Invalid response from GCM service.");

                    }
                }
                Result result = builder.build();
                return result;
            }
            else if (token.Equals(Constants.TOKEN_ERROR))
            {
                return new Result.Builder().errorCode(value).build();
            }
            else
            {
                throw new IOException("Invalid response from GCM: " + responseBody);
            }

        }

        private String[] split(String line)
        {
            String[] split = line.Split(new char[] { '=' });
            if (split.Length != 2)
            {
                throw new IOException("Received invalid response line from GCM: " + line);
            }
            return split;


        }

        protected HttpWebRequest post(String url, String body)
        {
            return post(url, "application/x-www-form-urlencoded;charset=UTF-8", body);
        }

        protected HttpWebRequest post(String url, String contentType, String body)
        {
            if (url == null || body == null)
            {
                throw new IOException("arguments cannot be null");
            }

            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] bytes = ascii.GetBytes(body);
            HttpWebRequest conn = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            conn.CachePolicy = noCachePolicy;
            conn.ContentLength = bytes.Length;
            conn.Method = "POST";
            conn.ContentType = contentType;
            conn.Headers[HttpRequestHeader.Authorization] = "key=" + key;
            Stream outt;
            try
            {
                outt = conn.GetRequestStream();
            }
            catch (WebException e)
            {
                throw e;
            }
            try
            {
                outt.Write(bytes, 0, bytes.Length);
            }
            finally
            {
                outt.Flush();
                outt.Close();
            }
            return conn;
        }

        private static String getAndClose(Stream stream)
        {
            try
            {
                using (var s = new MemoryStream())
                {
                    byte[] buffer = new byte[2048]; // read in chunks of 2KB
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        s.Write(buffer, 0, bytesRead);
                    }
                    byte[] r = s.ToArray();
                    string result = System.Text.Encoding.UTF8.GetString(r);
                    return result;
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        /**
  * Creates a {@link StringBuilder} to be used as the body of an HTTP POST.
  *
  * @param name initial parameter for the POST.
  * @param value initial value for that parameter.
  * @return StringBuilder to be used an HTTP POST body.
  */
        protected static StringBuilder newBody(String name, String value)
        {
            return new StringBuilder(name ?? name).Append('=').Append(value ?? value);
        }

        /**
         * Adds a new parameter to the HTTP POST body.
         *
         * @param body HTTP POST body.
         * @param name parameter's name.
         * @param value parameter's value.
         */
        protected static void addParameter(StringBuilder body, String name,
            String value)
        {
            body.Append('&')
                .Append(name ?? name).Append('=').Append(value ?? value);
        }


    }
}