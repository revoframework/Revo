using System;
using System.Collections.Generic;
using FluentAssertions;
using Revo.Core.Security;
using Xunit;

namespace Revo.Core.Tests.Security
{
    public class PermissionAuthorizationMatcherTests
    {
        private PermissionAuthorizationMatcher sut;
        private PermissionType permissionType1 = new PermissionType(Guid.Parse("FBF351D6-CA91-4D89-8601-2B6D71CA27BD"), "Permission1");
        private PermissionType permissionType2 = new PermissionType(Guid.Parse("FBF351D6-CA91-4D89-8601-2B6D71CA27BD"), "Permission2");

        public PermissionAuthorizationMatcherTests()
        {
            sut = new PermissionAuthorizationMatcher();
        }

        [Fact]
        public void AuthorizePermission_Passes()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, null),
                new Permission(permissionType2, null, null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_Fails()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType2, null, null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeFalse();
        }

        [Fact]
        public void AuthorizePermission_MatchingContextPasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, "abc", null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, "abc", null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_NullContextPasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, "abc", null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_NonMatchingContextFails()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, "def", null),
                new Permission(permissionType2, "abc", null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, "abc", null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeFalse();
        }

        [Fact]
        public void AuthorizePermission_NonMatchingNullContextFails()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, "abc", null),
                new Permission(permissionType2, null, null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeFalse();
        }

        [Fact]
        public void AuthorizePermission_MatchingResourcePasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, "abc")
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, "abc")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_NullResourcePasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, "abc")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_NonMatchingResourceFails()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, "def"),
                new Permission(permissionType2, null, "abc")
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, "abc")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeFalse();
        }

        [Fact]
        public void AuthorizePermission_NonMatchingNullResourceFails()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, "abc"),
                new Permission(permissionType2, null, null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeFalse();
        }


        [Fact]
        public void AuthorizePermission_MatchingContextAndResourcePasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, "abc", "def")
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, "abc", "def")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_ContextAndNullResourcePasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, "abc", null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, "abc", "def")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }


        [Fact]
        public void AuthorizePermission_NullContextAndResourcePasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, null, "def")
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, "abc", "def")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_MatchingContextNonAndMatchingResourceFails()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, "abc", "ghi"),
                new Permission(permissionType2, "abc", "def")
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, "abc", "def")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeFalse();
        }
        
        [Fact]
        public void AuthorizePermission_NonMatchingContextAndMatchingResourceFails()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1, "abc", "def"),
                new Permission(permissionType2, "abc", "def")
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1, "ghi", "def")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeFalse();
        }
    }
}