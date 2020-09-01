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
    public class InstructorsController : Controller
    {
        private readonly IConfiguration _config;

        public InstructorsController(IConfiguration config)
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
        // GET: InstructorsController
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id, i.FirstName, i.LastName, i.SlackHandle, i.CohortId, i.Specialty
                                        FROM Instructors i";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Instructor> instructors = new List<Instructor>();
                    while (reader.Read())
                    {
                        Instructor instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Specialty = reader.GetString(reader.GetOrdinal("Specialty"))
                        };

                        instructors.Add(instructor);
                    }

                    reader.Close();

                    return View(instructors);
                }
            }
        }


        // GET: InstructorsController/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            i.Id, i.FirstName, i.LastName, i.SlackHandle, i.CohortId, i.Specialty, c.Name
                        FROM Instructors i JOIN Cohorts c ON i.CohortId = c.id
                        WHERE i.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor instructor = null;

                    if (reader.Read())
                    {
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                            Cohort = new Cohort()
                            { 
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            
                            }

                        };
                    }
                    reader.Close();


                    // If we got something back to the db, send us to the details view
                    if (instructor != null)
                    {
                        return View(instructor);
                    }
                    else
                    {
                        // If we didn't get anything back from the db, we made a custom not found page down here
                        return RedirectToAction(nameof(NotFound));
                    }
                }
            }
        }


        // GET: InstructorsController/Create
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
                    InstructorViewModel viewModel = new InstructorViewModel();
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


        // POST: InstructorsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(InstructorViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Instructors
                ( FirstName, LastName, SlackHandle, CohortId, Specialty )
                VALUES
                ( @firstName, @lastName, @slackHandle, @cohortId, @specialty )";
                        cmd.Parameters.Add(new SqlParameter("@firstName", viewModel.instructor.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", viewModel.instructor.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", viewModel.instructor.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", viewModel.instructor.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@specialty", viewModel.instructor.Specialty));
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


        // GET: InstructorsController/Edit/5
        public ActionResult Edit(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = @"
                        SELECT
                            Id, FirstName, LastName, SlackHandle, CohortId, Specialty
                        FROM Instructors
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader ViewInstructorreader = cmd.ExecuteReader();
                    InstructorEditViewModel viewModel = new InstructorEditViewModel();


                    if (ViewInstructorreader.Read())
                    {
                        viewModel.instructor = new Instructor
                        {
                            Id = ViewInstructorreader.GetInt32(ViewInstructorreader.GetOrdinal("Id")),
                            FirstName = ViewInstructorreader.GetString(ViewInstructorreader.GetOrdinal("FirstName")),
                            LastName = ViewInstructorreader.GetString(ViewInstructorreader.GetOrdinal("LastName")),
                            SlackHandle = ViewInstructorreader.GetString(ViewInstructorreader.GetOrdinal("SlackHandle")),
                            CohortId = ViewInstructorreader.GetInt32(ViewInstructorreader.GetOrdinal("CohortId")),
                            Specialty = ViewInstructorreader.GetString(ViewInstructorreader.GetOrdinal("Specialty"))
                        };
                    }

                    ViewInstructorreader.Close();
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
                    if (viewModel.instructor != null)
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

        // POST: InstructorsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, InstructorEditViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Instructors
                                            SET FirstName = @firstName,
                                                LastName = @lastName,
                                                SlackHandle = @slackHandle,
                                                CohortId = @cohortId,
                                                Specialty = @specialty
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstName", viewModel.instructor.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", viewModel.instructor.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", viewModel.instructor.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", viewModel.instructor.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@specialty", viewModel.instructor.Specialty));
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

        // GET: InstructorsController/Delete/5
        public ActionResult Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, FirstName, LastName, SlackHandle, CohortId, Specialty
                        FROM Instructors
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor instructor = null;

                    if (reader.Read())
                    {
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Specialty = reader.GetString(reader.GetOrdinal("Specialty"))
                        };
                    }
                    reader.Close();

                    if (instructor != null)
                    {
                        return View(instructor);
                    }
                    else
                    {
                        return RedirectToAction(nameof(NotFound));
                    }
                }
            }
        }
    

    // POST: InstructorsController/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        using (SqlConnection conn = Connection)
        {
            conn.Open();
            using (SqlCommand cmd = conn.CreateCommand())
            {

                cmd.CommandText = @"DELETE FROM Instructors WHERE Id = @id";

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

    public new ActionResult NotFound()
    {
        return View();
    }

}
}