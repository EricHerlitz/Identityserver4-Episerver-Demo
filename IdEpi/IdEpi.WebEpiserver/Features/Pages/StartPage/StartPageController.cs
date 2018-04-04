using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;

namespace IdEpi.WebEpiserver.Features.Pages.Start
{
    public class StartPageController : PageController<StartPage>
    {
        private readonly IContentLoader _contentLoader;

        public StartPageController(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
        }

        //[Authorize]
        public ActionResult Index(StartPage currentPage)
        {
            var viewModel = new StartPageViewModel(currentPage)
            {
                TestProp = _contentLoader.Get<PageData>(ContentReference.RootPage).Name
            };

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Secure(StartPage currentPage)
        {
            var a = User.Identity.IsAuthenticated;

            var viewModel = new StartPageViewModel(currentPage)
            {
                TestProp = a.ToString()
            };

            return View("~/Views/Pages/StartPage/StartPageIndex.cshtml", viewModel);
        }

    }
}