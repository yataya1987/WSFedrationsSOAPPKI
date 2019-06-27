﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using DevExpress.EntityFramework.SecurityDataStore.Security.BaseSecurityEntity;

namespace EFCoreSecurityODataService.DataModel {
    public class ContactTask : BaseSecurityEntity {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public Contact Contact { get; set; }
        public int TaskId { get; set; }
        public DemoTask Task { get; set; }
    }
}