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
        private readonly Guid permissionType1Id = Guid.Parse("FBF351D6-CA91-4D89-8601-2B6D71CA27BD");
        private readonly Guid permissionType2Id = Guid.Parse("4047B2FA-BDF1-437C-BFBF-29E77E71F9FC");

        public PermissionAuthorizationMatcherTests()
        {
            sut = new PermissionAuthorizationMatcher();
        }

        [Fact]
        public void AuthorizePermission_Passes()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, null),
                new Permission(permissionType2Id, null, null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_Fails()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType2Id, null, null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeFalse();
        }

        [Fact]
        public void AuthorizePermission_MatchingContextPasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, "abc")
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, "abc")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_NullContextPasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, "abc")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_NonMatchingContextFails()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, "def"),
                new Permission(permissionType2Id, null, "abc")
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, "abc")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeFalse();
        }

        [Fact]
        public void AuthorizePermission_NonMatchingNullContextPasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, "abc"),
                new Permission(permissionType2Id, null, null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_MatchingResourcePasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "abc", null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "abc", null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_NullResourcePasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "abc", null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_NonMatchingResourceFails()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "def", null),
                new Permission(permissionType2Id, "abc", null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "abc", null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeFalse();
        }

        [Fact]
        public void AuthorizePermission_NonMatchingNullResourceFails()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "abc", null),
                new Permission(permissionType2Id, null, null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, null)
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_MatchingContextAndResourcePasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "abc", "def")
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "abc", "def")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }


        [Fact]
        public void AuthorizePermission_ContextAndNullResourcePasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, null, "abc")
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "def", "abc")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_ResourceAndNullContextPasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "abc", null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "abc", "def")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthorizePermission_NullContextAndResourcePasses()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "def", null)
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "def", "abc")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeTrue();
        }
        
        [Fact]
        public void AuthorizePermission_MatchingContextNonAndMatchingResourceFails()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "abc", "def"),
                new Permission(permissionType2Id, "abc", "def")
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "ghi", "def")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeFalse();
        }

        [Fact]
        public void AuthorizePermission_NonMatchingContextAndMatchingResourceFails()
        {
            var availablePermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "abc", "ghi"),
                new Permission(permissionType2Id, "abc", "def")
            };

            var requiredPermissions = new List<Permission>()
            {
                new Permission(permissionType1Id, "abc", "def")
            };

            var result = sut.CheckAuthorization(availablePermissions, requiredPermissions);
            result.Should().BeFalse();
        }
    }
}