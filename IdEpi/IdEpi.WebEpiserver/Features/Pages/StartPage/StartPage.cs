using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;

namespace IdEpi.WebEpiserver.Features.Pages.Start
{
    [ContentType(DisplayName = "StartPage", GUID = "bd957abd-cbe1-43bb-89f6-95ea37c1be00", Description = "")]
    public class StartPage : PageData
    {
        [CultureSpecific]
        [Display(
            Name = "Start title",
            Description = "Title for the start page",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual string Title { get; set; }

    }
}