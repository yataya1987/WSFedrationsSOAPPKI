﻿using System;

namespace Foundatio.Hosting.Startup {
    public class StartupPriorityAttribute : Attribute {
        public StartupPriorityAttribute(int priority) {
            Priority = priority;
        }

        public int Priority { get; private set; }
    }
}