using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketTracker.Models
{
    public interface IGroupRepository
    {
        IQueryable<AssignmentGroup> AssignmentGroups { get; }
        IQueryable<UserGroup> UserGroups { get; }
        void SaveAssignmentGroup(AssignmentGroup group);
        AssignmentGroup DeleteAssignmentGroup(int id);
        ICollection<UserGroup> AssociatedGroups(AppUser user);
        void AddUser(AppUser user, AssignmentGroup group);
        void RemoveUser(AppUser user, AssignmentGroup group);
    }
}
