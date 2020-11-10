using Microsoft.EntityFrameworkCore;
using Emporos.Core.Data;
using Emporos.Core.Entities; 
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Emporos.Data.Context
{
    public class EmporosContext : DbContext
    {
        private readonly string _cUser;
        public EmporosContext(DbContextOptions<EmporosContext> options,UserHelper usrRepo):base(options)
        {
            _cUser = usrRepo.GetUserId();
        }
         
        public virtual DbSet<object> Captures { get; set; } 


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            this.ChangeTracker.DetectChanges();
            var added = this.ChangeTracker.Entries()
                .Where(t => t.State == EntityState.Added)
                .Select(t => t.Entity)
                .ToArray();
            foreach (var entity in added)
            {
                if (!(entity is AuditableEntity track)) continue;
                track.CreatedOn = DateTimeOffset.UtcNow ;
                track.UpdatedOn = DateTimeOffset.UtcNow;
                track.CreatedBy = _cUser;
                track.UpdatedBy = _cUser;
                track.IsDeleted = false;
            }
            var modified = this.ChangeTracker.Entries()
                .Where(t => t.State == EntityState.Modified)
                .Select(t => t.Entity)
                .ToArray();
            foreach (var entity in modified)
            {
                if (!(entity is AuditableEntity track)) continue;
                track.UpdatedOn = DateTimeOffset.UtcNow;
                track.UpdatedBy = _cUser;
            }
            var deleted = this.ChangeTracker.Entries()
                .Where(t => t.State == EntityState.Deleted)
                .Select(t => t.Entity)
                .ToArray();
            foreach (var entity in deleted)
            {
                if (!(entity is AuditableEntity track)) continue;
                track.DeletedOn = DateTimeOffset.UtcNow;
                track.UpdatedOn = DateTimeOffset.UtcNow;
                track.DeletedBy = _cUser;
                track.UpdatedBy = _cUser;
                track.IsDeleted = true;
            }
            return base.SaveChangesAsync(cancellationToken);
        } 
    }
}