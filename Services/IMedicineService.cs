using System;
using System.Collections.Generic;
using QSProject.Data.Models;
using QSProject.Data.Repositories;

namespace QSProject.Data.Services
{
    public interface IMedicineService
    {
        // Initalise the repository - only to be used during development
        void Initialise();

        // -------------------- Patient Management -------------------------------------
        IList<Patient> GetPatients();
        Patient GetPatient(int id);
        Patient GetPatientByEmail(string email);
        bool IsDuplicateEmail(string email, int patientId);
        Patient AddPatient(string name, int age, string email, string photo);
        Patient UpdatePatient(Patient updated);
        bool DeletePatient(int id);
        IList<Patient> GetPatientsMedicineRequest(Func<Patient, bool> r);



        // -------------------- Medicine Request Management ----------------------------

        Medicine CreateMedicineRequest(int patientId, string issue);
        Medicine GetMedicineRequest(int id);
        Medicine CloseMedicineRequest(int id, string resolution = "Prescription resolved.");
        bool DeleteMedicineRequest(int id);
        IList<Medicine> GetAllMedicineRequests();
        IList<Medicine> GetOpenMedicineRequests();
        IList<Medicine> SearchMedicineRequests(MedicineRange range, string request);
        IList<Medicine> GetMedicineRequests(Func<Medicine, bool> r);

        // ---------------------- User Management ---------------------------------------
        IList<User> GetUsers();
        User GetUser(int id);
        User GetUserByEmail(string email);
       
        User UpdateUser(User user);
        bool DeleteUser(int id);
        User Authenticate(string email, string password);
        User Register(string name, string email, string password, Role role);

    }
}
