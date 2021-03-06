﻿using System.Collections.Generic;

namespace eFormAPI.Common.Models.Cases.Request
{
    public class CaseEditRequest
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public List<CaseEditRequestField> Fields { get; set; }
        public List<CaseEditRequestGroupField> GroupFields { get; set; }
        public List<CaseEditRequest> ElementList { get; set; }
    }
}