using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models
{
    public class Student
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [StringLength(12, MinimumLength = 3)]
        public string SlackHandle { get; set; }
        //public int Grade { get; set; }
        public int CohortId { get; set; }

        [Display(Name = "Cohort Name")]
        public Cohort Cohort { get; set; }

        public List<Exercise> AssignedExercises { get; set; } = new List<Exercise>();

    }
}
