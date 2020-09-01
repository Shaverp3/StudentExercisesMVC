using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using StudentExercisesMVC.Models;
using StudentExercisesMVC.Models.ViewModels;

namespace StudentExercisesMVC.Controllers
{

    public class StudentsController : Controller
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // GET: StudentsController
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Name
                                        FROM Students s JOIN Cohorts c on s.CohortId = c.id";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Student> students = new List<Student>();
                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort()
                            {
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }
                        };

                        students.Add(student);
                    }

                    reader.Close();

                    return View(students);
                }
            }
        }

        // GET: StudentsController/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Name, e.Name AS Exercise, e.Id AS ExerciseId, e.Language FROM Students s JOIN StudentsJoinExercises se ON se.StudentId = s.Id JOIN Exercises e ON e.id = se.ExerciseId LEFT JOIN Cohorts c ON s.CohortId = c.id
                            WHERE s.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;

                    while (reader.Read())
                    {
                        if (student == null)
                        {
                            student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort()
                                {
                                    Name = reader.GetString(reader.GetOrdinal("Name"))
                                }

                            };
                        }
                            if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                            {
                                Exercise exercise = new Exercise
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("Exercise")),
                                    Language = reader.GetString(reader.GetOrdinal("Language"))
                                };
                                //If exists, Add exercise to the student's AssignedExercises list
                                student.AssignedExercises.Add(exercise);
                            }
                        }

                        reader.Close();

                        // If we got something back to the db, send us to the details view
                        if (student != null)
                        {
                            return View(student);
                        }
                        else
                        {
                            // If we didn't get anything back from the db, we made a custom not found page down here
                            return RedirectToAction(nameof(NotFound));
                        }
                    }
                }
            }


            // GET: StudentsController/Create
            public ActionResult Create()
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        //Since we're creating a student, we don't need to pre-fill the property of type Student on our view model-- that will be filled in by whatever the user enters into the form fields! All we have to do is build our dropdown of cohorts. 

                        // Select all the cohorts
                        cmd.CommandText = @"SELECT Cohorts.Id, Cohorts.Name FROM Cohorts";

                        SqlDataReader reader = cmd.ExecuteReader();

                        // Create a new instance of our view model
                        StudentViewModel viewModel = new StudentViewModel();
                        while (reader.Read())
                        {
                            // Map the raw data to our cohort model
                            Cohort cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            };

                            // Use the info to build our SelectListItem
                            SelectListItem cohortOptionTag = new SelectListItem()
                            {
                                Text = cohort.Name,
                                Value = cohort.Id.ToString()
                            };

                            // Add the select list item to our list of dropdown options
                            viewModel.cohorts.Add(cohortOptionTag);

                        }

                        reader.Close();


                        // send it all to the view
                        return View(viewModel);
                    }
                }
            }


            // POST: StudentsController/Create
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Create(StudentViewModel viewModel)
            {
                try
                {
                    using (SqlConnection conn = Connection)
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"INSERT INTO Students
                ( FirstName, LastName, SlackHandle, CohortId )
                VALUES
                ( @firstName, @lastName, @slackHandle, @cohortId )";
                            cmd.Parameters.Add(new SqlParameter("@firstName", viewModel.student.FirstName));
                            cmd.Parameters.Add(new SqlParameter("@lastName", viewModel.student.LastName));
                            cmd.Parameters.Add(new SqlParameter("@slackHandle", viewModel.student.SlackHandle));
                            cmd.Parameters.Add(new SqlParameter("@cohortId", viewModel.student.CohortId));
                            cmd.ExecuteNonQuery();

                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
                catch
                {
                    return View();
                }
            }

            // GET: StudentsController/Edit/5
            public ActionResult Edit(int id)
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        //Since we're creating a student, we don't need to pre-fill the property of type Student on our view model-- that will be filled in by whatever the user enters into the form fields! All we have to do is build our dropdown of cohorts.

                        cmd.CommandText = @"
                        SELECT
                            Id, FirstName, LastName, SlackHandle, CohortId
                        FROM Students
                        WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader ViewStudentreader = cmd.ExecuteReader();
                        StudentEditViewModel viewModel = new StudentEditViewModel();


                        if (ViewStudentreader.Read())
                        {
                            viewModel.student = new Student
                            {
                                Id = ViewStudentreader.GetInt32(ViewStudentreader.GetOrdinal("Id")),
                                FirstName = ViewStudentreader.GetString(ViewStudentreader.GetOrdinal("FirstName")),
                                LastName = ViewStudentreader.GetString(ViewStudentreader.GetOrdinal("LastName")),
                                SlackHandle = ViewStudentreader.GetString(ViewStudentreader.GetOrdinal("SlackHandle")),
                                CohortId = ViewStudentreader.GetInt32(ViewStudentreader.GetOrdinal("CohortId"))
                            };
                        }

                        ViewStudentreader.Close();


                        // Select all the cohorts
                        cmd.CommandText = @"SELECT Cohorts.Id, Cohorts.Name FROM Cohorts";

                        SqlDataReader reader = cmd.ExecuteReader();
                        // Create a new instance of our view model

                        while (reader.Read())
                        {
                            // Map the raw data to our cohort model
                            Cohort cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            };

                            // Use the info to build our SelectListItem
                            SelectListItem cohortOptionTag = new SelectListItem()
                            {
                                Text = cohort.Name,
                                Value = cohort.Id.ToString()
                            };

                            // Add the select list item to our list of dropdown options
                            viewModel.cohorts.Add(cohortOptionTag);

                        }

                        reader.Close();
                        // If we got something back, send it to the view
                        if (viewModel.student != null)
                        {
                            // send it all to the view
                            return View(viewModel);
                        }
                        else
                        {
                            // If not, send it to our custom not found page
                            return RedirectToAction(nameof(NotFound));
                        }
                    }
                }
            }

            // POST: StudentsController/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Edit(int id, StudentEditViewModel viewModel)
            {
                try
                {
                    using (SqlConnection conn = Connection)
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"UPDATE Students
                                            SET FirstName = @firstName,
                                                LastName = @lastName,
                                                SlackHandle = @slackHandle,
                                                CohortId = @cohortId
                                            WHERE Id = @id";
                            cmd.Parameters.Add(new SqlParameter("@firstName", viewModel.student.FirstName));
                            cmd.Parameters.Add(new SqlParameter("@lastName", viewModel.student.LastName));
                            cmd.Parameters.Add(new SqlParameter("@slackHandle", viewModel.student.SlackHandle));
                            cmd.Parameters.Add(new SqlParameter("@cohortId", viewModel.student.CohortId));
                            cmd.Parameters.Add(new SqlParameter("@id", id));

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                return RedirectToAction(nameof(Index));
                            }
                            throw new Exception("No rows affected");
                        }
                    }

                }
                catch (Exception)
                {
                    return View(viewModel);
                }
            }

            // GET: StudentsController/Delete/5
            public ActionResult Delete(int id)
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                        SELECT
                            Id, FirstName, LastName, SlackHandle, CohortId
                        FROM Students
                        WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();

                        Student student = null;

                        if (reader.Read())
                        {
                            student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                            };
                        }
                        reader.Close();

                        if (student != null)
                        {
                            return View(student);
                        }
                        else
                        {
                            return RedirectToAction(nameof(NotFound));
                        }
                    }
                }
            }

            // POST: StudentsController/Delete/5
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public ActionResult DeleteConfirmed(int id)
            {
                try

                {
                    using (SqlConnection conn = Connection)
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {

                            cmd.CommandText = @"DELETE FROM StudentsJoinExercises WHERE
                                StudentId = @id
                                
                                DELETE FROM Students WHERE Id = @id";

                            cmd.Parameters.Add(new SqlParameter("@id", id));

                            int rowsAffected = cmd.ExecuteNonQuery();
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }

        // This is a method we made to handle 404's. This will show us the NotFound view in our students folder.
        public new ActionResult NotFound()
        {
            return View();
        }

        public ActionResult Taco()
        {
            return View();
        }


    }
}



