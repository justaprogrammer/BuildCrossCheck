using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;

namespace BCC.Web.Extensions
{
    public static class MultipartRequestHelperExtensions
    {
        public static string GetBoundary(this HttpRequest request, FormOptions formOptions)
        {
            var lengthLimit = formOptions.MultipartBoundaryLengthLimit;
            var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(request.ContentType).Boundary).Value;
            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary;
        }

        public static bool IsMultipartContentType(this HttpRequest request)
        {
            string contentType = request.ContentType;
            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}