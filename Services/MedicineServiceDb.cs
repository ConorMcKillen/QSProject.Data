using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QSProject.Data.Models;
using QSProject.Data.Repositories;
using QSProject.Data.Security;

namespace QSProject.Data.Services
{
    public class MedicineServiceDb : IMedicineService
    {
        private readonly MedicineDbContext db;

        public MedicineServiceDb()
        {
            db = new MedicineDbContext();
        }

        public void Initialise()
        {
            db.Initialise();
        }

        // ------------------------- Patient Related Operations -----------------------------

        // retrieve list of patients
        public IList<Patient> GetPatients()
        {
            // return the collection as a list
            return db.Patients.ToList();
        }

        // Retrieve patient by ID
        public Patient GetPatient(int id)
        {
            return db.Patients
                     .Include(p => p.Medicines)
                     .FirstOrDefault(p => p.Id == id);
        }

        // Add a new patient checking a patient with the same email does not exist
        public Patient AddPatient(string name, int age, string email, string photo)
        {
            // check if email is already in use by another account
            var existing = GetPatientByEmail(email);

            if (existing != null)
            {
                return null; // email in use so we cannot create patient
            }
            // email is unique so create patient
            var patient = new Patient
            {
                // Id is automatically set by the database
                Name = name,
                Age = age,
                Email = email,
                PhotoUrl = photo
            };

            db.Patients.Add(patient);
            db.SaveChanges(); // write to database
            return patient; // return newly added patient
        }

        // Delete the patient identified by ID returning true if deleted and false if not found
        public bool DeletePatient(int id)
        {
            var p = GetPatient(id);
            if (p == null)
            {
                return false;
            }

            db.Patients.Remove(p);
            db.SaveChanges(); // write to database
            return true;
        }

        // Update the patient with the details in updated
        public Patient UpdatePatient(Patient updated)
        {
            // verify the patient exists
            var patient = GetPatient(updated.Id);
            if (patient == null)
            {
                return null;
            }

            // update the details of the patient retrieved and save
            patient.Name = updated.Name;
            patient.Age = updated.Age;
            patient.Email = updated.Email;

            db.SaveChanges(); // write to database
            return patient;
        }

        public User GetUserByEmail(string email)
        {
            return db.Users.FirstOrDefault(u => u.Email == email);
        }

        public Patient GetPatientByEmail(string email)
        {
            return db.Patients.FirstOrDefault(p => p.Email == email);
        }


        public IList<Patient> GetPatientsMedicineRequest(Func<Patient, bool> r)
        {
            return db.Patients
                     .Include(m => m.Medicines)
                     .Where(r).ToList();
        }

        public bool IsDuplicateEmail(string email, int patientId)
        {
            var existing = GetPatientByEmail(email);
            // if a patient with email exists and the ID does not match
            // the patientId (if provided), then they cannot use the email
            return existing != null && patientId != existing.Id;
        }


        // ----------------------------------- Medicine Request Managment ---------------------------------------



        public Medicine CreateMedicineRequest(int patientId, string request)
        {
            var patient = GetPatient(patientId);

            if (patient == null) return null;

            var medicine = new Medicine
            {
                // ID created by database
                MedicineName = request,
                PatientId = patientId,

                // set by default in model but it can override here if required
                CreatedOn = DateTime.Now,
                Active = true
            };

            patient.Medicines.Add(medicine);
            db.SaveChanges(); // write to database

            return medicine;
        }

        // return medicine request and related patient
        public Medicine GetMedicineRequest(int id)
        {
            return db.Medicines
                     .Include(m => m.Patient)
                     .FirstOrDefault(m => m.Id == id);
        }

        // Close the specified medicine request - must exist and not already closed
        public Medicine CloseMedicineRequest(int id, string resolution)
        {
            var medicine = GetMedicineRequest(id);

            // if medicine request does not exist or is already closed return null
            if (medicine == null || medicine.Active == false) return null;

            // medicine request exists and is active so close
            medicine.Active = false;
            medicine.Resolution = resolution;
            medicine.ResolvedOn = DateTime.Now;

            db.SaveChanges(); // write to database
            return medicine; // return closed medicine request
        }

