using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopulateDatabase
{
    class PopulateDatabase
    {
        const string tableDelete = "IF OBJECT_ID('dbo.patients', 'U') IS NOT NULL DROP TABLE dbo.patients;";

        const string tableCreate =
            "CREATE TABLE mindlinc.dbo.patients (" +
	        "id nvarchar(100) NOT NULL," +
            "family_name nvarchar(100)," +
            "given_name nvarchar(100)," +
            "birth_date date," +
            "gender nvarchar(100)," +
            "marital_status nvarchar(100)," +
            "address nvarchar(100)," +
            "telecom nvarchar(100)," +
            "\"language\" nvarchar(100)," +
            "managing_organization nvarchar(100)," +
            "active bit NOT NULL," +
	        "CONSTRAINT PK__patients__3213E83F55F33EAB PRIMARY KEY(id))";

        const string indexCreate = "CREATE UNIQUE INDEX PK__patients__3213E83F55F33EAB ON mindlinc.dbo.patients(id);";

        static Random rnd = new Random();
        static void Main(string[] args)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.AppSettings["connectionString"];
                conn.Open();

                SqlCommand dropSql = new SqlCommand(tableDelete, conn);
                dropSql.ExecuteNonQuery();

                SqlCommand tableSql = new SqlCommand(tableCreate, conn);
                tableSql.ExecuteNonQuery();

                //SqlCommand indexSql = new SqlCommand(indexCreate, conn);
                //indexSql.ExecuteNonQuery();

                foreach (var i in Enumerable.Range(0, 1000))
                {
                    var insertCmd = "insert into patients values (" +
                        "@ID, @LastName, @FirstName, @Date, @Gender, @MaritalStatus, " +
                        "@Address, @Telecom, @Language, @Org, @Active" +
                        ");";
                    SqlCommand cmd = new SqlCommand(insertCmd, conn);
                    cmd.Parameters.AddWithValue("@ID", pickId());
                    cmd.Parameters.AddWithValue("@LastName", pickLastName());
                    cmd.Parameters.AddWithValue("@FirstName", pickFirstName());
                    cmd.Parameters.AddWithValue("@Date", pickDate().ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@Gender", pickGender());
                    cmd.Parameters.AddWithValue("@MaritalStatus", pickMaritalStatus());
                    cmd.Parameters.AddWithValue("@Address", pickAddress());
                    cmd.Parameters.AddWithValue("@Telecom", pickTelecom());
                    cmd.Parameters.AddWithValue("@Language", pickLanguage());
                    cmd.Parameters.AddWithValue("@Org", pickManagingOrganization());
                    cmd.Parameters.AddWithValue("@Active", pickActive());
                    string c = cmd.CommandText;
                    foreach (SqlParameter p in cmd.Parameters) c = c.Replace(p.ParameterName, p.Value.ToString());
                    Console.WriteLine(c);
                    cmd.ExecuteNonQuery();
                }
            }
            Console.ReadLine();
        }

        static string pickLastName()
        {
            return Data.lastNames[rnd.Next(0, Data.lastNames.Length)];
        }

        static string pickFirstName()
        {
            return Data.firstNames[rnd.Next(0, Data.firstNames.Length)];
        }

        private static int nextId = 0;
        static string pickId()
        {
            nextId++;
            return "ML" + nextId.ToString("0000");
        }

        static DateTime pickDate()
        {
            return DateTime.Today.AddDays(-3650 - rnd.Next(0, 30000));
        }

        static string pickGender()
        {
            return Data.gender[rnd.Next(0, Data.gender.Length)];
        }

        static string pickMaritalStatus()
        {
            return Data.maritalStatus[rnd.Next(0, Data.maritalStatus.Length)];
        }

        static string pickAddress()
        {
            return rnd.Next(1, 10).ToString() + " "
                + Data.streetNames[rnd.Next(0, Data.streetNames.Length)] + " "
                + Data.streetEndings[rnd.Next(0, Data.streetEndings.Length)];
        }

        static string pickTelecom()
        {
            return rnd.Next(1000000, 9999999).ToString();
        }

        static string pickLanguage()
        {
            return Data.language[rnd.Next(0, Data.language.Length)];
        }

        static string pickManagingOrganization()
        {
            return Data.careTaker[rnd.Next(0, Data.careTaker.Length)];
        }

        static Boolean pickActive()
        {
            return rnd.Next(0, 10) > 0;
        }
    }

    class Data
    {
        public static string[] lastNames =
        {
            "Smith", "Jones", "Gates", "Buffet", "Styles", "Lee", "Baratheon", "Stark", "Adams", "Belaus",
            "Aznavour", "Asimov", "Colbert", "Davis", "Geller", "Harris", "Mannix", "Nixon", "Kennedy",
            "Obama", "Teller", "Jillette", "Welch", "Wood", "Warner", "Wentworth", "Vermeer"
        };

        public static string[] firstNames =
        {
            "Abe", "Barry", "Bart", "Carl", "Derek", "Dwight", "Dolores", "Earl", "George", "Frank", "Ivan",
            "John", "Chris", "Lilian", "Maria", "Nora", "Oliver", "Paul", "Raul", "Stephen", "Tom", "Walt",
            "Victor", "Walt", "Zack", "Charles", "Adam", "Dilbert", "Arya", "Robert", "Ray"
        };

        public static string[] streetNames =
        {
            "Hillview", "Hume", "Orchard", "Victoria", "Madison", "Wall", "Battery", "Mountbatten"
        };

        public static string[] streetEndings =
        {
            "Street", "Lane", "Ave", "Circus", "Crescent", "Drive"
        };

        public static string[] gender =
        {
            "male", "female"
        };

        public static string[] careTaker =
        {
            "The Best Health Care Corp.", "The Second Best Health Corp.", "The Worst Health Care Corp."
        };

        public static string[] maritalStatus =
        {
            "Married", "Single", "Divorced", "Widowed"
        };

        public static string[] language =
        {
            "English", "French", "Italian", "Spanish", "Portugese", "Japanese", "Mandarin", "German"
        };
    }
}
