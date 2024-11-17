﻿namespace Chirp.Web.models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Services;
using Chirp.Core.DTO;
using System.Threading.Tasks;
#pragma warning disable CS8604 // Possible null reference argument.

public class PublicTimeLine(ICheepService cheepService) : TimeLine(cheepService)
{

    public async Task<ActionResult> OnGetAsync()
    {    
        int page = UpdatePage();

        await _cheepService.FindAuthorByName("Helge");
        await _cheepService.Follow(11, 11);
        await _cheepService.GetFollowers("Helge");
        await _cheepService.Unfollow(11, 11);
        await _cheepService.GetFollowers("Helge");


    
        Cheeps = await _cheepService.ReadPublicMessages(page);
        Count = await _cheepService.CountPublicMessages();
        if(!string.IsNullOrEmpty(SearchName))
            Authors = await _cheepService.FindAuthorByName(SearchName);
            
        UpdatePage(page);
        
        return Page();
    }
    
}
