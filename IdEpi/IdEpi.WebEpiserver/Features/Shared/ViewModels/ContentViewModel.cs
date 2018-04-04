using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using IdEpi.WebEpiserver.Features.Pages.Start;

namespace IdEpi.WebEpiserver.Features.Shared.ViewModels
{
    public class ContentViewModel<T> : IContentViewModel<T> where T : IContent
    {
        private Injected<IContentLoader> _contentLoader = default(Injected<IContentLoader>);
        private StartPage _startPage;

        public ContentViewModel()
        {

        }

        public ContentViewModel(T currentContent)
        {
            CurrentContent = currentContent;
        }

        public T CurrentContent { get; set; }

        public virtual StartPage StartPage => _startPage ?? (_startPage = _contentLoader.Service.Get<StartPage>(ContentReference.StartPage));

        public IContent Section { get; set; }
    }

    public static class ContentViewModel
    {
        /// <summary>
        /// Returns a PageViewModel of type <typeparam name="T"/>.
        /// </summary>
        /// <remarks>
        /// Convenience method for creating PageViewModels without having to specify the type as methods can use type inference while constructors cannot.
        /// </remarks>
        public static ContentViewModel<T> Create<T>(T page) where T : IContent
        {
            return new ContentViewModel<T>(page);
        }
    }

}