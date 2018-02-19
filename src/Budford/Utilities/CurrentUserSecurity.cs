using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Budford.Utilities
{
    public class CurrentUserSecurity
    {
        readonly WindowsIdentity currentUser;
        readonly WindowsPrincipal currentPrincipal;

        /// <summary>
        /// 
        /// </summary>
        public CurrentUserSecurity()
        {
            currentUser = WindowsIdentity.GetCurrent();
            currentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public bool HasAccess(DirectoryInfo directory, FileSystemRights right)
        {
            // Get the collection of authorization rules that apply to the directory.if
            if (Directory.Exists(directory.FullName))
            {
                AuthorizationRuleCollection acl = directory.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
                return HasFileOrDirectoryAccess(right, acl);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public bool HasAccess(FileInfo file, FileSystemRights right)
        {
            // Get the collection of authorization rules that apply to the file.
            if (File.Exists(file.FullName))
            {
                AuthorizationRuleCollection acl = file.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
                return HasFileOrDirectoryAccess(right, acl);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="acl"></param>
        /// <returns></returns>
        private bool HasFileOrDirectoryAccess(FileSystemRights right, AuthorizationRuleCollection acl)
        {
            bool allow = false;
            bool deny = false;
            bool inheritedAllow = false;
            bool inheritedDeny = false;

            for (int i = 0; i < acl.Count; i++)
            {
                CheckRule(right, acl, ref allow, ref deny, ref inheritedAllow, ref inheritedDeny, i);
            }

            return CheckRuleResults(allow, deny, inheritedAllow, inheritedDeny);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="acl"></param>
        /// <param name="allow"></param>
        /// <param name="deny"></param>
        /// <param name="inheritedAllow"></param>
        /// <param name="inheritedDeny"></param>
        /// <param name="i"></param>
        private void CheckRule(FileSystemRights right, AuthorizationRuleCollection acl, ref bool allow, ref bool deny, ref bool inheritedAllow, ref bool inheritedDeny, int i)
        {
            var currentRule = (FileSystemAccessRule)acl[i];
            // If the current rule applies to the current user.
            if (RuleAppliesToCurrentUser(currentRule))
            {
                if (currentRule != null && currentRule.AccessControlType.Equals(AccessControlType.Deny))
                {
                    if (RightIsRight(right, currentRule))
                    {
                        if (currentRule.IsInherited)
                        {
                            inheritedDeny = true;
                        }
                        else
                        { // Non inherited "deny" takes overall precedence.
                            deny = true;
                        }
                    }
                }
                else if (currentRule != null && currentRule.AccessControlType.Equals(AccessControlType.Allow))
                {
                    Allow(right, ref allow, ref inheritedAllow, currentRule);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="currentRule"></param>
        /// <returns></returns>
        private static bool RightIsRight(FileSystemRights right, FileSystemAccessRule currentRule)
        {
            return (currentRule.FileSystemRights & right) == right;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentRule"></param>
        /// <returns></returns>
        private bool RuleAppliesToCurrentUser(FileSystemAccessRule currentRule)
        {
            return currentUser.User != null && (currentUser.User.Equals(currentRule.IdentityReference) || currentPrincipal.IsInRole((SecurityIdentifier)currentRule.IdentityReference));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="allow"></param>
        /// <param name="deny"></param>
        /// <param name="inheritedAllow"></param>
        /// <param name="inheritedDeny"></param>
        /// <returns></returns>
        bool CheckRuleResults(bool allow, bool deny, bool inheritedAllow, bool inheritedDeny)
        {
            if (deny)
            {
                return false;
            }

            if (allow)
            { // Non inherited "allow" takes precedence over inherited rules.
                return true;
            }
            return inheritedAllow && !inheritedDeny;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="allow"></param>
        /// <param name="inheritedAllow"></param>
        /// <param name="currentRule"></param>
        private static void Allow(FileSystemRights right, ref bool allow, ref bool inheritedAllow, FileSystemAccessRule currentRule)
        {
            if ((currentRule.FileSystemRights & right) == right)
            {
                if (currentRule.IsInherited)
                {
                    inheritedAllow = true;
                }
                else
                {
                    allow = true;
                }
            }
        }
    }
}
