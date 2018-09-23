using System;
using Microsoft.Net.Http.Headers;

namespace BCC.Web.Extensions
{
    public static class ContentDispositionHeaderValueExtensions
    {
        public static bool IsFormDataContentDisposition(this ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="key";
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && String.IsNullOrEmpty(contentDisposition.FileName.Value)
                   && String.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        public static bool IsFileContentDisposition(this ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && (!String.IsNullOrEmpty(contentDisposition.FileName.Value)
                       || !String.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }
    }
}