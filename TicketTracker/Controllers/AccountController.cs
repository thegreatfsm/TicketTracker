using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace TicketTracker.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUser> userManager;
        private SignInManager<AppUser> signInManager;

        public AccountController(UserManager<AppUser> usr, SignInManager<AppUser> signin)
        {
            userManager = usr;
            signInManager = signin;
        }
        public IActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if(user != null)
                {
                    await signInManager.SignOutAsync();
                    var result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);
                    if (result.Succeeded)
                    {
                        return Redirect(returnUrl ?? "/");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Incorrect password");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email is incorrect");
                    return View();
                }
            }
            else
            {
                return View();
            }
        }
        public IActionResult Create() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Password.Equals(model.ConfirmPassword))
                {
                    var user = new AppUser
                    {
                        UserName = model.Username,
                        Email = model.Email
                    };
                    var result = await userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Login));
                    }
                    else
                    {
                        foreach(var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Passwords must match");
                    return View();
                }
            }
            else
            {
                return View();
            }
        }
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            if(user != null)
            {
                return View(user);
            }
            else
            {
                ModelState.AddModelError("", "User was not found");
                return RedirectToAction("Index", nameof(HomeController));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(string id, string email, string currentpassword)
        {
            var user = await userManager.FindByIdAsync(id);
            if(user != null)
            {
                user.Email = email;
                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return View(nameof(Edit), user);
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(nameof(Edit), user);
                }
            }
            else
            {
                ModelState.AddModelError("", "User not found");
                return View("Index", nameof(HomeController));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string id, string currentpassword, string newpassword, string confirmpassword)
        {
            var user = await userManager.FindByIdAsync(id);
            if(user != null)
            {
                if (newpassword.Equals(confirmpassword))
                {
                    var passwordUpdate = await userManager.ChangePasswordAsync(user, currentpassword, newpassword);
                    if (passwordUpdate.Succeeded)
                    {
                        return View(nameof(Edit), user);
                    }
                    else
                    {
                        foreach(var error in passwordUpdate.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(nameof(Edit), user);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "New passwords must match");
                    return View(nameof(Edit), user);
                }
            }
            else
            {
                ModelState.AddModelError("", "User not found");
                return View("Index", nameof(HomeController));
            }
        }
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
    }
}
