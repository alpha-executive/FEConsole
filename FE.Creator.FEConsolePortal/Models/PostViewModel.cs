using Microsoft.AspNetCore.Http;
using System;

namespace FE.Creator.FEConsolePortal.Models
{
    public class PostViewModel
    {
        private readonly HttpContext _context = null;
        public PostViewModel(HttpContext context)
        {
            this._context = context;
        }
        private string OriginalHost
        {
            get
            {
                var currentUri = this._context.Request.Host;
                string hostAndPort = string.Format("{0}://{0}{1}", 
                        this._context.Request.Scheme,
                        currentUri.Host,
                        (currentUri.Port == 80 || currentUri.Port == 443) 
                                    ? string.Empty: ":" + currentUri.Port);

                return hostAndPort;
            }
        }
        public int ObjectId
        {
            get; set;
        }
        public string PostTitle
        {
            get;set;
        }
        
        public string PostDesc
        {
            get;set;
        }

        public string PostContent
        {
            get;set;
        }

        public string ContentUrl
        {
            get
            {
                return string.Format("{0}/viewarticlecontent/{1}",
                    this.OriginalHost,
                    this.ObjectId);
            }
        }


        public string ImageUrl
        {
            get
            {
                if(this.ObjectId != 0)
                {
                   
                    return string.Format("{0}/home/DownloadArticleImage/{1}?thumbinal=true",
                       this.OriginalHost,
                       this.ObjectId);
                }

                return string.Empty;
            }
        }

        public string Author
        {
            get;set;
        }

        public bool IsOriginal
        {
            get;set;
        }

        public DateTime Updated
        {
            get;set;
        }
    }
}