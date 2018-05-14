using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace IdEpi.WebEpiserver
{
    public class Warmup : IProcessHostPreloadClient
    {
        public void Preload(string[] parameters)
        {
        }
    }
}