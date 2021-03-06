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

namespace GAPSeguros.Controllers
{
	public class PoliciesController : BaseController
	{
		private readonly IPolicyRepository _policyRepository;
		private readonly IRiskTypeRepository _riskTypeRepository;
		private readonly AbstractValidator<Policy> _abstractValidator;

		public PoliciesController(IPolicyRepository policyRepository, IRiskTypeRepository riskTypeRepository, AbstractValidator<Policy> abstractValidator)
		{
			_policyRepository = policyRepository;
			_riskTypeRepository = riskTypeRepository;
			_abstractValidator = abstractValidator;
		}

		// GET: Policies
		public async Task<IActionResult> Index()
		{
			return View(await _policyRepository.GetAll());
		}

		// GET: Policies/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var policy = await _policyRepository.GetById(id);
			if (policy == null)
			{
				return NotFound();
			}

			return View(policy);
		}

		// GET: Policies/Create
		public async Task<IActionResult> Create()
		{
			var riskTypes = await _riskTypeRepository.GetAll();

			ViewData["RiskTypeId"] = new SelectList(riskTypes, "RiskTypeId", "Name");
			return View();
		}

		// POST: Policies/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("PolicyId,Coverage,Name,Description,ValidityStart,CoverageSpan,Price,RiskTypeId")] Policy policy)
		{
			if (ModelState.IsValid)
			{
				await _policyRepository.Create(policy);

				return RedirectToAction(nameof(Index));
			}

			var riskTypes = await _riskTypeRepository.GetAll();

			ViewData["RiskTypeId"] = new SelectList(riskTypes, "RiskTypeId", "Name", policy.RiskTypeId);
			return View(policy);
		}

		// GET: Policies/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			// We use the repository to get the data
			var policy = await _policyRepository.GetById(id);
			if (policy == null)
			{
				return NotFound();
			}

			var riskTypes = await _riskTypeRepository.GetAll();

			ViewData["RiskTypeId"] = new SelectList(riskTypes, "RiskTypeId", "Name", policy.RiskTypeId);
			return View(policy);
		}

		// POST: Policies/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("PolicyId,Coverage,Name,Description,ValidityStart,CoverageSpan,Price,RiskTypeId")] Policy policy)
		{
			if (id != policy.PolicyId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					await _policyRepository.Update(policy);
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!PolicyExists(policy.PolicyId))
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

			var riskTypes = await _riskTypeRepository.GetAll();

			ViewData["RiskTypeId"] = new SelectList(riskTypes, "RiskTypeId", "Name", policy.RiskTypeId);
			return View(policy);
		}

		// GET: Policies/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var policy = await _policyRepository.GetById(id);

			if (policy == null)
			{
				return NotFound();
			}

			return View(policy);
		}

		// POST: Policies/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var entity = await _policyRepository.GetById(id);

			var result = _abstractValidator.Validate(entity, ruleSet: "delete");


			if (result.IsValid)
			{
				await _policyRepository.DeleteById(id);

				return RedirectToAction(nameof(Index));
			}
			else
			{
				result.AddToModelState(ModelState, null);

				return await Delete(id);
			}
		}

		private bool PolicyExists(int id)
		{
			return _policyRepository.GetById(id) != null;
		}
	}
}
