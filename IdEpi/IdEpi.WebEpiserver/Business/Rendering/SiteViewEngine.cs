using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;

namespace IdEpi.WebEpiserver.Business.Rendering
{
    public class SiteViewEngine : RazorViewEngine
    {
        private readonly ConcurrentDictionary<string, bool> _cache = new ConcurrentDictionary<string, bool>();

        private static readonly string[] AdditionalPartialViewFormats =
        {
            "~/Views/Blocks/{0}.cshtml",
            "~/Views/Shared/PagePartials/{0}.cshtml"
        };

        public SiteViewEngine()
        {
            ViewLocationCache = new DefaultViewLocationCache();

            // {0} = action
            // {1} = controller

            var featureFolders = new[]
                {
                    "~/Features/Pages/{1}/{1}{0}.cshtml",
                    "~/Views/Pages/{1}/{1}{0}.cshtml", // ~/Views/Pages/Start/StartIndex.cshtml
                    "~/Features/{1}{0}.cshtml",
                    "~/Features/{1}/{0}.cshtml",
                    "~/Features/{1}/Views/{0}.cshtml",
                    "~/Features/{1}/Views/{1}.cshtml"
                }
                .Union(SubFeatureFolders("~/Features"))
                .ToArray();

            featureFolders = featureFolders.Union(AdditionalPartialViewFormats).
                ToArray();

            ViewLocationFormats = ViewLocationFormats
                .Union(featureFolders)
                .ToArray();

            PartialViewLocationFormats = PartialViewLocationFormats
                .Union(featureFolders)
                .ToArray();
        }

        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            if (controllerContext.HttpContext != null && !controllerContext.HttpContext.IsDebuggingEnabled)
            {
                return _cache.GetOrAdd(virtualPath, p => HostingEnvironment.VirtualPathProvider.FileExists(virtualPath));
            }
            return HostingEnvironment.VirtualPathProvider.FileExists(virtualPath);
        }

        private IEnumerable<string> SubFeatureFolders(string rootFolder)
        {
            var rootPath = HostingEnvironment.MapPath(rootFolder);
            if (rootPath == null)
                return Enumerable.Empty<string>();

            var featureFolders = Directory.GetDirectories(rootPath)
                .Select(GetDirectory);

            var features = featureFolders.Select(a => new
            {
                a.Name,
                Features = Directory.GetDirectories(a.FullName)
                    .Select(GetDirectoryName)
            });

            return features.SelectMany(feature =>
            {
                return new[]
                    {
                        $"{rootFolder}/{feature.Name}/{{0}}.cshtml",
                        $"{rootFolder}/{feature.Name}/{{1}}{{0}}.cshtml",
                        $"{rootFolder}/{feature.Name}/Views/{{1}}{{0}}.cshtml",
                        $"{rootFolder}/{feature.Name}/Views/{{1}}/{{0}}.cshtml"
                    }
                    .Union(
                        feature.Features
                            .SelectMany(subFfeature => new[]
                            {
                                $"{rootFolder}/{feature.Name}/{subFfeature}/{{0}}.cshtml",
                                $"{rootFolder}/{feature.Name}/{subFfeature}/{{1}}{{0}}.cshtml",
                                $"{rootFolder}/{feature.Name}/{subFfeature}/Views/{{1}}/{{0}}.cshtml",
                                $"{rootFolder}/{feature.Name}/{subFfeature}/Views/{{1}}{{0}}.cshtml"
                            }));
            });
        }

        private string GetDirectoryName(string path)
        {
            return GetDirectory(path).Name;
        }

        private DirectoryInfo GetDirectory(string path)
        {
            return new DirectoryInfo(path);
        }
    }

}