using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QSProject.Data.Models;
using QSProject.Data.Repositories;


namespace QSProject.Data.Services
{
    public static class ServiceSeeder
    {
        public static void Seed(IMedicineService svc)
        {
            svc.Initialise();

            // add patients
            var p1 = svc.AddPatient("Arthur Morgan", 38, "arthurmorgan@ulster.com", "https://upload.wikimedia.org/wikipedia/en/a/ac/Arthur_Morgan_-_Red_Dead_Redemption_2.png");
            var p2 = svc.AddPatient("Julian Simmons", 55, "juliansimmons@utv.ie", "https://upload.wikimedia.org/wikipedia/commons/a/ac/Juliansimmonscastlecourt.jpg");
            var p3 = svc.AddPatient("Layne Staley", 24, "layne@aliceinchains.tv", "https://upload.wikimedia.org/wikipedia/commons/8/8c/Staley05_%28cropped%29.jpg");
            var p4 = svc.AddPatient("Peter Steele", 44, "bigpete@typeonegative.ie", "https://upload.wikimedia.org/wikipedia/commons/6/6d/Type_O_Negative_-_Coliseu_dos_Recreios.jpg");



            // add medicine requests
            var r1 = svc.CreateMedicineRequest(p1.Id, "Cough medicine");
            var r2 = svc.CreateMedicineRequest(p2.Id, "Amoxicilin");
            var r3 = svc.CreateMedicineRequest(p3.Id, "Vitamin B12 Tablets");
            var r4 = svc.CreateMedicineRequest(p4.Id, "Diazapam");

            svc.CloseMedicineRequest(p1.Id, "Done");
            svc.CloseMedicineRequest(p2.Id, "Done");

            // add users
            var u1 = svc.Register("Patient", "patient@quickscripts.com", "patient", Role.patient);
            var u2 = svc.Register("Staff", "staff@quickscripts.com", "staff", Role.staff);

        }



    }
}
