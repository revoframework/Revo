﻿using System.Linq;

namespace Revo.Core.Core
{
    public class Environment(IEnvironmentProvider[] environmentProviders) : IEnvironment
    {
        private readonly IEnvironmentProvider[] environmentProviders = environmentProviders;

        public bool IsDevelopment =>
            IsDevelopmentOverride
                    ?? environmentProviders.Any(x => x.IsDevelopment == true);

        public bool? IsDevelopmentOverride { get; set; }
    }
}