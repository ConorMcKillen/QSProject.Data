using QSProject.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QSProject.Data.Services
{
    public class PatientServiceList : IMedicineService
    {
        private readonly string PATIENT_STORE = "patients.json";
        private readonly string USER_STORE = "users.json";

        private IList<Patient> Patients;
        private IList<User> Users;

        public PatientServiceList()
        {
            Load();
        }

        // load data from local json store
        private void Load()
        {
            try
            {
                string patients = File.ReadAllText(PATIENT_STORE);
                string users = File.ReadAllText(USER_STORE);
                Patients = JsonSerializer.Deserialize<List<Patient>>(patients);

                // ensure each patient medicine request Patient property is set as this is lost in serialization
                foreach (var p in Patients)
                {
                    foreach(var m in p.Medicines)
                    {
                        m.Patient = p;
                    }
                }
                Users = JsonSerializer.Deserialize<List<User>>(users);
            }
            catch (Exception )
            {
                Patients = new List<Patient>();
                Users = new List<User>();
            }
        }

        // write to local json store
        private void Store()
        {
            var patients = JsonSerializer.Serialize(Patients);
            File.WriteAllText(PATIENT_STORE, patients);
            var users = JsonSerializer.Serialize(Users);
            File.WriteAllText(USER_STORE, users);
        }

        public void Initialise()
        {
            Patients.Clear(); // wipe all patients
            Users.Clear();
        }

        //----------------------- Patient Related Operations -----------------------

        // retrieve list of Patients
        public IList<Patient> GetPatients()
        {
            return Patients;
        }

        // retrieve patient by ID
        public Patient GetPatient(int id)
        {
            return Patients.FirstOrDefault(p => p.Id == id);
        }

        // Add a new patient checking a patient with the same email does not exist
        public Patient AddPatient(string Name, int age, string email, string photo)
        {
            // check if email is already in use by another patient
            var existing = GetPatientByEmail(email);
            if (existing != null)
            {
                return null; // email in use so we cannot create patient
            }

            // email is unique so create patient
            var p = new Patient
            {
                Id = Patients.Count + 1,
                Name = Name,
                Age = age,
                Email = email,
                PhotoUrl = photo
            };

            Patients.Add(p);
            Store(); // write to local file store

            return p; // return newly added patient
        }

        // Delete patient identified by ID returning true if deleted and false if not found
        public bool DeletePatient(int id)
        {
            var p = GetPatient(id);

            if (p == null)
            {
                return false;
            }

            Patients.Remove(p);
            Store(); // write to local file store

            return true;
        }

        // Update the patient with the details in updated
        public Patient UpdatePatient(Patient updated)
        {
            // verify that the patient exists
            var patient = GetPatient(updated.Id);

            if (patient == null)
            {
                return null;
            }

            // update the details of the patient retrieved and save
            patient.Name = updated.Name;
            patient.Age = updated.Age;
            patient.Email = updated.Email;

            Store(); // write to local file store
            return patient;
        }

        public Patient GetPatientByEmail(string email)
        {
            return Patients.FirstOrDefault(p => p.Email == email);
        }

        public IList<Patient> GetPatientsMedicineRequest(Func<Patient, bool> r)
        {
            return Patients.Where(r).ToList();
        }

        public bool IsDuplicateEmail(string email, int patientId)
        {
            var existing = GetPatientByEmail(email);

            // if a patient with email exists and the ID does not match
            // the patient ID then they cannot use the email
            return existing != null && patientId != existing.Id;
        }

        // ---------------------------- Medicine management ----------------------------------

        public Medicine CreateMedicineRequest(int patientId, string request)
        {
            var patient = GetPatient(patientId);
            if (patient == null) return null;

            var medicine = new Medicine
            {
                Id = Patients.Sum(p => p.Medicines.Count()) + 1,
                MedicineName = request,
                CreatedOn = DateTime.Now,
                Active = true,
                PatientId = patientId,
                Patient = patient
            };

            patient.Medicines.Add(medicine);
            Store();
            return medicine;
        }

        // retrieve medicine request by id
        public Medicine GetMedicineRequest(int id)
        {
            return Patients.SelectMany(p => p.Medicines)
                           .FirstOrDefault(m => m.Id == id);
        }

        // close specified medicine request
        public Medicine CloseMedicineRequest(int id, string resolution)
        {
            var medicine = GetMedicineRequest(id);
            if (medicine == null || !medicine.Active) return null;

            medicine.Active = false;
            medicine.ResolvedOn = DateTime.Now;
            medicine.Resolution = resolution;
            Store();

            return medicine;
        }


        // remove specified medicine request
        public bool DeleteMedicineRequest(int id)
        {
            // find request
            var medicine = GetMedicineRequest(id);
            if (medicine == null) return false;

            // remove medicine request from patient
            var result = medicine.Patient.Medicines.Remove(medicine);
            Store();

            return result;
        }

        // retrieve all medicine requests
        public IList<Medicine> GetAllMedicineRequests()
        {
            var medicines = Patients.SelectMany(p => p.Medicines).ToList();

            return medicines;
        }

        // retrieve only open (active) tickets
        public IList<Medicine> GetOpenMedicineRequests()
        {
            var medicines = Patients.SelectMany(p => p.Medicines.Where(m => m.Active)).ToList();

            return medicines;
        }

        // perform search on medicine requests
        public IList<Medicine> SearchMedicineRequests(MedicineRange range, string request)
        {
            request = request == null ? "" : request.ToLower();

            // search medicine request patient name
            var m1 = Patients.SelectMany(p => p.Medicines).Where(m => GetPatient(m.PatientId).Name.ToLower().Contains(request.ToLower()));

            // search medicine request
            var m2 = Patients.SelectMany(p => p.Medicines)
                             .Where(m => m.MedicineName.ToLower().Contains(request.ToLower()));

            // execute the join request (calling ToList() executes the query)
            var m = m1.Union(m2).Where(p =>
                    range == MedicineRange.OPEN && p.Active ||
                    range == MedicineRange.CLOSED && !p.Active ||
                    range == MedicineRange.ALL
            ).ToList();

            return m;
        }

        public IList<Medicine> GetMedicineRequests(Func<Medicine, bool> r)
        {
            return Patients.SelectMany(p => p.Medicines)
                           .Where(r)
                           .ToList();
        }

        // -------------------------- User Related Operations ------------------------------

        // Retrieve user by email
        public User GetUserByEmail(string email)
        {
            return Users.FirstOrDefault(u => u.Email == email);
        }

        // Authenticate a user
        public User Authenticate(string email, string password)
        {
            // retrieve the user based on the Email address (assumes Email is unique)
            var user = GetUserByEmail(email);

            // Verify the user exists 
            // password matches the password provided
            if (user == null || user.Password != password)
            {
                return null; // no such user
            }

            return user; // user authenticated
        }

        // Register a new user
        public User Register(string Name, string email, string password, Role role)
        {
            // check that the user does not already exist (unique username)
            var exist = GetUserByEmail(email);
            if (exist != null)
            {
                return null;
            }

            // create user
            var user = new User
            {
                Name = Name,
                Password = password,
                Role = role
            };

            Users.Add(user);
            Store();
            return user;
        }

        
    }
}
