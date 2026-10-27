using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Web.Controllers;

public class AccountController(
    IAuthService authService,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user = await userManager.FindByEmailAsync(model.Email.Trim().ToLowerInvariant());
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Correo electrónico o contraseña incorrectos.");
                return View(model);
            }

            var result = await signInManager.PasswordSignInAsync(user, model.Password, isPersistent: true, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Correo electrónico o contraseña incorrectos.");
                return View(model);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Workshops");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterParticipantDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await authService.RegisterParticipantAsync(model);

            // Iniciar sesión automáticamente
            var user = await userManager.FindByEmailAsync(model.Email.Trim().ToLowerInvariant());
            if (user != null)
            {
                await signInManager.SignInAsync(user, isPersistent: true);
            }

            TempData["Success"] = "¡Cuenta creada exitosamente! Bienvenido a SkillHub.";
            return RedirectToAction("Index", "Workshops");
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al registrarse: {ex.Message}");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Workshops");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
