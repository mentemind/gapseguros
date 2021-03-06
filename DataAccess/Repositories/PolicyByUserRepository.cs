﻿using DataAccess.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class PolicyByUserRepository : IPolicyByUserRepository
	{
		private readonly GAPSegurosContext _context;

		public PolicyByUserRepository(GAPSegurosContext context)
		{
			_context = context;
		}

		public async Task<PolicyByUser> Create(PolicyByUser model)
		{
			_context.Add(model);
			await _context.SaveChangesAsync();

			return model;
		}

		public async Task DeleteById(int id)
		{
			var policy = await _context.PolicyByUser.SingleOrDefaultAsync(m => m.PolicyByUserId == id);
			_context.PolicyByUser.Remove(policy);
			await _context.SaveChangesAsync();
		}

		public Task<IQueryable<PolicyByUser>> GetAll()
		{
			var result = _context.PolicyByUser
				.Include(p => p.Policy)
				.Include(p => p.User)
				.AsQueryable();

			return Task.FromResult(result);
		}

		public Task<PolicyByUser> GetById(int? id)
		{
			return _context.PolicyByUser
				.Include(p => p.Policy)
				.Include(p => p.User)
				.SingleOrDefaultAsync(m => m.PolicyByUserId == id);
		}

		public IQueryable<PolicyByUser> GetPolicyAssignations(int policyId)
		{
			return _context.PolicyByUser.Where(x => x.PolicyId == policyId);
		}

		public IQueryable<PolicyByUser> GetUserPolicies(int userId)
		{
			return _context.PolicyByUser.Where(x => x.UserId == userId);
		}

		public async Task Update(PolicyByUser model)
		{
			_context.Update(model);
			await _context.SaveChangesAsync();
		}
	}
}
