using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCManukauTech.Models.MemberViewModel
{
    public class MemberViewModels
    {
        public IEnumerable<MVCManukauTech.Models.ApplicationUser> UserList { get; set; }
        public IEnumerable<Microsoft.AspNet.Identity.EntityFramework.IdentityRole> RoleList { get; set; }

    }
}