        // delete specified medicine request returning true if successful otherwise false
        public bool DeleteMedicineRequest(int id)
        {
            // find medicine request
            var medicine = GetMedicineRequest(id);

            if (medicine == null) return false;

            // remove medicine request from patient
            var result = medicine.Patient.Medicines.Remove(medicine);
            db.SaveChanges();

            return result;
        }

        // return all medicine requests and the patient generating the medicine request
        public IList<Medicine> GetAllMedicineRequests()
        {
            var medicines = db.Medicines
                             .Include(m => m.Patient)
                             .ToList();

            return medicines;
        }

        // get only active medicine requests and the patient generating the medicine request
        public IList<Medicine> GetOpenMedicineRequests()
        {
            return db.Medicines
                     .Include(m => m.Patient)
                     .Where(m => m.Active)
                     .ToList();
        }

        public IList<Medicine> GetMedicineRequests(Func<Medicine, bool> r)
        {
            return db.Medicines
                     .Include(m => m.Patient)
                     .Where(r).ToList();
        }

        // perform a search of all medicine requests based on request
        // and an active range of ALL, OPEN, CLOSED

        public IList<Medicine> SearchMedicineRequests(MedicineRange range, string request)
        {
            // ensure request is not null
            request ??= "";

            // search the patient name
            var p1 = db.Medicines
                       .Include(m => m.Patient)
                       .Where(m => m.Patient.Name.ToLower().Contains(request.ToLower()));

            // search medicine request
            var p2 = db.Medicines
                       .Include(m => m.Patient)
                       .Where(m => m.MedicineName.ToLower().Contains(request.ToLower()));

            // Use union to join both requests and Where to filter by medicine request status
            // Calling ToList() executes the final combined request
            return p1.Union(p2).Where(m =>
                   range == MedicineRange.OPEN && m.Active ||
                   range == MedicineRange.CLOSED && !m.Active ||
                   range == MedicineRange.ALL
                   ).ToList();
        }


        // ------------------- User related operations ----------------------------

        // retrieve list of users
        public IList<User> GetUsers()
        {
            return db.Users.ToList();
        }

        // retrieve user by id
        public User GetUser(int id)
        {
            return db.Users.FirstOrDefault(s => s.Id == id);
        }

        public bool DeleteUser(int id)
        {
            var s = GetUser(id);
            if (s == null)
            {
                return false;
            }

            db.Users.Remove(s);
            db.SaveChanges();

            return true;
        }

        // Update the user with the details in updated
        public User UpdateUser(User updated)
        {
            // verify that user exists
            var user = GetUser(updated.Id);

            if (user == null)
            {
                return null;
            }

            // update the details of the user retrieved and save
            user.Name = updated.Name;
            user.Email = updated.Email;
            user.Password = Hasher.CalculateHash(updated.Password);
            user.Role = updated.Role;

            db.SaveChanges();

            return user;
        }

        public IList<User> GetUsersQuery(Func<User, bool> q)
        {
            return db.Users.Where(q).ToList();
        }


        // Authenticate a user
        public User Authenticate(string email, string password)
        {
            // retrieve the user based on the email address (assumes Email is unique)
            var user = GetUserByEmail(email);

            // Verify the user exists and Hased User password matches the password provided
            // return user if authenticated otherwise null

            return (user != null && Hasher.ValidateHash(user.Password, password)) ? user : null;
        }

        // Register a new user
        public User Register(string Name, string email, string password, Role role)
        {
            // check that the user does not already exist
            var exists = GetUserByEmail(email);
            
            if (exists != null)
            {
                return null;
            }

            // create user
            var user = new User
            {
                Name = Name,
                Email = email,
                Password = Hasher.CalculateHash(password),
                Role = role
            };

            db.Users.Add(user);
            db.SaveChanges();
            return user;
        }

    }
}
