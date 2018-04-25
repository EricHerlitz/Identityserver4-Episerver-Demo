using System.Collections.Generic;
using System.Security.Claims;
using IdEpi.WebEpiserver.Features.Shared.ViewModels;

namespace IdEpi.WebEpiserver.Features.Pages.Start
{
    public class StartPageViewModel : ContentViewModel<StartPage>
    {
        public string TestProp { get; set; }
        public List<Claim> Claims { get; set; }

        public StartPageViewModel(StartPage startPage)
        {
            CurrentContent = startPage;
        }

    }
}