﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models.ViewModels
{
    public class StudentViewModel
    {
        public Student student { get; set; }

        // This will be our dropdown
        public List<SelectListItem> cohorts { get; set; } = new List<SelectListItem>();
    }
}
