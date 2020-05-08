using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace TicketTracker.Models
{
    public class EFGroupRepository : IGroupRepository
    {
        private AppIdentityDbContext context;
        public EFGroupRepository(AppIdentityDbContext ctx) => context = ctx;
        public IQueryable<AssignmentGroup> AssignmentGroups => context.AssignmentGroups;
        public IQueryable<UserGroup> UserGroups => context.UserGroups.Include(ug => ug.Group);
        public void SaveAssignmentGroup(AssignmentGroup group)
        {
            if(group.Id == 0)
            {
                context.AssignmentGroups.Add(group);
            }
            else
            {
                var dbentry = context.AssignmentGroups.FirstOrDefault(g => g.Id == group.Id);
                if(dbentry != null)
                {
                    dbentry.Name = group.Name;
                    
                }
            }
            context.SaveChanges();
        }
        public AssignmentGroup DeleteAssignmentGroup(int id)
        {
            var dbentry = context.AssignmentGroups.FirstOrDefault(g => g.Id == id);
            if(dbentry != null)
            {
                context.AssignmentGroups.Remove(dbentry);
                context.SaveChanges();
            }
            return dbentry;
        }
        public ICollection<UserGroup> AssociatedGroups(AppUser user)
        {
            var all = UserGroups;
            return UserGroups.Where(ug => ug.UserId == user.Id).ToList();
        }
        public void AddUser(AppUser user, AssignmentGroup group)
        {
            var userGroup = new UserGroup
            {
                UserId = user.Id,
                User = user,
                GroupId = group.Id,
                Group = group
            };
            context.UserGroups.Add(userGroup);
            context.SaveChanges();
        }
        public void RemoveUser(AppUser user, AssignmentGroup group)
        {
            var entry = context.UserGroups.FirstOrDefault(ug => ug.GroupId == group.Id && ug.UserId == user.Id);
            context.Remove(entry);
            context.SaveChanges();
        }
    }
}
