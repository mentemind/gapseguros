﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataAccess.Database;
using DataAccess.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace GAPSeguros.Controllers
{
	public class AccountController : BaseController
	{
		private readonly IUserRepository _userRepository;
		private readonly AbstractValidator<User> _abstractValidator;

		public AccountController(IUserRepository userRepository, AbstractValidator<User> abstractValidator)
		{
			_userRepository = userRepository;
			_abstractValidator = abstractValidator;
		}

		[AllowAnonymous]
		public IActionResult AccessDenied()
		{
			return View();
		}

		[AllowAnonymous]
		public IActionResult Login(string returnUrl)
		{
			return RedirectToAction(nameof(AccessDenied), new { returnUrl });
		}

		// POST: Account/AccessDenied
		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> AccessDenied([Bind("Name,Password")] User model, string returnUrl = "/")
		{
			var user = await _userRepository.ValidateUserNameAndPassword(model.Name, model.Password);

			if (user != null)
			{
				// Since the login was successful, we autenticate the user
				var claims = new List<Claim> {
					new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
				};

				// We add the roles of the user as claims
				foreach (var role in user.RoleByUser)
				{
					claims.Add(new Claim(ClaimTypes.Role, role.RoleId.ToString()));
				}

				var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

				await HttpContext.SignInAsync(
				   CookieAuthenticationDefaults.AuthenticationScheme,
				   new ClaimsPrincipal(claimsIdentity),
				   new AuthenticationProperties());


				return Redirect(returnUrl);
			}

			ModelState.AddModelError("", "User name or/and password is/are invalid");

			return View();
		}


		// GET: Users
		public async Task<IActionResult> Index()
		{
			return View(await _userRepository.GetAll());
		}

		// GET: Users/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var user = await _userRepository.GetById(id);
			if (user == null)
			{
				return NotFound();
			}

			return View(user);
		}

		// GET: Users/Create
		public IActionResult Create() => View();


		// POST: Users/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Name,Password")] User user)
		{
			if (ModelState.IsValid)
			{
				await _userRepository.Create(user);

				return RedirectToAction(nameof(Index));
			}
			return View(user);
		}

		// GET: Users/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var user = await _userRepository.GetById(id);
			if (user == null)
			{
				return NotFound();
			}
			return View(user);
		}

		//POST: Users/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("UserId,Name,Password")] User user)
		{
			if (id != user.UserId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					await _userRepository.Update(user);
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!UserExists(user.UserId))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			return View(user);
		}

		// GET: Users/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var user = await _userRepository.GetById(id);
			if (user == null)
			{
				return NotFound();
			}

			return View(user);
		}

		// POST: Users/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var entity = await _userRepository.GetById(id);

			var result = _abstractValidator.Validate(entity, ruleSet: "delete");


			if (result.IsValid)
			{
				await _userRepository.DeleteById(id);

				return RedirectToAction(nameof(Index));
			}
			else
			{
				result.AddToModelState(ModelState, null);

				return await Delete(id);
			}
		}

		[HttpGet]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync();
			return Redirect("/");
		}

		private bool UserExists(int id)
		{
			return _userRepository.GetById(id) != null;
		}
	}
}
