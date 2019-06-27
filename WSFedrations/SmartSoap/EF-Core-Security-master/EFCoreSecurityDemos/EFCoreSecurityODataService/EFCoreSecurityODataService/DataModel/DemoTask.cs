﻿using System;
using System.Collections.Generic;
using DevExpress.EntityFramework.SecurityDataStore.Security.BaseSecurityEntity;

namespace EFCoreSecurityODataService.DataModel {
    public class DemoTask : BaseSecurityEntity {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DateCompleted { get; set; }
        public int PercentCompleted { get; set; }
        public List<ContactTask> ContactTasks { get; set; }
    }
}