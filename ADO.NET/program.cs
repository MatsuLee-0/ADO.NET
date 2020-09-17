using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace _ExecutingCommands
{

    class program
    {

         static void Main(string[] args)//ExecutingCommands部分代码
        {
            ExecuteNonQuery();
            ExecuteReader();
            ExecuteScalar();
            ExecuteXmlReader();
        }

        static void ExecuteNonQuery()
        {
            string select = "UPDATE Customers,SET ContactName = 'Bob',WHERE ContactName = 'Bill'";
            SqlConnection conn = new SqlConnection(GetDatabaseConnection());
            conn.Open();
            SqlCommand cmd = new SqlCommand(select, conn);
            int rowsReturned = cmd.ExecuteNonQuery();
            Console.WriteLine("{0} rows returned", rowsReturned);
            conn.Close();
        }

        static void ExecuteReader()
        {
            string select = "SELECT ContactName,CompanyName FROM Customers";
            SqlConnection conn = new SqlConnection(GetDatabaseConnection());
            conn.Open();
            SqlCommand cmd = new SqlCommand(select, conn);
            int rowsReturned = cmd.ExecuteNonQuery();
            while (reader.Read())
            {
                Console.WriteLine("Contact: {0,-20} Company: (1)", reader[0], reader[1]);
            }
        }

        static void ExecuteScalar()
        {
            string select = "SELECT COUNT(*) FROM Customers";
            SqlConnection conn = new SqlConnection(GetDatabaseConnection());
            conn.Open();
            SqlCommand cmd = new SqlCommand(select, conn);
            object o = cmd.ExecuteScalar();
            Console.WriteLine(o);
            conn.Close();
        }

        static void ExecuteXmlReader()
        {
            string select = "SELECT ContactName,CompanyName,FROM Customers FOR XML AUTO";
            SqlConnection conn = new SqlConnection(GetDatabaseConnection());
            conn.Open();
            SqlCommand cmd = new SqlCommand(select, conn);
            XmlReader xr = cmd.ExecuteXmlReader();
            xr.Read();
            string data;
            do
            {
                data = xr.ReadOuterXml();
                if (!string.IsNullOrEmpty(data))
                    Console.WriteLine(data);
            } while (!string.IsNullOrEmpty(data));
            conn.Close();
        }//ExecutingCommands部分代码

        

        public static void Main(string[] agrs)//StoreProcs部分代码
        {
            using (SqlConnection conn = new SqlConnection(GetDatabaseConnection()))
            {
                conn.Open();
                InitialiseDatabase(conn);

                SqlCommand updateCommand = GenerateUpdateCommand(conn);
                SqlCommand deleteCommand = generateDeleteCommand(conn);
                SqlCommand insertCommand = GenerateInsertCommand(conn);

                DumpRegions(conn, "Regions prior to any stored procedure calls");

                insertCommand.Parameters["@RegionDescription"].Value = "South West";
                insertCommand.ExecuteNonQuery();
                
                int newRegionID = (int)insertCommand.Parameters["@RegionID"].Value;

                DumpRegions(conn, "Regions after inserting'South West'");

                updateCommand.Parameters[0].Value = newRegionID;
                updateCommand.Parameters[1].Value = "South Western England";
                updateCommand.ExecuteNonQuery();

                DumpRegions(conn, "Regions after updating'South West'to'South Western England'");

                deleteCommand.Parameters["@RegionID"].Value = newRegionID;
                deleteCommand.ExecuteNonQuery();

                DumpRegions(conn, "Regions after deleting'South Western England'");

                conn.Close();

            }
        }

        private static void InitialiseDatabase(SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand(StringSplitOptions.CreateSprocs, conn);
            cmd.ExecuteNonQuery();
        }

        private static SqlCommand GenerateUpdateCommand(SqlConnection conn)
        {
            SqlCommand aCommand = new SqlCommand("RegionUpdate", conn);

            aCommand.CommandType = CommandType.StoredProcedure;
            aCommand.Parameters.Add(new SqlParameter("@RegionID", SqlDbType.Int, 0, "RegionID"));
            aCommand.Parameters.Add(new SqlParameter("@RegionDescription", SqlDbType.NChar, 50, "RegionDescription"));
            aCommand.UpdatedRowSource = UpdateRowSource.None;

            return aCommand;
        }

        private static SqlCommand GenerateInsertCommand(SqlConnection conn)
        {
            SqlCommand aCommand = new SqlCommand("RegionInsert", conn);

            aCommand.CommandType = CommandType.StoredProcedure;
            aCommand.Parameters.Add(new SqlParameter("@RegionDescription", SqlDbType.NChar, 50, "RegionDescription"));
            aCommand.Parameters.Add(new SqlParameter("@RegionID", SqlDbType.Int, 0, ParameterDirection.Output, false, 0, 0, "RegionID", DataRowVersion.Default, null));
            aCommand.UpdatedRowSource = UpdateRowSource.OutputParameters;

            return aCommand;
        }

        private static void DumpRegions(SqlConnection conn,string message)
        {
            SqlCommand aCommand = new SqlCommand("SELECT RegionID,RegionDescription From Region", conn);

            SqlDataReader aReader = aCommand.ExecuteReader(CommandBehavior.KeyInfo);

            Console.WriteLine(message);

            while (aReader.Read())
            {
                Console.WriteLine("  {0,20} {1,-40}", aReader[0], aReader[1]);
            }

            aReader.Close();
        }

        static string GetDatabaseConnection()
        {
            return "server=(local),integrated security=SSPI,database=northwind";

        }//StoreProcs部分代码

    }
}