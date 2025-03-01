using AutoMapper;
using ExpenseTracker.Common.Models;
using ExpenseTracker.Infrastructure.Identity.Models;

namespace ExpenseTracker.Infrastructure.Identity.Mapping
{
    public class IdentityProfile : Profile
    {
        public IdentityProfile()
        {
            CreateMap<AppUser, ApplicationUserDto>();
            CreateMap<AppRole, ApplicationRoleDto>();
        }
    }
}
